namespace MechanicShop.Client.Models;

public sealed class PartModel
{
    public Guid PartId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public int Quantity { get; set; }
}

