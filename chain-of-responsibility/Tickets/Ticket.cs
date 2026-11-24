namespace chain_of_responsibility.Tickets
{
    public enum CustomerPlan { Free, Pro, Enterprise }

    public record Ticket(string CustomerEmail, CustomerPlan Plan, string Subject, string Message)
    {
        /// <summary>Subject and message together, lower case - used by handlers for keyword matching.</summary>
        public string Text => $"{Subject} {Message}".ToLowerInvariant();
    }

    /// <param name="HandledBy">Handler which took the ticket</param>
    /// <param name="Status">What happened with the ticket</param>
    /// <param name="Reply">Answer sent to the customer</param>
    /// <param name="Trail">All handlers which looked at the ticket, in order</param>
    public record TicketResolution(string HandledBy, string Status, string Reply, IReadOnlyList<string> Trail);
}
