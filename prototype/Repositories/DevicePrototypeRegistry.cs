using prototype.Models;
using System.Collections.Generic;
using System.Linq;

namespace prototype.Repositories
{

    public class DevicePrototypeRegistry
    {

        private readonly List<IDevice> _items;

        public DevicePrototypeRegistry()
        {
            _items = new List<IDevice>();
        }

        public void Add(IDevice device) { _items.Add(device); }

        public IDevice GetCopyById(int id)
        {
            var item = _items.SingleOrDefault(c => c.Id.Equals(id));
            if (item != null) return item.Clone();
            else throw new System.Exception("Provided device id was not found");
        }

    }

}
