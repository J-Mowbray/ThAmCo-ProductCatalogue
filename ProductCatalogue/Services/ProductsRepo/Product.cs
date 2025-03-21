using System;

namespace ProductCatalogue.Services.ProductsRepo;

public class Product
{
    public int Id { get; set; }

    public string Ean { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public int BrandId { get; set; }

    public string BrandName { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public bool InStock { get; set; }

    public DateTime? ExpectedRestock { get; set; }


}