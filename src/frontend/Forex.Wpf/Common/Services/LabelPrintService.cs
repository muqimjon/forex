namespace Forex.Wpf.Common.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

public sealed record LabelItem(string Title, string Size, string UnitLabel, int Pairs, string? Barcode);

public static class LabelPrintService
{
    private const double MmToDip = 96.0 / 25.4;
    private const int MaxCopies = 500;

    // Physical thermal label size (Xprinter XP-365B, 203 DPI). Roll: 58×40 mm.
    private const double DefaultWidthMm = 58;
    private const double DefaultHeightMm = 40;

    // Fine-tune horizontal placement on the label (positive = shift content right, mm).
    private const double HorizontalOffsetMm = 5.5;

    // The printer feeds labels head-first, so the artwork must be rotated 180° to read upright.
    private const double RotationDegrees = 180;

    public static bool Print(IReadOnlyCollection<LabelItem> labels, string? printerName, int copies, double widthMm = DefaultWidthMm, double heightMm = DefaultHeightMm)
    {
        var valid = labels.Where(l => !string.IsNullOrWhiteSpace(l.Barcode)).ToList();
        if (valid.Count == 0)
        {
            MessageBox.Show("Chop etish uchun barkod yo'q.", "Yorliq", MessageBoxButton.OK, MessageBoxImage.Information);
            return false;
        }

        if (string.IsNullOrWhiteSpace(printerName))
        {
            MessageBox.Show("Avval Sozlamalar → Printer bo'limida yorliq printerini tanlang.", "Yorliq", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        PrintQueue? queue;
        try
        {
            queue = new LocalPrintServer()
                .GetPrintQueues(new[] { EnumeratedPrintQueueTypes.Local, EnumeratedPrintQueueTypes.Connections })
                .FirstOrDefault(q => q.FullName == printerName);
        }
        catch
        {
            MessageBox.Show("Printerlar ro'yxatini olib bo'lmadi. Windows Print Spooler xizmati ishlayotganini tekshiring.", "Yorliq", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (queue is null)
        {
            MessageBox.Show("Tanlangan printer topilmadi. Sozlamalardan qayta tanlang.", "Yorliq", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        try
        {
            var ticket = BuildTicket(queue, widthMm, heightMm);
            new PrintDialog { PrintQueue = queue, PrintTicket = ticket }
                .PrintDocument(BuildDocument(valid, Math.Clamp(copies, 1, MaxCopies), widthMm, heightMm).DocumentPaginator, "Yorliqlar");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Chop etishda xatolik: {ex.Message}", "Yorliq", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        return true;
    }

    // Tells the driver the exact label media size (in DIPs) so it feeds one 58×40 mm
    // label per page from the left origin — otherwise the driver falls back to its wide
    // default media, shifting content sideways and letting barcodes drift across the gap.
    private static PrintTicket BuildTicket(PrintQueue queue, double widthMm, double heightMm)
    {
        var ticket = queue.DefaultPrintTicket ?? queue.UserPrintTicket ?? new PrintTicket();

        ticket.PageMediaSize = new PageMediaSize(widthMm * MmToDip, heightMm * MmToDip);
        ticket.PageOrientation = PageOrientation.Portrait;
        ticket.OutputColor = OutputColor.Monochrome;
        ticket.PageResolution = new PageResolution(203, 203);
        ticket.PageBorderless = PageBorderless.Borderless;
        ticket.CopyCount = 1;

        try
        {
            var result = queue.MergeAndValidatePrintTicket(queue.DefaultPrintTicket, ticket);
            if (result.ValidatedPrintTicket is not null)
            {
                result.ValidatedPrintTicket.PageMediaSize = new PageMediaSize(widthMm * MmToDip, heightMm * MmToDip);
                return result.ValidatedPrintTicket;
            }
        }
        catch
        {
            // Driver rejected validation — fall back to the raw ticket below.
        }

        return ticket;
    }

    public static void ShowPreview(IReadOnlyCollection<LabelItem> labels, string? printerName, double widthMm = DefaultWidthMm, double heightMm = DefaultHeightMm)
    {
        var valid = labels.Where(l => !string.IsNullOrWhiteSpace(l.Barcode)).ToList();
        if (valid.Count == 0)
        {
            MessageBox.Show("Chop etish uchun barkod yo'q.", "Yorliq", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var copiesBox = new TextBox
        {
            Width = 56,
            Text = "1",
            TextAlignment = TextAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center
        };

        var printButton = new Button
        {
            Content = "Chop etish",
            Padding = new Thickness(18, 6, 18, 6),
            Margin = new Thickness(16, 0, 0, 0),
            Background = new SolidColorBrush(Color.FromRgb(0x16, 0xA3, 0x4A)),
            Foreground = Brushes.White,
            FontWeight = FontWeights.SemiBold,
            BorderThickness = new Thickness(0),
            Cursor = System.Windows.Input.Cursors.Hand
        };

        var bar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 8, 10, 8)
        };
        bar.Children.Add(new TextBlock { Text = string.IsNullOrWhiteSpace(printerName) ? "Printer: (sozlanmagan)" : $"Printer: {printerName}", VerticalAlignment = VerticalAlignment.Center });
        bar.Children.Add(new TextBlock { Text = "Nusxa:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(16, 0, 6, 0) });
        bar.Children.Add(copiesBox);
        bar.Children.Add(printButton);

        var viewer = new DocumentViewer { Document = BuildDocument(valid, 1, widthMm, heightMm), Margin = new Thickness(8) };

        var layout = new DockPanel();
        DockPanel.SetDock(bar, Dock.Top);
        layout.Children.Add(bar);
        layout.Children.Add(viewer);

        var window = new Window
        {
            Title = "Yorliq — oldindan ko'rish",
            Width = 720,
            Height = 560,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            Content = layout,
            Owner = Application.Current.MainWindow
        };

        printButton.Click += (_, _) =>
        {
            var copies = int.TryParse(copiesBox.Text, out var c) && c > 0 ? c : 1;
            if (Print(valid, printerName, copies, widthMm, heightMm))
                window.Close();
        };

        window.ShowDialog();
    }

    private static FixedDocument BuildDocument(IReadOnlyCollection<LabelItem> labels, int copies, double widthMm, double heightMm)
    {
        var pageWidth = widthMm * MmToDip;
        var pageHeight = heightMm * MmToDip;

        var doc = new FixedDocument();
        doc.DocumentPaginator.PageSize = new Size(pageWidth, pageHeight);

        foreach (var label in labels)
        {
            // Render at high resolution so the bars stay crisp when scaled to the 203 DPI head.
            var barcode = BarcodeImageService.Render(label.Barcode!, 600, 200);
            for (var i = 0; i < copies; i++)
            {
                var page = new FixedPage { Width = pageWidth, Height = pageHeight, Background = Brushes.White };
                page.Children.Add(BuildLabel(label, barcode, pageWidth, pageHeight));

                var content = new PageContent();
                ((IAddChild)content).AddChild(page);
                doc.Pages.Add(content);
            }
        }

        return doc;
    }

    private static UIElement BuildLabel(LabelItem label, ImageSource barcode, double pageWidth, double pageHeight)
    {
        var ink = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00));

        // ~1.6 mm safe margin on every side keeps content off the label edges/gap.
        var margin = 1.6 * MmToDip;
        var offset = HorizontalOffsetMm * MmToDip;

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // title
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // size + unit
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // barcode fills the rest
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // human-readable code

        var title = new TextBlock
        {
            Text = label.Title,
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            Foreground = ink,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            MaxHeight = 30,
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        Grid.SetRow(title, 0);
        grid.Children.Add(title);

        var info = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 1, 0, 2)
        };
        info.Children.Add(new TextBlock
        {
            Text = label.Size,
            FontSize = 17,
            FontWeight = FontWeights.Bold,
            Foreground = ink,
            VerticalAlignment = VerticalAlignment.Center
        });
        info.Children.Add(new TextBlock
        {
            Text = $"   {label.UnitLabel} · {label.Pairs} juft",
            FontSize = 11,
            FontWeight = FontWeights.SemiBold,
            Foreground = ink,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 0, 2)
        });
        Grid.SetRow(info, 1);
        grid.Children.Add(info);

        var barcodeImage = new Image
        {
            Source = barcode,
            Stretch = Stretch.Fill,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = new Thickness(0, 1, 0, 1)
        };
        // Keep bars sharp on the thermal head rather than anti-aliasing them into grey.
        RenderOptions.SetBitmapScalingMode(barcodeImage, BitmapScalingMode.NearestNeighbor);
        RenderOptions.SetEdgeMode(barcodeImage, EdgeMode.Aliased);
        Grid.SetRow(barcodeImage, 2);
        grid.Children.Add(barcodeImage);

        var codeText = new TextBlock
        {
            Text = label.Barcode,
            FontSize = 10,
            FontWeight = FontWeights.SemiBold,
            FontFamily = new FontFamily("Consolas"),
            Foreground = ink,
            TextAlignment = TextAlignment.Center
        };
        Grid.SetRow(codeText, 3);
        grid.Children.Add(codeText);

        var border = new Border
        {
            Width = pageWidth - offset,
            Height = pageHeight,
            Background = Brushes.White,
            Padding = new Thickness(margin),
            Child = grid,
            // Flip in place around the label centre so orientation is fixed without moving the block.
            RenderTransformOrigin = new Point(0.5, 0.5),
            RenderTransform = new RotateTransform(RotationDegrees)
        };

        // Push the whole label block sideways; narrowing the width by the same amount
        // keeps the right edge inside the page so nothing gets clipped.
        FixedPage.SetLeft(border, offset);
        FixedPage.SetTop(border, 0);
        return border;
    }
}
