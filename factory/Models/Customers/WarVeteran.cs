namespace factory.Models.Customers
{
    public class WarVeteran : ICustomer
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public int DiscountPercentage { get { return 40; } }
        public CustomerType Type { get { return CustomerType.WarVeteran; } }

    }
}
