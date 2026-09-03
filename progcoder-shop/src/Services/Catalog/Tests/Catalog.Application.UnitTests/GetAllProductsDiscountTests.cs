#region using

using Catalog.Application.Features.Product.Queries;
using Xunit;

#endregion

namespace Catalog.Application.UnitTests;

public class GetAllProductsDiscountTests
{
    [Theory]
    [InlineData(4900d, 0d)]     // incident 2: zero sale price must not divide by zero
    [InlineData(4900d, null)]   // no sale price at all
    [InlineData(0d, 100d)]      // zero regular price
    [InlineData(-5d, 100d)]     // negative regular price
    [InlineData(4900d, 4900d)]  // sale equals regular: no saving, no badge
    [InlineData(4900d, 5000d)]  // sale above regular: not a discount
    public void Returns_zero_when_there_is_no_genuine_discount(double price, double? salePrice)
    {
        var result = GetAllProductsQueryHandler.GetDiscountPercentage((decimal)price, (decimal?)salePrice);

        Assert.Equal(0, result);
    }

    [Fact]
    public void Computes_discount_as_a_share_of_the_regular_price()
    {
        Assert.Equal(22, GetAllProductsQueryHandler.GetDiscountPercentage(4900m, 3800m));
        Assert.Equal(7, GetAllProductsQueryHandler.GetDiscountPercentage(165000m, 154000m));
        Assert.Equal(50, GetAllProductsQueryHandler.GetDiscountPercentage(100m, 50m));
    }

    [Fact]
    public void Handles_a_tiny_nonzero_sale_price_without_an_absurd_badge()
    {
        // The old code divided by SalePrice, so a very small but nonzero sale
        // price produced a huge badge percentage. The share of the regular
        // price is capped at 100 by construction, which is the honest
        // description of "almost free".
        Assert.Equal(100, GetAllProductsQueryHandler.GetDiscountPercentage(100, 0.0001m));
        Assert.Equal(100, GetAllProductsQueryHandler.GetDiscountPercentage(100, 0.01m));
    }
}
