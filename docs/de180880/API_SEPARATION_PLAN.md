# Lộ trình tách Web API thành project riêng (giữ 100% tính năng + giao diện)

> **Mục tiêu:** Tách phần Web API khỏi `GearZone.Web` thành project độc lập `GearZone.Api`, để Razor Pages **tiêu thụ API qua HTTP** (yêu cầu môn PRN232), **mà không thay đổi giao diện hay hành vi** so với hiện tại.
>
> **Quyết định đã chốt:** giữ **cookie auth (chia sẻ Data Protection key)**, **không dùng JWT**. Làm **cuốn chiếu**, không big-bang.

---

## 1. Nguyên tắc đảm bảo "giống 100%"

1. **Không sửa file `.cshtml`** → giao diện không đổi (chỉ đổi cách `PageModel` lấy data trong `OnGet/OnPost`).
2. **Move code theo assembly trước, đổi runtime wiring sau** → mỗi phase kết thúc app vẫn chạy đủ.
3. **Mỗi phase có checkpoint** so với checklist smoke-test cố định (mục 7).
4. **Điểm dừng an toàn:** có thể dừng ở bất kỳ phase nào mà app vẫn đủ tính năng.

---

## 2. Hiện trạng (căn cứ code thật)

- **Một app duy nhất** `GearZone.Web` chứa: Razor Pages (`Pages/*`) + API controllers (`Controllers/Api/*`) + SignalR (`Hubs/`) + Hangfire + Identity + EF.
- **Auth:** Identity **cookie** (`IdentityConstants.ApplicationScheme`) + Google OAuth; login ở Razor page `/Auth/Login`.
- **`GearZone.Web` tham chiếu** `GearZone.Application` + `GearZone.Infrastructure` (có EF/DbContext).
- **`ApiResponse<T>`** nằm ở `GearZone.Web/Common` (mọi API bọc trong `{ success, data, message }`).
- **DTOs** nằm ở `GearZone.Application/Features/**/Dtos` (đã dùng chung).
- **React** (`gearzone-react`) là client riêng, gọi `/api/...`.
- **Phần coupling khó:** SignalR (chat, order-tracking) + Hangfire (payout/auto-complete) + PayOS webhook (trong `CheckoutController`).

---

## 3. Cấu trúc đích

```
GearZone.Domain            (giữ nguyên)
GearZone.Application        (giữ nguyên — chứa DTO + service)
GearZone.Infrastructure     (giữ nguyên — EF, DbContext, repo)
GearZone.Api      (MỚI)     → API controllers, Swagger, cookie-validation; ref Application + Infrastructure
GearZone.Web      (client)  → Razor Pages gọi GearZone.Api qua HttpClient
   • TẠM giữ: SignalR hubs, Hangfire, PayOS webhook, trang login (nơi phát cookie)
```

**`ApiResponse<T>`** → chuyển từ `GearZone.Web/Common` sang **`GearZone.Application/Common`** để cả Api lẫn Web dùng chung (phương án đơn giản nhất; nếu muốn sạch hơn có thể tạo `GearZone.Contracts`).

---

## 4. Chiến lược Auth — cookie chia sẻ key (không JWT)

Ý tưởng: **`GearZone.Web` vẫn là nơi login & phát cookie** (không đụng trang login → login giống 100%). **`GearZone.Api` chỉ validate lại đúng cookie đó.**

**4.1. Chia sẻ Data Protection key ring** — cấu hình **giống hệt** ở cả 2 app:
```csharp
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(sharedKeysPath)) // vd ổ chung / volume / DB
    .SetApplicationName("GearZone");                            // BẮT BUỘC trùng nhau
```
> `SetApplicationName` trùng + chung key ring là điều kiện để cookie Identity giải mã được ở app kia.

**4.2. `GearZone.Api` cần cấu hình Identity cookie giống Web** (cùng cookie name mặc định `.AspNetCore.Identity.Application`, cùng `ConfigureApplicationCookie`), và `AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>()` để validate security-stamp. (Api đã ref Infrastructure nên có sẵn DbContext.)

**4.3. Forward cookie khi Razor gọi API (server-to-server):**
```csharp
// DelegatingHandler
var cookie = _httpContextAccessor.HttpContext?.Request.Headers.Cookie.ToString();
if (!string.IsNullOrEmpty(cookie))
    request.Headers.TryAddWithoutValidation("Cookie", cookie);
```
> Đây là **server gọi server**, KHÔNG qua trình duyệt → **không dính SameSite/CORS**. `[Authorize(Roles=...)]` bên API chạy đúng như cũ. Đây chính là chỗ lần refactor trước hay hỏng (401).

