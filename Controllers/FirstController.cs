using System.Security.Cryptography.X509Certificates;
using App.Services;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    public class FirstController : Controller
    {
        private readonly ILogger<FirstController> _logger;
        public static string? ContentRootPath{set;get;}// thiết lập đường dẫn file
        private readonly ProductService _productService;
        public FirstController(ILogger<FirstController> logger,IWebHostEnvironment environment,ProductService productService)
        {
            _logger=logger;
            ContentRootPath=environment.ContentRootPath;
            _productService=productService;
        }

        public string Index() 
        {
            /* this.HttpContext :   chứa tất cả thông tin liên quan đến một yêu cầu HTTP cụ thể. 
Nó bao gồm thông tin về yêu cầu, phản hồi, phiên làm việc (session), và nhiều thông tin khác.*/

            /* this.Request     :   cung cấp thông tin về yêu cầu HTTP hiện tại như URL, headers, cookies, 
dữ liệu query string, dữ liệu form, v.v.*/

            /* this.Response    :   dùng để tạo phản hồi HTTP gửi về cho client. Bạn có thể đặt trạng thái, 
headers, và nội dung phản hồi.*/

            /* this.RouteData   :   chứa thông tin về tuyến đường (route) đã được ánh xạ đến hành động (action) hiện tại.*/


            /* this.User        :   đại diện cho người dùng hiện tại đã xác thực, cung cấp các thông tin về danh tính 
và vai trò của người dùng.*/

            /* this.ModelState  :   chứa dữ liệu liên quan đến việc xác thực các dữ liệu được gửi từ client. 
Nó giúp kiểm tra tính hợp lệ của dữ liệu.*/

            /* this.ViewData    :   là một từ điển (dictionary) cho phép truyền dữ liệu từ controller đến view.
Dữ liệu này chỉ tồn tại trong một yêu cầu duy nhất.*/

            /* this.ViewBag     :   là một dynamic object cho phép truyền dữ liệu từ controller đến view giống như ViewData, 
nhưng có cú pháp đơn giản hơn.*/

            /* this.Url         :   cung cấp các phương thức để tạo URL dựa trên tuyến đường (route) 
và các thành phần khác của URL.*/

            /* this.TempData    :   là một từ điển tạm thời cho phép truyền dữ liệu giữa các yêu cầu HTTP,
thường dùng để lưu trữ dữ liệu khi chuyển hướng (redirect).*/ 


            // LogLevel: các cấp độ của log
            // _logger.Log(LogLevel.Warning,"Thongbao"); -> dùng cụ thể
            // dùng trực tiếp: 
            _logger.LogWarning("Thong bao");
            _logger.LogDebug("thong bao");
            _logger.LogError("thong bao");
            _logger.LogTrace("Thon bao");
            _logger.LogCritical("Thong bao");
            _logger.LogInformation("Index Action");
            //serilog
            //Console.WriteLine("Index Action");// tương tự logger tuy nhiên tập thói quen dùng log
            
            return "Tôi là Index của First";
        }
        //action là 1 phương thưc public k đc là static có thể trả về bất kì 1 đối tượng j
        public void Nothing()
        {
            _logger.LogInformation("Nothing Action");
            Response.Headers.Append("hi","xinchaocacban");
        }
        public object Anything() =>DateTime.Now;

        //thương khai báo trả về những đối tượng chỉ triển khai từ giao diện IActionResult
        // ContentResult               | Content()          -- trả về respone
        // EmptyResult                 | new EmptyResult()  -- tương đương vs việc trả về void
        // FileResult                  | File()             -- trả về 1 nội dung file nào đó (ảnh, ...)
        // ForbidResult                | Forbid()
        // JsonResult                  | Json()
        // LocalRedirectResult         | LocalRedirect()
        // RedirectResult              | Redirect()
        // RedirectToActionResult      | RedirectToAction()
        // RedirectToPageResult        | RedirectToRoute()
        // RedirectToRouteResult       | RedirectToPage()
        // PartialViewResult           | PartialView()
        // ViewComponentResult         | ViewComponent()
        // StatusCodeResult            | StatusCode()
        // ViewResult                  | View() 
        public IActionResult Reame()
        {
            var content =@"
            Xin chao cac ban
            dang học về MVC
                    Lê Hoàn
            ";
            return this.Content(content,"text/html");//kiểu văn bản trả về(tham số thứ 2) nó hỉu đây là file text còn nếu html thì nó sẽ hiểu html
        }
        public IActionResult Nature()
        {
            // Kiểm tra xem ContentRootPath có được thiết lập không
            if (ContentRootPath == null)
            {
                return NotFound("Không tìm thấy đường dẫn gốc");
            }

            // Xây dựng đường dẫn đến tệp hình ảnh
            string filePath = Path.Combine(ContentRootPath, "Files", "anh1.jpg");

            // Kiểm tra xem tệp có tồn tại không
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Tệp hình ảnh không tồn tại");
            }

            // Đọc nội dung tệp vào mảng byte
            var bytes = System.IO.File.ReadAllBytes(filePath);

            // Trả về tệp với loại MIME là image/jpeg
            return File(bytes, "image/jpeg");

        }
    
        public IActionResult IphonePrice()
        {
            return Json(
                new{
                    productName="Iphone X",
                    Price =1000
                }
            );
        }
    
        public IActionResult Privacy()
        {
            var url= Url.Action("Privacy","Home");
            _logger.LogInformation("chuyen huong den: "+url);
            if(url == null)
            {
                return NotFound("Không tìm thấy url");
            }
            return LocalRedirect(url);// đảm bảo url chuyển hướng là local ~ địa chỉ url k có phần host
        }
    
        public IActionResult Google()
        {
            var url= "https://google.com";
            _logger.LogInformation("chuyen huong den: "+url);
            if(url == null)
            {
                return NotFound("Không tìm thấy url");
            }
            return Redirect(url);// đảm bảo url chuyển hướng là local ~ địa chỉ url k có phần host
        }
    
        public IActionResult HelloView(string username)
        {
            if(string.IsNullOrEmpty(username))
            {
                username="Khách";
            }
            //View() ->Razor engine,đọc và thì hành file .cshtml (template)
            //----------
            //View(template) --template là đường dẫn tuyệt đối tới .cshtml
            //View(template, model)
            // return View("/MyView/xinchao1.cshtml",username); -> trong view sử dụng là model string.

            //xinchao2.cshtml ->View/First/xinchao2.cshtml
            // return View("xinchao2",username);

            //HelloView.cshtml ->View/First/HelloView.cshtml
            // /View/Controller/Action.cshtml
            // return View((object)username);// phải cast nó sang object để nó biết đây là model 

            return View("xinchao3",username);

            //thường dùng:
            //View();
            //View(Model);
        }
    
        [TempData]
        public string? StatusMessage{set;get;}

        [AcceptVerbs("POST","GET")]// chỉ có thể truy cập bằng pt post
        public IActionResult ViewProduct(int? id)
        {
            var product= _productService.Where(p=> p.Id == id).FirstOrDefault();
            if(product == null)
            {

                // TempData["StatusMessage"]="sản phẩm bạn yêu cầu không có";
                StatusMessage ="sản phẩm bạn yêu cầu không có"; //tương đương vs cái trên
                return RedirectToAction("Index", "Home");
            }

            // /View/First/ViewProduct.cshtml
            // return View(product);//-> truyền dữ liệu sang view bằng cách sử dụng model; thiết lập model cho view

            // ViewData -> bằng cách sử dụng key và value
            // this.ViewData["product"]=product;
            // ViewData["Title"]=product.Name;
            // return View("ViewProduct2");

            // truyền dữ liệu từ trang này sang trang khác -> TempData -> truy cập lần 2 sẽ k có
            // TempData ->  thiết lập dữ liệu thông qua key -> sử dụng session của hệ thống để lưu dữ liệu và trang khác có thể đọc đc


            // ViewBag -> sử dụng giống ViewData nhưng nó là kiểu dynamic -> có thể thiết lập ở thời điểm thực thi thì ở trong view cx có đối tượng chính là viewbag
            ViewBag.product= product;
            return View("ViewProduct3");


        }
    }
}