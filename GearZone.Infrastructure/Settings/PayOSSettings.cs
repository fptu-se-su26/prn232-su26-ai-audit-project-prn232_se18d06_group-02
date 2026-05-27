using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Infrastructure.Settings
{
    public class PayOSSettings
    {
        public string ClientId { get; set; } = default!;
        public string ApiKey { get; set; } = default!;
        public string ChecksumKey { get; set; } = default!;

        public string ReturnUrl { get; set; } = default!;
        public string CancelUrl { get; set; } = default!;
    }
}
