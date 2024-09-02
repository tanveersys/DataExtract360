using System.ComponentModel.DataAnnotations;

namespace DataExtract360.Models
{
    public class PurchaseOrder
    {
        public string PONumber { get; set; }
        public string OrderType { get; set; }
        public DateTime CreationDate { get; set; }
        public string ShippingInformation { get; set; }
        public string BillingInformation { get; set; }
        public List<OrderItem> Items { get; set; }
    }
}
