namespace ExcelImporter.WebApi;

/// <summary>
/// Representing a product.
/// </summary>
public class Product
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public DateTime ExpiryDate { get; set; }
}