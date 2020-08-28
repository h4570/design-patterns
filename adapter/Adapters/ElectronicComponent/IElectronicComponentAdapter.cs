using adapter.Models;

namespace adapter.Adapters.ElectronicComponent
{
    public interface IElectronicComponentAdapter<T> where T : IWarehouseElectronicComponent
    {
        public IElectronicComponent Adapt(T component);
    }
}
