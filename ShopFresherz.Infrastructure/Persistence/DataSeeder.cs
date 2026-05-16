using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShopFresherz.Domain.Entities;
using ShopFresherz.Domain.Enums;

namespace ShopFresherz.Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedAsync(ShopFresherzDbContext context)
    {
        try { await SeedCategoriesAsync(context); } catch { }
        try { await SeedAdminUserAsync(context); } catch { }
        try { await SeedBrandsAsync(context); } catch { }
        try { await SeedProductsAsync(context); } catch { }
        try { await SeedFlashDealAsync(context); } catch { }
        try { await SeedBannersAsync(context); } catch { }
        try { await SeedCouponsAsync(context); } catch { }
        try { await SeedReviewsAsync(context); } catch { }
    }

    private static async Task SeedCategoriesAsync(ShopFresherzDbContext context)
    {
        if (await context.Categories.AnyAsync()) return;

        List<Category> categories = new()
        {
            new() { Name="New Arrivals",         Slug="new-arrivals",          SortOrder=1,  IsActive=true },
            new() { Name="Mobile Phones",         Slug="mobile-phones",         SortOrder=2,  IsActive=true },
            new() { Name="Tablets",               Slug="tablets",               SortOrder=3,  IsActive=true },
            new() { Name="Laptops & Computers",   Slug="laptops-computers",     SortOrder=4,  IsActive=true },
            new() { Name="Smart Watches",         Slug="smart-watches",         SortOrder=5,  IsActive=true },
            new() { Name="Accessories",           Slug="accessories",           SortOrder=6,  IsActive=true },
            new() { Name="Games & Consoles",      Slug="games-consoles",        SortOrder=7,  IsActive=true },
            new() { Name="Electronics",           Slug="electronics",           SortOrder=8,  IsActive=true },
            new() { Name="Computing Accessories", Slug="computing-accessories", SortOrder=9,  IsActive=true },
            new() { Name="Home & Kitchen Tech",   Slug="home-kitchen-tech",     SortOrder=10, IsActive=true },
            new() { Name="Cables",                Slug="cables",                SortOrder=11, IsActive=true },
            new() { Name="Musical Equipment",     Slug="musical-equipment",     SortOrder=12, IsActive=true },
            new() { Name="Romoss",                Slug="romoss",                SortOrder=13, IsActive=true },
            new() { Name="Computer Mouse",        Slug="computer-mouse",        SortOrder=14, IsActive=true },
            new() { Name="Other Categories",      Slug="other-categories",      SortOrder=15, IsActive=true },
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
    }

    private static async Task SeedAdminUserAsync(ShopFresherzDbContext context)
    {
        if (await context.Users.AnyAsync(u => u.Role == UserRole.SuperAdmin)) return;

        User admin = new()
        {
            Id           = Guid.NewGuid(),
            Email        = "admin@shopfresherz.com",
            Phone        = "+2349075308722",
            FirstName    = "Shop",
            LastName     = "Admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@ShopFresherz2026!"),
            Role         = UserRole.SuperAdmin,
            IsVerified   = true,
            CreatedAt    = DateTime.UtcNow,
        };

        await context.Users.AddAsync(admin);
        await context.SaveChangesAsync();
    }

    private static async Task SeedBrandsAsync(ShopFresherzDbContext context)
    {
        if (await context.Brands.AnyAsync()) return;

        List<Brand> brands = new()
        {
            new() { Name = "Apple",    Slug = "apple",    IsActive = true },
            new() { Name = "Samsung",  Slug = "samsung",  IsActive = true },
            new() { Name = "Tecno",    Slug = "tecno",    IsActive = true },
            new() { Name = "Infinix",  Slug = "infinix",  IsActive = true },
            new() { Name = "HP",       Slug = "hp",       IsActive = true },
            new() { Name = "Dell",     Slug = "dell",     IsActive = true },
            new() { Name = "Sony",     Slug = "sony",     IsActive = true },
            new() { Name = "JBL",      Slug = "jbl",      IsActive = true },
            new() { Name = "Romoss",   Slug = "romoss",   IsActive = true },
            new() { Name = "Logitech", Slug = "logitech", IsActive = true },
        };

        await context.Brands.AddRangeAsync(brands);
        await context.SaveChangesAsync();
    }

    private static async Task SeedProductsAsync(ShopFresherzDbContext context)
    {
        if (await context.Products.CountAsync() >= 5) return;

        Dictionary<string, Guid> brandIds = await context.Brands
            .ToDictionaryAsync(b => b.Slug, b => b.Id);

        Dictionary<string, int> categoryIds = await context.Categories
            .ToDictionaryAsync(c => c.Slug, c => c.Id);

        List<Product> products = new()
        {
            // MOBILE PHONES
            new Product
            {
                SKU = "IP16PM-256-BLK",
                Name = "Apple iPhone 16 Pro Max 256GB",
                Slug = "apple-iphone-16-pro-max-256gb",
                BrandId = brandIds["apple"],
                CategoryId = categoryIds["mobile-phones"],
                Description = "The most powerful iPhone ever. A18 Pro chip, 48MP camera system, titanium design.",
                ShortDescription = "A18 Pro chip, 48MP camera, titanium build",
                Price = 1850000,
                CompareAtPrice = 1999000,
                StockQty = 25,
                IsActive = true,
                IsFeatured = true,
                AverageRating = 4.8m,
                ReviewCount = 124,
                SoldCount = 89,
                MetaTitle = "Apple iPhone 16 Pro Max 256GB - ShopFresherz",
                MetaDescription = "Buy iPhone 16 Pro Max in Nigeria. A18 Pro chip, 48MP ProRAW camera.",
                WeightKg = 0.227m,
                AttributesJson = "{\"display\":\"6.9 inch Super Retina XDR\",\"ram\":\"8GB\",\"storage\":\"256GB\",\"battery\":\"4685mAh\",\"network\":\"5G\",\"os\":\"iOS 18\",\"color\":\"Black Titanium\"}"
            },
            new Product
            {
                SKU = "SGS25U-512-TIT",
                Name = "Samsung Galaxy S25 Ultra 512GB",
                Slug = "samsung-galaxy-s25-ultra-512gb",
                BrandId = brandIds["samsung"],
                CategoryId = categoryIds["mobile-phones"],
                Description = "Galaxy AI on the most powerful Galaxy ever. Snapdragon 8 Elite, built-in S Pen.",
                ShortDescription = "Snapdragon 8 Elite, S Pen, 200MP camera",
                Price = 1650000,
                CompareAtPrice = 1800000,
                StockQty = 18,
                IsActive = true,
                IsFeatured = true,
                AverageRating = 4.7m,
                ReviewCount = 98,
                SoldCount = 67,
                MetaTitle = "Samsung Galaxy S25 Ultra 512GB - ShopFresherz",
                MetaDescription = "Buy Samsung Galaxy S25 Ultra in Nigeria.",
                WeightKg = 0.218m,
                AttributesJson = "{\"display\":\"6.9 inch QHD+ Dynamic AMOLED\",\"ram\":\"12GB\",\"storage\":\"512GB\",\"battery\":\"5000mAh\",\"network\":\"5G\",\"os\":\"Android 15\",\"color\":\"Titanium Silver\"}"
            },
            new Product
            {
                SKU = "TECNO-CAM20-128",
                Name = "Tecno Camon 20 Pro 128GB",
                Slug = "tecno-camon-20-pro-128gb",
                BrandId = brandIds["tecno"],
                CategoryId = categoryIds["mobile-phones"],
                Description = "Professional photography smartphone with 50MP RGBW camera and 6.67 inch AMOLED display.",
                ShortDescription = "50MP RGBW camera, AMOLED display",
                Price = 189000,
                CompareAtPrice = 220000,
                StockQty = 45,
                IsActive = true,
                IsFeatured = false,
                AverageRating = 4.2m,
                ReviewCount = 67,
                SoldCount = 134,
                MetaTitle = "Tecno Camon 20 Pro 128GB - ShopFresherz",
                MetaDescription = "Buy Tecno Camon 20 Pro in Nigeria at best price.",
                WeightKg = 0.192m,
                AttributesJson = "{\"display\":\"6.67 inch AMOLED\",\"ram\":\"8GB\",\"storage\":\"128GB\",\"battery\":\"5000mAh\",\"network\":\"4G\",\"os\":\"Android 13\",\"color\":\"Serenity Blue\"}"
            },

            // LAPTOPS & COMPUTERS
            new Product
            {
                SKU = "MBA-M3-16-512",
                Name = "Apple MacBook Air 15 M3 16GB 512GB",
                Slug = "apple-macbook-air-15-m3-16gb-512gb",
                BrandId = brandIds["apple"],
                CategoryId = categoryIds["laptops-computers"],
                Description = "Supercharged by M3 chip. Up to 18 hours battery, 15.3 inch Liquid Retina display.",
                ShortDescription = "M3 chip, 18hr battery, 15.3 inch Retina",
                Price = 2100000,
                CompareAtPrice = 2299000,
                StockQty = 12,
                IsActive = true,
                IsFeatured = true,
                AverageRating = 4.9m,
                ReviewCount = 45,
                SoldCount = 28,
                MetaTitle = "Apple MacBook Air 15 M3 - ShopFresherz",
                MetaDescription = "Buy MacBook Air M3 in Nigeria. Best price guaranteed.",
                WeightKg = 1.51m,
                AttributesJson = "{\"display\":\"15.3 inch Liquid Retina\",\"ram\":\"16GB\",\"storage\":\"512GB SSD\",\"processor\":\"Apple M3\",\"battery\":\"18 hours\",\"os\":\"macOS Sonoma\",\"ports\":\"2x Thunderbolt 3, MagSafe 3\"}"
            },
            new Product
            {
                SKU = "DELL-XPS15-I7-1TB",
                Name = "Dell XPS 15 Intel i7 32GB 1TB",
                Slug = "dell-xps-15-intel-i7-32gb-1tb",
                BrandId = brandIds["dell"],
                CategoryId = categoryIds["laptops-computers"],
                Description = "Premium 15.6 inch laptop with OLED touch display, Intel Core i7, NVIDIA RTX 4060.",
                ShortDescription = "i7-13700H, RTX 4060, OLED touch display",
                Price = 1450000,
                CompareAtPrice = 1600000,
                StockQty = 8,
                IsActive = true,
                IsFeatured = false,
                AverageRating = 4.6m,
                ReviewCount = 32,
                SoldCount = 19,
                MetaTitle = "Dell XPS 15 i7 32GB 1TB - ShopFresherz",
                MetaDescription = "Buy Dell XPS 15 in Nigeria. Premium OLED laptop.",
                WeightKg = 1.86m,
                AttributesJson = "{\"display\":\"15.6 inch OLED Touch\",\"ram\":\"32GB DDR5\",\"storage\":\"1TB NVMe SSD\",\"processor\":\"Intel Core i7-13700H\",\"gpu\":\"NVIDIA RTX 4060 8GB\",\"battery\":\"86Whr\",\"os\":\"Windows 11 Pro\"}"
            },

            // GAMING
            new Product
            {
                SKU = "PS5-SLIM-DISC",
                Name = "Sony PlayStation 5 Slim Disc Edition",
                Slug = "sony-playstation-5-slim-disc-edition",
                BrandId = brandIds["sony"],
                CategoryId = categoryIds["games-consoles"],
                Description = "Experience lightning-fast loading with an ultra-high speed SSD, deeper immersion with haptic feedback.",
                ShortDescription = "PS5 Slim, disc drive, 1TB SSD",
                Price = 680000,
                CompareAtPrice = 750000,
                StockQty = 15,
                IsActive = true,
                IsFeatured = true,
                AverageRating = 4.9m,
                ReviewCount = 203,
                SoldCount = 178,
                MetaTitle = "Sony PlayStation 5 Slim - ShopFresherz",
                MetaDescription = "Buy PS5 Slim in Nigeria. Official Sony PlayStation 5.",
                WeightKg = 3.2m,
                AttributesJson = "{\"storage\":\"1TB SSD\",\"optical\":\"Ultra HD Blu-ray\",\"resolution\":\"4K 120fps\",\"network\":\"WiFi 6\",\"color\":\"White\",\"controllers_included\":\"1x DualSense\"}"
            },

            // ACCESSORIES
            new Product
            {
                SKU = "AIRPODS-PRO2-USB",
                Name = "Apple AirPods Pro 2nd Gen USB-C",
                Slug = "apple-airpods-pro-2nd-gen-usbc",
                BrandId = brandIds["apple"],
                CategoryId = categoryIds["accessories"],
                Description = "Active Noise Cancellation, Adaptive Audio, and Personalised Spatial Audio.",
                ShortDescription = "ANC, Adaptive Audio, USB-C charging",
                Price = 320000,
                CompareAtPrice = 369000,
                StockQty = 30,
                IsActive = true,
                IsFeatured = true,
                AverageRating = 4.8m,
                ReviewCount = 156,
                SoldCount = 243,
                MetaTitle = "Apple AirPods Pro 2 USB-C - ShopFresherz",
                MetaDescription = "Buy AirPods Pro 2 in Nigeria. Active noise cancellation.",
                WeightKg = 0.061m,
                AttributesJson = "{\"type\":\"In-ear TWS\",\"anc\":\"Yes - Active Noise Cancellation\",\"battery\":\"6hrs + 30hrs case\",\"charging\":\"USB-C / MagSafe\",\"water_resistance\":\"IPX4\",\"connectivity\":\"Bluetooth 5.3\"}"
            },
            new Product
            {
                SKU = "JBL-CHARGE5-BLK",
                Name = "JBL Charge 5 Portable Speaker Black",
                Slug = "jbl-charge-5-portable-speaker-black",
                BrandId = brandIds["jbl"],
                CategoryId = categoryIds["accessories"],
                Description = "Massive sound with a powerful built-in powerbank. IP67 waterproof and dustproof.",
                ShortDescription = "IP67 waterproof, 20hr battery, powerbank",
                Price = 89000,
                CompareAtPrice = 110000,
                StockQty = 22,
                IsActive = true,
                IsFeatured = false,
                AverageRating = 4.5m,
                ReviewCount = 89,
                SoldCount = 167,
                MetaTitle = "JBL Charge 5 Portable Speaker - ShopFresherz",
                MetaDescription = "Buy JBL Charge 5 in Nigeria. Waterproof Bluetooth speaker.",
                WeightKg = 0.96m,
                AttributesJson = "{\"type\":\"Portable Bluetooth Speaker\",\"battery\":\"20 hours\",\"waterproof\":\"IP67\",\"powerbank\":\"Yes\",\"connectivity\":\"Bluetooth 5.1\",\"color\":\"Black\"}"
            },

            // SMART WATCHES
            new Product
            {
                SKU = "AW-S10-45-BLK",
                Name = "Apple Watch Series 10 45mm Black",
                Slug = "apple-watch-series-10-45mm-black",
                BrandId = brandIds["apple"],
                CategoryId = categoryIds["smart-watches"],
                Description = "Thinnest Apple Watch ever. Advanced health sensors, crash detection, sleep apnea notifications.",
                ShortDescription = "Thinnest design, health sensors, Always-On",
                Price = 580000,
                CompareAtPrice = 650000,
                StockQty = 20,
                IsActive = true,
                IsFeatured = true,
                AverageRating = 4.7m,
                ReviewCount = 78,
                SoldCount = 92,
                MetaTitle = "Apple Watch Series 10 45mm - ShopFresherz",
                MetaDescription = "Buy Apple Watch Series 10 in Nigeria.",
                WeightKg = 0.042m,
                AttributesJson = "{\"display\":\"45mm Always-On Retina\",\"health\":\"ECG, Blood Oxygen, Temperature\",\"battery\":\"18 hours\",\"water_resistance\":\"50m\",\"connectivity\":\"GPS + Cellular\",\"os\":\"watchOS 11\"}"
            },

            // COMPUTING ACCESSORIES
            new Product
            {
                SKU = "LG-MX-MASTER3S",
                Name = "Logitech MX Master 3S Wireless Mouse",
                Slug = "logitech-mx-master-3s-wireless-mouse",
                BrandId = brandIds["logitech"],
                CategoryId = categoryIds["computing-accessories"],
                Description = "8K DPI precision tracking on any surface including glass. Ultra-fast MagSpeed scroll.",
                ShortDescription = "8K DPI, MagSpeed scroll, works on glass",
                Price = 52000,
                CompareAtPrice = 65000,
                StockQty = 35,
                IsActive = true,
                IsFeatured = false,
                AverageRating = 4.8m,
                ReviewCount = 112,
                SoldCount = 198,
                MetaTitle = "Logitech MX Master 3S - ShopFresherz",
                MetaDescription = "Buy Logitech MX Master 3S in Nigeria.",
                WeightKg = 0.141m,
                AttributesJson = "{\"dpi\":\"200-8000 DPI\",\"connectivity\":\"Bluetooth / USB receiver\",\"battery\":\"70 days\",\"buttons\":\"7 programmable\",\"compatibility\":\"Windows, macOS, Linux\",\"color\":\"Graphite\"}"
            },

            // CABLES
            new Product
            {
                SKU = "ANKER-USBC-240W-2M",
                Name = "Anker USB-C Cable 240W 2 Metres",
                Slug = "anker-usbc-cable-240w-2m",
                BrandId = brandIds["apple"],
                CategoryId = categoryIds["cables"],
                Description = "240W fast charging cable compatible with MacBook Pro, iPad Pro, Samsung Galaxy and more.",
                ShortDescription = "240W PD, 2m, braided nylon",
                Price = 12500,
                CompareAtPrice = 16000,
                StockQty = 80,
                IsActive = true,
                IsFeatured = false,
                AverageRating = 4.6m,
                ReviewCount = 234,
                SoldCount = 567,
                MetaTitle = "Anker 240W USB-C Cable 2m - ShopFresherz",
                MetaDescription = "Buy Anker 240W USB-C cable in Nigeria.",
                WeightKg = 0.08m,
                AttributesJson = "{\"wattage\":\"240W\",\"length\":\"2 metres\",\"connector\":\"USB-C to USB-C\",\"material\":\"Braided Nylon\",\"compatibility\":\"Universal USB-C devices\"}"
            },

            // ELECTRONICS
            new Product
            {
                SKU = "SAMSUNG-65-QLED-4K",
                Name = "Samsung 65 Inch QLED 4K Smart TV",
                Slug = "samsung-65-inch-qled-4k-smart-tv",
                BrandId = brandIds["samsung"],
                CategoryId = categoryIds["electronics"],
                Description = "Quantum Dot technology delivers vivid colour. Tizen OS with built-in streaming apps.",
                ShortDescription = "QLED, 4K, Tizen OS, 120Hz",
                Price = 980000,
                CompareAtPrice = 1150000,
                StockQty = 7,
                IsActive = true,
                IsFeatured = true,
                AverageRating = 4.6m,
                ReviewCount = 56,
                SoldCount = 34,
                MetaTitle = "Samsung 65 QLED 4K Smart TV - ShopFresherz",
                MetaDescription = "Buy Samsung 65 inch QLED TV in Nigeria.",
                WeightKg = 22.5m,
                AttributesJson = "{\"display\":\"65 inch QLED\",\"resolution\":\"4K UHD\",\"refresh_rate\":\"120Hz\",\"os\":\"Tizen\",\"hdmi_ports\":\"4\",\"smart_features\":\"Netflix, YouTube, Prime Video\"}"
            },

            // ROMOSS
            new Product
            {
                SKU = "ROMOSS-30000-PD",
                Name = "Romoss Sense 8P+ 30000mAh Power Bank",
                Slug = "romoss-sense-8p-30000mah-power-bank",
                BrandId = brandIds["romoss"],
                CategoryId = categoryIds["romoss"],
                Description = "30000mAh massive capacity with 65W PD fast charging. Charges MacBook, iPad and phones.",
                ShortDescription = "30000mAh, 65W PD, charges MacBook",
                Price = 35000,
                CompareAtPrice = 45000,
                StockQty = 50,
                IsActive = true,
                IsFeatured = false,
                AverageRating = 4.5m,
                ReviewCount = 189,
                SoldCount = 312,
                MetaTitle = "Romoss 30000mAh Power Bank - ShopFresherz",
                MetaDescription = "Buy Romoss 30000mAh power bank in Nigeria.",
                WeightKg = 0.58m,
                AttributesJson = "{\"capacity\":\"30000mAh\",\"max_output\":\"65W PD\",\"ports\":\"2x USB-A, 1x USB-C\",\"input\":\"USB-C 65W\",\"compatibility\":\"MacBook, iPad, Phones\",\"color\":\"Black\"}"
            },

            // HOME & KITCHEN TECH
            new Product
            {
                SKU = "XIAOMI-ROBOT-S10",
                Name = "Xiaomi Robot Vacuum S10 Pro",
                Slug = "xiaomi-robot-vacuum-s10-pro",
                BrandId = brandIds["samsung"],
                CategoryId = categoryIds["home-kitchen-tech"],
                Description = "4000Pa suction, sonic mopping, LiDAR navigation. Auto empty base station included.",
                ShortDescription = "4000Pa suction, LiDAR, auto-empty base",
                Price = 285000,
                CompareAtPrice = 350000,
                StockQty = 10,
                IsActive = true,
                IsFeatured = false,
                AverageRating = 4.4m,
                ReviewCount = 43,
                SoldCount = 29,
                MetaTitle = "Xiaomi Robot Vacuum S10 Pro - ShopFresherz",
                MetaDescription = "Buy Xiaomi Robot Vacuum in Nigeria.",
                WeightKg = 3.8m,
                AttributesJson = "{\"suction\":\"4000Pa\",\"navigation\":\"LiDAR\",\"mopping\":\"Sonic vibration\",\"battery\":\"5200mAh (180min)\",\"auto_empty\":\"Yes\",\"app\":\"Mi Home / Xiaomi Home\"}"
            },

            // TABLETS
            new Product
            {
                SKU = "IPAD-PRO-M4-11-256",
                Name = "Apple iPad Pro M4 11 Inch 256GB WiFi",
                Slug = "apple-ipad-pro-m4-11-inch-256gb-wifi",
                BrandId = brandIds["apple"],
                CategoryId = categoryIds["tablets"],
                Description = "Thinnest Apple product ever. M4 chip, Ultra Retina XDR OLED display.",
                ShortDescription = "M4 chip, OLED display, thinnest ever",
                Price = 1250000,
                CompareAtPrice = 1399000,
                StockQty = 14,
                IsActive = true,
                IsFeatured = true,
                AverageRating = 4.8m,
                ReviewCount = 67,
                SoldCount = 45,
                MetaTitle = "Apple iPad Pro M4 11 inch - ShopFresherz",
                MetaDescription = "Buy iPad Pro M4 in Nigeria.",
                WeightKg = 0.444m,
                AttributesJson = "{\"display\":\"11 inch Ultra Retina XDR OLED\",\"chip\":\"Apple M4\",\"storage\":\"256GB\",\"connectivity\":\"WiFi 6E\",\"usb\":\"USB-C Thunderbolt 4\",\"os\":\"iPadOS 17\"}"
            },

            // NEW ARRIVALS
            new Product
            {
                SKU = "INFINIX-GT20-PRO",
                Name = "Infinix GT 20 Pro Gaming Phone",
                Slug = "infinix-gt-20-pro-gaming-phone",
                BrandId = brandIds["infinix"],
                CategoryId = categoryIds["new-arrivals"],
                Description = "144Hz AMOLED gaming display, Dimensity 8200 Ultimate, RGB lighting system.",
                ShortDescription = "144Hz AMOLED, Dimensity 8200, RGB lights",
                Price = 235000,
                CompareAtPrice = 270000,
                StockQty = 30,
                IsActive = true,
                IsFeatured = true,
                AverageRating = 4.3m,
                ReviewCount = 28,
                SoldCount = 41,
                MetaTitle = "Infinix GT 20 Pro Gaming Phone - ShopFresherz",
                MetaDescription = "Buy Infinix GT 20 Pro in Nigeria.",
                WeightKg = 0.215m,
                AttributesJson = "{\"display\":\"6.78 inch 144Hz AMOLED\",\"ram\":\"12GB\",\"storage\":\"256GB\",\"processor\":\"Dimensity 8200 Ultimate\",\"battery\":\"5000mAh 45W\",\"network\":\"5G\",\"special\":\"RGB lighting system\"}"
            },

            // MUSICAL EQUIPMENT
            new Product
            {
                SKU = "BLUE-YETI-USB-BLK",
                Name = "Blue Yeti USB Microphone Black",
                Slug = "blue-yeti-usb-microphone-black",
                BrandId = brandIds["logitech"],
                CategoryId = categoryIds["musical-equipment"],
                Description = "Professional USB microphone for streaming, podcasting, gaming and home studio recording.",
                ShortDescription = "Professional USB mic, 4 polar patterns",
                Price = 68000,
                CompareAtPrice = 85000,
                StockQty = 18,
                IsActive = true,
                IsFeatured = false,
                AverageRating = 4.7m,
                ReviewCount = 145,
                SoldCount = 89,
                MetaTitle = "Blue Yeti USB Microphone - ShopFresherz",
                MetaDescription = "Buy Blue Yeti USB microphone in Nigeria.",
                WeightKg = 0.55m,
                AttributesJson = "{\"type\":\"USB Condenser Microphone\",\"polar_patterns\":\"Cardioid, Bidirectional, Omnidirectional, Stereo\",\"sample_rate\":\"48kHz/16-bit\",\"connection\":\"USB\",\"compatible\":\"Windows, macOS\",\"color\":\"Blackout\"}"
            },

            // COMPUTER MOUSE
            new Product
            {
                SKU = "RAZER-DA-V3-HYP",
                Name = "Razer DeathAdder V3 HyperSpeed",
                Slug = "razer-deathadder-v3-hyperspeed",
                BrandId = brandIds["logitech"],
                CategoryId = categoryIds["computer-mouse"],
                Description = "Ultra-lightweight 63g gaming mouse. 300hr battery, Focus X 26K optical sensor.",
                ShortDescription = "63g ultralight, 300hr battery, 26K DPI",
                Price = 38000,
                CompareAtPrice = 48000,
                StockQty = 25,
                IsActive = true,
                IsFeatured = false,
                AverageRating = 4.6m,
                ReviewCount = 76,
                SoldCount = 112,
                MetaTitle = "Razer DeathAdder V3 HyperSpeed - ShopFresherz",
                MetaDescription = "Buy Razer DeathAdder V3 in Nigeria.",
                WeightKg = 0.063m,
                AttributesJson = "{\"dpi\":\"100-26000 DPI\",\"weight\":\"63g\",\"battery\":\"300 hours\",\"connectivity\":\"Wireless 2.4GHz HyperSpeed\",\"sensor\":\"Focus X 26K Optical\",\"color\":\"Black\"}"
            },

            // FLASH DEAL PRODUCT (HP OMEN)
            new Product
            {
                SKU = "HP-OMEN-16-RTX4070",
                Name = "HP Omen 16 Gaming Laptop RTX 4070",
                Slug = "hp-omen-16-gaming-laptop-rtx4070",
                BrandId = brandIds["hp"],
                CategoryId = categoryIds["laptops-computers"],
                Description = "High-performance gaming laptop. Intel i7-13700HX, RTX 4070, 165Hz IPS display.",
                ShortDescription = "i7-13700HX, RTX 4070, 165Hz display",
                Price = 1100000,
                CompareAtPrice = 1350000,
                StockQty = 6,
                IsActive = true,
                IsFeatured = true,
                AverageRating = 4.5m,
                ReviewCount = 38,
                SoldCount = 22,
                MetaTitle = "HP Omen 16 RTX 4070 Gaming Laptop - ShopFresherz",
                MetaDescription = "Buy HP Omen 16 gaming laptop in Nigeria.",
                WeightKg = 2.4m,
                AttributesJson = "{\"display\":\"16.1 inch 165Hz IPS\",\"ram\":\"16GB DDR5\",\"storage\":\"512GB NVMe SSD\",\"processor\":\"Intel i7-13700HX\",\"gpu\":\"NVIDIA RTX 4070 8GB\",\"battery\":\"83Whr\",\"os\":\"Windows 11 Home\"}"
            },
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();
    }

    private static async Task SeedFlashDealAsync(ShopFresherzDbContext context)
    {
        if (await context.FlashDeals.AnyAsync()) return;

        Guid? productId = await context.Products
            .Where(p => p.Slug == "hp-omen-16-gaming-laptop-rtx4070")
            .Select(p => (Guid?)p.Id)
            .FirstOrDefaultAsync();

        if (!productId.HasValue) return;

        FlashDeal deal = new()
        {
            ProductId = productId.Value,
            SalePrice = 899000,
            OriginalPrice = 1100000,
            StartsAt = DateTime.UtcNow,
            EndsAt = DateTime.UtcNow.AddDays(3),
            MaxQuantity = 5,
            IsActive = true,
        };

        await context.FlashDeals.AddAsync(deal);
        await context.SaveChangesAsync();
    }

    private static async Task SeedBannersAsync(ShopFresherzDbContext context)
    {
        if (await context.Set<HomepageBanner>().AnyAsync()) return;

        List<HomepageBanner> banners = new()
        {
            new()
            {
                Title = "Freshest Tech in Nigeria",
                SubTitle = "New arrivals every week",
                CtaText = "Shop New Arrivals",
                LinkUrl = "/category/new-arrivals",
                ImageUrl = "https://images.unsplash.com/photo-1592750475338-74b7b21085ab?w=1280&q=80",
                SortOrder = 1,
                IsActive = true,
            },
            new()
            {
                Title = "PS5 Slim Now Available",
                SubTitle = "Limited stock \u2014 order now",
                CtaText = "Get Yours Now",
                LinkUrl = "/category/games-consoles",
                ImageUrl = "https://images.unsplash.com/photo-1606813907291-d86efa9b94db?w=1280&q=80",
                SortOrder = 2,
                IsActive = true,
            },
            new()
            {
                Title = "Up to 30% Off Accessories",
                SubTitle = "AirPods, JBL, Anker and more",
                CtaText = "Shop Accessories",
                LinkUrl = "/category/accessories",
                ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=1280&q=80",
                SortOrder = 3,
                IsActive = true,
            },
        };

        await context.Set<HomepageBanner>().AddRangeAsync(banners);
        await context.SaveChangesAsync();
    }

    private static async Task SeedCouponsAsync(ShopFresherzDbContext context)
    {
        if (await context.Coupons.AnyAsync()) return;

        List<Coupon> coupons = new()
        {
            new()
            {
                Code = "WELCOME10",
                Type = CouponType.Percentage,
                Value = 10,
                MinimumOrderAmount = 50000,
                MaxUses = 1000,
                MaxUsesPerUser = 1,
                ExpiresAt = DateTime.UtcNow.AddMonths(6),
                IsActive = true,
            },
            new()
            {
                Code = "FRESHERZ5000",
                Type = CouponType.Fixed,
                Value = 5000,
                MinimumOrderAmount = 100000,
                MaxUses = 500,
                MaxUsesPerUser = 1,
                ExpiresAt = DateTime.UtcNow.AddMonths(3),
                IsActive = true,
            },
            new()
            {
                Code = "GADGET15",
                Type = CouponType.Percentage,
                Value = 15,
                MinimumOrderAmount = 200000,
                MaxUses = 200,
                MaxUsesPerUser = 1,
                ExpiresAt = DateTime.UtcNow.AddMonths(2),
                IsActive = true,
            },
        };

        await context.Coupons.AddRangeAsync(coupons);
        await context.SaveChangesAsync();
    }

    private static async Task SeedReviewsAsync(ShopFresherzDbContext context)
    {
        Guid? adminId = await context.Users
            .Where(u => u.Role == UserRole.SuperAdmin)
            .Select(u => (Guid?)u.Id)
            .FirstOrDefaultAsync();

        Guid? iphoneId = await context.Products
            .Where(p => p.Slug == "apple-iphone-16-pro-max-256gb")
            .Select(p => (Guid?)p.Id)
            .FirstOrDefaultAsync();

        if (!adminId.HasValue || !iphoneId.HasValue) return;

        if (await context.Reviews.AnyAsync(r => r.ProductId == iphoneId.Value)) return;

        List<Review> reviews = new()
        {
            new()
            {
                UserId = adminId.Value,
                ProductId = iphoneId.Value,
                Rating = 5,
                Title = "Absolutely incredible phone",
                Body = "The camera system is unmatched. Face ID is instant. Battery easily lasts a full day of heavy use. Best iPhone I have ever owned.",
                IsApproved = true,
                IsVerifiedPurchase = true,
            },
            new()
            {
                UserId = adminId.Value,
                ProductId = iphoneId.Value,
                Rating = 5,
                Title = "Worth every kobo",
                Body = "Titanium build feels premium. The action button is very useful. Display is stunning outdoors. Highly recommend to anyone upgrading.",
                IsApproved = true,
                IsVerifiedPurchase = true,
            },
            new()
            {
                UserId = adminId.Value,
                ProductId = iphoneId.Value,
                Rating = 4,
                Title = "Great but pricey",
                Body = "Performance is flawless, camera is top notch. Only downside is the price but you get what you pay for. ShopFresherz delivery was fast too.",
                IsApproved = true,
                IsVerifiedPurchase = false,
            },
        };

        await context.Reviews.AddRangeAsync(reviews);
        await context.SaveChangesAsync();
    }
}