**4.4. React → Api** (browser cross-origin): chỉ cần trỏ base URL sang host mới; CORS đã bật sẵn. (Nếu bật production cross-origin + cookie thì siết `WithOrigins(...).AllowCredentials()` + `SameSite=None`; dev proxy thì không cần.)

---

## 5. Xử lý phần coupling (để không vỡ)

| Thành phần | Cách xử lý |
|---|---|
| **SignalR (chat, order-tracking)** | **Giữ hub ở `GearZone.Web`** suốt quá trình. Controller chat/realtime migrate **wave cuối** hoặc giữ lại — tránh dựng backplane Redis. |
| **Hangfire (payout, auto-complete)** | Giữ ở `GearZone.Web` (không phải API). Không đụng. |
| **PayOS webhook (`CheckoutController`)** | Webhook do PayOS gọi trực tiếp → giữ ở nơi có endpoint ổn định; migrate cuối cùng, cẩn thận. |
| **Login / phát cookie** | Giữ ở Razor `/Auth/Login` (Web). API chỉ validate. Không đổi luồng login. |

---

## 6. Lộ trình theo phase

> Mỗi phase kết thúc = app chạy 100% như cũ + pass checklist mục 7.

### Phase 0 — Safety net (chưa move gì)
- [ ] Tạo nhánh riêng, vd `refactor/api-separation`.
- [ ] Lập **checklist smoke-test** theo role (mục 7) — "thước đo parity".
- [ ] Bật **shared Data Protection keys** trong app hiện tại (`PersistKeysToFileSystem` + `SetApplicationName`). Chưa đổi hành vi, nhưng chuẩn bị cho chia sẻ cookie.
- [ ] Chuyển `ApiResponse<T>` → `GearZone.Application/Common` (đổi namespace, cập nhật `using`). Build lại.
- **Verify:** app chạy y hệt; login/logout OK.

### Phase 1 — Tạo `GearZone.Api`, move controllers
- [ ] Tạo project `GearZone.Api` (Web API, net8.0), thêm vào solution; ref `Application` + `Infrastructure`.
- [ ] **Move** `Controllers/Api/*` + `BaseApiController` sang `GearZone.Api` (giữ nguyên namespace route `api/...` hoặc cập nhật).
- [ ] `Program.cs` của Api: `AddControllers` + Swagger + Identity cookie (mục 4.2) + shared Data Protection (mục 4.1) + CORS + `AddApplication`/`AddInfrastructure` + `AddDatabase`.
- [ ] Gỡ các controller đã move khỏi `GearZone.Web`.
- [ ] React: đổi `baseURL` sang host của `GearZone.Api`.
- [ ] **Razor CHƯA đổi** — vẫn gọi service trực tiếp.
- **Verify:** chạy song song 2 app; React hoạt động qua Api; toàn bộ Razor vẫn như cũ; Swagger của Api lên được; gọi 1 endpoint có `[Authorize]` bằng cookie thật → 200.

### Phase 2 — Hạ tầng client + trang pilot
- [ ] Trong `GearZone.Web`: `AddHttpContextAccessor`, đăng ký **typed `HttpClient`** trỏ Api + **`DelegatingHandler` forward cookie** (mục 4.3).
- [ ] Viết helper `GetDataAsync<T>()` unwrap `ApiResponse<T>` (+ xử lý null / non-200).
- [ ] Migrate **1 trang pilot** (đề xuất **Reports** — read-only, đã có sẵn API): `OnGetAsync` đổi `_service.Get...()` → `httpClient.GetFromJsonAsync<ApiResponse<T>>()`.
- **Verify:** trang Reports hiển thị **giống hệt** (số liệu, tab, chart, export CSV); các trang khác nguyên vẹn.

### Phase 3..N — Cuốn chiếu theo wave
Migrate PageModel theo độ khó tăng dần, verify sau mỗi wave:

