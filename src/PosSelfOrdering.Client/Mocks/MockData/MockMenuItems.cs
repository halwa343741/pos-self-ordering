using PosSelfOrdering.Client.DTOs.Menu;

namespace PosSelfOrdering.Client.Mocks.MockData;

public static class MockMenuItems
{
    private static readonly ModifierGroupDto DrinkSizeModifiers = new(
        "mod_size", "Pilihan Ukuran", true, 1, 1,
        new List<ModifierOptionDto>
        {
            new("opt_size_reg", "Regular (12oz)", 0, true),
            new("opt_size_lrg", "Large (16oz)", 6000, false)
        }
    );

    private static readonly ModifierGroupDto IceLevelModifiers = new(
        "mod_ice", "Tingkat Es", true, 1, 1,
        new List<ModifierOptionDto>
        {
            new("opt_ice_normal", "Normal Ice", 0, true),
            new("opt_ice_less", "Less Ice", 0, false),
            new("opt_ice_none", "No Ice", 0, false)
        }
    );

    private static readonly ModifierGroupDto SugarLevelModifiers = new(
        "mod_sugar", "Tingkat Gula", true, 1, 1,
        new List<ModifierOptionDto>
        {
            new("opt_sugar_normal", "Normal Sugar (100%)", 0, true),
            new("opt_sugar_less", "Less Sugar (50%)", 0, false),
            new("opt_sugar_none", "No Sugar (0%)", 0, false)
        }
    );

    private static readonly ModifierGroupDto CoffeeToppingModifiers = new(
        "mod_coffee_toppings", "Pilihan Topping / Add-on", false, 0, 3,
        new List<ModifierOptionDto>
        {
            new("opt_top_espresso", "Extra Espresso Shot", 8000, false),
            new("opt_top_grassjelly", "Grass Jelly", 4000, false),
            new("opt_top_boba", "Brown Sugar Boba", 5000, false),
            new("opt_top_cream", "Sea Salt Cream", 6000, false)
        }
    );

    private static readonly ModifierGroupDto SpicyLevelModifiers = new(
        "mod_spicy", "Level Kepedasan", true, 1, 1,
        new List<ModifierOptionDto>
        {
            new("opt_spicy_0", "Level 0 (Tidak Pedas)", 0, true),
            new("opt_spicy_1", "Level 1 (Sedang)", 0, false),
            new("opt_spicy_2", "Level 2 (Pedas)", 0, false),
            new("opt_spicy_3", "Level 3 (Super Pedas)", 2000, false)
        }
    );

    private static readonly ModifierGroupDto EggAddonModifiers = new(
        "mod_egg", "Tambahan Telur", false, 0, 1,
        new List<ModifierOptionDto>
        {
            new("opt_egg_ceplok", "Telur Ceplok (Mata Sapi)", 5000, false),
            new("opt_egg_dadar", "Telur Dadar Crispy", 6000, false)
        }
    );

