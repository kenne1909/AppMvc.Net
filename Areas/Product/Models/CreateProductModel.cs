using System.ComponentModel.DataAnnotations;
using App.Models.Product;

namespace App.Areas.Product.Models
{
    public class CreateProductModel : ProductModel
    {
        [Display(Name ="Chuyên mục")]
        public int[]? CategoryIDs{set;get;}//để cho nó biết bài post thuộc category nào
    }
}