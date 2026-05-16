namespace ShopFresherz.Domain.Enums;

/// <summary>Delivery options available at checkout.</summary>
public enum DeliveryMethod
{
    /// <summary>Standard delivery 3–5 business days. Free above ₦50,000 subtotal.</summary>
    Standard = 0,

    /// <summary>Express delivery 1–2 business days. ₦3,500 flat fee.</summary>
    Express = 1,

    /// <summary>Customer collects from store. Free.</summary>
    Pickup = 2
}
