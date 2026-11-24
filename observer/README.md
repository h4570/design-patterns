# Observer pattern

In this example observer pattern was used for **parcel tracking in a courier company**.  
Every time a courier scans a parcel, `ShipmentTracker` publishes the new status. It does not know who is listening:
- `CustomerSmsNotifier` - customer asked for SMS about one parcel, gets only "out for delivery" and "delivered", then **unsubscribes itself**,
- `ShopWebhookNotifier` - online shop gets every change of every parcel on its webhook,
- `DeliveryTimeAnalytics` - counts average time from pickup to delivery.

New observers (e.g. loyalty points, fraud detection) can be added without touching the tracker.  
This example uses .NET's built-in `IObservable<T>` / `IObserver<T>` interfaces - `Subscribe` returns `IDisposable`, disposing it unsubscribes.  
To test this example, run "**/shipments/demo**" endpoint, or use the shared tracker:  
`POST /shipments/{tracking}/sms` with `{"phone":"+48 500 000 000"}`, `POST /shipments/{tracking}/status` with `{"status":"PickedUp","location":"Warsaw"}`, then `GET /shipments/notifications` and `GET /shipments/analytics`.  
Please look on **ShipmentTracker.cs** and **Observers.cs** to get better view of this implementation.

## Participants

- `IObservable<ShipmentEvent>` / `IObserver<ShipmentEvent>` - **subject** and **observer** interfaces, built into .NET
- `ShipmentTracker` - **concrete subject**, keeps the list of observers and notifies them
- `CustomerSmsNotifier`, `ShopWebhookNotifier`, `DeliveryTimeAnalytics` - **concrete observers**, each decides what is interesting for it
- `ShipmentsController` - courier's scanner and customer subscribing for SMS

### Subject
```csharp
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
```

### Observer
```csharp
public void OnNext(ShipmentEvent value)
{
    if (value.TrackingNumber != _trackingNumber || !Important.Contains(value.Status)) return;

    _log.Add("SMS", $"to {_phone}: Your parcel {_trackingNumber} {text}");

    if (value.Status == ShipmentStatus.Delivered)
        _subscription?.Dispose();   // safe during notification - tracker iterates over a copy
}
```

### Client code
```csharp
// Long living observers subscribe once at startup (Program.cs)
tracker.Subscribe(app.Services.GetRequiredService<DeliveryTimeAnalytics>());
tracker.Subscribe(app.Services.GetRequiredService<ShopWebhookNotifier>());

// Customer's SMS notifier comes and goes at runtime
new CustomerSmsNotifier(trackingNumber, subscription.Phone, _log).SubscribeTo(_tracker);

// Courier's scanner
_tracker.UpdateStatus(trackingNumber, change.Status, change.Location);
```

### Result (`/shipments/demo`)
Two parcels, SMS subscription only for PL123, shop's server is down when PL456 goes to the hub:
```
[Webhook] POST https://fashion-shop.pl/webhooks/shipments { "tracking": "PL123", "status": "PickedUp" }
[Webhook] POST https://fashion-shop.pl/webhooks/shipments { "tracking": "PL456", "status": "PickedUp" }
[Webhook] POST https://fashion-shop.pl/webhooks/shipments { "tracking": "PL123", "status": "InTransit" }
[Webhook] POST https://fashion-shop.pl/webhooks/shipments { "tracking": "PL123", "status": "OutForDelivery" }
[SMS] to +48 600 100 200: Your parcel PL123 is out for delivery today
[Webhook] POST https://fashion-shop.pl/webhooks/shipments { "tracking": "PL123", "status": "Delivered" }
[SMS] to +48 600 100 200: Your parcel PL123 was delivered at 11:15. Thank you!
[Webhook] POST https://fashion-shop.pl/webhooks/shipments { "tracking": "PL456", "status": "OutForDelivery" }
[Webhook] POST https://fashion-shop.pl/webhooks/shipments { "tracking": "PL456", "status": "Delivered" }
```
```json
{
  "observersAtStart": 3,
  "observersAtEnd": 2,
  "failedNotifications": [
    "Webhook https://fashion-shop.pl/webhooks/shipments: POST https://fashion-shop.pl/webhooks/shipments - 503 Service Unavailable"
  ],
  "analytics": { "deliveredShipments": 2, "inProgress": 0, "averageDeliveryHours": "28.1" }
}
```
The failed webhook did not stop analytics from getting PL456's "InTransit", and the SMS notifier is gone after PL123 was delivered.

## Observer in .NET

C# `event` is the observer pattern built into the language, `IObservable<T>` is its interface form, and Reactive Extensions (System.Reactive) adds LINQ on top of it.  
In a real system with many services the same idea becomes a message broker (RabbitMQ, Kafka, Azure Service Bus) - the subject publishes, consumers subscribe.  
Watch out for the classic pitfall: an observer which never unsubscribes is never garbage collected (memory leak via event handlers).
