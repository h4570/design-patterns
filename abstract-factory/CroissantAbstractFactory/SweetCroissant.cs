using abstract_factory.Services;

namespace abstract_factory.CroissantAbstractFactory
{

    public class SweetCroissant : Croissant
    {

        public SweetCroissant(MachineService service, decimal price, CroissantCustomization customization)
        {
            Service = service;
            Price = price;
            Customization = customization;
        }

        public override CroissantType Type => CroissantType.Chocolate;
        public override decimal Price { get; }

        // Important part here (getter/setter)
        public CroissantSugarAmount Sugar { get => Customization.Sugar; set => Customization.Sugar = value; }

        protected override MachineService Service { get; }
        protected sealed override CroissantCustomization Customization { get; set; }
        /// <exception cref="T:System.NotImplementedException"></exception>
        protected override void CustomOperation()
        {
            AddSugar(Customization.Sugar);
        }

        /// <exception cref="T:System.NotImplementedException"></exception>
        public void AddSugar(CroissantSugarAmount sugar)
        {
            Service.CustomOperation("left arm: 10cm lower");
            Service.CustomOperation("left arm: 50cm to the right");
            Service.CustomOperation("left arm: pick up in grams: " + (int)sugar);
            Service.CustomOperation("left arm: neutral position");
            Service.CustomOperation("left arm: open hand");
        }

    }

}
