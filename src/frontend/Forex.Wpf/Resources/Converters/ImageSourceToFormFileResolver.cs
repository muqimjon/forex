namespace Forex.Wpf.Resources.Converters;

using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using System.IO;
using System.Windows.Media.Imaging;

public class ImageSourceToFormFileResolver<TSource, TDestination> : IValueResolver<TSource, TDestination, IFormFile>
{
    public IFormFile Resolve(TSource source, TDestination destination, IFormFile destMember, ResolutionContext context)
    {
        var propInfo = typeof(TSource).GetProperty("Photo") ?? typeof(TSource).GetProperty("Image");
        if (propInfo == null)
            return null!;

        if (propInfo.GetValue(source) is not BitmapSource imageValue)
            return null!;

        // ❌ oldingi joyda `using var` bor edi — endi yo‘q
        var memoryStream = new MemoryStream();
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(imageValue));
        encoder.Save(memoryStream);
        memoryStream.Position = 0;

        var fileName = $"{typeof(TSource).Name}_{Guid.NewGuid()}.png";

        // ⚡️ stream endi “ochiq” qoladi — HTTP yuborilgunga qadar ishlaydi
        return new FormFile(memoryStream, 0, memoryStream.Length, "Photo", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };
    }
}
