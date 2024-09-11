using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.Models;
using App.Models.Product;
using Microsoft.AspNetCore.Authorization;
using App.Data;
using App.Areas.Blog.Models;
using Microsoft.AspNetCore.Identity;
using App.Utilities;
using Bogus.DataSets;
using App.Areas.Product.Models;
using System.ComponentModel.DataAnnotations;

namespace App.Areas.Product.Controllers
{
    [Area("Product")]
    [Route("admin/productmanager/product/{action}/{id?}", Name ="pageproduct")]

    [Authorize(Roles =RoleName.Administrator +","+RoleName.Editor)]
    public class ProductManagerController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        [TempData]
        public string? StatusMessage {set;get;}

        public ProductManagerController(AppDbContext context,UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager=userManager;
        }

        // GET: Post
        // [Route("admin/blog/post/index/{page?}")]
        public async Task<IActionResult> Index([FromQuery(Name ="page")]int page = 1)
        {
            int pageSize = 10;
            //truy vấn lấy tất cả các sản phẩm bao gồm thông tin về người đăng(User) rồi sắp xếp theo DateUpdated
            var query = _context.Products.Include(p => p.Author)
                                    .OrderByDescending(p => p.DateUpdated);
            
            //đếm tất cả các sản phẩm
            var totalItems = await query.CountAsync();
            //tính tổng sô trang cần hiện sản phẩm// Math.Ceiling dùng để làm tròn đến số nguyên gần nhất
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            ViewBag.totalItems=totalItems;

            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var products = await query.Skip((page - 1) * pageSize)  // bỏ qua sản phẩm của các trang trước
                                    .Take(pageSize)                 // lấy các sản phẩm của trang hiện tại
                                    .Include(p =>p.productCategoryProducts!)//// Nạp danh sách PostCategory cho mỗi bài viết
                                    .ThenInclude(pc => pc.Category)// Sau đó, nạp danh mục (Category) cho mỗi PostCategory
                                    .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;

            return View(products);
        }


        // GET: Post/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.ProductId == id);// có thể dùng where để kiểm tra điều kiện
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Post/Create
        public async Task<IActionResult> Create()
        {
            //ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id");
            var categories = await _context.CategoryProducts.ToListAsync();
            ViewData["categories"]= new MultiSelectList(categories,"Id","Title");
            return View();
        }

        // POST: Post/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Slug,Content,Published,CategoryIDs,Price")] CreateProductModel product)
        {
            //danh sách các danh mục
            var categories = await _context.CategoryProducts.ToListAsync();
            ViewData["categories"]= new MultiSelectList(categories,"Id","Title");

            // tạo một giá trị Slug tự động từ Title nếu slug = null
            if(product.Slug==null)
            {
                product.Slug= AppUtilities.GenerateSlug(product.Title!);
            }
            //kiểm tra nếu slug  ng dùng nhập trùng vs slug trong cơ sở dữ liệu
            if(await _context.Products.AnyAsync( p => p.Slug == product.Slug))
            {
                ModelState.AddModelError("Slug","Nhập chuổi URL khác");
                return View(product);
            }


            if (ModelState.IsValid)
            {
                //lấy thông tin ng dùng hiện tại(ng dùng đang đăng nhập)
                var user = await _userManager.GetUserAsync(this.User);
                //đặt ngày tạo và ngày cập nhật là tg hiện tại
                product.DateCreated=product.DateUpdated=DateTime.Now;
                //gán AuthorId cho sản phẩm là id của ng dùng đang đăng nhập
                product.AuthorId= user!.Id;

                //Thêm sản phẩm vào cơ sở dữ liệu
                _context.Add(product);

                //Nếu có các ID danh mục được chọn;thêm các mối liên hệ giữa sản phẩm và danh mục vào cơ sở dữ liệu.
                //xử lý việc thêm các mối liên hệ giữa sản phẩm và các danh mục vào cơ sở dữ liệu.
                if(product.CategoryIDs != null)
                {
                    //Lặp qua tất cả các ID danh mục và tạo một đối tượng ProductCategoryProduct cho mỗi danh mục.
                    foreach (var CateId in product.CategoryIDs)
                    {
                        _context.Add(new ProductCategoryProduct(){
                            CategoryID = CateId,
                            Product = product
                        });
                    }
                }
                
                //Lưu tất cả các thay đổi vào cơ sở dữ liệu
                await _context.SaveChangesAsync();
                StatusMessage="Vừa tạo bài viết mới";
                //Chuyển hướng người dùng đến phương thức Index của controller
                return RedirectToAction(nameof(Index));
            }
            // ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id", post.AuthorId);
            //Nếu mô hình không hợp lệ, hoặc nếu có lỗi, trả lại view với mô hình hiện tại để người dùng có thể sửa lỗi và gửi lại.
            return View(product);
        }

