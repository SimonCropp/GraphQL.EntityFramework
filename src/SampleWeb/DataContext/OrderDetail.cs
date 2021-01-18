using System.ComponentModel.DataAnnotations.Schema;

public class OrderDetail
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }

    public StreetAddress ShippingAddress { get; set; } = null!;
    public StreetAddress BillingAddress { get; set; } = null!;
}