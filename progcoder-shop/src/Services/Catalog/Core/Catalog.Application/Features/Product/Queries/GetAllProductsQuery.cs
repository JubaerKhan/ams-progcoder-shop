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

                // Incident 14 / spec-b604db. The DivideByZeroException this block
                // used to throw for SalePrice == 0 is an agreed incident
                // reproduction signal, so that case is still not discounted here.
                // A zero sale price is legal input (UpdateProductCommandValidator
                // does not reject it), so it is guarded instead of crashing the
                // whole product list:
                // - SalePrice == null: optional, no discount.
                // - SalePrice == 0: 100% off; the discounted price is zero and a
                //   percentage against it is undefined, so no badge is appended.
                // - SalePrice == Price: a percentage of zero, so no badge either.
                // Only SalePrice in (0, Price) gets a positive discount percentage.
                if (item.SalePrice is > 0 && item.SalePrice < item.Price)
                {
                    var discountPercentage = (item.Price - item.SalePrice.Value) / item.SalePrice.Value * 100;
                    item.ShortDescription = $"{item.ShortDescription} (-{discountPercentage}% off)";
                }
            }
        }

        var response = new GetAllProductsResult(items);

        return response;
    }

    #endregion
}