        // GET: Post/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //var post = await _context.Posts.FindAsync(id);
            //tìm sản phẩm với id tương ứng
            var product= await _context.Products.Include(p=> p.productCategoryProducts)//Nạp dữ liệu liên quan đến mối liên hệ giữa sản phẩm và các danh mục
                                    .FirstOrDefaultAsync(p => p.ProductId==id);

            if (product == null)
            {
                return NotFound();
            }
            
            //Tạo một đối tượng CreateProductModel mới để gửi dữ liệu đến view cho form chỉnh sửa.
            var productEdit = new CreateProductModel(){
                ProductId=product.ProductId,
                Title= product.Title,
                Content=product.Content,
                Description=product.Description,
                Slug=product.Slug,
                Published=product.Published,
                CategoryIDs=product.productCategoryProducts!.Select(pc =>pc.CategoryID).ToArray(),
                //product là đối tượng của ProductModel chứa thông tin của một sản phẩm.
                //productCategoryProducts là một danh sách (List) các đối tượng ProductCategoryProduct liên kết sản phẩm với các danh mục.
                //Select Nó được dùng để duyệt qua từng phần tử trong danh sách productCategoryProducts.
                //pc đại diện cho từng đối tượng ProductCategoryProduct trong danh sách.
                Price=product.Price
            };

            //danh sách các danh mục
            var categories = await _context.CategoryProducts.ToListAsync();
            ViewData["categories"]= new MultiSelectList(categories,"Id","Title");

