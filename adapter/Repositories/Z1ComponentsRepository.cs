using adapter.Models.WarehouseZ1;
using System;
using System.Collections.Generic;

namespace adapter.Repositories
{
    public class Z1ComponentsRepository
    {

        private readonly List<Z1ElectronicComponent> _database;

        public Z1ComponentsRepository()
        {
            _database = new List<Z1ElectronicComponent>()
            {
                new Z1ElectronicComponent(){ Id=Guid.NewGuid(), MPN="CX92F1", Barcode="1P111;1T11;1D20200828" },
                new Z1ElectronicComponent(){ Id=Guid.NewGuid(), MPN="CX92F2", Barcode="1P222;;1D20200827" },
                new Z1ElectronicComponent(){ Id=Guid.NewGuid(), MPN="CX92F3", Barcode="1P333;1T33;1D20200826" }
            };
        }

        public List<Z1ElectronicComponent> GetAll()
        {
            return _database;
        }

    }
}
