using abstract_factory.CroissantAbstractFactory;

namespace abstract_factory.Services
{
    /// <summary>
    /// Croissant machine driver. There is no real hardware here, so every operation is only recorded,
    /// which lets you see what exactly each croissant type asked the machine to do.
    /// </summary>
    public class MachineService
    {

        private readonly List<string> _operations = new();

        public IReadOnlyList<string> Operations => _operations;

        public void WaitForMoney(decimal price) => _operations.Add($"wait for money: {price}");

        public void WaitForPickup() => _operations.Add("wait for pickup");

        public void OpenDoor() => _operations.Add("open door");

        public void CustomOperation(string command) => _operations.Add(command);

        public void AddChocolate(CroissantChocolateAmount chocolate) => _operations.Add($"add chocolate: {chocolate}");

        public void Cook(CroissantSize size) => _operations.Add($"cook: {size}");

        public void CloseDoor() => _operations.Add("close door");

    }
}
