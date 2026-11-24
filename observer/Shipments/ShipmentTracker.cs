namespace observer.Shipments
{
    public enum ShipmentStatus { Created, PickedUp, InTransit, OutForDelivery, Delivered, DeliveryFailed }

    public record ShipmentEvent(string TrackingNumber, ShipmentStatus Status, string Location, DateTime At);

    public record PublishResult(ShipmentEvent Event, int Notified, IReadOnlyList<string> Failures);

    /// <summary>
    /// Subject - publishes status changes of all shipments. It does not know who listens and what they do.
    /// </summary>
    public class ShipmentTracker : IObservable<ShipmentEvent>
    {

        private readonly List<IObserver<ShipmentEvent>> _observers = new();
        private readonly Dictionary<string, ShipmentStatus> _statuses = new();
        private readonly object _lock = new();

        public int ObserverCount
        {
            get { lock (_lock) return _observers.Count; }
        }

        public IDisposable Subscribe(IObserver<ShipmentEvent> observer)
        {
            lock (_lock)
                if (!_observers.Contains(observer))
                    _observers.Add(observer);
            return new Unsubscriber(this, observer);
        }

        public PublishResult UpdateStatus(string trackingNumber, ShipmentStatus status, string location, DateTime? at = null)
        {
            IObserver<ShipmentEvent>[] observers;
            lock (_lock)
            {
                if (_statuses.TryGetValue(trackingNumber, out var current) && current == ShipmentStatus.Delivered)
                    throw new InvalidOperationException($"Shipment {trackingNumber} was already delivered");
                _statuses[trackingNumber] = status;
                // Copy - observers can unsubscribe while being notified, and notifying under a lock would block other requests
                observers = _observers.ToArray();
            }

            var shipmentEvent = new ShipmentEvent(trackingNumber, status, location, at ?? DateTime.Now);
            var failures = new List<string>();
            foreach (var observer in observers)
            {
                try
                {
                    observer.OnNext(shipmentEvent);
                }
                catch (Exception ex)
                {
                    // One broken observer cannot stop the others from being notified
                    failures.Add($"{observer}: {ex.Message}");
                }
            }
            return new PublishResult(shipmentEvent, observers.Length - failures.Count, failures);
        }

        private void Unsubscribe(IObserver<ShipmentEvent> observer)
        {
            lock (_lock) _observers.Remove(observer);
        }

        private sealed class Unsubscriber : IDisposable
        {

            private readonly ShipmentTracker _tracker;
            private readonly IObserver<ShipmentEvent> _observer;

            public Unsubscriber(ShipmentTracker tracker, IObserver<ShipmentEvent> observer)
            {
                _tracker = tracker;
                _observer = observer;
            }

            public void Dispose() => _tracker.Unsubscribe(_observer);

        }

    }
}
