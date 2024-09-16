using App.Areas.Product.Models;
using App.Models;
using App.Models.Product;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Areas.Product.Controllers
{
    [Area("Product")]
    public class ViewProductController : Controller
    {
        private readonly ILogger<ViewProductController> _logger;
        private readonly AppDbContext _context;
        private readonly CartSerivce _cartSerivce;

        public ViewProductController(ILogger<ViewProductController> logger,AppDbContext context,CartSerivce cartSerivce)
        {
            _logger=logger;
            _context=context;
            _cartSerivce=cartSerivce;
        }

        // GET: ViewPostController
        // /post/ hiển thị tất cả bài của tất cả chuyên mục
        // /post/{categoryslug?} hiển thị chuyên mục nào
        [HttpGet("/product/{categoryslug?}", Name = "ProductRoute")]
        public ActionResult Index(string categoryslug,[FromQuery(Name ="page")]int page=1)
        {
            int pageSize = 6;
            var categories =GetCategories();
            ViewBag.categories = categories;
            ViewBag.categoryslug = categoryslug;

            CategoryProduct? category=  null;

            if(!string.IsNullOrEmpty(categoryslug))
            {
                category =  _context.CategoryProducts.Where(c =>c.Slug ==categoryslug)
                                .Include(c =>c.CategoryChildren)
                                .FirstOrDefault();
                if(category == null)
                {
                    return NotFound("Không tìm thấy category");
                }
            }

            ViewBag.category = category;

            var products =   _context.Products.Include(p => p.Author)
                                        .Include(p=>p.Photos)
                                        .Include(p=> p.productCategoryProducts!)
                                        .ThenInclude(p=> p.Category)
                                        .AsQueryable();    

            products = products.OrderByDescending(p=> p.DateUpdated);

            if(category!=null)
            {
                var ids =  new List<int>();
                category.ChildCategoryIDs(ids,null);
                ids.Add(category.Id);

                products =  products.Where(p => p.productCategoryProducts!.Where(pc => ids.Contains(pc.CategoryID)).Any());
            }
            
            var totalItems =  products.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.totalItems=totalItems;

            if (totalItems == 0)
            {
                return View(new List<ProductModel>());
            }


            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var productsInPage =  products.Skip((page - 1) * pageSize)
                                    .Take(pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            
            return View(productsInPage.ToList());
        }
        
        [Route("/product/{productslug}.html")]
        public IActionResult Detail(string productslug)
        {
            var categories =GetCategories();
            ViewBag.categories = categories;

            var product = _context.Products.Where(p => p.Slug == productslug)
                                .Include(p=> p.Author)
                                .Include(p=>p.Photos)
                                .Include(p=>p.productCategoryProducts!)
                                .ThenInclude(pc => pc.Category)
                                .FirstOrDefault();

            if(product == null)
            {
                return NotFound("không tìm thấy bài viết");
            }

            CategoryProduct category = product.productCategoryProducts!.FirstOrDefault()?.Category!;

            ViewBag.category = category;

            var otherProducts =_context.Products.Where(p=> p.productCategoryProducts!.Any(c => c.Category!.Id ==category.Id)) //lấy ra những bài post có cùng chuyên mục vs bài post này
                                        .Where(p => p.ProductId != product.ProductId)
                                        .OrderByDescending(p => p.DateUpdated)
                                        .Take(5);

            ViewBag.otherProducts =otherProducts;

            return View(product);
        }
        public List<CategoryProduct> GetCategories()
        {
            var categories = _context.CategoryProducts
                            .Include(c => c.CategoryChildren)
                            .AsEnumerable()
                            .Where(c => c.ParentCategory == null)
                            .ToList();
            return categories;
        }

        /// Thêm sản phẩm vào cart
        [Route ("addcart/{productid:int}", Name = "addcart")]
        public IActionResult AddToCart ([FromRoute] int productid) {

            var product = _context.Products
                .Where (p => p.ProductId == productid)
                .FirstOrDefault ();
            if (product == null)
                return NotFound ("Không có sản phẩm");

            // Xử lý đưa vào Cart ...
            var cart = _cartSerivce.GetCartItems ();
            var cartitem = cart.Find (p => p.product?.ProductId == productid);
            if (cartitem != null) {
                // Đã tồn tại, tăng thêm 1
                cartitem.quantity++;
            } else {
                //  Thêm mới
                cart.Add (new CartItem () { quantity = 1, product = product });
            }

            // Lưu cart vào Session
            _cartSerivce.SaveCartSession (cart);
            // Chuyển đến trang hiện thị Cart
            return RedirectToAction (nameof (Cart));
        }

        // Hiện thị giỏ hàng
        [Route ("/cart", Name = "cart")]
        public IActionResult Cart () {
            return View (_cartSerivce.GetCartItems());
        }
        /// xóa item trong cart
        [Route ("/removecart/{productid:int}", Name = "removecart")]
        public IActionResult RemoveCart ([FromRoute] int productid) {
            var cart = _cartSerivce.GetCartItems ();
            var cartitem = cart.Find (p => p.product?.ProductId == productid);
            if (cartitem != null) {
                // Đã tồn tại, tăng thêm 1
                cart.Remove(cartitem);
            }

            _cartSerivce.SaveCartSession (cart);
            return RedirectToAction (nameof (Cart));
        }   

        /// Cập nhật
        [Route ("/updatecart", Name = "updatecart")]
        [HttpPost]
        public IActionResult UpdateCart ([FromForm] int productid, [FromForm] int quantity) {
            // Cập nhật Cart thay đổi số lượng quantity ...
            var cart = _cartSerivce.GetCartItems ();
            var cartitem = cart.Find (p => p.product!.ProductId == productid);
            if (cartitem != null) {
                // Đã tồn tại, tăng thêm 1
                cartitem.quantity = quantity;
            }
            _cartSerivce.SaveCartSession (cart);
            // Trả về mã thành công (không có nội dung gì - chỉ để Ajax gọi)
            return Ok();
        }

        [Route("/checkout")]
        public IActionResult Checkout()
        {
            var cart = _cartSerivce.GetCartItems();
            //....
            _cartSerivce.ClearCart();
            return Content("Đã gửi đơn hàng");
        }

    }
}
