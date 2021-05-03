using abstract_factory.Services;

namespace abstract_factory.CroissantAbstractFactory
{

    public abstract class Croissant
    {

        public abstract decimal Price { get; }
        public CroissantSize Size { get => Customization.Size; set => Customization.Size = value; }
        public abstract CroissantType Type { get; }
        /// <exception cref="T:System.NotImplementedException"></exception>
        public void Make()
        {
            Service.OpenDoor();
            Service.WaitForPickup();
            Service.CloseDoor();
            CustomOperation();
            Service.WaitForMoney(Price);
            Service.Cook(Customization.Size);
        }

        protected abstract MachineService Service { get; }
        // Important part here (protected)
        protected abstract CroissantCustomization Customization { get; set; }
        protected abstract void CustomOperation();
    }

}
