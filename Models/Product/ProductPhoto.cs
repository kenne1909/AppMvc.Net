using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Product{
    [Table("ProductPhoto")]
    public class ProductPhoto
    {
        [Key]
        public int id {set;get;}
        //abc.png; 123.jpg ...
        // -> /contents/Products/abc.png
        public string? FileName{set;get;}

        public int ProductID {set;get;}

        [ForeignKey("ProductID")]
        public ProductModel?  Product{set;get;}

    }
}