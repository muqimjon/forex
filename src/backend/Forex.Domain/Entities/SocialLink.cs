namespace Forex.Domain.Entities;

using Forex.Domain.Commons;

public class SocialLink : Auditable
{
    public string PlatformName { get; set; } = default!;
    public string Url { get; set; } = default!;
    public string Icon { get; set; } = string.Empty;
    public int Position { get; set; }
    public bool IsActive { get; set; }

    public long CompanyInfoId { get; set; }
    public CompanyInfo CompanyInfo { get; set; } = default!;
}