| Wave | Nhóm trang | Ghi chú |
|---|---|---|
| **A. Read-only** | Reports, Revenue (list), Orders (list/detail), Products (list), Admin dashboards/list | Chỉ đổi `OnGetAsync`; dễ & cơ học |
| **B. CRUD/form** | Tạo/sửa/xoá Voucher, Product; duyệt/từ chối đơn; settings | `OnPostAsync` → `PostAsJsonAsync/Put/Delete`; map lỗi từ `ApiResponse.Errors` về `ModelState` để giữ hiển thị validation như cũ |
| **C. Coupled** | Chat (SignalR), Checkout/PayOS, upload ảnh, login | Rủi ro cao — làm cuối; cân nhắc giữ lại ở Web nếu không đáng công |

### Phase cuối — Dọn dẹp (decoupling thật)
- [ ] Khi mọi trang cần thiết đã qua API → **gỡ `ProjectReference` tới `GearZone.Infrastructure`** khỏi `GearZone.Web`.
- [ ] `GearZone.Web` chỉ còn ref `Application` (cho DTO) + hạ tầng HttpClient.
- **Verify:** full checklist; build sạch; không còn EF trong Web.

---

## 7. Checklist smoke-test parity (thước đo sau mỗi phase)

**Guest/Buyer:** xem trang chủ, search, xem sản phẩm/cửa hàng · thêm giỏ · checkout (COD + PayOS) · theo dõi đơn · viết review · chat với shop.
**Store Owner:** login · Dashboard/Reports (số liệu + chart + export) · Products CRUD + upload ảnh · duyệt/từ chối/giao đơn · Vouchers CRUD · Payouts · Reviews reply · Messages realtime.
**Admin:** login · Dashboard · Users/Stores/Applications · Products/Brands/Categories · Vouchers · Payouts (batch/transaction) · Settings · Wallet/Transactions.
**Chung:** login/logout, Google login, phân quyền (truy cập nhầm role → redirect), 401/403 đúng chỗ.

> Mỗi mục: thao tác → so kết quả & giao diện với bản `main`. Lệch = dừng, sửa trước khi đi tiếp.

---

## 8. Rủi ro & giảm thiểu

| Rủi ro | Giảm thiểu |
|---|---|
| **401 khi Razor gọi Api** (cookie không tới) | Kiểm chứng shared Data Protection + forward cookie ở Phase 1 trước khi migrate trang nào |
| **Mất validation hiển thị** ở form | Wave B: map `ApiResponse.Errors` → `ModelState` để `asp-validation` chạy như cũ |
| **SignalR realtime đứt** | Giữ hub ở Web; không tách chat sớm |
| **React đứt do đổi origin** | Cập nhật base URL + proxy; kiểm CORS |
| **Vòng HTTP dư thừa (perf)** | Chấp nhận (yêu cầu môn); không migrate webhook/Hangfire |
| **Regression khó thấy** | Checklist mục 7 + commit nhỏ theo wave để dễ rollback |

---

## 9. Ước lượng công sức (thô)

| Phase | Ước lượng |
|---|---|
| 0 — Safety net + move `ApiResponse` | ~0.5 buổi |
| 1 — Tạo `GearZone.Api` + move controllers + auth chung | ~1 buổi (chủ yếu vật lộn cookie/Data Protection) |
| 2 — Hạ tầng HttpClient + pilot Reports | ~0.5 buổi |
| 3..N — mỗi trang read-only ~15–30′, mỗi form ~1–2h | tuỳ số trang |
| Cuối — dọn ref + verify | ~0.5 buổi |

---

## 11. TRẠNG THÁI THỰC TẾ (cập nhật sau khi triển khai)

Nhánh: `bugfix/de180880-fix-api`

| Phase | Trạng thái | Ghi chú |
|---|---|---|
| **0 — Safety net** | ✅ Xong | `ApiResponse<T>` chuyển sang `Application/Common`; bật shared Data Protection (`SetApplicationName("GearZone")`). Làm đăng xuất 1 lần khi restart. |
| **1 — Tách `GearZone.Api`** | ✅ Xong, **verified runtime** | Move 31 controller + `BaseApiController`; Web proxy `/api/*` bằng **YARP**; Hangfire server chỉ chạy ở Web. Xác nhận: `/api/catalog/categories` trả JSON giống nhau ở `:5200` và `:5107`. |
| **2 — Pilot Reports** | ✅ Xong, **auth verified** | Dựng `IApiClient` + `CookieForwardingHandler`; Reports gọi `/api/seller/reports/*`. Log Api cho thấy nó resolve store theo userId ⟹ **cookie auth qua ranh giới project PASS**. |
| **3 — Trang tiêu biểu** | ✅ Xong (theo phạm vi đã chốt) | **Revenue** (read-only: filter/sort/paging) và **Vouchers** (full CRUD: POST/PUT/PATCH + map lỗi về `ModelState`). |
| **4 — Admin management pages** | ✅ Xong, **build verified** | 16 PageModel của Store Applications, Stores, Users, Orders, Products, Categories, Brands và Vouchers đã chuyển sang `IApiClient`; bổ sung typed response DTO, DELETE, multipart upload và category attributes qua API. |
| **5 — Toàn bộ Admin còn lại** | ✅ Xong, **build verified** | Dashboard, Wallet, Transactions, Settings, Payouts, Payout Batches, Payout Transaction Detail và Seller Payable Summary đã chuyển sang `IApiClient`. Toàn bộ Admin PageModel hiện consume API. |
| **Dọn ref Infrastructure khỏi Web** | ❌ Không làm | Vì chỉ migrate trang tiêu biểu, các trang còn lại vẫn cần gọi service in-process. |

