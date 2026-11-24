using System.Collections.Concurrent;
using System.Globalization;

namespace observer.Shipments
{
    /// <summary>
    /// Instead of real SMS gateway and HTTP calls - every "sent" message lands here.
    /// </summary>
    public class NotificationLog
    {

        private readonly ConcurrentQueue<string> _entries = new();

        public void Add(string channel, string message) => _entries.Enqueue($"[{channel}] {message}");
        public IReadOnlyList<string> All() => _entries.ToList();

    }

    /// <summary>
    /// Observer created when a customer asks for SMS updates of one parcel.
    /// Customers do not want 6 messages, only the ones which matter. Unsubscribes itself when the parcel is delivered.
    /// </summary>
    public class CustomerSmsNotifier : IObserver<ShipmentEvent>
    {

        private static readonly ShipmentStatus[] Important =
        {
            ShipmentStatus.OutForDelivery, ShipmentStatus.Delivered, ShipmentStatus.DeliveryFailed
        };

        private readonly string _trackingNumber;
        private readonly string _phone;
        private readonly NotificationLog _log;
        private IDisposable _subscription;

        public CustomerSmsNotifier(string trackingNumber, string phone, NotificationLog log)
        {
            _trackingNumber = trackingNumber;
            _phone = phone;
            _log = log;
        }

        public void SubscribeTo(IObservable<ShipmentEvent> tracker) => _subscription = tracker.Subscribe(this);

        public void OnNext(ShipmentEvent value)
        {
            if (value.TrackingNumber != _trackingNumber || !Important.Contains(value.Status)) return;

            var text = value.Status switch
            {
                ShipmentStatus.OutForDelivery => "is out for delivery today",
                ShipmentStatus.Delivered => $"was delivered at {value.At:HH:mm}. Thank you!",
                _ => "could not be delivered, courier will try again tomorrow"
            };
            _log.Add("SMS", $"to {_phone}: Your parcel {_trackingNumber} {text}");

            if (value.Status == ShipmentStatus.Delivered)
                _subscription?.Dispose();   // safe during notification - tracker iterates over a copy
        }

        public void OnError(Exception error) { }
        public void OnCompleted() => _subscription?.Dispose();

        public override string ToString() => $"SMS {_phone}";

    }

    /// <summary>
    /// Observer of all shipments of one online shop - forwards every change to the shop's webhook.
    /// </summary>
    public class ShopWebhookNotifier : IObserver<ShipmentEvent>
    {

        private readonly string _url;
        private readonly NotificationLog _log;

        public ShopWebhookNotifier(string url, NotificationLog log)
        {
            _url = url;
            _log = log;
        }

        /// <summary>
        /// Set to false to simulate shop's server being down.
        /// </summary>
        public bool ShopIsOnline { get; set; } = true;

        public void OnNext(ShipmentEvent value)
        {
            if (!ShopIsOnline)
                throw new HttpRequestException($"POST {_url} - 503 Service Unavailable");
            _log.Add("Webhook", $"POST {_url} {{ \"tracking\": \"{value.TrackingNumber}\", \"status\": \"{value.Status}\" }}");
        }

        public void OnError(Exception error) { }
        public void OnCompleted() { }

        public override string ToString() => $"Webhook {_url}";

    }

    /// <summary>
    /// Observer which only counts - average time from pickup to delivery.
    /// </summary>
    public class DeliveryTimeAnalytics : IObserver<ShipmentEvent>
    {

        private readonly ConcurrentDictionary<string, DateTime> _pickedUp = new();
        private readonly ConcurrentBag<TimeSpan> _deliveryTimes = new();

        public void OnNext(ShipmentEvent value)
        {
            if (value.Status == ShipmentStatus.PickedUp)
                _pickedUp[value.TrackingNumber] = value.At;
            else if (value.Status == ShipmentStatus.Delivered && _pickedUp.TryRemove(value.TrackingNumber, out var pickedUp))
                _deliveryTimes.Add(value.At - pickedUp);
        }

        public object GetReport() => new
        {
            DeliveredShipments = _deliveryTimes.Count,
            InProgress = _pickedUp.Count,
            AverageDeliveryHours = _deliveryTimes.IsEmpty
                ? null
                : string.Create(CultureInfo.InvariantCulture, $"{_deliveryTimes.Average(c => c.TotalHours):0.0}")
        };

        public void OnError(Exception error) { }
        public void OnCompleted() { }

        public override string ToString() => "Delivery time analytics";

    }
}
