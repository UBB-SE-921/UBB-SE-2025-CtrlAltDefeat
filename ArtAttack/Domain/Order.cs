using System;

namespace ArtAttack.Domain
{
    public class Order
    {
        public int OrderID { get; set; }
        public int ProductID { get; set; }
        public int BuyerID { get; set; }
        public int ProductType { get; set; }
        public int OrderSummaryID { get; set; }
        public int OrderHistoryID { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
