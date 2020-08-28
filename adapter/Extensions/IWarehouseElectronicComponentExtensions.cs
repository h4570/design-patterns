using adapter.Adapters.ElectronicComponent;
using adapter.Models;
using System.Collections.Generic;

namespace adapter.Extensions
{
    public static class IWarehouseElectronicComponentExtensions
    {
        public static IEnumerable<IElectronicComponent> ToElectronicComponents<T>(
                this IEnumerable<T> warehouseComponents,
                IElectronicComponentAdapter<T> adapter
            ) where T : IWarehouseElectronicComponent
        {
            foreach (var component in warehouseComponents)
                yield return adapter.Adapt(component);
        }
    }
}
