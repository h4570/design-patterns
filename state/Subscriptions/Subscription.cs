namespace state.Subscriptions
{
    public record StateChange(DateOnly Date, string From, string To, string Reason);

    /// <summary>
    /// Context - behaves differently depending on its current state, but has no "if (status == ...)" anywhere.
    /// Every operation is delegated to the state object.
    /// </summary>
    public class Subscription
    {

        public const decimal MonthlyPrice = 99m;

        private readonly List<StateChange> _history = new();

        public Subscription(string customer, DateOnly startDate)
        {
            Customer = customer;
            Today = startDate;
            State = new TrialState();
            _history.Add(new StateChange(startDate, "-", State.Name, "Sign up"));
        }

        public string Customer { get; }
        public SubscriptionState State { get; private set; }
        public DateOnly Today { get; private set; }
        public int DaysInState { get; private set; }
        public DateOnly? PaidUntil { get; set; }
        public int FailedPayments { get; set; }
        public IReadOnlyList<StateChange> History => _history;
        public bool CanUsePremiumFeatures => State.CanUsePremiumFeatures;

        public void Pay(decimal amount)
        {
            if (amount != MonthlyPrice)
                throw new ArgumentException($"Monthly price is {MonthlyPrice} PLN");
            State.Pay(this);
        }

        public void PaymentFailed() => State.PaymentFailed(this);
        public void Cancel() => State.Cancel(this);
        public void Reactivate() => State.Reactivate(this);

        /// <summary>
        /// Simulated calendar - in real application a daily background job would call it.
        /// </summary>
        public void PassDays(int days)
        {
            for (var i = 0; i < days; i++)
            {
                Today = Today.AddDays(1);
                DaysInState++;
                State.DayPassed(this);
            }
        }

        /// <summary>
        /// Called only by states - they decide what comes next.
        /// </summary>
        internal void TransitionTo(SubscriptionState next, string reason)
        {
            _history.Add(new StateChange(Today, State.Name, next.Name, reason));
            State = next;
            DaysInState = 0;
        }

    }
}
