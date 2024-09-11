using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Product
{
    [Table("CategoryProduct")] 
    public class CategoryProduct
    {

        [Key]
        public int Id { get; set; }

        // Tiều đề Category
        [Required(ErrorMessage = "Phải có tên danh mục")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "{0} dài {1} đến {2}")]
        [Display(Name = "Tên danh mục")]
        public string? Title { get; set; }

        // Nội dung, thông tin chi tiết về Category
        [DataType(DataType.Text)]
        [Display(Name = "Nội dung danh mục")]
        public string? Description { set; get; }

        //chuỗi Url-> khi truy cập chuyên mục nào thì trên url k cần hiển thị id của mục đó
        [Required(ErrorMessage = "Phải tạo url")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "{0} dài {1} đến {2}")]
        [RegularExpression(@"^[a-z0-9-]*$", ErrorMessage = "Chỉ dùng các ký tự [a-z0-9-]")]
        [Display(Name = "Url hiện thị")]
        public string? Slug { set; get; }

        // Các Category con
        public ICollection<CategoryProduct>? CategoryChildren { get; set; }

        // Category cha (FKey)
        [Display(Name = "Danh mục cha")]
        public int? ParentCategoryId { get; set; }


        [ForeignKey("ParentCategoryId")]
        [Display(Name = "Danh mục cha")]
        public CategoryProduct? ParentCategory { set; get; }
        //được sử dụng để lấy tất cả các ID của danh mục con (Category Children) cho một danh mục cụ thể (Category Product),
        public void ChildCategoryIDs(List<int> lists,ICollection<CategoryProduct>? childcates = null)
        //List<int> lists: là danh sách mà bạn sẽ lưu trữ tất cả ID của danh mục con. 
        //childcates: là danh sách các danh mục con của danh mục hiện tại
        {
            if(childcates ==null)
            {
                childcates = this.CategoryChildren!;//phương thức sẽ lấy CategoryChildren của đối tượng danh mục hiện tại làm danh sách ban đầu để bắt đầu quá trình xử lý.
            }
            foreach (CategoryProduct category in childcates)
            {
                lists.Add(category.Id);//: Thêm ID của danh mục con hiện tại vào danh sách lists.
                ChildCategoryIDs(lists,category.CategoryChildren);//gọi đệ quy
            }
        }

        //truy xuất danh sách các danh mục cha của một danh mục hiện tại
        public List<CategoryProduct> ListParents()
        {
            List<CategoryProduct> li = new List<CategoryProduct>();//danh sách trống để lưu tất cả các danh mục cha trong quá trình duyệt.
            var parent = this.ParentCategory;//trỏ tới danh mục cha của danh mục hiện tại. Nếu danh mục hiện tại không có cha, this.ParentCategory sẽ là null.
            while(parent != null)
            {
                li.Add(parent);
                parent = parent.ParentCategory;
            }
            li.Reverse();//Đảo ngược danh sách để danh mục gốc xuất hiện trước, và danh mục cha trực tiếp xuất hiện cuối cùng
            return li;
        }

    }   
}