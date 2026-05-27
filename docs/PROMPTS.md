# AI Prompts Log -- Feature -- Seller Payout System

Branch: `feature/payout-system`
Scope: Seller revenue disbursement: payout batch creation, payout items per sub-order, PayOS transfer, admin approval workflow

This file records the actual prompts submitted to AI tools during development of this branch,
along with a summary of the output and which parts were incorporated.

---

## Prompt 1 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** Seller revenue disbursement: payout batch creation, payout items per sub-order, PayOS transfer, admin approval workflow

**Prompt:**
> Design a seller payout system for a multi-vendor marketplace: how do I aggregate completed orders into weekly payout batches?

**AI Output Summary:**
PayoutBatch created weekly via Hangfire; PayoutItems = all SubOrders with Status=Completed and no existing PayoutItem; admin reviews then approves.

**Used in files:** Features/Payout/*, Controllers/Api/Admin/PayoutsController.cs, Pages/Admin/Payouts/*, Pages/Admin/PayoutBatches/*, Infrastructure/Jobs/PayoutBatchJob.cs

---
## Prompt 2 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** Seller revenue disbursement: payout batch creation, payout items per sub-order, PayOS transfer, admin approval workflow

**Prompt:**
> How does PayOS payout (disbursement) API work -- what fields are required for a bank transfer?

**AI Output Summary:**
PayOS payout requires: accountNo, accountName, bankBin, amount, description, reference; returns transferId for tracking.

**Used in files:** Features/Payout/*, Controllers/Api/Admin/PayoutsController.cs, Pages/Admin/Payouts/*, Pages/Admin/PayoutBatches/*, Infrastructure/Jobs/PayoutBatchJob.cs

---
## Prompt 3 -- 2026-05-27

**Tool:** Claude Code / GitHub Copilot
**Context:** Seller revenue disbursement: payout batch creation, payout items per sub-order, PayOS transfer, admin approval workflow

**Prompt:**
> How should I calculate the platform fee before paying out to sellers?

**AI Output Summary:**
NetAmount = GrossAmount * (1 - PlatformFeeRate); PlatformFeeRate loaded from SystemSetting for configurability.

**Used in files:** Features/Payout/*, Controllers/Api/Admin/PayoutsController.cs, Pages/Admin/Payouts/*, Pages/Admin/PayoutBatches/*, Infrastructure/Jobs/PayoutBatchJob.cs

---
