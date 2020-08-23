namespace factory.Models.Customers
{
    public interface ICustomer
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public int DiscountPercentage { get; }
        public CustomerType Type { get; }
    }
}
