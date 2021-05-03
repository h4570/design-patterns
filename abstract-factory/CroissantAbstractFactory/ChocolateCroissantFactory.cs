using abstract_factory.Services;

namespace abstract_factory.CroissantAbstractFactory
{

    public class ChocolateCroissantFactory : CroissantFactory
    {
        public override Croissant GetCroissant(MachineService service, CroissantCustomization customization)
        {
            return new ChocolateCroissant(service, 10, customization);
        }
    }

}