### Phạm vi đã chốt: chỉ migrate trang tiêu biểu
Mục tiêu môn học là **chứng minh có consume Web API**, không phải viết lại toàn bộ 25+ trang. Bộ 3 trang đã chọn phủ đủ các kiểu tương tác:

| Trang | Chứng minh điều gì |
|---|---|
| **Reports** | GET với nhiều query param + **tải file** (export CSV) qua API |
| **Revenue** | GET có **filter / sort / paging** |
| **Vouchers** | **Full CRUD** — POST, PUT, PATCH + hiển thị lỗi validation từ API |

### Các trang CHƯA migrate (cố ý)
Các trang ngoài phạm vi hiện tại (StoreOwner: Orders/Products/Reviews/Settings/Messages/Disputes; Cart/Checkout/Public) **vẫn chạy cửa cũ** (PageModel → service in-process) và **hoạt động bình thường**. Toàn bộ PageModel thuộc `Pages/Admin` đã chuyển sang API.

### Chi tiết wave Admin đã migrate
- **Read-only:** Orders list/detail.
- **Workflow:** Store Applications approve/reject/request-info; Store Management lock/unlock/status.
- **CRUD:** Users, Vouchers, Categories và Brands.
- **Moderation:** Products list/detail, approve/reject/suspend/delete và bulk status.
- **Platform operations:** Dashboard, Wallet top-up, Transactions, Settings.
- **Payout:** transaction list/detail, batch list/detail/actions và Seller Payable Summary; batch thông thường enqueue Hangfire tại API, còn luồng payable giữ xử lý đồng bộ để trả kết quả cuối như trước.
- API trả typed DTO thay cho anonymous object; Category create trả ID để lưu attributes; Brand upload giữ `multipart/form-data`.
- `GearZone.Api` trả **401/403** cho authentication/authorization failure thay vì redirect HTML tới trang login.
- Checkpoint: `dotnet build GearZone.sln --no-restore` thành công và không còn Admin service injection trong 16 PageModel thuộc wave này.

Muốn migrate tiếp trang nào thì lặp lại đúng công thức:
1. Audit: API đã có endpoint trả đúng data chưa? (thiếu thì thêm — **Disputes chưa có API nào**)
2. Nếu API trả anonymous object → tạo DTO có kiểu để client deserialize.
3. Logic trùng giữa PageModel và controller → tách ra service ở Application.
4. Đổi PageModel sang `IApiClient`, **giữ nguyên tên/kiểu property** để `.cshtml` không phải sửa.
5. Build (Razor view compile = bằng chứng shape khớp) → test runtime.

### ⚠️ Vận hành
Từ giờ **phải chạy cả 2 project**: `GearZone.Api` (:5200) và `GearZone.Web` (:5107). Tắt Api thì Reports/Revenue/Vouchers của Seller, toàn bộ khu vực Admin, React và các JS gọi `/api` sẽ lỗi; các trang chưa migrate vẫn chạy.

---

## 10. Điểm quyết định còn mở (chốt khi triển khai)
- Chỗ **persist Data Protection keys** (thư mục chung / DB / volume) — tuỳ môi trường chạy của nhóm.
- Có tạo `GearZone.Contracts` cho DTO/ApiResponse không (sạch hơn) hay để tạm ở `Application` (nhanh hơn).
- `GearZone.Web` có tiếp tục serve React không, hay React deploy riêng.
- Các controller ở `Controllers/` gốc (Banks, Cart, Maps, OrderApi) — move sang Api hay giữ ở Web.
