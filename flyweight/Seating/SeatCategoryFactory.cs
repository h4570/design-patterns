using System.Collections.Concurrent;

namespace flyweight.Seating
{
    /// <summary>
    /// Flyweight factory - hands out shared SeatCategory instances.
    /// A category is loaded only once, every next request gets the very same object.
    /// </summary>
    public class SeatCategoryFactory
    {

        private readonly ConcurrentDictionary<string, SeatCategory> _categories = new();

        public SeatCategory Get(string name) => _categories.GetOrAdd(name, SeatCategoryCatalog.Load);

        public int CreatedCount => _categories.Count;

    }
}
