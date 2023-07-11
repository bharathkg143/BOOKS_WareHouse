using BOOKS_WareHouse.DataAccess.Data;
using BOOKS_WareHouse.DataAccess.Repository.IRepository;
using BOOKS_WareHouse.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BOOKS_WareHouse.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>,IProductRepository
    {
        private ApplicationDbContext _context;
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(Product product)
        {
            var updateProduct=_context.Products.FirstOrDefault(x => x.Id == product.Id);

            if (updateProduct != null)
            {
                updateProduct.Title = product.Title;
                updateProduct.Description = product.Description;
                updateProduct.Price = product.Price;
                updateProduct.ISBN = product.ISBN;
                updateProduct.Author = product.Author;
                updateProduct.Price100 = product.Price100;
                updateProduct.Price50 = product.Price50;
                updateProduct.CategoryId = product.CategoryId;
                updateProduct.ListPrice = product.ListPrice;

                if(product.ImageUrl != null)
                {
                    updateProduct.ImageUrl = product.ImageUrl;
                }
            }
        }
    }
}
