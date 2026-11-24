namespace mediator.RentalForm
{
    public record FieldChange(string Field, string Value);

    public record FieldState(object Value, bool Enabled, string Hint);

    public record FormState(string Change, IReadOnlyDictionary<string, FieldState> Fields, IReadOnlyList<string> PriceLines, decimal? Total);

    /// <summary>
    /// The form (page) - creates components and connects them through the mediator.
    /// </summary>
    public class CarRentalForm
    {

        private readonly Dictionary<string, FormComponent> _fields;
        private readonly PriceSummary _price;

        public CarRentalForm()
        {
            var pickup = new StationPicker("pickupStation");
            var returnStation = new StationPicker("returnStation");
            var dates = new DateRangePicker("dates");
            var driverAge = new NumberInput("driverAge");
            var carClass = new CarClassSelect("carClass");
            var insurance = new Checkbox("fullInsurance");
            _price = new PriceSummary("price");

            _ = new RentalFormMediator(pickup, returnStation, dates, driverAge, carClass, insurance, _price);

            _fields = new FormComponent[] { pickup, returnStation, dates, driverAge, carClass, insurance }
                .ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);
        }

        public FormState Apply(FieldChange change)
        {
            if (!_fields.TryGetValue(change.Field, out var field))
                throw new ArgumentException($"Unknown field '{change.Field}'. Fields: {string.Join(", ", _fields.Keys)}");
            field.Change(change.Value);
            return GetState($"{field.Name} = {change.Value}");
        }

        public FormState GetState(string change) => new(
            change,
            _fields.Values.Append(_price).ToDictionary(c => c.Name, c => new FieldState(c.Value, c.Enabled, c.Hint)),
            _price.Lines,
            _price.Total);

    }
}
