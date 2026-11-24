using System.Globalization;

namespace mediator.RentalForm
{

    public interface IFormMediator
    {
        void Notify(FormComponent sender);
    }

    /// <summary>
    /// Colleague - knows only the mediator, never other components.
    /// </summary>
    public abstract class FormComponent
    {

        private IFormMediator _mediator;

        protected FormComponent(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public bool Enabled { get; set; } = true;
        public string Hint { get; set; }
        public abstract object Value { get; }

        public void SetMediator(IFormMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Called by the user (client). Component validates its own value and tells the mediator that it changed.
        /// </summary>
        public void Change(string value)
        {
            if (!Enabled)
                throw new InvalidOperationException($"Field '{Name}' is disabled: {Hint}");
            Parse(value);
            _mediator?.Notify(this);
        }

        protected abstract void Parse(string value);

    }

    public class StationPicker : FormComponent
    {

        public static readonly string[] Stations =
        {
            "Warsaw Chopin Airport", "Warsaw Central Station", "Krakow Airport", "Gdansk Main Station"
        };

        public StationPicker(string name) : base(name) { }

        public string Station { get; private set; }
        public bool IsAirport => Station?.Contains("Airport") == true;
        public override object Value => Station;

        /// <summary>
        /// Used by the mediator - sets value without notifying it back (no ping-pong between components).
        /// </summary>
        public void Prefill(string station) => Station = station;

        protected override void Parse(string value)
        {
            if (!Stations.Contains(value))
                throw new ArgumentException($"Unknown station '{value}'. Available: {string.Join(", ", Stations)}");
            Station = value;
        }

    }

    public class DateRangePicker : FormComponent
    {

        public DateRangePicker(string name) : base(name) { }

        public DateOnly? From { get; private set; }
        public DateOnly? To { get; private set; }
        public int Days => From.HasValue ? To.Value.DayNumber - From.Value.DayNumber : 0;
        public override object Value => From.HasValue ? $"{From:yyyy-MM-dd}/{To:yyyy-MM-dd} ({Days} days)" : null;

        /// <param name="value">Format: 2026-12-20/2026-12-27</param>
        protected override void Parse(string value)
        {
            var parts = value.Split('/');
            if (parts.Length != 2
                || !DateOnly.TryParse(parts[0], CultureInfo.InvariantCulture, out var from)
                || !DateOnly.TryParse(parts[1], CultureInfo.InvariantCulture, out var to))
                throw new ArgumentException("Dates have to look like 2026-12-20/2026-12-27");
            if (to <= from)
                throw new ArgumentException("Return date has to be after pickup date");
            (From, To) = (from, to);
        }

    }

    public class NumberInput : FormComponent
    {

        public NumberInput(string name) : base(name) { }

        public int? Number { get; private set; }
        public override object Value => Number;

        protected override void Parse(string value)
        {
            if (!int.TryParse(value, out var number) || number is < 18 or > 99)
                throw new ArgumentException($"{Name} has to be a number between 18 and 99");
            Number = number;
        }

    }

    public enum CarClass { Economy, Compact, Van, Premium }

    public class CarClassSelect : FormComponent
    {

        public CarClassSelect(string name) : base(name) { }

        public CarClass? Selected { get; private set; }
        public IReadOnlyList<CarClass> Options { get; private set; } = Enum.GetValues<CarClass>();
        public override object Value => Selected?.ToString();

        public void LimitOptions(IReadOnlyList<CarClass> options) => Options = options;
        public void Prefill(CarClass? carClass) => Selected = carClass;

        protected override void Parse(string value)
        {
            if (!Enum.TryParse<CarClass>(value, ignoreCase: true, out var carClass) || !Options.Contains(carClass))
                throw new ArgumentException($"Car class '{value}' is not available. Available: {string.Join(", ", Options)}");
            Selected = carClass;
        }

    }

    public class Checkbox : FormComponent
    {

        public Checkbox(string name) : base(name) { }

        public bool Checked { get; private set; }
        public override object Value => Checked;

        public void Prefill(bool isChecked) => Checked = isChecked;

        protected override void Parse(string value)
        {
            if (!bool.TryParse(value, out var isChecked))
                throw new ArgumentException($"{Name} has to be true or false");
            Checked = isChecked;
        }

    }

    /// <summary>
    /// Read only component - user does not change it, only the mediator fills it.
    /// </summary>
    public class PriceSummary : FormComponent
    {

        public PriceSummary(string name) : base(name)
        {
            Enabled = false;
        }

        public IReadOnlyList<string> Lines { get; private set; } = Array.Empty<string>();
        public decimal? Total { get; private set; }
        public override object Value => Total;

        public void Show(IReadOnlyList<string> lines, decimal? total) => (Lines, Total) = (lines, total);

        protected override void Parse(string value) => throw new InvalidOperationException("Price is calculated");

    }
}
