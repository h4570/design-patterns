using System.Globalization;

namespace mediator.RentalForm
{

    /// <summary>
    /// Concrete mediator - all rules about how fields depend on each other live here, in one place.
    /// Without it every component would need references to half of the other components.
    /// </summary>
    public class RentalFormMediator : IFormMediator
    {

        private const int MinimumAge = 21;
        private const int YoungDriverAge = 25;
        private const decimal YoungDriverFeePerDay = 35m;
        private const decimal InsurancePerDay = 45m;
        private const decimal AirportFee = 60m;
        private const decimal OneWayFee = 150m;

        private static readonly Dictionary<CarClass, decimal> PricePerDay = new()
        {
            [CarClass.Economy] = 120m, [CarClass.Compact] = 160m, [CarClass.Van] = 260m, [CarClass.Premium] = 390m
        };

        private readonly StationPicker _pickup;
        private readonly StationPicker _return;
        private readonly DateRangePicker _dates;
        private readonly NumberInput _driverAge;
        private readonly CarClassSelect _carClass;
        private readonly Checkbox _insurance;
        private readonly PriceSummary _price;

        public RentalFormMediator(StationPicker pickup, StationPicker returnStation, DateRangePicker dates,
            NumberInput driverAge, CarClassSelect carClass, Checkbox insurance, PriceSummary price)
        {
            (_pickup, _return, _dates, _driverAge, _carClass, _insurance, _price) =
                (pickup, returnStation, dates, driverAge, carClass, insurance, price);

            foreach (var component in new FormComponent[] { pickup, returnStation, dates, driverAge, carClass, insurance, price })
                component.SetMediator(this);

            RecalculatePrice();
        }

        public void Notify(FormComponent sender)
        {
            // Most people return the car where they picked it up
            if (sender == _pickup && _return.Station == null)
                _return.Prefill(_pickup.Station);

            // User picked the class on their own - old "we changed your class" message is no longer true
            if (sender == _carClass)
                _carClass.Hint = null;

            if (sender == _driverAge)
                ApplyAgeRules();

            if (sender == _carClass || sender == _driverAge)
                ApplyInsuranceRules();

            // Every change can affect the price
            RecalculatePrice();
        }

        private void ApplyAgeRules()
        {
            var age = _driverAge.Number.Value;
            _carClass.Hint = null;

            if (age < MinimumAge)
            {
                _carClass.Enabled = false;
                _carClass.Prefill(null);
                _carClass.Hint = $"Drivers have to be at least {MinimumAge} years old";
                return;
            }

            _carClass.Enabled = true;
            var allowed = age < YoungDriverAge
                ? new[] { CarClass.Economy, CarClass.Compact }
                : Enum.GetValues<CarClass>();
            _carClass.LimitOptions(allowed);

            if (_carClass.Selected.HasValue && !allowed.Contains(_carClass.Selected.Value))
            {
                _carClass.Hint = $"{_carClass.Selected} is available from {YoungDriverAge} years, changed to {CarClass.Economy}";
                _carClass.Prefill(CarClass.Economy);
            }
        }

        private void ApplyInsuranceRules()
        {
            if (_carClass.Selected == CarClass.Premium)
            {
                _insurance.Prefill(true);
                _insurance.Enabled = false;
                _insurance.Hint = "Full insurance is mandatory for Premium cars";
            }
            else
            {
                _insurance.Enabled = true;
                _insurance.Hint = null;
            }
        }

        private void RecalculatePrice()
        {
            var missing = new (FormComponent Component, bool IsEmpty)[]
            {
                (_pickup, _pickup.Station == null), (_dates, _dates.Days == 0),
                (_driverAge, _driverAge.Number == null), (_carClass, _carClass.Selected == null)
            }.Where(c => c.IsEmpty).Select(c => c.Component.Name).ToList();

            if (missing.Count > 0)
            {
                _price.Hint = $"Fill in: {string.Join(", ", missing)}";
                _price.Show(Array.Empty<string>(), null);
                return;
            }

            var days = _dates.Days;
            var lines = new List<string>();
            decimal total = 0;

            void Add(string line, decimal amount)
            {
                lines.Add(string.Create(CultureInfo.InvariantCulture, $"{line}: {amount:0.00} PLN"));
                total += amount;
            }

            var carClass = _carClass.Selected.Value;
            Add($"{carClass} x {days} days", PricePerDay[carClass] * days);
            if (days >= 7) Add("Weekly discount -10%", -PricePerDay[carClass] * days * 0.1m);
            if (_insurance.Checked) Add($"Full insurance x {days} days", InsurancePerDay * days);
            if (_driverAge.Number < YoungDriverAge) Add($"Young driver fee x {days} days", YoungDriverFeePerDay * days);
            if (_pickup.IsAirport) Add("Airport fee", AirportFee);
            if (_return.Station != _pickup.Station) Add("One-way fee", OneWayFee);

            _price.Hint = null;
            _price.Show(lines, total);
        }

    }
}
