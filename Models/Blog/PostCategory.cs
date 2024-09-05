using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Blog{
    [Table("PostCategory")]
    public class PostCategory
    {
        public int PostID {set; get;}// tham chiếu đến đối tượng post

        public int CategoryID {set; get;}

        [ForeignKey("PostID")]
        public Post? Post {set; get;}

        [ForeignKey("CategoryID")]
        public Category? Category {set; get;}
    }
}