            //ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id", post.AuthorId);
            return View(productEdit);
        }

        // POST: Post/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,Title,Description,Slug,Content,Published,CategoryIDs,Price")] CreateProductModel product)
        {
            //kiểm tra tính hợp lệ của id
            if (id != product.ProductId)
            {
                return NotFound();
            }
            // lấy danh sách các danh mục
            var categories = await _context.CategoryProducts.ToListAsync();
            ViewData["categories"]= new MultiSelectList(categories,"Id","Title");

            //xỬ LÝ SLUG nếu null thì sẽ phát sinh slug
            if(product.Slug==null)
            {
                product.Slug= AppUtilities.GenerateSlug(product.Title!);
            }

            // kiểm tra xem Slug có trùng với bất kỳ sản phẩm nào khác có ProductId khác với id hiện tại hay không
            if(await _context.Products.AnyAsync( p => p.Slug == product.Slug && p.ProductId!= id))
            {
                ModelState.AddModelError("Slug","Nhập chuổi URL khác");
                return View(product);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var productUpdate= await _context.Products.Include(p=> p.productCategoryProducts)
                                    .FirstOrDefaultAsync(p => p.ProductId==id);

                    if (productUpdate == null)
                    {
                        return NotFound();
                    }

                    //cập nhật thông tin sản phẩm
                    productUpdate.Title=product.Title;
                    productUpdate.Description=product.Description;
                    productUpdate.Content=product.Content;
                    productUpdate.Published=product.Published;
                    productUpdate.Slug=product.Slug;
                    productUpdate.DateUpdated=DateTime.Now;
                    productUpdate.Price=product.Price;

                    //update Postcategory
                    //cập nhật danh mục liên kết với sản phẩm
                    if(product.CategoryIDs ==null)
                    {
                        product.CategoryIDs= new int[]{};
                    }
                    //chứa danh sách các CategoryID đã liên kết với sản phẩm trước khi chingr sửa
                    var oldCateId =productUpdate.productCategoryProducts!.Select(c => c.CategoryID).ToArray();
                    //chứa danh sách các cateid ng dùng chọn trên giao diện sau khi chỉnh sửa
                    var newCateId =product.CategoryIDs;

                    //xóa cách danh mục không còn liên kết
                    var removeCateProduct= from productCate in productUpdate.productCategoryProducts
                                        where(!newCateId.Contains(productCate.CategoryID))
                                        //productCate.CategoryIDlà ID danh mục của từng đối tượng trong danh sách hiện tại của sản phẩm.
                                        select productCate;

                    //xóa nhìu danh mục cùng lúc
                    _context.ProductCategoryProducts.RemoveRange(removeCateProduct);

                    // Chưa các danh mục mà k có trong danh sách danh mục cũ
                    var addCateId= from CateId in newCateId
                                    where(!oldCateId.Contains(CateId))
                                    //CateId là một biến đại diện cho từng ID danh mục trong newCateId khi vòng lặp đang xử lý.
                                    select CateId;

                    //Mỗi danh mục mới được thêm vào bảng ProductCategoryProduct, liên kết sản phẩm với danh mục đó.
                    foreach (var CateId in addCateId)
                    {
                        _context.ProductCategoryProducts.Add(new ProductCategoryProduct(){
                            ProductID=id,
                            CategoryID=CateId
                        });
                    }
                    //Sau khi tất cả các thay đổi đã được thực hiện, 
                    //hàm này gọi Update để cập nhật sản phẩm và lưu các thay đổi vào cơ sở dữ liệu bằng SaveChangesAsync.
                    _context.Update(productUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                StatusMessage="Vừa cập nhật bài viết";
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id", product.AuthorId);
            return View(product);
        }

        // GET: Post/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Post/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            StatusMessage = "Bạn vừa xóa bài viết: "+product!.Title;
            return RedirectToAction(nameof(Index));
        }

        public class UploadOneFile{
            [Required(ErrorMessage ="Phải chọn file Upload")]
            [DataType(DataType.Upload)]
            [FileExtensions(Extensions ="png,jpg,jpeg,gif")]
            [Display(Name ="Chọn file upload")]
            public IFormFile? FileUpload{set;get;}

        }

        private bool PostExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
        [HttpGet]
        public IActionResult UploadPhoto(int id)
        {
            var product = _context.Products.Where(e => e.ProductId== id)
                                    .Include(p => p.Photos)
                                    .FirstOrDefault();
            if(product == null)
            {
                return NotFound("Không tìm thấy sản phẩm");
            }
            ViewData["product"]=product;
            return View( new UploadOneFile());
        }
        [HttpPost, ActionName("UploadPhoto")]
        public async Task<IActionResult> UploadPhotoAsync(int id,[Bind("FileUpload")]UploadOneFile f)
        {
            var product = _context.Products.Where(e => e.ProductId== id)
                                    .Include(p => p.Photos)
                                    .FirstOrDefault();
            if(product == null)
            {
                return NotFound("Không tìm thấy sản phẩm");
            }
            ViewData["product"]=product;

            if(f !=null)
            {
                //var file1= System.IO.Path.GetRandomFileName();//có cả phần tên file và phần mở rộng
                var file1= Path.GetFileNameWithoutExtension(Path.GetRandomFileName())//chỉ lấy phần tên file chưa có phần mở rộng
                                +Path.GetExtension(f.FileUpload!.FileName); //lầy phần mở rộng

                var file= Path.Combine("Uploads","Products",file1);

                using(var filestream = new FileStream(file,FileMode.Create))
                {
                    await f.FileUpload.CopyToAsync(filestream);
                }

                _context.Add(new ProductPhoto(){
                   ProductID= product.ProductId,
                   FileName = file1 
                });
            
                await _context.SaveChangesAsync();
            }

            return View( new UploadOneFile());
        }

        [HttpPost]
        public IActionResult ListPhotos(int id)
        {
            var product = _context.Products.Where(e => e.ProductId== id)
                                    .Include(p => p.Photos)
                                    .FirstOrDefault();
            if(product == null)
            {
                return Json(
                    new {
                        success=0,
                        message="Product not found",
                    }
                );
            }
            var listphotos = product.Photos!.Select(photo => new{
                                id=photo.id,
                                path = "/contents/Products/"+photo.FileName
                            });

            return Json(
                new{
                    success=1,
                    photos= listphotos
                }
            );
        }
        [HttpPost]
        public IActionResult DeletePhoto(int id)
        {
            var photo= _context.ProductPhotos.Where(p => p.id==id).FirstOrDefault();
            if(photo!=null)
            {
                _context.Remove(photo);
                _context.SaveChanges();

                var filename= "Uploads/Products/"+photo.FileName;
                System.IO.File.Delete(filename);
            }
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> UploadPhotoApi(int id,[Bind("FileUpload")]UploadOneFile f)
        {
            var product = _context.Products.Where(e => e.ProductId== id)
                                    .Include(p => p.Photos)
                                    .FirstOrDefault();
            if(product == null)
            {
                return NotFound("Không tìm thấy sản phẩm");
            }

            if(f !=null)
            {
                //var file1= System.IO.Path.GetRandomFileName();//có cả phần tên file và phần mở rộng
                var file1= Path.GetFileNameWithoutExtension(Path.GetRandomFileName())//chỉ lấy phần tên file chưa có phần mở rộng
                                +Path.GetExtension(f.FileUpload!.FileName); //lầy phần mở rộng

                var file= Path.Combine("Uploads","Products",file1);

                using(var filestream = new FileStream(file,FileMode.Create))
                {
                    await f.FileUpload.CopyToAsync(filestream);
                }

                _context.Add(new ProductPhoto(){
                   ProductID= product.ProductId,
                   FileName = file1 
                });
            
                await _context.SaveChangesAsync();
            }

            return Ok();
        }
    }
}
