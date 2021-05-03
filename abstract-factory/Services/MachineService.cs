using System;
using abstract_factory.CroissantAbstractFactory;

namespace abstract_factory.Services
{
    public class MachineService
    {

        /// <exception cref="T:System.NotImplementedException"></exception>
        public void WaitForMoney(decimal price)
        {
            throw new NotImplementedException();
        }

        /// <exception cref="T:System.NotImplementedException"></exception>
        public void WaitForPickup()
        {
            throw new NotImplementedException();
        }

        /// <exception cref="T:System.NotImplementedException"></exception>
        public void OpenDoor()
        {
            throw new NotImplementedException();
        }

        /// <exception cref="T:System.NotImplementedException"></exception>
        public void CustomOperation(string command)
        {
            throw new NotImplementedException();
        }

        /// <exception cref="T:System.NotImplementedException"></exception>
        public void AddChocolate(CroissantChocolateAmount chocolate)
        {
            throw new NotImplementedException();
        }

        /// <exception cref="T:System.NotImplementedException"></exception>
        public void Cook(CroissantSize size)
        {
            throw new NotImplementedException();
        }

        /// <exception cref="T:System.NotImplementedException"></exception>
        public void CloseDoor()
        {
            throw new NotImplementedException();
        }

    }
}
