using OrderManagement.Domain.Common;

namespace OrderManagement.Domain.Entities.Products;

public sealed class Product : Entity
{
    private Product() { }

    public string Sku { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }

    public static Product Create(string sku, string name, decimal unitPrice)
    {
        string validatedSku = DomainGuard.RequiredText(sku, 64, nameof(Sku));
        string validatedName = DomainGuard.RequiredText(name, 200, nameof(Name));
        DomainGuard.PositiveMoney(unitPrice, nameof(UnitPrice));

        return new Product { Sku = validatedSku, Name = validatedName, UnitPrice = unitPrice };
    }

    public void ChangePrice(decimal unitPrice)
    {
        DomainGuard.PositiveMoney(unitPrice, nameof(UnitPrice));
        UnitPrice = unitPrice;
    }
}
