namespace state.Subscriptions
{
    /// <summary>
    /// State - by default every operation is not allowed, concrete states override only what makes sense for them.
    /// </summary>
    public abstract class SubscriptionState
    {

        public abstract string Name { get; }
        public virtual bool CanUsePremiumFeatures => false;

        public virtual void Pay(Subscription subscription) => throw NotAllowed("Payment");
        public virtual void PaymentFailed(Subscription subscription) => throw NotAllowed("Failed payment");
        public virtual void Cancel(Subscription subscription) => throw NotAllowed("Cancelling");
        public virtual void Reactivate(Subscription subscription) => throw NotAllowed("Reactivation");
        public virtual void DayPassed(Subscription subscription) { }

        protected InvalidOperationException NotAllowed(string action) =>
            new($"{action} is not possible when subscription is {Name}");

        protected static void StartNewPeriod(Subscription subscription, DateOnly from)
        {
            subscription.PaidUntil = from.AddMonths(1);
            subscription.FailedPayments = 0;
        }

    }

    /// <summary>
    /// 14 days for free, full access, no card needed.
    /// </summary>
    public class TrialState : SubscriptionState
    {

        public const int TrialDays = 14;

        public override string Name => "Trial";
        public override bool CanUsePremiumFeatures => true;

        public override void Pay(Subscription subscription)
        {
            StartNewPeriod(subscription, subscription.Today);
            subscription.TransitionTo(new ActiveState(), "First payment");
        }

        public override void Cancel(Subscription subscription) =>
            subscription.TransitionTo(new CancelledState(), "Cancelled during trial");

        public override void DayPassed(Subscription subscription)
        {
            if (subscription.DaysInState >= TrialDays)
                subscription.TransitionTo(new SuspendedState(), "Trial ended without payment");
        }

    }

    public class ActiveState : SubscriptionState
    {

        public override string Name => "Active";
        public override bool CanUsePremiumFeatures => true;

        public override void PaymentFailed(Subscription subscription)
        {
            subscription.FailedPayments++;
            subscription.TransitionTo(new PastDueState(), "Card payment failed");
        }

        /// <summary>
        /// Already paid month is not lost - access stays until the end of the period.
        /// </summary>
        public override void Cancel(Subscription subscription) =>
            subscription.TransitionTo(new PendingCancellationState(), $"Cancelled, access until {subscription.PaidUntil:yyyy-MM-dd}");

        public override void DayPassed(Subscription subscription)
        {
            if (subscription.Today >= subscription.PaidUntil)
                subscription.TransitionTo(new PastDueState(), "Renewal payment is due");
        }

    }

    /// <summary>
    /// Grace period - customer still has access, but has 7 days and 3 attempts to pay.
    /// </summary>
    public class PastDueState : SubscriptionState
    {

        public const int GraceDays = 7;
        public const int MaxFailedPayments = 3;

        public override string Name => "PastDue";
        public override bool CanUsePremiumFeatures => true;

        public override void Pay(Subscription subscription)
        {
            // New period continues from the old one - paying late does not give extra days
            StartNewPeriod(subscription, subscription.PaidUntil ?? subscription.Today);
            subscription.TransitionTo(new ActiveState(), "Overdue payment received");
        }

        public override void PaymentFailed(Subscription subscription)
        {
            subscription.FailedPayments++;
            if (subscription.FailedPayments >= MaxFailedPayments)
                subscription.TransitionTo(new SuspendedState(), $"{MaxFailedPayments} failed payments");
        }

        public override void Cancel(Subscription subscription) =>
            subscription.TransitionTo(new CancelledState(), "Cancelled with unpaid balance");

        public override void DayPassed(Subscription subscription)
        {
            if (subscription.DaysInState >= GraceDays)
                subscription.TransitionTo(new SuspendedState(), $"Not paid within {GraceDays} days");
        }

    }

    /// <summary>
    /// No access, but data is kept for 30 days - paying brings everything back.
    /// </summary>
    public class SuspendedState : SubscriptionState
    {

        public const int DataRetentionDays = 30;

        public override string Name => "Suspended";

        public override void Pay(Subscription subscription)
        {
            StartNewPeriod(subscription, subscription.Today);
            subscription.TransitionTo(new ActiveState(), "Payment after suspension");
        }

        public override void Cancel(Subscription subscription) =>
            subscription.TransitionTo(new CancelledState(), "Cancelled while suspended");

        public override void DayPassed(Subscription subscription)
        {
            if (subscription.DaysInState >= DataRetentionDays)
                subscription.TransitionTo(new CancelledState(), $"Suspended for {DataRetentionDays} days, data deleted");
        }

    }

    public class PendingCancellationState : SubscriptionState
    {

        public override string Name => "PendingCancellation";
        public override bool CanUsePremiumFeatures => true;

        public override void Reactivate(Subscription subscription) =>
            subscription.TransitionTo(new ActiveState(), "Customer changed their mind");

        public override void DayPassed(Subscription subscription)
        {
            if (subscription.Today >= subscription.PaidUntil)
                subscription.TransitionTo(new CancelledState(), "Paid period ended");
        }

    }

    /// <summary>
    /// Final state - nothing is allowed, customer has to sign up again.
    /// </summary>
    public class CancelledState : SubscriptionState
    {
        public override string Name => "Cancelled";
    }
}
