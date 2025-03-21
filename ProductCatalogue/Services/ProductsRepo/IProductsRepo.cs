using System;

namespace ProductCatalogue.Services.ProductsRepo;

public interface IProductsRepo
{
    Task<IEnumerable<Product>> GetProductsAsync();

}