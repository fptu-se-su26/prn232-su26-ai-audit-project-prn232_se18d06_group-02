namespace GearZone.Domain.Enums
{
    public enum PayoutStatus
    {
        Unpaid,     // Store hasn't been paid for this order yet
        Pending,    // Included in a PayoutBatch, waiting for processing
        Processing, // Payment to store is being processed by the bank
        Paid,       // Store has been successfully paid
        Failed,      // Payment to store failed
        Locked
    }
}
