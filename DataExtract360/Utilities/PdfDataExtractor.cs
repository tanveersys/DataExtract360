using DataExtract360.Models;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf;
using System.Text.RegularExpressions;
using System.Text;

namespace DataExtract360.Utilities
{
    public class PdfDataExtractor
    {
        public PurchaseOrder ExtractData(string filePath)
        {
            var purchaseOrder = new PurchaseOrder();
            using (var pdfReader = new PdfReader(filePath))
            using (var pdfDocument = new PdfDocument(pdfReader))
            {
                var fullText = new StringBuilder();
                var images = new List<byte[]>();

                for (int page = 1; page <= pdfDocument.GetNumberOfPages(); page++)
                {
                    var pageText = PdfTextExtractor.GetTextFromPage(pdfDocument.GetPage(page), new SimpleTextExtractionStrategy());
                    fullText.AppendLine(pageText);

                    var imageExtractionListener = new ImageExtractionListener();
                    var processor = new PdfCanvasProcessor(imageExtractionListener);
                    processor.ProcessPageContent(pdfDocument.GetPage(page));

                    images.AddRange(imageExtractionListener.Images);
                }

                purchaseOrder = ParseText(fullText.ToString());
                if (purchaseOrder.Items != null && purchaseOrder.Items.Count > 0 && images.Count > 0)
                {
                    purchaseOrder.Items[0].ImageData = images[0];
                }
            }
            return purchaseOrder;
        }

        private PurchaseOrder ParseText(string text)
        {
            var purchaseOrder = new PurchaseOrder();

            var poMatch = Regex.Match(text, @"PO#:\s*(\d+)");
            if (poMatch.Success) purchaseOrder.PONumber = poMatch.Groups[1].Value;

            var orderTypeMatch = Regex.Match(text, @"Order\s*Type:\s*(\w+)");
            if (orderTypeMatch.Success) purchaseOrder.OrderType = orderTypeMatch.Groups[1].Value;

            var creationDateMatch = Regex.Match(text, @"Created:\s*([\d/]+)");
            if (creationDateMatch.Success) purchaseOrder.CreationDate = DateTime.Parse(creationDateMatch.Groups[1].Value);

            var shippingInfoMatch = Regex.Match(text, @"Shipping\s*Information\s*([\s\S]*?)\s*Billing\s*Information");
            if (shippingInfoMatch.Success) purchaseOrder.ShippingInformation = shippingInfoMatch.Groups[1].Value.Trim();

            var billingInfoMatch = Regex.Match(text, @"Billing\s*Information\s*([\s\S]*?)\s*Wholesale:");
            if (billingInfoMatch.Success) purchaseOrder.BillingInformation = billingInfoMatch.Groups[1].Value.Trim();

            purchaseOrder.Items = ExtractItems(text);

            return purchaseOrder;
        }

        private List<OrderItem> ExtractItems(string text)
        {
            var items = new List<OrderItem>();

            var itemPattern = @"Style\s*#(\S+)\s*\|\s*([\s\S]*?)\nColors\s*([\s\S]*?)\n\n";
            var itemMatches = Regex.Matches(text, itemPattern);

            foreach (Match match in itemMatches)
            {
                var item = new OrderItem
                {
                    StyleNumber = match.Groups[1].Value,
                    ItemName = match.Groups[2].Value.Trim(),
                    ColorDetails = match.Groups[3].Value.Trim()
                };

                var quantityMatch = Regex.Match(match.Groups[3].Value, @"Qty\s+(\d+)");
                if (quantityMatch.Success) item.Quantity = int.Parse(quantityMatch.Groups[1].Value);

                var priceMatch = Regex.Match(match.Groups[3].Value, @"AUD\s+([\d\.]+)");
                if (priceMatch.Success) item.Price = decimal.Parse(priceMatch.Groups[1].Value);

                items.Add(item);
            }

            return items;
        }

        private class ImageExtractionListener : IEventListener
        {
            public List<byte[]> Images { get; } = new List<byte[]>();

            public void EventOccurred(IEventData data, EventType type)
            {
                if (type == EventType.RENDER_IMAGE)
                {
                    var renderInfo = (ImageRenderInfo)data;
                    var img = renderInfo.GetImage();
                    Images.Add(img.GetImageBytes(true));
                }
            }

            public ICollection<EventType> GetSupportedEvents()
            {
                return new HashSet<EventType> { EventType.RENDER_IMAGE };
            }
        }
    }
}
