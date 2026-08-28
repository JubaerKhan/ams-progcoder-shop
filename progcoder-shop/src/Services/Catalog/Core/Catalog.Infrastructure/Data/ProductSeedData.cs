#region using

using Catalog.Domain.Entities;
using Catalog.Domain.Enums;

#endregion

namespace Catalog.Infrastructure.Data;

public static class ProductSeedData
{
    #region Constants

    public const int TargetProductCount = 15;

    #endregion

    #region Methods

    public static ProductEntity[] GetAllProducts(string performedBy)
    {
        return new[]
        {
            InStock(ProductEntity.Create(
                id: Guid.Parse("a1b2c3d4-e5f6-4a1b-9c8d-7e6f5a4b3c21"),
                name: "iPhone 15 Pro",
                sku: "IPHONE-15-PRO-001",
                shortDescription: "Latest iPhone with A17 Pro chip and titanium design",
                longDescription: "The iPhone 15 Pro features a 6.1-inch Super Retina XDR display, A17 Pro chip for exceptional performance, advanced camera system with 48MP main camera, and titanium construction for durability. Available in Natural Titanium, Blue Titanium, White Titanium, and Black Titanium.",
                slug: "iphone-15-pro",
                price: 165000,
                salePrice: 154000,
                categoryIds: new List<Guid> { CategorySeedData.PhonesId, CategorySeedData.ElectronicsId },
                brandId: BrandSeedData.AppleId,
                performedBy: performedBy)),

            ProductEntity.Create(
                id: Guid.Parse("a1b2c3d4-e5f6-4a1b-9c8d-7e6f5a4b3c22"),
                name: "Samsung Galaxy S24 Ultra",
                sku: "SAMSUNG-S24-ULTRA-001",
                shortDescription: "Premium Android flagship with S Pen and advanced AI features",
                longDescription: "The Samsung Galaxy S24 Ultra features a 6.8-inch Dynamic AMOLED 2X display, Snapdragon 8 Gen 3 processor, 200MP main camera with 100x Space Zoom, integrated S Pen, and AI-powered features. Built with titanium frame for premium durability.",
                slug: "samsung-galaxy-s24-ultra",
                price: 155000,
                salePrice: null,
                categoryIds: new List<Guid> { CategorySeedData.PhonesId, CategorySeedData.ElectronicsId },
                brandId: BrandSeedData.SamsungId,
                performedBy: performedBy),

            InStock(ProductEntity.Create(
                id: Guid.Parse("a1b2c3d4-e5f6-4a1b-9c8d-7e6f5a4b3c23"),
                name: "MacBook Pro 16-inch M3 Pro",
                sku: "MACBOOK-PRO-16-M3-001",
                shortDescription: "Powerful laptop for professionals with M3 Pro chip",
                longDescription: "The MacBook Pro 16-inch features the M3 Pro chip with up to 12-core CPU and 19-core GPU, 16.2-inch Liquid Retina XDR display, up to 36GB unified memory, and all-day battery life. Perfect for video editing, software development, and creative work.",
                slug: "macbook-pro-16-inch-m3-pro",
                price: 330000,
                salePrice: null,
                categoryIds: new List<Guid> { CategorySeedData.LaptopsId, CategorySeedData.ElectronicsId },
                brandId: BrandSeedData.AppleId,
                performedBy: performedBy)),

            ProductEntity.Create(
                id: Guid.Parse("a1b2c3d4-e5f6-4a1b-9c8d-7e6f5a4b3c24"),
                name: "Dell XPS 15",
                sku: "DELL-XPS-15-001",
                shortDescription: "Premium Windows laptop with OLED display",
                longDescription: "The Dell XPS 15 features a 15.6-inch OLED 3.5K display, Intel Core i7 processor, NVIDIA RTX graphics, up to 32GB RAM, and premium build quality. Ideal for content creators and professionals.",
                slug: "dell-xps-15",
                price: 255000,
                salePrice: 238000,
                categoryIds: new List<Guid> { CategorySeedData.LaptopsId, CategorySeedData.ElectronicsId },
                brandId: null,
                performedBy: performedBy),

            InStock(ProductEntity.Create(
                id: Guid.Parse("a1b2c3d4-e5f6-4a1b-9c8d-7e6f5a4b3c25"),
                name: "Classic Cotton Dress Shirt",
                sku: "ZARA-SHIRT-MEN-001",
                shortDescription: "Men's classic dress shirt in premium cotton",
                longDescription: "A timeless men's dress shirt cut from 100% breathable cotton with a soft, smooth finish. The regular fit sits comfortably without bunching, making it equally suited to the office and formal occasions. Available in a range of colours and in sizes S through XXL.",
                slug: "classic-cotton-dress-shirt",
                price: 4900,
                salePrice: 3800,
                categoryIds: new List<Guid> { CategorySeedData.MenFashionId, CategorySeedData.FashionId },
                brandId: BrandSeedData.ZaraId,
                performedBy: performedBy)),

            InStock(ProductEntity.Create(
                id: Guid.Parse("a1b2c3d4-e5f6-4a1b-9c8d-7e6f5a4b3c26"),
                name: "Women's Skinny Fit Jeans",
                sku: "ZARA-JEANS-WOMEN-001",
                shortDescription: "High-stretch skinny jeans with a flattering fit",
                longDescription: "Skinny fit women's jeans in a high-stretch denim that holds its shape all day. The clean, modern cut comes in dark wash, light wash and black, making them easy to dress up or down and pair with almost anything in your wardrobe.",
                slug: "womens-skinny-fit-jeans",
                price: 7100,
                salePrice: 5450,
                categoryIds: new List<Guid> { CategorySeedData.WomenFashionId, CategorySeedData.FashionId },
                brandId: BrandSeedData.ZaraId,
                performedBy: performedBy)),

            ProductEntity.Create(
                id: Guid.Parse("a1b2c3d4-e5f6-4a1b-9c8d-7e6f5a4b3c27"),
                name: "Men's Bomber Jacket",
                sku: "MANGO-JACKET-MEN-001",
                shortDescription: "Streetwear bomber jacket with a modern cut",
                longDescription: "A contemporary take on the classic bomber, cut for a relaxed streetwear silhouette. The lightweight water-resistant polyester shell is paired with a warm quilted lining, making it an easy layer for autumn and winter. Offered in several fresh colourways.",
                slug: "mens-bomber-jacket",
                price: 10400,
                salePrice: null,
                categoryIds: new List<Guid> { CategorySeedData.MenFashionId, CategorySeedData.FashionId },
                brandId: BrandSeedData.MangoId,
                performedBy: performedBy),

            InStock(ProductEntity.Create(
                id: Guid.Parse("a1b2c3d4-e5f6-4a1b-9c8d-7e6f5a4b3c28"),
                name: "Women's Running Sneakers",
                sku: "ZARA-SNEAKERS-WOMEN-001",
                shortDescription: "Cushioned running sneakers with a grippy rubber sole",
                longDescription: "Lightweight women's sneakers built for everyday movement. A cushioned insole absorbs impact while the non-slip rubber outsole keeps you steady on wet pavement. Equally at home on a morning run, a long walk or a full day on your feet. Multiple sizes and colours available.",
                slug: "womens-running-sneakers",
                price: 8750,
                salePrice: 7100,
                categoryIds: new List<Guid> { CategorySeedData.WomenFashionId, CategorySeedData.FashionId },
                brandId: BrandSeedData.ZaraId,
                performedBy: performedBy)),

            ProductEntity.Create(
                id: Guid.Parse("a1b2c3d4-e5f6-4a1b-9c8d-7e6f5a4b3c29"),
                name: "Gucci Leather Handbag",
                sku: "GUCCI-HANDBAG-001",
                shortDescription: "Luxury Gucci handbag in genuine leather",
                longDescription: "Crafted from full-grain genuine leather and finished with the signature GG hardware, this Gucci handbag is made to be noticed. A structured silhouette and roomy lined interior make it as practical as it is elegant. Available in several colours and sizes.",
                slug: "gucci-leather-handbag",
                price: 252000,
                salePrice: null,
                categoryIds: new List<Guid> { CategorySeedData.WomenFashionId, CategorySeedData.FashionId },
                brandId: BrandSeedData.GucciId,
                performedBy: performedBy),

            ProductEntity.Create(
                id: Guid.Parse("a1b2c3d4-e5f6-4a1b-9c8d-7e6f5a4b3c30"),
                name: "Solid Wood Writing Desk",
                sku: "DESK-WOOD-001",
                shortDescription: "Natural solid wood desk with a modern profile",
                longDescription: "A writing desk built from solid natural timber and sealed with a protective scratch-resistant lacquer. The clean modern frame includes a handy drawer for cables and stationery, and the surface is sized to fit a monitor and laptop comfortably. Suits both home offices and studios.",
                slug: "solid-wood-writing-desk",
                price: 25300,
                salePrice: 21900,
                categoryIds: new List<Guid> { CategorySeedData.HomeId },
                brandId: null,
                performedBy: performedBy),

            InStock(ProductEntity.Create(
                id: Guid.Parse("a1b2c3d4-e5f6-4a1b-9c8d-7e6f5a4b3c31"),
                name: "Ergonomic Office Chair",
                sku: "CHAIR-ERGONOMIC-001",
                shortDescription: "Ergonomic office chair with proper lumbar support",
                longDescription: "An ergonomic task chair with a contoured lumbar rest and a padded seat that stays comfortable through long sessions. Seat height, backrest tilt and armrests all adjust independently, so you can dial in a posture that reduces fatigue and lower back strain.",
                slug: "ergonomic-office-chair",
                price: 18100,
                salePrice: null,
                categoryIds: new List<Guid> { CategorySeedData.HomeId },
                brandId: null,
                performedBy: performedBy)),

            InStock(ProductEntity.Create(
                id: Guid.Parse("a1b2c3d4-e5f6-4a1b-9c8d-7e6f5a4b3c32"),
                name: "LED Desk Lamp",
                sku: "LAMP-LED-DESK-001",
                shortDescription: "Dimmable LED desk lamp with adjustable arm",
                longDescription: "A flicker-free LED desk lamp designed to light your work without glare. Brightness steps and the angle of the head both adjust, so you can aim light exactly where it is needed. Energy efficient and quietly modern, it suits late-night study and desk work alike.",
                slug: "led-desk-lamp",
                price: 4900,
                salePrice: 3800,
                categoryIds: new List<Guid> { CategorySeedData.HomeId },
                brandId: null,
                performedBy: performedBy)),

            ProductEntity.Create(
                id: Guid.Parse("a1b2c3d4-e5f6-4a1b-9c8d-7e6f5a4b3c33"),
                name: "5-Tier Bookshelf",
                sku: "BOOKSHELF-5-TIER-001",
                shortDescription: "Five-tier engineered wood bookshelf",
                longDescription: "A five-tier shelving unit built from premium MDF and surfaced with moisture-resistant melamine. The pared-back design assembles quickly with the included hardware and holds books, plants and display pieces without crowding a room.",
                slug: "5-tier-bookshelf",
                price: 13700,
                salePrice: null,
                categoryIds: new List<Guid> { CategorySeedData.HomeId },
                brandId: null,
                performedBy: performedBy),

            InStock(ProductEntity.Create(
                id: Guid.Parse("a1b2c3d4-e5f6-4a1b-9c8d-7e6f5a4b3c34"),
                name: "Xiaomi Redmi Note 13 Pro",
                sku: "XIAOMI-REDMI-NOTE-13-001",
                shortDescription: "Budget smartphone with flagship-level performance",
                longDescription: "The Xiaomi Redmi Note 13 Pro pairs a 6.67-inch AMOLED display with the Snapdragon 7s Gen 2 chipset, a 200MP main camera and a 5100mAh battery that refills over 67W fast charging. Serious performance at a price that undercuts the flagships.",
                slug: "xiaomi-redmi-note-13-pro",
                price: 44000,
                salePrice: 38500,
                categoryIds: new List<Guid> { CategorySeedData.PhonesId, CategorySeedData.ElectronicsId },
                brandId: null,
                performedBy: performedBy)),

            InStock(ProductEntity.Create(
                id: Guid.Parse("a1b2c3d4-e5f6-4a1b-9c8d-7e6f5a4b3c35"),
                name: "Men's Crew Neck T-Shirt",
                sku: "ZARA-TSHIRT-MEN-001",
                shortDescription: "Everyday crew neck tee in soft cotton",
                longDescription: "A staple crew neck t-shirt in soft, breathable 100% cotton. The simple cut layers easily and holds its shape after washing. Stocked in the essentials — white, black, grey and navy — and light enough to wear through summer.",
                slug: "mens-crew-neck-t-shirt",
                price: 2700,
                salePrice: null,
                categoryIds: new List<Guid> { CategorySeedData.MenFashionId, CategorySeedData.FashionId },
                brandId: BrandSeedData.ZaraId,
                performedBy: performedBy))
        };
    }

