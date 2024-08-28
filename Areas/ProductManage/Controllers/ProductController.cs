using App.Services;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    [Area("ProductManage")]
    public class ProductController : Controller
    {
        private readonly ProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(ProductService productService,ILogger<ProductController> logger)
        {
            _productService=productService;
            _logger=logger;
        }

        // GET: ProductController
        [Route("cac-san-pham")]
        public ActionResult Index()
        {
            // /Area/AreaName/Views/Controller/Action.cshtml
            var  products = _productService.OrderBy(p => p.Name).ToList();
            return View(products);  // Area/ProductManage/Views/Product/Index.cshtml
        }

    }
}
