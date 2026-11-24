using observer.Shipments;
using Microsoft.AspNetCore.Mvc;

namespace observer.Controllers
{
    public record StatusChange(ShipmentStatus Status, string Location);
    public record SmsSubscription(string Phone);

    [ApiController]
    [Route("[controller]")]
    public class ShipmentsController : ControllerBase
    {

        private readonly ShipmentTracker _tracker;
        private readonly NotificationLog _log;
        private readonly DeliveryTimeAnalytics _analytics;

        public ShipmentsController(ShipmentTracker tracker, NotificationLog log, DeliveryTimeAnalytics analytics)
        {
            _tracker = tracker;
            _log = log;
            _analytics = analytics;
        }

        /// <summary>
        /// Called by courier's scanner.
        /// </summary>
        [HttpPost("{trackingNumber}/status")]
        public ActionResult<PublishResult> UpdateStatus(string trackingNumber, StatusChange change)
        {
            try
            {
                return Ok(_tracker.UpdateStatus(trackingNumber, change.Status, change.Location));
            }
            catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        }

        [HttpPost("{trackingNumber}/sms")]
        public ActionResult SubscribeSms(string trackingNumber, SmsSubscription subscription)
        {
            new CustomerSmsNotifier(trackingNumber, subscription.Phone, _log).SubscribeTo(_tracker);
            return Ok(new { Subscribed = $"{subscription.Phone} -> {trackingNumber}", Observers = _tracker.ObserverCount });
        }

        [HttpGet("notifications")]
        public ActionResult<IReadOnlyList<string>> GetNotifications() => Ok(_log.All());

        [HttpGet("analytics")]
        public ActionResult GetAnalytics() => Ok(_analytics.GetReport());

        /// <summary>
        /// Browser friendly example - two parcels on a separate tracker, shop's server goes down for a moment.
        /// </summary>
        [HttpGet("demo")]
        public ActionResult Demo()
        {
            var log = new NotificationLog();
            var tracker = new ShipmentTracker();
            var analytics = new DeliveryTimeAnalytics();
            var shop = new ShopWebhookNotifier("https://fashion-shop.pl/webhooks/shipments", log);

            tracker.Subscribe(analytics);
            tracker.Subscribe(shop);
            new CustomerSmsNotifier("PL123", "+48 600 100 200", log).SubscribeTo(tracker);
            var observersAtStart = tracker.ObserverCount;

            var day = new DateTime(2026, 11, 23);
            var failures = new List<string>();
            void Update(string tracking, ShipmentStatus status, string location, double hour) =>
                failures.AddRange(tracker.UpdateStatus(tracking, status, location, day.AddHours(hour)).Failures);

            Update("PL123", ShipmentStatus.PickedUp, "Warsaw", 8);
            Update("PL456", ShipmentStatus.PickedUp, "Poznan", 9);
            Update("PL123", ShipmentStatus.InTransit, "Lodz hub", 13);

            shop.ShopIsOnline = false;
            Update("PL456", ShipmentStatus.InTransit, "Lodz hub", 15);
            shop.ShopIsOnline = true;

            Update("PL123", ShipmentStatus.OutForDelivery, "Krakow", 31.5);
            Update("PL123", ShipmentStatus.Delivered, "Krakow", 35.25);
            Update("PL456", ShipmentStatus.OutForDelivery, "Gdansk", 32);
            Update("PL456", ShipmentStatus.Delivered, "Gdansk", 38);

            return Ok(new
            {
                ObserversAtStart = observersAtStart,
                ObserversAtEnd = tracker.ObserverCount,
                Notifications = log.All(),
                FailedNotifications = failures,
                Analytics = analytics.GetReport()
            });
        }

    }
}
