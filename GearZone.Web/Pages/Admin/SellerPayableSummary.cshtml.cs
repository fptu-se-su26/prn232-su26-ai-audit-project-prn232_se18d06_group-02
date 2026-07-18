using Microsoft.AspNetCore.Authorization;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using GearZone.Domain.Enums;

namespace GearZone.Web.Pages.Admin
{
    [Authorize(Roles = "Super Admin")]
    public class SellerPayableSummaryModel : PageModel
    {
        private readonly IApiClient _api;

        public SellerPayableSummaryModel(IApiClient api)
        {
            _api = api;
        }

        public List<AdminSellerPayableSummaryDto> Payables { get; set; } = new();
        public decimal CurrentWalletBalance { get; private set; }
        public bool IsWalletBalanceAvailable { get; private set; }

        [BindProperty(SupportsGet = true)]
        public string RangeType { get; set; } = "this-week";

        [BindProperty(SupportsGet = true)]
        public DateTime? CustomStart { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? CustomEnd { get; set; }

        [BindProperty]
        public List<Guid> SelectedStoreIds { get; set; } = new();

        public DateTime CurrentStart { get; private set; }
        public DateTime CurrentEnd { get; private set; }

        public async Task OnGetAsync(CancellationToken ct)
        {
            CalculateDates();
            await LoadSummaryAndWalletAsync(ct);
        }

        public async Task<IActionResult> OnPostProcessBulkAsync(CancellationToken ct)
        {
            CalculateDates();

            var storeIds = SelectedStoreIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (!storeIds.Any())
            {
                TempData["ErrorMessage"] = "Please select at least one seller.";
                return RedirectToCurrentRange();
            }

            return await ProcessSelectedStoresAsync(storeIds, null, ct);
        }

        public async Task<IActionResult> OnPostProcessSingleAsync(Guid storeId, CancellationToken ct)
        {
            CalculateDates();

            if (storeId == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Invalid seller.";
                return RedirectToCurrentRange();
            }

            return await ProcessSelectedStoresAsync([storeId], storeId, ct);
        }

        private async Task<IActionResult> ProcessSelectedStoresAsync(
            IReadOnlyCollection<Guid> storeIds,
            Guid? singleStoreId,
            CancellationToken ct)
        {
            try
            {
                var summaryTask = LoadSellerSummaryAsync(ct);
                var walletTask = GetCurrentWalletBalanceAsync(ct);
                await Task.WhenAll(summaryTask, walletTask);

                var summary = await summaryTask;
                var requiredAmount = summary.Summary
                    .Where(x => storeIds.Contains(x.StoreId))
                    .Sum(x => x.TotalNetAmount);

                var walletBalance = await walletTask;
                if (walletBalance < requiredAmount)
                {
                    TempData["ErrorMessage"] = $"Current wallet balance ({walletBalance:N0} VND) is insufficient for payout ({requiredAmount:N0} VND). Please top up before processing.";
                    return RedirectToPage("/Admin/Wallet/Index");
                }

                ApiResult<PayoutBatchCreatedDto> generated;
                var rangeRequest = new
                {
                    rangeType = RangeType,
                    customStart = RangeType == "custom" ? CustomStart : null,
                    customEnd = RangeType == "custom" ? CustomEnd : null
                };

                if (singleStoreId.HasValue)
                {
                    generated = await _api.PostAndReadAsync<object, PayoutBatchCreatedDto>(
                        $"/api/admin/payouts/process-single/{singleStoreId.Value}", rangeRequest, ct);
                }
                else
                {
                    generated = await _api.PostAndReadAsync<object, PayoutBatchCreatedDto>(
                        "/api/admin/payouts/process-bulk",
                        new
                        {
                            storeIds,
                            rangeType = RangeType,
                            customStart = RangeType == "custom" ? CustomStart : null,
                            customEnd = RangeType == "custom" ? CustomEnd : null
                        }, ct);
                }

                if (!generated.Success || string.IsNullOrWhiteSpace(generated.Data?.BatchCode))
                    throw new InvalidOperationException(generated.FirstError ?? "Failed to generate payout batch.");

                var batchCode = generated.Data.BatchCode;
                var processed = await _api.PostAndReadAsync<object, AdminPayoutBatchDto>(
                    "/api/admin/payouts/process-generated", new { batchCode }, ct);

                if (!processed.Success)
                    throw new InvalidOperationException(processed.FirstError ?? "Failed to process payout batch.");

                var processedBatch = processed.Data;
                if (processedBatch == null)
                {
                    TempData["InfoMessage"] = $"Batch '{batchCode}' has been processed. Please check the payout batch list for status details.";
                }
                else if (processedBatch.Status == PayoutBatchStatus.Completed)
                {
                    TempData["SuccessMessage"] = $"Payout completed successfully. Batch '{batchCode}' finished ({processedBatch.SuccessCount} transactions).";
                }
                else
                {
                    TempData["ErrorMessage"] = $"Payout partially/fully failed. Batch '{batchCode}' has {processedBatch.FailedCount} failed transactions.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to process payout: {ex.Message}";
            }

            return RedirectToCurrentRange();
        }

        private RedirectToPageResult RedirectToCurrentRange()
        {
            return RedirectToPage(new
            {
                RangeType,
                CustomStart = RangeType == "custom" ? CustomStart : null,
                CustomEnd = RangeType == "custom" ? CustomEnd : null
            });
        }

        private void CalculateDates()
        {
            var now = DateTime.UtcNow;

            switch (RangeType?.Trim().ToLowerInvariant())
            {
                case "last-week":
                {
                    var currentWeekStart = StartOfWeek(now.Date, DayOfWeek.Monday);
                    CurrentStart = currentWeekStart.AddDays(-7);
                    CurrentEnd = currentWeekStart.AddTicks(-1);
                    break;
                }
                case "custom":
                {
                    CurrentStart = (CustomStart ?? now.AddDays(-7)).Date;
                    CurrentEnd = (CustomEnd ?? now).Date.AddDays(1).AddTicks(-1);
                    break;
                }
                case "this-week":
                default:
                {
                    CurrentStart = StartOfWeek(now.Date, DayOfWeek.Monday);
                    CurrentEnd = now;
                    break;
                }
            }
        }

        private static DateTime StartOfWeek(DateTime date, DayOfWeek startOfWeek)
        {
            var diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
            return date.AddDays(-diff).Date;
        }

        private async Task LoadSummaryAndWalletAsync(CancellationToken ct)
        {
            var summaryTask = LoadSellerSummaryAsync(ct);
            var walletTask = GetCurrentWalletBalanceAsync(ct);
            try
            {
                await Task.WhenAll(summaryTask, walletTask);
                var summary = await summaryTask;
                Payables = summary.Summary;
                CurrentStart = summary.PeriodStart;
                CurrentEnd = summary.PeriodEnd;
                CurrentWalletBalance = await walletTask;
                IsWalletBalanceAvailable = true;
            }
            catch
            {
                if (summaryTask.IsCompletedSuccessfully)
                {
                    var summary = await summaryTask;
                    Payables = summary.Summary;
                    CurrentStart = summary.PeriodStart;
                    CurrentEnd = summary.PeriodEnd;
                }
                CurrentWalletBalance = 0m;
                IsWalletBalanceAvailable = false;
            }
        }

        private async Task<AdminSellerPayableResponseDto> LoadSellerSummaryAsync(CancellationToken ct)
        {
            var query = new
            {
                rangeType = RangeType,
                customStart = RangeType == "custom" ? CustomStart : null,
                customEnd = RangeType == "custom" ? CustomEnd : null
            };
            return await _api.GetAsync<AdminSellerPayableResponseDto>(
                $"/api/admin/payouts/seller-summary{ApiQueryStringBuilder.Build(query)}", ct)
                ?? new AdminSellerPayableResponseDto { PeriodStart = CurrentStart, PeriodEnd = CurrentEnd };
        }

        private async Task<decimal> GetCurrentWalletBalanceAsync(CancellationToken ct)
        {
            var query = new PlatformTransactionQuery
            {
                PageNumber = 1,
                PageSize = 1
            };
            var summary = await _api.GetAsync<AdminPlatformTransactionSummaryDto>(
                $"/api/admin/transactions/summary{ApiQueryStringBuilder.Build(query)}", ct);
            if (summary is null) throw new InvalidOperationException("Wallet balance is unavailable.");
            return summary.WalletBalance;
        }
    }
}
