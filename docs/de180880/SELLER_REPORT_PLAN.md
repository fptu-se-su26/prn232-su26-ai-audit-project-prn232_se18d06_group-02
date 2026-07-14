# Seller Report / Analytics — Kế hoạch triển khai

**Branch:** `feature/DE180880-seller-report`
**Mục tiêu:** Bổ sung module **Báo cáo (Report/Analytics)** cho Store Owner, chuẩn theo các Seller Center thực tế (Shopee, Lazada, Amazon Seller Central).

---

## 1. Bối cảnh — cái đã có vs cái làm thêm

| Đã có | Vai trò |
|---|---|
| `Seller/DashboardController` | KPI tổng quan tức thời (đơn, doanh thu, chart 6 tháng, đơn/payout gần đây) |
| `Seller/RevenueController` | Danh sách payout transaction (settlement), lọc theo trạng thái/ngày/số tiền |

**Khác biệt của module Report** so với Dashboard: có **chọn khoảng thời gian + granularity (ngày/tuần/tháng)**, **so sánh với kỳ trước (%)**, **drill-down theo sản phẩm/đơn**, và **export file (CSV)**. Đây là các đặc trưng định nghĩa một "report" thật.

---

## 2. Phạm vi (Full — 3 phase)

### Phase 1 — Sales Report (cốt lõi)
- Doanh thu (gross), doanh thu ròng (net sau commission), số đơn, số sản phẩm bán (units), **AOV** (giá trị đơn trung bình).
- Đường xu hướng theo **ngày / tuần / tháng** (tự chọn theo độ dài khoảng thời gian).
- **So sánh kỳ trước** cùng độ dài → mỗi chỉ số kèm `% thay đổi`.
- **Export CSV**.

### Phase 2 — Product & Operations
- **Product performance:** top sản phẩm theo doanh thu/units/số đơn; danh sách **tồn kho thấp** (≤ 5).
- **Operations/Fulfillment:** tỷ lệ hủy, tỷ lệ hoàn, tỷ lệ hoàn tất, **thời gian giao trung bình** (CreatedAt → DeliveredAt), phân bố trạng thái đơn.

### Phase 3 — Customer, Marketing, Reviews
- **Customer:** khách unique, khách mới vs quay lại (repeat rate), follower tăng trưởng theo thời gian.
- **Marketing:** hiệu quả voucher (số lượt dùng, tổng giảm giá) trong kỳ.
- **Reviews:** rating trung bình, phân bố sao (1–5), tỷ lệ đã phản hồi, xu hướng rating theo thời gian.

---

## 3. Thiết kế kỹ thuật (giữ đúng Clean Architecture)

### 3.1. Domain — KHÔNG thêm bảng mới
Tận dụng entity sẵn có:
`SubOrder`, `OrderItem`, `Order`, `ProductVariant`/`Product`, `StoreFollow`, `Voucher`/`VoucherUsage`, `ProductReview`.

### 3.2. Application layer
- `Features/Seller/Dtos/SellerReportDtos.cs` — query DTO (`SellerReportQueryDto`) + các response DTO; `MetricDto` (current/previous/changePct) cho so sánh kỳ.
- `Abstractions/Services/ISellerReportService.cs`
- `Features/Seller/SellerReportService.cs` — toàn bộ logic tính toán:
  - Chuẩn hóa khoảng thời gian: `today / 7d / 30d / thisMonth / lastMonth / custom`.
  - Kỳ trước = kỳ liền trước cùng độ dài.
  - Doanh thu **loại trừ** `Cancelled / Rejected / Refunded`.
  - Bucketing time-series theo `day/week/month`.
  - Tính toán bằng in-memory projection (an toàn với EF Core translation, quy mô 1 store).
- Đăng ký DI trong `Application/DependencyInjection.cs`.

### 3.3. Web layer — `Seller/ReportsController` (`[Authorize(Roles = "Store Owner")]`)
| Endpoint | Trả về |
|---|---|
| `GET /api/seller/reports/sales` | `SalesReportDto` |
| `GET /api/seller/reports/sales/export` | file `text/csv` |
| `GET /api/seller/reports/products` | `ProductPerformanceReportDto` |
| `GET /api/seller/reports/operations` | `OperationsReportDto` |
| `GET /api/seller/reports/customers` | `CustomerReportDto` |
| `GET /api/seller/reports/marketing` | `MarketingReportDto` |
| `GET /api/seller/reports/reviews` | `ReviewsReportDto` |

Query chung: `?range=30d&granularity=day&from=&to=`.

### 3.4. Frontend — có **2 giao diện song song** dùng chung service/DTO

**A. Razor Pages (`GearZone.Web/Pages/StoreOwner/`) — giao diện seller chính (server-rendered)**
- `Reports/Index.cshtml` + `Index.cshtml.cs` — PageModel `@inject ISellerReportService`, tab điều hướng bằng query string (`?Tab=&Range=`), CSS bar chart, export CSV qua handler `OnGetExport`.
- `Shared/_StoreOwnerLayout.cshtml` — thêm link "Reports" vào sidebar.

**B. React SPA (`gearzone-react/`) — gọi API `ReportsController`**
- `src/api/seller.ts` — nhóm `reports` (6 endpoint + `exportSalesCsv` tải blob).
- `src/pages/seller/ReportsPage.tsx` — UI tab + chọn khoảng thời gian + export CSV.
- `src/App.tsx` — route `/seller/reports`; `Navbar.tsx` + `DashboardPage.tsx` — link "Reports".

> Lớp Application (`SellerReportService`) là nguồn logic duy nhất; Razor PageModel gọi trực tiếp, React gọi qua `ReportsController`.

---

## 4. Trạng thái triển khai

- [x] DTOs (`SellerReportDtos.cs`)
- [x] `ISellerReportService` + `SellerReportService` (6 mảng báo cáo + so sánh kỳ + CSV)
- [x] Đăng ký DI
- [x] `ReportsController` (API cho React)
- [x] Razor Page `StoreOwner/Reports` + sidebar nav
- [x] React `ReportsPage.tsx` + api + route + nav
- [ ] Build & kiểm thử end-to-end

---

## 5. Quyết định thiết kế đáng lưu ý
- **Doanh thu tính trên `SubOrder.Subtotal`**, net trên `SubOrder.NetAmount` — nhất quán với `DashboardController` hiện tại.
- **In-memory aggregation** thay vì `GroupBy` phía SQL cho các phần có `Distinct().Count()` / trung bình `TimeSpan`, tránh lỗi EF Core translation; chấp nhận được ở quy mô một cửa hàng.
- **Ngưỡng tồn kho thấp = 5**, top sản phẩm giới hạn 20 dòng, low-stock 50 dòng (đủ cho báo cáo, tránh payload lớn).
- **Không thêm bảng/migration** — toàn bộ dựa trên dữ liệu giao dịch sẵn có.
