using App.Data;
using App.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Areas.Database.Controllers
{
    [Area("Database")]
    [Route("/database-manage/[action]")]
    public class DbManageController : Controller
    {
        private readonly AppDbContext _appDbContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbManageController(AppDbContext appDbContext,UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _appDbContext=appDbContext;
            _roleManager=roleManager;
            _userManager=userManager;
        }

        // GET: DbManageController
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult DeleteDb()
        {
            return View();
        }
        [TempData]
        public string? StatusMessage{set;get;}
        [HttpPost]
        public async Task<IActionResult> DeleteDbAsync()
        {
            var success =  await _appDbContext.Database.EnsureDeletedAsync();
            StatusMessage = success ? "Xóa Database thành công" : "Không xóa được";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Migrate()
        {
            await _appDbContext.Database.MigrateAsync();
            StatusMessage ="Cập nhật Database thành công";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> SeedDataAsync()
        {
            var rolenames = typeof(RoleName).GetFields().ToList();
            foreach (var r in rolenames)
            {
                var rolename = r.GetRawConstantValue() as string ?? "DefaultRoleName";
                var rfound = await _roleManager.FindByNameAsync(rolename);
                if(rfound ==null)
                {
                    await _roleManager.CreateAsync(new IdentityRole(rolename));
                }
            }
            // admin, pass=admin123, admin@example.com
            var useradmin = await _userManager.FindByNameAsync("admin");
            if(useradmin == null)
            {
                useradmin = new AppUser(){
                    UserName = "admin",
                    Email = "admin@example.com",
                    EmailConfirmed =true
                };

                await _userManager.CreateAsync(useradmin,"admin123");
                await _userManager.AddToRoleAsync(useradmin,RoleName.Administrator);

            } 
            StatusMessage ="Vừa seed Database";
            return RedirectToAction("Index");
        }

    }
}
