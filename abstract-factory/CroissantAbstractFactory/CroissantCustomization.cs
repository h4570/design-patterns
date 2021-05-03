namespace abstract_factory.CroissantAbstractFactory
{

    public enum CroissantType
    {
        Sweet,
        Chocolate,
    }

    public enum CroissantSize
    {
        Small,
        Standard,
        Big
    }

    public enum CroissantSugarAmount
    {
        NoSugar,
        Standard,
        DoubleSugar
    }

    public enum CroissantChocolateAmount
    {
        NoChocolate,
        Standard,
        DoubleChocolate
    }

    public class CroissantCustomization
    {

        public CroissantCustomization()
        {
            Chocolate = CroissantChocolateAmount.NoChocolate;
            Sugar = CroissantSugarAmount.NoSugar;
            Size = CroissantSize.Standard;
        }

        public CroissantChocolateAmount Chocolate { get; set; }
        public CroissantSugarAmount Sugar { get; set; }
        public CroissantSize Size { get; set; }
    }

}
