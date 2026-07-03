namespace Forex.Application.Features.Products.ProductTypes.Commands;

using Forex.Application.Common.Extensions;
using Forex.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GenerateMissingBarcodesCommand : IRequest<int>;

public class GenerateMissingBarcodesCommandHandler(IAppDbContext context)
    : IRequestHandler<GenerateMissingBarcodesCommand, int>
{
    public async Task<int> Handle(GenerateMissingBarcodesCommand request, CancellationToken ct)
    {
        var types = await context.ProductTypes
            .Where(t => t.QopBarcode == null || t.PackBarcode == null)
            .ToListAsync(ct);

        foreach (var type in types)
            BarcodeGenerator.EnsureBarcodes(type);

        await context.SaveAsync(ct);
        return types.Count;
    }
}
