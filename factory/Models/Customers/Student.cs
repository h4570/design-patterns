namespace factory.Models.Customers
{
    public class Student : ICustomer
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public int DiscountPercentage { get { return 10; } }
        public CustomerType Type { get { return CustomerType.Student; } }
    }
}
