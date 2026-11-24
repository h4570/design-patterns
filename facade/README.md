# Facade pattern

In this example facade pattern was used to hide a complex **e-commerce checkout process** behind one method.  
Placing an order needs five subsystems: *inventory*, *payment gateway*, *shipping*, *invoicing* and *notifications*.  
They have to be called in the right order, and when payment is declined, reserved products have to go back to the warehouse.  
`CheckoutFacade` knows all of that, so the client (here `CheckoutController`) only calls `PlaceOrder()`.  
To test this example, run "**/checkout/demo**", "**/checkout/demo-declined**" and "**/checkout/stock**" endpoints, or send your own order with `POST /checkout`.  
Please look on **CheckoutFacade.cs** to get better view of this implementation.

## Facade vs Adapter

Adapter changes the interface of **one** object, so it fits an interface expected by the client.  
Facade defines a **new, simpler** interface for a **whole group** of objects.  
Subsystems are still public - facade does not forbid using them directly (controller uses `InventoryService` for `/checkout/stock`).

### Facade implementation
```csharp
public CheckoutResult PlaceOrder(OrderRequest request)
{
    // 1. Reserve products, so nobody else can buy them during payment
    reservationId = _inventory.Reserve(request.Lines);

    // 2. Calculate totals
    var productsTotal = request.Lines.Sum(c => _inventory.GetPrice(c.Sku) * c.Quantity);
    var shippingCost = _shipping.Quote(request.ShippingAddress, productsTotal);

    // 3. Charge customer - on failure give products back to the warehouse
    try { paymentId = _payments.Charge(request.CardNumber, total, $"Order {orderId}"); }
    catch (PaymentDeclinedException ex)
    {
        _inventory.Release(reservationId);
        _notifications.SendPaymentFailed(request.CustomerEmail, orderId, ex.Message);
        return CheckoutResult.Failed(orderId, ex.Message);
    }

    // 4. Payment is done - finalize everything else
    _inventory.Commit(reservationId);
    var trackingNumber = _shipping.CreateShipment(orderId, request.ShippingAddress);
    var invoiceNumber = _invoices.Issue(orderId, request.ShippingAddress, total);
    _notifications.SendOrderConfirmation(request.CustomerEmail, orderId, trackingNumber, invoiceNumber);
    // ...
}
```

### Client code
```csharp
[HttpPost]
public ActionResult<CheckoutResult> PlaceOrder(OrderRequest request)
{
    var result = _checkout.PlaceOrder(request);
    return result.Success ? Ok(result) : UnprocessableEntity(result);
}
```

### Result
```json
{
  "success": true,
  "orderId": "ORD-20261124-2858",
  "productsTotal": 757.00,
  "shippingCost": 0,
  "total": 757.00,
  "paymentId": "PAY-8E1B9FDB",
  "trackingNumber": "TRK478314505PL",
  "invoiceNumber": "FV/0001/11/2026",
  "error": null
}
```
