[CmdletBinding()]
param(
    [string]$EnvironmentFile = (Join-Path $PSScriptRoot '..\GearZone.Web\.env'),
    [string]$CloudinaryFolder = 'GearZone/products/imported',
    [int]$DelayMilliseconds = 350,
    [int]$Limit = 0
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Data
Add-Type -AssemblyName System.Net.Http
Add-Type -AssemblyName System.Web
$newtonsoftAssembly = Join-Path ([Environment]::GetFolderPath('UserProfile')) `
    '.nuget\packages\newtonsoft.json\13.0.3\lib\net45\Newtonsoft.Json.dll'
$cloudinaryAssembly = Join-Path ([Environment]::GetFolderPath('UserProfile')) `
    '.nuget\packages\cloudinarydotnet\1.28.0\lib\net452\CloudinaryDotNet.dll'
if (-not (Test-Path -LiteralPath $newtonsoftAssembly)) {
    throw "Newtonsoft.Json assembly not found. Restore NuGet packages first: $newtonsoftAssembly"
}
if (-not (Test-Path -LiteralPath $cloudinaryAssembly)) {
    throw "CloudinaryDotNet assembly not found. Restore NuGet packages first: $cloudinaryAssembly"
}
Add-Type -Path $newtonsoftAssembly
Add-Type -Path $cloudinaryAssembly
$script:DuckDuckGoSession = [Microsoft.PowerShell.Commands.WebRequestSession]::new()
$script:SearchHeaders = @{
    'User-Agent' = 'Mozilla/5.0'
    'Accept-Language' = 'en-US,en;q=0.9'
}

function Read-DotEnv {
    param([Parameter(Mandatory)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Environment file not found: $Path"
    }

    $values = @{}
    foreach ($line in Get-Content -LiteralPath $Path) {
        $trimmed = $line.Trim()
        if (-not $trimmed -or $trimmed.StartsWith('#')) {
            continue
        }

        $separator = $trimmed.IndexOf('=')
        if ($separator -lt 1) {
            continue
        }

        $name = $trimmed.Substring(0, $separator).Trim()
        $value = $trimmed.Substring($separator + 1).Trim()
        if (($value.StartsWith('"') -and $value.EndsWith('"')) -or
            ($value.StartsWith("'") -and $value.EndsWith("'"))) {
            $value = $value.Substring(1, $value.Length - 2)
        }

        $values[$name] = $value
    }

    return $values
}

function New-HttpClient {
    $handler = [System.Net.Http.HttpClientHandler]::new()
    $handler.AutomaticDecompression = [System.Net.DecompressionMethods]::GZip -bor
        [System.Net.DecompressionMethods]::Deflate
    $client = [System.Net.Http.HttpClient]::new($handler)
    $client.Timeout = [TimeSpan]::FromSeconds(45)
    $client.DefaultRequestHeaders.UserAgent.ParseAdd(
        'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/124.0 Safari/537.36'
    )
    $client.DefaultRequestHeaders.Accept.ParseAdd('text/html,application/json,image/avif,image/webp,image/*,*/*')
    return $client
}

function Get-DuckDuckGoToken {
    param(
        [Parameter(Mandatory)][System.Net.Http.HttpClient]$Client,
        [Parameter(Mandatory)][string]$Query
    )

    $uri = 'https://duckduckgo.com/?q=' + [Uri]::EscapeDataString($Query)
    $html = (Invoke-WebRequest `
        -UseBasicParsing `
        -WebSession $script:DuckDuckGoSession `
        -Headers $script:SearchHeaders `
        -Uri $uri).Content
    $match = [regex]::Match($html, 'vqd=["'']?([\d-]+)')
    if (-not $match.Success) {
        throw 'DuckDuckGo did not return an image-search token.'
    }

    return $match.Groups[1].Value
}

function Get-SearchTokens {
    param([Parameter(Mandatory)][string]$Text)

    $ignored = @(
        'and', 'the', 'with', 'for', 'gaming', 'wireless', 'wired', 'full', 'size',
        'inch', 'inches', 'monitor', 'keyboard', 'mouse', 'headset', 'headphones',
        'microphone', 'power', 'supply', 'edition', 'product', 'official'
    )
    $normalized = $Text.ToLowerInvariant() -replace '[^a-z0-9]+', ' '
    return @($normalized.Split(' ', [StringSplitOptions]::RemoveEmptyEntries) |
        Where-Object { $_.Length -ge 2 -and $_ -notin $ignored } |
        Select-Object -Unique)
}

function Get-ImageCandidates {
    param(
        [Parameter(Mandatory)][System.Net.Http.HttpClient]$Client,
        [Parameter(Mandatory)][string]$ProductName,
        [Parameter(Mandatory)][string]$Brand
    )

    $query = '"' + $ProductName + '" official product image white background'
    $vqd = Get-DuckDuckGoToken -Client $Client -Query $query
    $uri = 'https://duckduckgo.com/i.js?l=us-en&o=json&q=' +
        [Uri]::EscapeDataString($query) + '&vqd=' + [Uri]::EscapeDataString($vqd) +
        '&f=size%3ALarge'

    $headers = @{}
    foreach ($key in $script:SearchHeaders.Keys) {
        $headers[$key] = $script:SearchHeaders[$key]
    }
    $headers['Accept'] = 'application/json'
    $headers['Referer'] = 'https://duckduckgo.com/'
    $headers['X-Requested-With'] = 'XMLHttpRequest'
    $json = (Invoke-WebRequest `
        -UseBasicParsing `
        -WebSession $script:DuckDuckGoSession `
        -Headers $headers `
        -Uri $uri).Content | ConvertFrom-Json

    $tokens = Get-SearchTokens -Text $ProductName
    $brandToken = ($Brand.ToLowerInvariant() -replace '[^a-z0-9]+', '')
    $ranked = foreach ($result in @($json.results)) {
        if ([string]::IsNullOrWhiteSpace($result.image)) {
            continue
        }

        $haystack = (($result.title + ' ' + $result.url + ' ' + $result.image).ToLowerInvariant() -replace '[^a-z0-9]+', '')
        $matched = 0
        foreach ($token in $tokens) {
            $compactToken = $token -replace '[^a-z0-9]+', ''
            if ($compactToken -and $haystack.Contains($compactToken)) {
                $matched++
            }
        }

        $score = if ($tokens.Count) { 10.0 * $matched / $tokens.Count } else { 0.0 }
        if ($brandToken -and $haystack.Contains($brandToken)) {
            $score += 2.0
        }
        if ($result.width -ge 700 -and $result.height -ge 500) {
            $score += 1.0
        }
        if ($result.image -match '(?i)logo|icon|banner|avatar') {
            $score -= 5.0
        }
        if ($result.title -match '(?i)review|unboxing') {
            $score -= 0.5
        }

        [PSCustomObject]@{
            ImageUrl = [string]$result.image
            ThumbnailUrl = [string]$result.thumbnail
            SourceUrl = [string]$result.url
            Title = [string]$result.title
            Width = [int]$result.width
            Height = [int]$result.height
            Score = $score
        }
    }

    return @($ranked | Sort-Object Score -Descending | Select-Object -First 12)
}

function Get-ImageBytes {
    param(
        [Parameter(Mandatory)][System.Net.Http.HttpClient]$Client,
        [Parameter(Mandatory)][object[]]$Candidates
    )

    foreach ($candidate in $Candidates) {
        foreach ($url in @($candidate.ImageUrl, $candidate.ThumbnailUrl) | Where-Object { $_ }) {
            try {
                $request = [System.Net.Http.HttpRequestMessage]::new([System.Net.Http.HttpMethod]::Get, $url)
                if ($candidate.SourceUrl) {
                    try { $request.Headers.Referrer = [Uri]$candidate.SourceUrl } catch {}
                }
                $response = $Client.SendAsync($request).GetAwaiter().GetResult()
                if (-not $response.IsSuccessStatusCode) {
                    $response.Dispose()
                    $request.Dispose()
                    continue
                }

                $contentType = [string]$response.Content.Headers.ContentType.MediaType
                $bytes = $response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult()
                $response.Dispose()
                $request.Dispose()

                if ($bytes.Length -lt 8000) {
                    continue
                }
                if ($contentType -and -not $contentType.StartsWith('image/', [StringComparison]::OrdinalIgnoreCase)) {
                    continue
                }

                return [PSCustomObject]@{
                    Bytes = $bytes
                    ContentType = $(if ($contentType) { $contentType } else { 'image/jpeg' })
                    SourceUrl = $candidate.SourceUrl
                    OriginalImageUrl = $url
                    Title = $candidate.Title
                    Width = $candidate.Width
                    Height = $candidate.Height
                }
            }
            catch {
                if ($null -ne $response) { $response.Dispose() }
                if ($null -ne $request) { $request.Dispose() }
            }
        }
    }

    throw 'No usable image could be downloaded from the search results.'
}

function Send-ToCloudinary {
    param(
        [Parameter(Mandatory)][System.Net.Http.HttpClient]$Client,
        [Parameter(Mandatory)][byte[]]$Bytes,
        [Parameter(Mandatory)][string]$ContentType,
        [Parameter(Mandatory)][string]$PublicId,
        [Parameter(Mandatory)][string]$Folder,
        [Parameter(Mandatory)][string]$CloudName,
        [Parameter(Mandatory)][string]$ApiKey,
        [Parameter(Mandatory)][string]$ApiSecret
    )

    $account = [CloudinaryDotNet.Account]::new($CloudName, $ApiKey, $ApiSecret)
    $cloudinary = [CloudinaryDotNet.Cloudinary]::new($account)
    $stream = [IO.MemoryStream]::new($Bytes, $false)
    try {
        $uploadParams = [CloudinaryDotNet.Actions.ImageUploadParams]::new()
        $uploadParams.File = [CloudinaryDotNet.FileDescription]::new("$PublicId.jpg", $stream)
        $uploadParams.Folder = $Folder
        $uploadParams.PublicId = $PublicId
        $uploadParams.Overwrite = $true
        $uploadParams.UniqueFilename = $false
        $uploadParams.UseFilename = $false

        $result = $cloudinary.UploadAsync($uploadParams).GetAwaiter().GetResult()
        if ($null -ne $result.Error) {
            throw "Cloudinary upload failed: $($result.Error.Message)"
        }
        if ($null -eq $result.SecureUrl -or [string]::IsNullOrWhiteSpace($result.SecureUrl.AbsoluteUri)) {
            throw 'Cloudinary response did not include a secure URL.'
        }

        return [string]$result.SecureUrl.AbsoluteUri
    }
    finally {
        $stream.Dispose()
    }
}

function Get-ProductsWithoutImages {
    param([Parameter(Mandatory)][System.Data.SqlClient.SqlConnection]$Connection)

    $command = $Connection.CreateCommand()
    $command.CommandText = @'
SELECT p.Id, p.Name, p.Slug, b.Name AS Brand, c.Name AS Category
FROM Products p
INNER JOIN Brands b ON b.Id = p.BrandId
INNER JOIN Categories c ON c.Id = p.CategoryId
WHERE p.IsDeleted = 0
  AND NOT EXISTS (SELECT 1 FROM ProductImages pi WHERE pi.ProductId = p.Id)
ORDER BY c.Name, p.Name;
'@

    $products = [Collections.Generic.List[object]]::new()
    $reader = $command.ExecuteReader()
    try {
        while ($reader.Read()) {
            $products.Add([PSCustomObject]@{
                Id = $reader.GetGuid(0)
                Name = $reader.GetString(1)
                Slug = $reader.GetString(2)
                Brand = $reader.GetString(3)
                Category = $reader.GetString(4)
            })
        }
    }
    finally {
        $reader.Dispose()
        $command.Dispose()
    }

    return @($products)
}

function Add-ProductImage {
    param(
        [Parameter(Mandatory)][System.Data.SqlClient.SqlConnection]$Connection,
        [Parameter(Mandatory)][Guid]$ProductId,
        [Parameter(Mandatory)][string]$ImageUrl
    )

    $command = $Connection.CreateCommand()
    $command.CommandText = @'
IF NOT EXISTS (SELECT 1 FROM ProductImages WHERE ProductId = @ProductId)
BEGIN
    INSERT INTO ProductImages (Id, ProductId, ImageUrl, IsPrimary, SortOrder)
    VALUES (NEWID(), @ProductId, @ImageUrl, 1, 0);
END;
'@
    [void]$command.Parameters.Add('@ProductId', [System.Data.SqlDbType]::UniqueIdentifier)
    $command.Parameters['@ProductId'].Value = $ProductId
    [void]$command.Parameters.Add('@ImageUrl', [System.Data.SqlDbType]::NVarChar, 1000)
    $command.Parameters['@ImageUrl'].Value = $ImageUrl
    try {
        [void]$command.ExecuteNonQuery()
    }
    finally {
        $command.Dispose()
    }
}

$envValues = Read-DotEnv -Path $EnvironmentFile
$requiredKeys = @(
    'DB_CONNECTION_STRING',
    'CLOUDINARY_CLOUD_NAME',
    'CLOUDINARY_API_KEY',
    'CLOUDINARY_API_SECRET'
)
foreach ($key in $requiredKeys) {
    if ([string]::IsNullOrWhiteSpace($envValues[$key])) {
        throw "Missing required environment value: $key"
    }
}

$connection = [System.Data.SqlClient.SqlConnection]::new($envValues['DB_CONNECTION_STRING'])
$httpClient = New-HttpClient
$failures = [Collections.Generic.List[object]]::new()
$imported = [Collections.Generic.List[object]]::new()

try {
    $connection.Open()
    $products = @(Get-ProductsWithoutImages -Connection $connection)
    if ($Limit -gt 0) {
        $products = @($products | Select-Object -First $Limit)
    }

    Write-Host "Products without images: $($products.Count)"
    for ($index = 0; $index -lt $products.Count; $index++) {
        $product = $products[$index]
        $position = $index + 1
        Write-Host "[$position/$($products.Count)] $($product.Name)" -ForegroundColor Cyan

        try {
            $candidates = @(Get-ImageCandidates -Client $httpClient -ProductName $product.Name -Brand $product.Brand)
            if (-not $candidates.Count) {
                throw 'Image search returned no candidates.'
            }

            $image = Get-ImageBytes -Client $httpClient -Candidates $candidates
            $cloudinaryUrl = Send-ToCloudinary `
                -Client $httpClient `
                -Bytes $image.Bytes `
                -ContentType $image.ContentType `
                -PublicId $product.Slug `
                -Folder $CloudinaryFolder `
                -CloudName $envValues['CLOUDINARY_CLOUD_NAME'] `
                -ApiKey $envValues['CLOUDINARY_API_KEY'] `
                -ApiSecret $envValues['CLOUDINARY_API_SECRET']

            Add-ProductImage -Connection $connection -ProductId $product.Id -ImageUrl $cloudinaryUrl
            $imported.Add([PSCustomObject]@{
                ProductId = $product.Id
                Product = $product.Name
                ImageUrl = $cloudinaryUrl
                SourcePage = $image.SourceUrl
                SourceImage = $image.OriginalImageUrl
            })
            Write-Host "  Imported: $cloudinaryUrl" -ForegroundColor Green
        }
        catch {
            $failures.Add([PSCustomObject]@{
                ProductId = $product.Id
                Product = $product.Name
                Error = $_.Exception.Message
            })
            Write-Warning "  Failed: $($_.Exception.Message)"
        }

        if ($DelayMilliseconds -gt 0) {
            Start-Sleep -Milliseconds $DelayMilliseconds
        }
    }
}
finally {
    $httpClient.Dispose()
    $connection.Dispose()
}

Write-Host ''
Write-Host "Imported: $($imported.Count)" -ForegroundColor Green
Write-Host "Failed: $($failures.Count)" -ForegroundColor $(if ($failures.Count) { 'Yellow' } else { 'Green' })

if ($failures.Count) {
    $failures | Format-Table Product, Error -AutoSize | Out-String | Write-Host
    exit 1
}
