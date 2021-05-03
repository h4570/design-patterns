using abstract_factory.Services;

namespace abstract_factory.CroissantAbstractFactory
{

    public class ChocolateCroissant : Croissant
    {

        public ChocolateCroissant(MachineService service, decimal price, CroissantCustomization customization)
        {
            Service = service;
            Price = price;
            Customization = customization;
        }

        public override CroissantType Type => CroissantType.Chocolate;
        public override decimal Price { get; }
        // Important part here (getter/setter)
        public CroissantChocolateAmount Chocolate { get => Customization.Chocolate; set => Customization.Chocolate = value; }

        protected override MachineService Service { get; }
        protected sealed override CroissantCustomization Customization { get; set; }
        /// <exception cref="T:System.NotImplementedException"></exception>
        protected override void CustomOperation()
        {
            AddChocolate(Customization.Chocolate);
        }

        /// <exception cref="T:System.NotImplementedException"></exception>
        private void AddChocolate(CroissantChocolateAmount chocolate)
        {
            Service.CustomOperation("left arm: 10cm lower");
            Service.CustomOperation("left arm: 60cm to the right");
            Service.CustomOperation("left arm: pick up in grams: " + (int)chocolate);
            Service.CustomOperation("left arm: neutral position");
            Service.CustomOperation("left arm: open hand");
        }

    }

}
