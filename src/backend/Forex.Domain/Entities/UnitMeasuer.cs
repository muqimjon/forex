namespace Forex.Domain.Entities;

using Forex.Domain.Commons;

public class UnitMeasure : Auditable
{
        public string Name { get; set; } = string.Empty;
        public string NormalizedName { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
}