#region using

using System.Runtime.CompilerServices;
using AutoMapper;
using Catalog.Application.Dtos.Products;
using Catalog.Application.Models.Filters;
using Catalog.Application.Models.Results;
using Catalog.Domain.Entities;
using Marten;

#endregion

[assembly: InternalsVisibleTo("Catalog.Application.UnitTests")]

namespace Catalog.Application.Features.Product.Queries;

public sealed record GetAllProductsQuery(GetAllProductsFilter Filter) : IQuery<GetAllProductsResult>;

public sealed class GetAllProductsQueryHandler(IDocumentSession session, IMapper mapper)
    : IQueryHandler<GetAllProductsQuery, GetAllProductsResult>
{
    #region Implementations

    public async Task<GetAllProductsResult> Handle(GetAllProductsQuery query, CancellationToken cancellationToken)
    {
        var filter = query.Filter;
        var productQuery = session.Query<ProductEntity>().AsQueryable();

        if (!filter.SearchText.IsNullOrWhiteSpace())
        {
            var search = filter.SearchText.Trim();
            productQuery = productQuery.Where(x => x.Name != null && x.Name.Contains(search));
        }
        if (filter.Ids?.Length > 0)
        {
            productQuery = productQuery.Where(x => filter.Ids.Contains(x.Id));
        }

        var result = await productQuery
            .OrderByDescending(x => x.CreatedOnUtc)
            .ToListAsync(cancellationToken);

        var items = mapper.Map<List<ProductDto>>(result);

        if (items.Count > 0)
        {
            var categories = await session.Query<CategoryEntity>()
            .ToListAsync(cancellationToken);
            var brands = await session.Query<BrandEntity>()
                .ToListAsync(cancellationToken);

            foreach (var item in items)
            {
                var product = result.FirstOrDefault(p => p.Id == item.Id);

                if (product == null) continue;

                if (product.CategoryIds != null && product.CategoryIds.Count > 0)
                {
                    foreach (var categoryId in product.CategoryIds)
                    {
                        var category = categories.FirstOrDefault(c => c.Id == categoryId);
                        if (category != null)
                        {
                            item.CategoryNames ??= [];
                            item.CategoryNames.Add(category.Name!);
                            item.CategoryIds ??= [];
                            item.CategoryIds.Add(category.Id);
                        }
                    }
                }

                if (product.BrandId.HasValue)
                {
                    var brand = brands.FirstOrDefault(b => b.Id == product.BrandId.Value);
                    if (brand != null)
                    {
                        item.BrandName = brand.Name;
                        item.BrandId = brand.Id;
                    }
                }

                // Incident 188: this badge used to divide by SalePrice, so any product
                // with SalePrice = 0 (an optional field with no validation) threw
                // DivideByZeroException and failed the whole list response with a 500.
                // The discount is now computed against the regular Price, which is a
                // required, always-positive field, and only a genuine saving is badged.
                var discountPercentage = GetDiscountPercentage(item.Price, item.SalePrice);

                if (discountPercentage > 0)
                {
                    item.ShortDescription = $"{item.ShortDescription} (-{discountPercentage}% off)";
                }
            }
        }

        var response = new GetAllProductsResult(items);

        return response;
    }

    /// <summary>
    /// Discount between the regular price and an optional sale price, as a whole
    /// percentage of the regular price. Returns 0 (no badge) when there is no sale
    /// price, when it is zero or free, when the regular price is zero, or when the
    /// sale price is not lower than the regular price.
    /// </summary>
    internal static int GetDiscountPercentage(decimal price, decimal? salePrice)
    {
        if (salePrice is not > 0 || price <= 0 || salePrice >= price)
        {
            return 0;
        }

        var discount = (price - salePrice.Value) / price * 100;

        return (int)Math.Round(discount, MidpointRounding.AwayFromZero);
    }

    #endregion
}