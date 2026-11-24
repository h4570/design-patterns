using System.Globalization;
using System.Text.RegularExpressions;
using chain_of_responsibility.Tickets;

namespace chain_of_responsibility.Handlers
{

    /// <summary>
    /// Rejects obvious spam before anybody (or anything) spends time on it.
    /// </summary>
    public class SpamFilterHandler : TicketHandler
    {
        private static readonly string[] _spamPhrases = { "crypto giveaway", "casino", "click here", "make money fast" };

        protected override (string Status, string Reply)? TryHandle(Ticket ticket)
        {
            var links = Regex.Matches(ticket.Text, "https?://").Count;
            if (links > 3 || _spamPhrases.Any(ticket.Text.Contains))
                return ("Rejected", null);
            return null;
        }
    }

    /// <summary>
    /// Enterprise customers skip bots and go straight to their dedicated account manager.
    /// </summary>
    public class EnterpriseCustomerHandler : TicketHandler
    {
        protected override (string Status, string Reply)? TryHandle(Ticket ticket)
        {
            if (ticket.Plan != CustomerPlan.Enterprise) return null;
            return ("AssignedToAccountManager",
                "Your dedicated account manager will contact you within 1 hour.");
        }
    }

    /// <summary>
    /// Answers frequently asked questions automatically with a link to the help center.
    /// </summary>
    public class FaqBotHandler : TicketHandler
    {
        private static readonly (string[] Keywords, string Article)[] _faq =
        {
            (new[] { "reset password", "forgot password", "can't log in", "cannot log in" }, "https://help.example.com/reset-password"),
            (new[] { "download invoice", "where is my invoice", "invoice copy" }, "https://help.example.com/invoices"),
            (new[] { "change email", "update email" }, "https://help.example.com/change-email"),
        };

        protected override (string Status, string Reply)? TryHandle(Ticket ticket)
        {
            var match = _faq.FirstOrDefault(c => c.Keywords.Any(ticket.Text.Contains));
            if (match.Article == null) return null;
            return ("AutoAnswered", $"Hi! This article should help: {match.Article}. If not, just reply to this message.");
        }
    }

    /// <summary>
    /// Approves small refunds without involving a human. Bigger amounts go further.
    /// </summary>
    public class AutoRefundHandler : TicketHandler
    {
        private readonly decimal _limit;

        public AutoRefundHandler(decimal limit) { _limit = limit; }

        protected override (string Status, string Reply)? TryHandle(Ticket ticket)
        {
            if (!ticket.Text.Contains("refund")) return null;

            var amount = Regex.Match(ticket.Text, @"(\d+(?:[.,]\d{1,2})?)\s*(pln|zł|zl)");
            if (!amount.Success) return null;

            var value = decimal.Parse(amount.Groups[1].Value.Replace(',', '.'), CultureInfo.InvariantCulture);
            if (value > _limit) return null;

            return ("RefundApproved", string.Create(CultureInfo.InvariantCulture,
                $"Refund of {value:0.00} PLN was approved. Money will be back on your account within 3 days."));
        }
    }

    /// <summary>
    /// Last link - catch-all. Every ticket which reached this place goes to the human support queue.
    /// </summary>
    public class HumanSupportHandler : TicketHandler
    {
        protected override (string Status, string Reply)? TryHandle(Ticket ticket)
        {
            var hours = ticket.Plan == CustomerPlan.Pro ? 4 : 24;
            return ("QueuedForSupport", $"Thanks for contacting us. Our support team will answer within {hours} hours.");
        }
    }

}