    public static string GetThumbnailUrl(string productName)
    {
        return productName.ToLower() switch
        {
            "iphone 15 pro" => "https://images.unsplash.com/photo-1592750475338-74b7b21085ab?w=800&h=800&fit=crop",
            "samsung galaxy s24 ultra" => "https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?w=800&h=800&fit=crop",
            "macbook pro 16-inch m3 pro" => "https://images.unsplash.com/photo-1541807084-5c52b6b3adef?w=800&h=800&fit=crop",
            "dell xps 15" => "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?w=800&h=800&fit=crop",
            "classic cotton dress shirt" => "https://images.unsplash.com/photo-1642764873654-9eef0467b342?w=800&h=800&fit=crop",
            "women's skinny fit jeans" => "https://images.unsplash.com/photo-1542272604-787c3835535d?w=800&h=800&fit=crop",
            "men's bomber jacket" => "https://images.unsplash.com/photo-1551028719-00167b16eac5?w=800&h=800&fit=crop",
            "women's running sneakers" => "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=800&h=800&fit=crop",
            "gucci leather handbag" => "https://images.unsplash.com/photo-1590874103328-eac38a683ce7?w=800&h=800&fit=crop",
            "solid wood writing desk" => "https://images.unsplash.com/photo-1586023492125-27b2c045efd7?w=800&h=800&fit=crop",
            "ergonomic office chair" => "https://images.unsplash.com/photo-1506439773649-6e0eb8cfb237?w=800&h=800&fit=crop",
            "led desk lamp" => "https://images.unsplash.com/photo-1507473885765-e6ed057f782c?w=800&h=800&fit=crop",
            "5-tier bookshelf" => "https://images.unsplash.com/photo-1586023492125-27b2c045efd7?w=800&h=800&fit=crop",
            "xiaomi redmi note 13 pro" => "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=800&h=800&fit=crop",
            "men's crew neck t-shirt" => "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?w=800&h=800&fit=crop",
            _ => "https://images.unsplash.com/photo-1441986300917-64674bd600d8?w=800&h=800&fit=crop"
        };
    }

