using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Application.Features.Payout.Dtos
{
    public class PayoutResult
    {
        public bool IsSuccess { get; set; }
        public string? ReferenceId { get; set; }
        public string? ErrorMessage { get; set; }

        public PayoutResult(bool isSuccess, string? referenceId = null, string? errorMessage = null)
        {
            IsSuccess = isSuccess;
            ReferenceId = referenceId;
            ErrorMessage = errorMessage;
        }
    }
}
