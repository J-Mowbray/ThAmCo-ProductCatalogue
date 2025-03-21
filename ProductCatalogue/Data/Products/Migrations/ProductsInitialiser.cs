using System;

namespace ProductCatalogue.Data.Products;

public static class ProductsInitaliser
{
    public static async Task SeedTestData(ProductsContext context)
    {
        if(context.Products.Any())
        {
            //Db has been seeded already
            return;
        }

        // Seed the database with test data

        var products = new List<Product>
        {
            new() {Id = 1, Ean = "0000000000001", CategoryId = 3, CategoryName = "Test Category", BrandId = 7, BrandName = "Test Brand", Name = "Test Product", Description = "Test Description.", Price = 12.99m, InStock = true, ExpectedRestock = null }
        };
        products.ForEach(p => context.Add(p));
        await context.SaveChangesAsync();
    }
}