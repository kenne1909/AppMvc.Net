using App.Models;

namespace App.Services
{
    public class ProductService : List<ProductModel>
    {
        public ProductService()
        {
            this.AddRange(new ProductModel[] {
                new ProductModel() {Id=1,Name="Iphone",Price =30000},
                new ProductModel() {Id=2,Name="SanSung",Price =50000},
                new ProductModel() {Id=3,Name="Sony",Price =40000},
                new ProductModel() {Id=4,Name="Nokia",Price =30000}
            });
        }
    }
}