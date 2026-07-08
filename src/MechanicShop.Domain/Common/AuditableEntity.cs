namespace MechanicShop.Domain.Common;

public abstract class AuditableEntity : Entity
{
    public string? CreatedBy { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTimeOffset LastModifiedAtUtc { get; set; }

    public AuditableEntity() { }

    protected AuditableEntity(Guid id)
        : base(id) { }
}
