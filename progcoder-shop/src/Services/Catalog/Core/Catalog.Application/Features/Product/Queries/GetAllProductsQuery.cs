#region using

using AutoMapper;
using Catalog.Application.Dtos.Products;
using Catalog.Application.Models.Filters;
using Catalog.Application.Models.Results;
using Catalog.Domain.Entities;
using Marten;

#endregion

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

                // Seeded incident 2 (see DEV-RUNBOOK.md "Seeded incidents"). The zero
                // SalePrice product itself stays seeded and reproducible; only the
                // unguarded division by it was the fault. Per spec-b604db the endpoint
                // must keep rendering a badge for every genuinely discounted product
                // instead of throwing for the whole list.
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
    ///     Discount badge percentage as a share of the regular price. Returns 0 when
    ///     there is no genuine saving: no sale price, a zero sale price (the seeded
    ///     incident 2 product), a non-positive regular price, or a sale price at or
    ///     above the regular price.
    /// </summary>
    public static int GetDiscountPercentage(decimal price, decimal? salePrice) =>
        price <= 0 || salePrice is null or <= 0 || salePrice >= price
            ? 0
            : (int)Math.Round((price - salePrice.Value) / price * 100, MidpointRounding.AwayFromZero);

    #endregion
}