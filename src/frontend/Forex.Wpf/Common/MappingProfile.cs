namespace Forex.Wpf.Common;

using Forex.ClientService.Models.Requests;
using Forex.ClientService.Models.Responses;
using Forex.Wpf.Pages.Processes.ViewModels;
using Forex.Wpf.ViewModels;
using Mapster;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

public static class MappingProfile
{
    public static void Register(TypeAdapterConfig config)
    {
        // 🔹 Product
        config.NewConfig<ProductResponse, ProductViewModel>();
        config.NewConfig<ProductViewModel, ProductRequest>()
            .Map(dest => dest.UnitMeasureId, src => src.UnitMeasure.Id)
            .Map(dest => dest.ImageBytes, src => ImageToBytes(src.Image));

        // 🔹 ProductTypes
        config.NewConfig<ProductTypeResponse, ProductTypeViewModel>();
        config.NewConfig<ProductTypeViewModel, ProductTypeRequest>();

        // 🔹 ProductTypeItem
        config.NewConfig<ProductTypeItemViewModel, ProductTypeItemRequest>();

        // 🔹 SemiProduct
        config.NewConfig<SemiProductResponse, SemiProductViewModel>();
        config.NewConfig<SemiProductViewModel, SemiProductRequest>()
            .Map(dest => dest.UnitMeasureId, src => src.UnitMeasure.Id)
            .Map(dest => dest.ImageBytes, src => ImageToBytes(src.Image));

        // 🔹 Processes
        config.NewConfig<InProcessResponse, InProcessViewModel>();
        config.NewConfig<InProcessViewModel, InProcessRequest>();

        // 🔹 EntryToProcess
        config.NewConfig<EntryToProcessResponse, EntryToProcessViewModel>();
        config.NewConfig<EntryToProcessViewModel, EntryToProcessRequest>();
        config.NewConfig<EntryToProcessByProductViewModel, EntryToProcessRequest>()
            .Map(dest => dest.ProductTypeId, src => src.Product.SelectedType!.Id);

        // 🔹 UnitMeasures
        config.NewConfig<UnitMeasureResponse, UnitMeasuerViewModel>();
        config.NewConfig<UnitMeasuerViewModel, UnitMeasureRequest>();

        // 🔹 User
        config.NewConfig<UserResponse, UserViewModel>();
        config.NewConfig<UserViewModel, UserRequest>();

        // 🔹 Currencies
        config.NewConfig<CurrencyResponse, CurrencyViewModel>();
        config.NewConfig<CurrencyViewModel, CurrencyRequest>();

        // 🔹 Manufactory
        config.NewConfig<ManufactoryResponse, ManufactoryViewModel>();

        // 🔹 Invoice
        config.NewConfig<InvoiceResponse, InvoiceViewModel>();
        config.NewConfig<InvoiceViewModel, InvoiceRequest>()
            .Map(dest => dest.ManufactoryId, src => src.Manufactory.Id);

        // Invoice payment
        config.NewConfig<InvoicePaymentResponse, InvoicePaymentViewModel>();
        config.NewConfig<InvoicePaymentViewModel, InvoicePaymentRequest>()
            .Map(dest => dest.UserId, src => src.User.Id)
            .Map(dest => dest.CurrencyId, src => src.Currency.Id);

        config.NewConfig<CurrencyResponse, CurrencyViewModel>();
        config.NewConfig<UserAccountResponse, UserAccountViewModel>();

        // 🔹 Transaction
        config.NewConfig<TransactionResponse, TransactionViewModel>()
            .Map(dest => dest.Date, src => src.Date.ToLocalTime())
      .AfterMapping((src, dest) =>
      {
          if (src.IsIncome)
          {
              dest.Income = src.Amount;
              dest.Expense = 0;
          }
          else
          {
              dest.Expense = -src.Amount;
              dest.Income = 0;
          }
      });
        config.NewConfig<TransactionViewModel, TransactionRequest>()
            .Map(dest => dest.CurrencyId, src => src.Currency.Id)
            .Map(dest => dest.UserId, src => src.User.Id);
    }

    // 🔹 ImageSource → byte[] maplash (Minioga upload uchun)
    private static byte[]? ImageToBytes(ImageSource? img)
    {
        if (img is not BitmapSource bmp) return null;

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bmp));

        using var ms = new MemoryStream();
        encoder.Save(ms);
        return ms.ToArray();
    }
}
