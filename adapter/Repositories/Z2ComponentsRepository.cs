using adapter.Models.WarehouseZ2;
using System.Collections.Generic;

namespace adapter.Repositories
{
    public class Z2ComponentsRepository
    {

        private readonly List<Z2ElectronicComponent> _database;

        public Z2ComponentsRepository()
        {
            _database = new List<Z2ElectronicComponent>()
            {
                new Z2ElectronicComponent(){
                    Id=1,
                    MPN="CS91F1",
                    Details= new Z2ElectronicComponentDetails() { Id=1, DateCode="20200728", Lot="11" }
                },
                new Z2ElectronicComponent(){
                    Id=2,
                    MPN="CS91F2",
                    Details= new Z2ElectronicComponentDetails() { Id=2, Lot="22" }
                },
                new Z2ElectronicComponent(){
                    Id=3,
                    MPN="CS91F3",
                    Details= new Z2ElectronicComponentDetails() { Id=3, DateCode="20200726", Lot="33" }
                }
            };
        }

        public List<Z2ElectronicComponent> GetAll()
        {
            return _database;
        }

    }
}
