namespace GearZone.Domain.Enums
{
    public enum AttributeScope
    {
        Product = 1,  // Technical specification (e.g. CUDA Cores, TDP)
        Variant = 2,  // Selectable option (e.g. Color, RAM Capacity)
        Both = 3      // Can be either (e.g. GPU Series)
    }
}
