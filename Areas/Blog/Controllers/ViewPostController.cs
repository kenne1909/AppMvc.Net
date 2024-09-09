using App.Models;
using App.Models.Blog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Areas.Blog.Controllers
{
    [Area("Blog")]
    public class ViewPostController : Controller
    {
        private readonly ILogger<ViewPostController> _logger;
        private readonly AppDbContext _context;

        public ViewPostController(ILogger<ViewPostController> logger,AppDbContext context)
        {
            _logger=logger;
            _context=context;
        }

        // GET: ViewPostController
        // /post/ hiển thị tất cả bài của tất cả chuyên mục
        // /post/{categoryslug?} hiển thị chuyên mục nào
        [HttpGet("/post/{categoryslug?}", Name = "PostRoute")]
        public ActionResult Index(string categoryslug,[FromQuery(Name ="page")]int page=1)
        {
            int pageSize = 5;
            var categories =GetCategories();
            ViewBag.categories = categories;
            ViewBag.categoryslug = categoryslug;

            Category? category=  null;

            if(!string.IsNullOrEmpty(categoryslug))
            {
                category =  _context.Categories.Where(c =>c.Slug ==categoryslug)
                                .Include(c =>c.CategoryChildren)
                                .FirstOrDefault();
                if(category == null)
                {
                    return NotFound("Không tìm thấy category");
                }
            }

            ViewBag.category = category;

            var posts =   _context.Posts.Include(p => p.Author)
                                        .Include(p=> p.PostCategories!)
                                        .ThenInclude(p=> p.Category)
                                        .AsQueryable();    

            posts.OrderByDescending(p=> p.DateUpdated);

            if(category!=null)
            {
                var ids =  new List<int>();
                category.ChildCategoryIDs(ids,null);
                ids.Add(category.Id);

                posts =  posts.Where(p => p.PostCategories!.Where(pc => ids.Contains(pc.CategoryID)).Any());
            }
            
            var totalItems =  posts.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.totalItems=totalItems;

            if (totalItems == 0)
            {
                return View(new List<Post>());
            }


            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var postsInPage =  posts.Skip((page - 1) * pageSize)
                                    .Take(pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            
            return View(postsInPage.ToList());
        }
        
        [Route("/post/{postslug}.html")]
        public IActionResult Detail(string postslug)
        {
            var categories =GetCategories();
            ViewBag.categories = categories;

            var post = _context.Posts.Where(p => p.Slug == postslug)
                                .Include(p=> p.Author)
                                .Include(p=>p.PostCategories!)
                                .ThenInclude(pc => pc.Category)
                                .FirstOrDefault();

            if(post == null)
            {
                return NotFound("không tìm thấy bài viết");
            }

            Category category = post.PostCategories!.FirstOrDefault()?.Category!;

            ViewBag.category = category;

            var otherPosts =_context.Posts.Where(p=> p.PostCategories!.Any(c => c.Category!.Id ==category.Id)) //lấy ra những bài post có cùng chuyên mục vs bài post này
                                        .Where(p => p.PostId != post.PostId)
                                        .OrderByDescending(p => p.DateUpdated)
                                        .Take(5);

            ViewBag.otherPosts =otherPosts;

            return View(post);
        }
        public List<Category> GetCategories()
        {
            var categories = _context.Categories
                            .Include(c => c.CategoryChildren)
                            .AsEnumerable()
                            .Where(c => c.ParentCategory == null)
                            .ToList();
            return categories;
        }

    }
}
