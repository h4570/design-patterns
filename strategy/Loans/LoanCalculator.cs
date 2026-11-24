namespace strategy.Loans
{
    /// <summary>
    /// Context - knows all available strategies (registered in DI), but no details of any of them.
    /// </summary>
    public class LoanCalculator
    {

        private readonly IReadOnlyDictionary<string, IRepaymentStrategy> _strategies;

        public LoanCalculator(IEnumerable<IRepaymentStrategy> strategies)
        {
            _strategies = strategies.ToDictionary(c => c.Key, StringComparer.OrdinalIgnoreCase);
        }

        public IEnumerable<IRepaymentStrategy> Strategies => _strategies.Values;

        public RepaymentSchedule Calculate(LoanRequest loan, string strategyKey)
        {
            if (!_strategies.TryGetValue(strategyKey ?? "", out var strategy))
                throw new ArgumentException($"Unknown strategy '{strategyKey}'. Available: {string.Join(", ", _strategies.Keys)}");
            return strategy.Calculate(loan);
        }

        public IReadOnlyList<RepaymentSchedule> Compare(LoanRequest loan) =>
            _strategies.Values.Select(c => c.Calculate(loan)).ToList();

    }
}
