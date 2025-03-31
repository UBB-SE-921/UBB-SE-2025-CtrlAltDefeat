using System;

namespace ArtAttack.Domain
{
    internal enum OrderStatus
    {
        PROCESSING,
        SHIPPED,
        IN_WAREHOUSE,
        IN_TRANSIT,
        OUT_FOR_DELIVERY,
        DELIVERED
    }

    internal class OrderCheckpoint
    {
        public int CheckpointID { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Location { get; set; }
        public string Description { get; set; }
        public OrderStatus Status { get; set; }
        public int TrackedOrderID { get; set; }

    }
}
