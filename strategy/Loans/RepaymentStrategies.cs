namespace strategy.Loans
{
    public record LoanRequest(decimal Amount, decimal AnnualRatePercent, int Months);

    public record Installment(int Month, decimal Payment, decimal Principal, decimal Interest, decimal RemainingBalance);

    public record RepaymentSchedule(string Strategy, IReadOnlyList<Installment> Installments)
    {
        public decimal TotalPaid => Installments.Sum(c => c.Payment);
        public decimal TotalInterest => Installments.Sum(c => c.Interest);
        public decimal FirstPayment => Installments[0].Payment;
        public decimal LastPayment => Installments[^1].Payment;
    }

    /// <summary>
    /// Strategy - one way of splitting a loan into monthly installments.
    /// </summary>
    public interface IRepaymentStrategy
    {
        string Key { get; }
        string Description { get; }
        RepaymentSchedule Calculate(LoanRequest loan);
    }

    /// <summary>
    /// Shared math - strategies differ only in how much principal is paid every month.
    /// </summary>
    public abstract class RepaymentStrategyBase : IRepaymentStrategy
    {

        public abstract string Key { get; }
        public abstract string Description { get; }

        public RepaymentSchedule Calculate(LoanRequest loan)
        {
            if (loan.Amount <= 0 || loan.Months is < 1 or > 480 || loan.AnnualRatePercent is < 0 or > 50)
                throw new ArgumentException("Amount has to be positive, months 1 - 480 and rate 0 - 50%");

            var monthlyRate = loan.AnnualRatePercent / 100m / 12m;
            var balance = loan.Amount;
            var installments = new List<Installment>();

            for (var month = 1; month <= loan.Months; month++)
            {
                var interest = Math.Round(balance * monthlyRate, 2);
                var principal = month == loan.Months
                    ? balance   // last installment pays off everything left, including rounding differences
                    : Math.Min(balance, PrincipalFor(loan, monthlyRate, month, interest));
                balance -= principal;
                installments.Add(new Installment(month, principal + interest, principal, interest, balance));
            }
            return new RepaymentSchedule(Key, installments);
        }

        protected abstract decimal PrincipalFor(LoanRequest loan, decimal monthlyRate, int month, decimal interest);

        /// <summary>
        /// Monthly payment which pays off "amount" in "months", leaving "leftAtTheEnd" (annuity formula).
        /// </summary>
        protected static decimal AnnuityPayment(decimal amount, decimal monthlyRate, int months, decimal leftAtTheEnd = 0)
        {
            if (monthlyRate == 0) return Math.Round((amount - leftAtTheEnd) / months, 2);
            var growth = Pow(1 + monthlyRate, months);
            return Math.Round((amount * growth - leftAtTheEnd) * monthlyRate / (growth - 1), 2);
        }

        private static decimal Pow(decimal value, int power)
        {
            var result = 1m;
            for (var i = 0; i < power; i++) result *= value;
            return result;
        }

    }

    /// <summary>
    /// "Raty równe" - the same payment every month, most of it is interest at the beginning.
    /// </summary>
    public class EqualInstallmentsStrategy : RepaymentStrategyBase
    {

        public override string Key => "equal";
        public override string Description => "Equal installments - the same payment every month";

        protected override decimal PrincipalFor(LoanRequest loan, decimal monthlyRate, int month, decimal interest) =>
            AnnuityPayment(loan.Amount, monthlyRate, loan.Months) - interest;

    }

    /// <summary>
    /// "Raty malejące" - the same part of principal every month, so the payment goes down. Higher at start, cheaper in total.
    /// </summary>
    public class DecreasingInstallmentsStrategy : RepaymentStrategyBase
    {

        public override string Key => "decreasing";
        public override string Description => "Decreasing installments - fixed principal, payment goes down every month";

        protected override decimal PrincipalFor(LoanRequest loan, decimal monthlyRate, int month, decimal interest) =>
            Math.Round(loan.Amount / loan.Months, 2);

    }

    /// <summary>
    /// Balloon payment - low installments, and a big part of the loan (e.g. 30%) paid in the last month.
    /// Popular in car leasing and car loans.
    /// </summary>
    public class BalloonPaymentStrategy : RepaymentStrategyBase
    {

        private readonly decimal _balloonPercent;

        public BalloonPaymentStrategy(decimal balloonPercent)
        {
            _balloonPercent = balloonPercent;
        }

        public override string Key => "balloon";
        public override string Description => $"Balloon - low installments, {_balloonPercent}% of the loan paid in the last month";

        protected override decimal PrincipalFor(LoanRequest loan, decimal monthlyRate, int month, decimal interest) =>
            AnnuityPayment(loan.Amount, monthlyRate, loan.Months, loan.Amount * _balloonPercent / 100m) - interest;

    }
}
