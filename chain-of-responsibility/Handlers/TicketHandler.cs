using chain_of_responsibility.Tickets;

namespace chain_of_responsibility.Handlers
{
    /// <summary>
    /// Base handler - keeps the link to the next handler in the chain.
    /// Each handler either resolves the ticket or passes it further. It does not know who is next.
    /// </summary>
    public abstract class TicketHandler
    {

        private TicketHandler _next;

        protected string Name => GetType().Name;

        /// <returns>Handler passed as parameter - allows fluent chain building</returns>
        public TicketHandler SetNext(TicketHandler next)
        {
            _next = next;
            return next;
        }

        public TicketResolution Handle(Ticket ticket) => Handle(ticket, new List<string>());

        private TicketResolution Handle(Ticket ticket, List<string> trail)
        {
            trail.Add(Name);

            var result = TryHandle(ticket);
            if (result != null)
                return new TicketResolution(Name, result.Value.Status, result.Value.Reply, trail);

            if (_next == null)
                throw new InvalidOperationException($"Nobody handled the ticket '{ticket.Subject}' - chain should end with a catch-all handler");
            return _next.Handle(ticket, trail);
        }

        /// <returns>Status and reply when the ticket was handled, null to pass it to the next handler</returns>
        protected abstract (string Status, string Reply)? TryHandle(Ticket ticket);

    }
}
