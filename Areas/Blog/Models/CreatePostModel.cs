using System.ComponentModel.DataAnnotations;
using App.Models.Blog;

namespace App.Areas.Blog.Models
{
    public class CreatePostModel : Post
    {
        [Display(Name ="Chuyên mục")]
        public int[]? CategoryIDs{set;get;}//để cho nó biết bài post thuộc category nào
    }
}