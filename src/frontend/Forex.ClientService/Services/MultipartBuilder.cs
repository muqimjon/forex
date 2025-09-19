namespace Forex.ClientService.Services;

using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Invoices;

public static class MultipartBuilder
{
    public static MultipartFormDataContent BuildIntake(InvoiceRequest command)
    {
        var content = new MultipartFormDataContent();

        content.AddFormField(nameof(command.SenderId), command.SenderId);
        content.AddFormField(nameof(command.ManufactoryId), command.ManufactoryId);
        content.AddFormField(nameof(command.EntryDate), command.EntryDate.ToString("o"));
        content.AddFormField(nameof(command.TransferFeePerContainer), command.TransferFeePerContainer);

        content.AddIndexedFields(command.Containers, (c, item, i) =>
        {
            c.AddFormField($"Containers[{i}].Count", item.Count);
            c.AddFormField($"Containers[{i}].Price", item.Price);
        });

        content.AddIndexedFields(command.Items, (c, item, i) =>
        {
            c.AddFormField($"Items[{i}].SemiProductId", item.SemiProductId);
            c.AddFormField($"Items[{i}].Name", item.Name);
            c.AddFormField($"Items[{i}].Code", item.Code);
            c.AddFormField($"Items[{i}].Measure", item.Measure);
            c.AddFormField($"Items[{i}].Quantity", item.Quantity);
            c.AddFormField($"Items[{i}].CostPrice", item.CostPrice);
            c.AddFormField($"Items[{i}].CostDelivery", item.CostDelivery);
            c.AddFormField($"Items[{i}].TransferFee", item.TransferFee);

            if (!string.IsNullOrWhiteSpace(item.PhotoPath))
            {
                c.AddFileField($"Items[{i}].Photo", item.PhotoPath, item.PhotoContentType, item.PhotoFileName);
            }
        });

        return content;
    }
}
