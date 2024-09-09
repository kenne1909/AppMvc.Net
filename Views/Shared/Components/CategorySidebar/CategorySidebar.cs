using App.Models.Blog;
using Microsoft.AspNetCore.Mvc;

namespace App.Components
{
    [ViewComponent]
    public class CategorySidebar :ViewComponent
    {
        public class CategorySidebarData
        {
            public List<Category>? Categories{set;get;}//chứa những danh mục cần render
            public int level {set;get;}
            public string? categoryslug{set;get;}//cho biết hiện tại đang truy cập category nào
        }
        public IViewComponentResult Invoke(CategorySidebarData data)
        {
            return View(data);
        }
    }
}