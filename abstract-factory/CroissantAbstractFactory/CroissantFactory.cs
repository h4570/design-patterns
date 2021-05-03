using System;
using abstract_factory.Services;

namespace abstract_factory.CroissantAbstractFactory
{

    public abstract class CroissantFactory
    {

        public abstract Croissant GetCroissant(MachineService service, CroissantCustomization customization);

        /// <exception cref="T:System.NotImplementedException"></exception>
        public static CroissantFactory GetFactory(CroissantType type)
        {
            return type switch
            {
                CroissantType.Chocolate => new ChocolateCroissantFactory(),
                CroissantType.Sweet => new SweetCroissantFactory(),
                _ => throw new NotImplementedException()
            };
        }

    }

}
