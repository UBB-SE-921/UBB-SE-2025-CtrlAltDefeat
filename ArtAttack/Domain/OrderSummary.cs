namespace ArtAttack.Domain
{
    public class OrderSummary
    {
        public int ID { get; set; }

        public float Subtotal { get; set; }
        public float WarrantyTax { get; set; }
        public float DeliveryFee { get; set; }
        public float FinalTotal { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string AdditionalInfo { get; set; }
        public string ContractDetails { get; set; }

        //public virtual ICollection<Order> Orders { get; set; } //not sure this should end up in the model 
    }
}