    public static List<string> GetProductImages(string productName)
    {
        return productName.ToLower() switch
        {
            "iphone 15 pro" => new List<string>
            {
                "https://images.unsplash.com/photo-1592750475338-74b7b21085ab?w=1200&h=1200&fit=crop",
                "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=1200&h=1200&fit=crop",
                "https://images.unsplash.com/photo-1556656793-08538906a9f8?w=1200&h=1200&fit=crop"
            },
            "samsung galaxy s24 ultra" => new List<string>
            {
                "https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?w=1200&h=1200&fit=crop",
                "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=1200&h=1200&fit=crop",
                "https://images.unsplash.com/photo-1556656793-08538906a9f8?w=1200&h=1200&fit=crop"
            },
            "macbook pro 16-inch m3 pro" => new List<string>
            {
                "https://images.unsplash.com/photo-1541807084-5c52b6b3adef?w=1200&h=1200&fit=crop",
                "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?w=1200&h=1200&fit=crop",
                "https://images.unsplash.com/photo-1525547719571-a2d4ac8945e2?w=1200&h=1200&fit=crop"
            },
            "dell xps 15" => new List<string>
            {
                "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?w=1200&h=1200&fit=crop",
                "https://images.unsplash.com/photo-1541807084-5c52b6b3adef?w=1200&h=1200&fit=crop",
                "https://images.unsplash.com/photo-1525547719571-a2d4ac8945e2?w=1200&h=1200&fit=crop"
            },
            "classic cotton dress shirt" => new List<string>
            {
                "https://images.unsplash.com/photo-1594938291221-94f18b6fa0e1?w=1200&h=1200&fit=crop",
                "https://images.unsplash.com/photo-1622445275576-721325763afe?w=1200&h=1200&fit=crop",
                "https://images.unsplash.com/photo-1603252109303-2751441dd157?w=1200&h=1200&fit=crop"
            },
            "women's skinny fit jeans" => new List<string>
            {
                "https://images.unsplash.com/photo-1542272604-787c3835535d?w=1200&h=1200&fit=crop",
                "https://images.unsplash.com/photo-1582418702059-97ebafbcdb1d?w=1200&h=1200&fit=crop",
                "https://images.unsplash.com/photo-1541099649105-f69ad21f3246?w=1200&h=1200&fit=crop"
            },
            "men's bomber jacket" => new List<string>
            {
                "https://images.unsplash.com/photo-1551028719-00167b16eac5?w=1200&h=1200&fit=crop",
                "https://images.unsplash.com/photo-1539533018447-63fcce2678e3?w=1200&h=1200&fit=crop",
                "https://images.unsplash.com/photo-1556821840-3a63f95609a7?w=1200&h=1200&fit=crop"
            },
            "women's running sneakers" => new List<string>
            {
                "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=1200&h=1200&fit=crop",
                "https://images.unsplash.com/photo-1460353581641-37baddab0fa2?w=1200&h=1200&fit=crop",
                "https://images.unsplash.com/photo-1606107557195-0e29a4b5b4aa?w=1200&h=1200&fit=crop"
            },
            "gucci leather handbag" => new List<string>
            {
                "https://images.unsplash.com/photo-1590874103328-eac38a683ce7?w=1200&h=1200&fit=crop",
                "https://images.unsplash.com/photo-1553062407-98eeb64c6a62?w=1200&h=1200&fit=crop",
                "https://images.unsplash.com/photo-1594223274512-ad4803739b7c?w=1200&h=1200&fit=crop"
            },
            "solid wood writing desk" => new List<string>
            {
                "https://images.unsplash.com/photo-1586023492125-27b2c045efd7?w=1200&h=1200&fit=crop",
                "https://images.unsplash.com/photo-1532372320572-cda25653a26d?w=1200&h=1200&fit=crop",
                "https://images.unsplash.com/photo-1581539250439-c96689b516dd?w=1200&h=1200&fit=crop"
            },
            "ergonomic office chair" => new List<string>
            {
                "https://images.unsplash.com/photo-1506439773649-6e0eb8cfb237?w=1200&h=1200&fit=crop",
                "https://images.unsplash.com/photo-1586023492125-27b2c045efd7?w=1200&h=1200&fit=crop",
                "https://images.unsplash.com/photo-1532372320572-cda25653a26d?w=1200&h=1200&fit=crop"
            },
            "led desk lamp" => new List<string>
            {
                "https://images.unsplash.com/photo-1507473885765-e6ed057f782c?w=1200&h=1200&fit=crop",
                "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=1200&h=1200&fit=crop",
                "https://images.unsplash.com/photo-1526170375885-4d8ecf77b99f?w=1200&h=1200&fit=crop"
            },
            "5-tier bookshelf" => new List<string>
            {
                "https://images.unsplash.com/photo-1586023492125-27b2c045efd7?w=1200&h=1200&fit=crop",
                "https://images.unsplash.com/photo-1532372320572-cda25653a26d?w=1200&h=1200&fit=crop",
                "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=1200&h=1200&fit=crop"
            },
            "xiaomi redmi note 13 pro" => new List<string>
            {
                "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=1200&h=1200&fit=crop",
                "https://images.unsplash.com/photo-1592750475338-74b7b21085ab?w=1200&h=1200&fit=crop",
                "https://images.unsplash.com/photo-1556656793-08538906a9f8?w=1200&h=1200&fit=crop"
            },
            "men's crew neck t-shirt" => new List<string>
            {
                "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?w=1200&h=1200&fit=crop",
                "https://images.unsplash.com/photo-1594938291221-94f18b6fa0e1?w=1200&h=1200&fit=crop",
                "https://images.unsplash.com/photo-1622445275576-721325763afe?w=1200&h=1200&fit=crop"
            },
            _ => new List<string>
            {
                "https://images.unsplash.com/photo-1441986300917-64674bd600d8?w=1200&h=1200&fit=crop",
                "https://images.unsplash.com/photo-1441986300917-64674bd600d8?w=1200&h=1200&fit=crop",
                "https://images.unsplash.com/photo-1441986300917-64674bd600d8?w=1200&h=1200&fit=crop"
            }
        };
    }

    public static void AddProductImages(ProductEntity product)
    {
        var thumbnail = new ProductImageEntity
        {
            PublicURL = GetThumbnailUrl(product.Name!),
            FileName = $"{product.Slug}-thumbnail.jpg",
            OriginalFileName = $"{product.Slug}-thumbnail.jpg"
        };

        var images = GetProductImages(product.Name!).Select(url => new ProductImageEntity
        {
            PublicURL = url,
            FileName = $"{product.Slug}-{Guid.NewGuid()}.jpg",
            OriginalFileName = $"{product.Slug}-{Guid.NewGuid()}.jpg"
        }).ToList();

        product.AddOrUpdateThumbnail(thumbnail);
        product.AddOrUpdateImages(images);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Seed helper: products are created OutOfStock by default, since stock is normally
    /// owned by the Inventory service. This marks a seeded product as available so the
    /// storefront has a realistic mix without requiring Inventory to be running.
    /// </summary>
    private static ProductEntity InStock(ProductEntity product)
    {
        product.Status = ProductStatus.InStock;
        return product;
    }

    #endregion
}
