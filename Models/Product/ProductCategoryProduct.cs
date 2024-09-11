using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Product{
    [Table("ProductCategoryProduct")]
    public class ProductCategoryProduct
    {
        public int ProductID {set; get;}// tham chiếu đến đối tượng post

        public int CategoryID {set; get;}

        [ForeignKey("ProductID")]
        public ProductModel? Product {set; get;}

        [ForeignKey("CategoryID")]
        public CategoryProduct? Category {set; get;}
    }
}