using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Application.Abstractions.External
{
    public interface IEmailService
    {
        Task SendStoreStatusEmailAsync(Store store, StoreStatus status, string? reason);
        Task SendAsync(string to, string subject, string htmlBody);
    }
}
