public class Supplier : AuditableEntity
{
    public string Name { get; set; }
    public string Contact { get; set; }
    public string Address { get; set; }
    public decimal AccountsPayable { get; set; }
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; }
}
