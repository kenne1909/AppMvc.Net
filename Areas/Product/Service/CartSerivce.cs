using App.Areas.Product.Models;
using Newtonsoft.Json;

public class CartSerivce
{
    // Key lưu chuỗi json của Cart
    public const string CARTKEY = "cart";

    private readonly IHttpContextAccessor _context;
    private readonly HttpContext? HttpContext;

    public CartSerivce(IHttpContextAccessor context)
    {
        _context=context;
        HttpContext=context.HttpContext;
    }

    // Lấy cart từ Session (danh sách CartItem)
    public List<CartItem> GetCartItems () {
        var session = HttpContext?.Session;
        string? jsoncart = session?.GetString (CARTKEY);
        if (jsoncart != null) {
            var cartItems = JsonConvert.DeserializeObject<List<CartItem>>(jsoncart);
            return cartItems ?? new List<CartItem>(); // Đảm bảo không trả về null  
        }
        return new List<CartItem> ();
    }

    // Xóa cart khỏi session
    public void ClearCart () {
        var session = HttpContext?.Session;
        session?.Remove (CARTKEY);
    }

    // Lưu Cart (Danh sách CartItem) vào session
    public void SaveCartSession (List<CartItem> ls) {
        var session = HttpContext?.Session;
        string jsoncart = JsonConvert.SerializeObject (ls);
        session?.SetString (CARTKEY, jsoncart);
    }
}