    public static readonly IReadOnlyList<MenuItemDto> All = new List<MenuItemDto>
    {
        // ☕ Coffee & Espresso
        new(
            "item_kopi_susu",
            "cat_coffee",
            "Kopi Susu Gula Aren",
            "Espresso house blend dipadukan dengan susu segar creamy dan sirup gula aren murni khas Nusantara.",
            25000,
            "https://images.unsplash.com/photo-1541167760496-1628856ab772?w=600&auto=format&fit=crop&q=80",
            true, true, false,
            new List<ModifierGroupDto> { DrinkSizeModifiers, IceLevelModifiers, SugarLevelModifiers, CoffeeToppingModifiers }
        ),
        new(
            "item_caramel_macchiato",
            "cat_coffee",
            "Caramel Macchiato",
            "Perpaduan espresso kaya rasa dengan vanilla syrup, steamed milk, dan saus karamel legit di atasnya.",
            35000,
            "https://images.unsplash.com/photo-1485808191679-5f86510681a2?w=600&auto=format&fit=crop&q=80",
            true, false, false,
            new List<ModifierGroupDto> { DrinkSizeModifiers, IceLevelModifiers, SugarLevelModifiers, CoffeeToppingModifiers }
        ),
        new(
            "item_americano",
            "cat_coffee",
            "Iced Americano Classic",
            "Double shot espresso Arabika segar dengan air dingin yang menyegarkan dan aroma floral fruity.",
            22000,
            "https://images.unsplash.com/photo-1514432324607-a09d9b4aefdd?w=600&auto=format&fit=crop&q=80",
            true, false, false,
            new List<ModifierGroupDto> { DrinkSizeModifiers, IceLevelModifiers, CoffeeToppingModifiers }
        ),
        new(
            "item_cappuccino",
            "cat_coffee",
            "Hot Cappuccino",
            "Single espresso dengan lapisan busa susu tebal lembut dan taburan bubuk kakao murni.",
            28000,
            "https://images.unsplash.com/photo-1572442388796-11668a67e53d?w=600&auto=format&fit=crop&q=80",
            true, false, false,
            new List<ModifierGroupDto> { SugarLevelModifiers, CoffeeToppingModifiers }
        ),
        new(
            "item_avocado_coffee",
            "cat_coffee",
            "Avocado Coffee Float",
            "Jus alpukat mentega kental disiram espresso double shot dan satu scoop es krim vanilla premium.",
            38000,
            "https://images.unsplash.com/photo-1578314675249-a6910f80cc4e?w=600&auto=format&fit=crop&q=80",
            true, true, false,
            new List<ModifierGroupDto> { SugarLevelModifiers }
        ),
        new(
            "item_hazelnut_latte",
            "cat_coffee",
            "Hazelnut Cream Latte",
            "Espresso dengan sirup hazelnut panggang harum, susu segar dan taburan remahan kacang.",
            32000,
            "https://images.unsplash.com/photo-1534778101976-62847782c213?w=600&auto=format&fit=crop&q=80",
            true, false, false,
            new List<ModifierGroupDto> { DrinkSizeModifiers, IceLevelModifiers, SugarLevelModifiers }
        ),

        // 🧋 Non-Coffee & Tea
        new(
            "item_matcha_latte",
            "cat_non_coffee",
            "Uji Matcha Supreme Latte",
            "Matcha autentik dari Uji, Kyoto berpadu harmonis dengan fresh milk lembut dan aroma earthy manis.",
            32000,
            "https://images.unsplash.com/photo-1536256263959-770b48d82b0a?w=600&auto=format&fit=crop&q=80",
            true, true, false,
            new List<ModifierGroupDto> { DrinkSizeModifiers, IceLevelModifiers, SugarLevelModifiers, CoffeeToppingModifiers }
        ),
        new(
            "item_earl_grey_milk_tea",
            "cat_non_coffee",
            "Earl Grey Milk Tea with Boba",
            "Seduhan teh hitam beraroma bergamot dengan susu segar dan topping boba kenyal lembut.",
            28000,
            "https://images.unsplash.com/photo-1558857563-b37cf5a9d311?w=600&auto=format&fit=crop&q=80",
            true, true, false,
            new List<ModifierGroupDto> { DrinkSizeModifiers, IceLevelModifiers, SugarLevelModifiers }
        ),
        new(
            "item_dark_chocolate",
            "cat_non_coffee",
            "Signature Dark Chocolate",
            "Cokelat Belgia pekat 70% dengan susu segar bertekstur kental dan rasa manis pahit seimbang.",
            30000,
            "https://images.unsplash.com/photo-1542990253-0d0f5be5f0ed?w=600&auto=format&fit=crop&q=80",
            true, false, false,
            new List<ModifierGroupDto> { DrinkSizeModifiers, IceLevelModifiers, SugarLevelModifiers }
        ),
        new(
            "item_peach_tea",
            "cat_non_coffee",
            "Iced Peach Jasmine Tea",
            "Teh melati wangi dengan potongan buah persik manis segar dan selasih.",
            24000,
            "https://images.unsplash.com/photo-1556679343-c7306c1976bc?w=600&auto=format&fit=crop&q=80",
            true, false, false,
            new List<ModifierGroupDto> { DrinkSizeModifiers, IceLevelModifiers, SugarLevelModifiers }
        ),

        // 🍛 Makanan Utama (Main Course)
        new(
            "item_nasi_goreng_kampung",
            "cat_main_course",
            "Nasi Goreng Spesial Nusantara",
            "Nasi goreng bumbu rempah tradisional dengan suwiran ayam, sate ayam, kerupuk udang, dan acar.",
            42000,
            "https://images.unsplash.com/photo-1603133872878-684f208fb84b?w=600&auto=format&fit=crop&q=80",
            true, true, true,
            new List<ModifierGroupDto> { SpicyLevelModifiers, EggAddonModifiers }
        ),
        new(
            "item_mie_goreng_aceh",
            "cat_main_course",
            "Mie Goreng Rempah Daging Sapi",
            "Mie kuning tebal dengan kuah kari kental medok, potongan daging sapi empuk dan emping gurih.",
            45000,
            "https://images.unsplash.com/photo-1612927601601-6638404737ce?w=600&auto=format&fit=crop&q=80",
            true, true, true,
            new List<ModifierGroupDto> { SpicyLevelModifiers, EggAddonModifiers }
        ),
        new(
            "item_ayam_geprek",
            "cat_main_course",
            "Ayam Geprek Sambal Bawang + Nasi",
            "Ayam goreng tepung renyah dengan ulekan sambal bawang pedas nendang, disajikan dengan nasi hangat.",
            32000,
            "https://images.unsplash.com/photo-1626082927389-6cd097cdc6ec?w=600&auto=format&fit=crop&q=80",
            true, true, true,
            new List<ModifierGroupDto> { SpicyLevelModifiers, EggAddonModifiers }
        ),
        new(
            "item_spaghetti_carbonara",
            "cat_main_course",
            "Creamy Spaghetti Carbonara",
            "Pasta spaghetti al dente diselimuti saus krim keju parmesan gurih, smoked beef crispy, dan lada hitam.",
            48000,
            "https://images.unsplash.com/photo-1612874742237-6526221588e3?w=600&auto=format&fit=crop&q=80",
            true, false, false,
            new List<ModifierGroupDto> { EggAddonModifiers }
        ),
        new(
            "item_beef_teriyaki_bowl",
            "cat_main_course",
            "Beef Teriyaki Rice Bowl",
            "Irisan daging sapi US shortplate lembut dengan saus teriyaki manis gurih dan taburan wijen sangrai.",
            46000,
            "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=600&auto=format&fit=crop&q=80",
            true, false, false,
            new List<ModifierGroupDto> { EggAddonModifiers }
        ),

        // 🥐 Snack & Pastry
        new(
            "item_croissant_butter",
            "cat_snacks",
            "French Butter Croissant",
            "Pastry renyah berlapis dengan aroma mentega Prancis yang wangi dan tekstur dalam yang lembut.",
            24000,
            "https://images.unsplash.com/photo-1555507036-ab1f4038808a?w=600&auto=format&fit=crop&q=80",
            true, false, false,
            new List<ModifierGroupDto>()
        ),
        new(
            "item_french_fries",
            "cat_snacks",
            "Truffle Parmesan French Fries",
            "Kentang goreng renyah dengan minyak truffle aromatik, taburan keju parmesan dan saus cocolan mayo.",
            28000,
            "https://images.unsplash.com/photo-1573080496219-bb080dd4f877?w=600&auto=format&fit=crop&q=80",
            true, true, false,
            new List<ModifierGroupDto>()
        ),
        new(
            "item_cireng_rujak",
            "cat_snacks",
            "Cireng Crispy Bumbu Rujak",
            "Cireng renyah di luar kenyal di dalam disajikan dengan sambal rujak gula merah pedas manis.",
            20000,
            "https://images.unsplash.com/photo-1563245372-f21724e3856d?w=600&auto=format&fit=crop&q=80",
            true, false, true,
            new List<ModifierGroupDto>()
        ),
        new(
            "item_pisang_goreng",
            "cat_snacks",
            "Pisang Goreng Madu Wijen",
            "Pisang raja manis legit digoreng renyah dengan balutan madu murni dan taburan wijen.",
            22000,
            "https://images.unsplash.com/photo-1587314168485-3236d6710814?w=600&auto=format&fit=crop&q=80",
            true, true, false,
            new List<ModifierGroupDto>()
        ),

        // 🍨 Dessert & Ice Cream
        new(
            "item_waffle_icecream",
            "cat_desserts",
            "Belgian Waffle with Ice Cream",
            "Wafel hangat renyah disajikan dengan 1 scoop es krim vanilla, stroberi segar, dan maple syrup.",
            34000,
            "https://images.unsplash.com/photo-1562376552-0d160a2f238d?w=600&auto=format&fit=crop&q=80",
            true, true, false,
            new List<ModifierGroupDto>()
        ),
        new(
            "item_burnt_cheesecake",
            "cat_desserts",
            "Basque Burnt Cheesecake",
            "Kue keju panggang lembut creamy dengan permukaan karamelisasi yang harum memikat.",
            36000,
            "https://images.unsplash.com/photo-1533134242443-d4fd215305ad?w=600&auto=format&fit=crop&q=80",
            true, false, false,
            new List<ModifierGroupDto>()
        )
    };
}
