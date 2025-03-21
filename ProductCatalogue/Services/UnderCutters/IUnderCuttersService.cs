using System;

namespace ProductCatalogue.Services.UnderCutters;

public interface IUnderCuttersService
{
    Task<IEnumerable<ProductDto>> GetProductsAsync();

}