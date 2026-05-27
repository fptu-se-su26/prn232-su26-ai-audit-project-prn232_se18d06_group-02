using GearZone.Application.Abstractions.External;
using GearZone.Domain.Enums;

namespace GearZone.Application.Features.Payment
{
    public class PaymentStrategyFactory
    {
        private readonly IEnumerable<IPaymentStrategy> _strategies;

        public PaymentStrategyFactory(IEnumerable<IPaymentStrategy> strategies)
        {
            _strategies = strategies;
        }

        public IPaymentStrategy GetStrategy(PaymentMethod method)
        {
            var strategy = _strategies.FirstOrDefault(s => s.Method == method);

            if (strategy == null)
                throw new NotSupportedException($"Payment method {method} not supported");

            return strategy;
        }
    }
}
