using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Common.Models;
using GearZone.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GearZone.Web.Pages.StoreOwner.Revenue
{
    [Authorize(Roles = "Store Owner")]
    public class IndexModel : PageModel
    {
        private readonly IStoreRepository _storeRepository;
        private readonly IPayoutTransactionRepository _payoutTransactionRepository;

        private const int DefaultPageSize = 10;

        public IndexModel(
            IStoreRepository storeRepository,
            IPayoutTransactionRepository payoutTransactionRepository)
        {
            _storeRepository = storeRepository;
            _payoutTransactionRepository = payoutTransactionRepository;
        }

        public bool HasStore { get; set; }
        public string StoreName { get; set; } = "Your Store";
        public SellerPayoutSummaryViewModel Summary { get; set; } = new();
        public PagedResult<SellerPayoutTransactionItemViewModel> Transactions { get; set; } =
            new(new List<SellerPayoutTransactionItemViewModel>(), 0, 1, DefaultPageSize);

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public PayoutTransactionStatus? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MinAmount { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MaxAmount { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? DateRangeShortcut { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? DateRange { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "date";

        [BindProperty(SupportsGet = true)]
        public string SortDirection { get; set; } = "desc";

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public async Task<IActionResult> OnGetAsync()
        {
            var ownerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(ownerUserId))
            {
                return RedirectToPage("/Public/Auth/Login");
            }

            var store = await _storeRepository.GetStoreByOwnerIdAsync(ownerUserId);
            if (store == null)
            {
                return Page();
            }

            HasStore = true;
            StoreName = store.StoreName;
            PageNumber = PageNumber < 1 ? 1 : PageNumber;
            SortBy = string.IsNullOrWhiteSpace(SortBy) ? "date" : SortBy;
            SortDirection = string.Equals(SortDirection, "asc", StringComparison.OrdinalIgnoreCase) ? "asc" : "desc";

            var (startDate, endDate) = ResolveDateRange();

            var query = _payoutTransactionRepository.Query()
                .Where(x => x.StoreId == store.Id);

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var search = SearchTerm.Trim().ToLowerInvariant();
                query = query.Where(x =>
                    x.TransactionCode.ToLower().Contains(search) ||
                    (x.Batch != null && x.Batch.BatchCode.ToLower().Contains(search)));
            }

            if (Status.HasValue)
            {
                query = query.Where(x => x.Status == Status.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(x => x.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(x => x.CreatedAt <= endOfDay);
            }

            if (MinAmount.HasValue)
            {
                query = query.Where(x => x.NetAmount >= MinAmount.Value);
            }

            if (MaxAmount.HasValue)
            {
                query = query.Where(x => x.NetAmount <= MaxAmount.Value);
            }

            Summary.TotalTransactions = await query.CountAsync();
            Summary.TotalNetAmount = await query.SumAsync(x => (decimal?)x.NetAmount) ?? 0m;
            Summary.PendingCount = await query.CountAsync(x =>
                x.Status == PayoutTransactionStatus.Queued ||
                x.Status == PayoutTransactionStatus.Processing ||
                x.Status == PayoutTransactionStatus.ManualRequired);
            Summary.CompletedCount = await query.CountAsync(x => x.Status == PayoutTransactionStatus.Success);
            Summary.FailedCount = await query.CountAsync(x =>
                x.Status == PayoutTransactionStatus.Failed ||
                x.Status == PayoutTransactionStatus.Excluded);
            Summary.PendingAmount = await query
                .Where(x =>
                    x.Status == PayoutTransactionStatus.Queued ||
                    x.Status == PayoutTransactionStatus.Processing ||
                    x.Status == PayoutTransactionStatus.ManualRequired)
                .SumAsync(x => (decimal?)x.NetAmount) ?? 0m;
            Summary.PaidAmount = await query
                .Where(x => x.Status == PayoutTransactionStatus.Success)
                .SumAsync(x => (decimal?)x.NetAmount) ?? 0m;

            var isAsc = SortDirection == "asc";
            query = SortBy.ToLowerInvariant() switch
            {
                "code" => isAsc ? query.OrderBy(x => x.TransactionCode) : query.OrderByDescending(x => x.TransactionCode),
                "net" => isAsc ? query.OrderBy(x => x.NetAmount) : query.OrderByDescending(x => x.NetAmount),
                "status" => isAsc ? query.OrderBy(x => x.Status) : query.OrderByDescending(x => x.Status),
                _ => isAsc ? query.OrderBy(x => x.CreatedAt) : query.OrderByDescending(x => x.CreatedAt)
            };

            var totalCount = Summary.TotalTransactions;
            var items = await query
                .Skip((PageNumber - 1) * DefaultPageSize)
                .Take(DefaultPageSize)
                .Select(x => new SellerPayoutTransactionItemViewModel
                {
                    Id = x.Id,
                    TransactionCode = x.TransactionCode,
                    BatchCode = x.Batch != null ? x.Batch.BatchCode : string.Empty,
                    OrderCount = x.OrderCount,
                    GrossAmount = x.GrossAmount,
                    CommissionAmount = x.CommissionAmount,
                    NetAmount = x.NetAmount,
                    BankName = x.BankName,
                    BankAccountNumber = x.BankAccountNumber,
                    Status = x.Status,
                    CreatedAt = x.CreatedAt,
                    ProcessedAt = x.ProcessedAt
                })
                .ToListAsync();

            Transactions = new PagedResult<SellerPayoutTransactionItemViewModel>(
                items,
                totalCount,
                PageNumber,
                DefaultPageSize);

            return Page();
        }

        private (DateTime? StartDate, DateTime? EndDate) ResolveDateRange()
        {
            if (string.IsNullOrWhiteSpace(DateRangeShortcut))
            {
                return (null, null);
            }

            var today = DateTime.UtcNow.Date;
            return DateRangeShortcut.ToLowerInvariant() switch
            {
                "today" => (today, today),
                "week" => (today.AddDays(-7), today),
                "month" => (today.AddDays(-30), today),
                "custom" => ParseCustomDateRange(),
                _ => (null, null)
            };
        }

        private (DateTime? StartDate, DateTime? EndDate) ParseCustomDateRange()
        {
            if (string.IsNullOrWhiteSpace(DateRange))
            {
                return (null, null);
            }

            var dates = DateRange.Split(" to ", StringSplitOptions.RemoveEmptyEntries);
            if (dates.Length == 2)
            {
                var start = DateTime.TryParse(dates[0], out var parsedStart) ? parsedStart : (DateTime?)null;
                var end = DateTime.TryParse(dates[1], out var parsedEnd) ? parsedEnd : (DateTime?)null;
                return (start, end);
            }

            if (dates.Length == 1 && DateTime.TryParse(dates[0], out var singleDate))
            {
                return (singleDate, singleDate);
            }

            return (null, null);
        }

        public class SellerPayoutSummaryViewModel
        {
            public int TotalTransactions { get; set; }
            public int PendingCount { get; set; }
            public int CompletedCount { get; set; }
            public int FailedCount { get; set; }
            public decimal TotalNetAmount { get; set; }
            public decimal PendingAmount { get; set; }
            public decimal PaidAmount { get; set; }
        }

        public class SellerPayoutTransactionItemViewModel
        {
            public Guid Id { get; set; }
            public string TransactionCode { get; set; } = string.Empty;
            public string BatchCode { get; set; } = string.Empty;
            public int OrderCount { get; set; }
            public decimal GrossAmount { get; set; }
            public decimal CommissionAmount { get; set; }
            public decimal NetAmount { get; set; }
            public string BankName { get; set; } = string.Empty;
            public string BankAccountNumber { get; set; } = string.Empty;
            public PayoutTransactionStatus Status { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? ProcessedAt { get; set; }
        }
    }
}
