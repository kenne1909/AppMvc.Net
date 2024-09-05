using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.Models;
using App.Models.Blog;
using Microsoft.AspNetCore.Authorization;
using App.Data;
using App.Areas.Blog.Models;
using Microsoft.AspNetCore.Identity;
using App.Utilities;
using Bogus.DataSets;

namespace App.Areas.Blog.Controllers
{
    [Area("Blog")]
    [Route("admin/blog/post/{action}/{id?}")]

    [Authorize(Roles =RoleName.Administrator +","+RoleName.Editor)]
    public class PostController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        [TempData]
        public string? StatusMessage {set;get;}

        public PostController(AppDbContext context,UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager=userManager;
        }

        // GET: Post
        //[Route("admin/blog/post/index/{page?}")]
        public async Task<IActionResult> Index([FromQuery(Name ="page")]int page = 1)
        {
            int pageSize = 10;
            var query = _context.Posts.Include(p => p.Author)
                                    .OrderByDescending(p => p.DateUpdated);
            
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            ViewBag.totalItems=totalItems;

            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var posts = await query.Skip((page - 1) * pageSize)
                                    .Take(pageSize)
                                    .Include(p =>p.PostCategories!)//// Nạp danh sách PostCategory cho mỗi bài viết
                                    .ThenInclude(pc => pc.Category)// Sau đó, nạp danh mục (Category) cho mỗi PostCategory
                                    .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;

            return View(posts);
        }


        // GET: Post/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Post/Create
        public async Task<IActionResult> Create()
        {
            //ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id");
            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"]= new MultiSelectList(categories,"Id","Title");
            return View();
        }

        // POST: Post/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Slug,Content,Published,CategoryIDs")] CreatePostModel post)
        {
            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"]= new MultiSelectList(categories,"Id","Title");

            if(post.Slug==null)
            {
                post.Slug= AppUtilities.GenerateSlug(post.Title!);
            }
            if(await _context.Posts.AnyAsync( p => p.Slug == post.Slug))
            {
                ModelState.AddModelError("Slug","Nhập chuổi URL khác");
                return View(post);
            }


            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(this.User);
                post.DateCreated=post.DateUpdated=DateTime.Now;
                post.AuthorId= user!.Id;

                _context.Add(post);

                if(post.CategoryIDs != null)
                {
                    foreach (var CateId in post.CategoryIDs)
                    {
                        _context.Add(new PostCategory(){
                            CategoryID = CateId,
                            Post = post
                        });
                    }
                }
                
                await _context.SaveChangesAsync();
                StatusMessage="Vừa tạo bài viết mới";
                return RedirectToAction(nameof(Index));
            }
            // ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id", post.AuthorId);
            return View(post);
        }

        // GET: Post/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //var post = await _context.Posts.FindAsync(id);
            var post= await _context.Posts.Include(p=> p.PostCategories)
                                    .FirstOrDefaultAsync(p => p.PostId==id);

            if (post == null)
            {
                return NotFound();
            }

            var postEdit = new CreatePostModel(){
                PostId=post.PostId,
                Title= post.Title,
                Content=post.Content,
                Description=post.Description,
                Slug=post.Slug,
                Published=post.Published,
                CategoryIDs=post.PostCategories!.Select(pc =>pc.CategoryID).ToArray()
            };

            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"]= new MultiSelectList(categories,"Id","Title");

            //ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id", post.AuthorId);
            return View(postEdit);
        }

        // POST: Post/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PostId,Title,Description,Slug,Content,Published,CategoryIDs")] CreatePostModel post)
        {
            if (id != post.PostId)
            {
                return NotFound();
            }
            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"]= new MultiSelectList(categories,"Id","Title");

            //xỬ LÝ SLUG nếu null thì sẽ phát sinh slug
            if(post.Slug==null)
            {
                post.Slug= AppUtilities.GenerateSlug(post.Title!);
            }
            if(await _context.Posts.AnyAsync( p => p.Slug == post.Slug && p.PostId!= id))
            {
                ModelState.AddModelError("Slug","Nhập chuổi URL khác");
                return View(post);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var postUpdate= await _context.Posts.Include(p=> p.PostCategories)
                                    .FirstOrDefaultAsync(p => p.PostId==id);

                    if (postUpdate == null)
                    {
                        return NotFound();
                    }

                    postUpdate.Title=post.Title;
                    postUpdate.Description=post.Description;
                    postUpdate.Content=post.Content;
                    postUpdate.Published=post.Published;
                    postUpdate.Slug=post.Slug;
                    post.DateUpdated=DateTime.Now;

                    //update Postcategory
                    if(post.CategoryIDs ==null)
                    {
                        post.CategoryIDs= new int[]{};
                    }

                    var oldCateId =postUpdate.PostCategories!.Select(c => c.CategoryID).ToArray();
                    var newCateId =post.CategoryIDs;

                    var removeCatePosts= from postCate in postUpdate.PostCategories
                                        where(!newCateId.Contains(postCate.CategoryID))
                                        select postCate;

                    _context.PostCategories.RemoveRange(removeCatePosts);

                    var addCateId= from CateId in newCateId
                                    where(!oldCateId.Contains(CateId))
                                    select CateId;

                    foreach (var CateId in addCateId)
                    {
                        _context.PostCategories.Add(new PostCategory(){
                            PostID=id,
                            CategoryID=CateId
                        });
                    }

                    _context.Update(postUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.PostId))
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
            ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id", post.AuthorId);
            return View(post);
        }

        // GET: Post/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Post/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
            }

            await _context.SaveChangesAsync();
            StatusMessage = "Bạn vừa xóa bài viết: "+post!.Title;
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.PostId == id);
        }
    }
}
