// In ProductCatalogue/Data/Products/Product.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductCatalogue.Data.Products;

public class Product
{
    // Don't use DatabaseGenerated attribute - we handle this in the context configuration
    public int Id { get; set; }

    public string? Ean { get; set; }

    public int CategoryId { get; set; }

    public string? CategoryName { get; set; }

    public int BrandId { get; set; }

    public string? BrandName { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public bool InStock { get; set; }

    public DateTime? ExpectedRestock { get; set; }
}