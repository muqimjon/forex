namespace Forex.ClientService.Models.SemiProducts;

public class SemiProductIntakeCommand
{
    public long SenderId { get; set; }
    public long ManufactoryId { get; set; }
    public DateTime EntryDate { get; set; }
    public decimal TransferFeePerContainer { get; set; }

    public List<ContainerCommand> Containers { get; set; } = new();
    public List<SemiProductItemCommand> Items { get; set; } = new();
}
