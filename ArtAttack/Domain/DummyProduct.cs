using System;

namespace ArtAttack.Domain
{
    public class DummyProduct
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public float Price { get; set; }
        public int? SellerID { get; set; }
        public string ProductType { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
    }
}
