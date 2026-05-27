using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.External
{

    public interface IPaymentGateway
    {
        Task<PaymentGatewayResult> GetPaymentStatusAsync(long orderCode);
    }

    public class PaymentGatewayResult
    {
        public bool IsPaid { get; set; }
        public string? Status { get; set; }
        public string? TransactionId { get; set; }
        public string? ErrorMessage { get; set; }

        public static PaymentGatewayResult Paid(string? transactionId = null) =>
            new() { IsPaid = true, Status = "PAID", TransactionId = transactionId };

        public static PaymentGatewayResult NotPaid(string status) =>
            new() { IsPaid = false, Status = status };

        public static PaymentGatewayResult Error(string error) =>
            new() { IsPaid = false, ErrorMessage = error };
    }
}
