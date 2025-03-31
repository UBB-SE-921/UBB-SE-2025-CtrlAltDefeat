using System;

namespace ArtAttack.Domain
{
    internal class TrackedOrder
    {
        public int TrackedOrderID { get; set; }
        public int OrderID { get; set; }
        public OrderStatus CurrentStatus { get; set; }
        public DateOnly EstimatedDeliveryDate { get; set; }
        public required string DeliveryAddress { get; set; }
    }
}
