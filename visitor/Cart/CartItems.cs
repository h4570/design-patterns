using System.Text.Json.Serialization;

namespace visitor.Cart
{
    /// <summary>
    /// Visitor - one Visit method per item type. Every new operation on the cart is a new visitor,
    /// item classes do not change.
    /// </summary>
    public interface ICartVisitor<out T>
    {
        T Visit(PhysicalProduct item);
        T Visit(Ebook item);
        T Visit(GiftCard item);
        T Visit(InstallationService item);
    }

    /// <summary>
    /// Element. Accept() is the "double dispatch" - the item knows its own type and calls the right Visit overload.
    /// </summary>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(PhysicalProduct), "physical")]
    [JsonDerivedType(typeof(Ebook), "ebook")]
    [JsonDerivedType(typeof(GiftCard), "giftCard")]
    [JsonDerivedType(typeof(InstallationService), "installation")]
    public interface ICartItem
    {
        string Name { get; }
        T Accept<T>(ICartVisitor<T> visitor);
    }

    /// <param name="VatPercent">23 for electronics, 5 for paper books</param>
    public record PhysicalProduct(string Name, decimal GrossPrice, int Quantity, decimal WeightKg, int VatPercent) : ICartItem
    {
        public T Accept<T>(ICartVisitor<T> visitor) => visitor.Visit(this);
    }

    public record Ebook(string Name, decimal GrossPrice, string Format) : ICartItem
    {
        public T Accept<T>(ICartVisitor<T> visitor) => visitor.Visit(this);
    }

    public record GiftCard(string Name, decimal Value, string RecipientEmail) : ICartItem
    {
        public T Accept<T>(ICartVisitor<T> visitor) => visitor.Visit(this);
    }

    public record InstallationService(string Name, decimal GrossPrice, string City) : ICartItem
    {
        public T Accept<T>(ICartVisitor<T> visitor) => visitor.Visit(this);
    }

    public static class SampleCart
    {
        public static IReadOnlyList<ICartItem> Create() => new ICartItem[]
        {
            new PhysicalProduct("Laptop 14\"", 4999m, 1, 1.4m, 23),
            new PhysicalProduct("USB-C cable 2m", 49.99m, 2, 0.1m, 23),
            new PhysicalProduct("Book: Design Patterns (paper)", 129m, 1, 0.9m, 5),
            new Ebook("Clean Architecture", 59.90m, "EPUB"),
            new GiftCard("Gift card", 200m, "piotr@example.com"),
            new InstallationService("Laptop setup at home", 149m, "Krakow")
        };
    }
}
