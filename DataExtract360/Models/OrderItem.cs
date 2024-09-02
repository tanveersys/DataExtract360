using System.ComponentModel.DataAnnotations;

namespace DataExtract360.Models
{
    public class OrderItem
    {
        public string StyleNumber { get; set; }
        public string ItemName { get; set; }
        public string ColorDetails { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public byte[] ImageData { get; set; }
        public int PurchaseOrderId { get; set; }
        public PurchaseOrder PurchaseOrder { get; set; }
    }
}
