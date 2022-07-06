using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Task1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly DataContext _context;

        public ProductController(DataContext dataContext)
        {
            _context = dataContext;
        }

        [HttpPost("addProduct")]
        public async Task<ActionResult> AddProduct(string name, string brandName, [Required] decimal price, [Required] IFormFile file)
        {
            User? user = (User?)HttpContext.Items["User"];
            Brand? brand = await _context.Brands.FindAsync(brandName);
            if (brand == null) { return BadRequest("No such brand exists"); }
            Store? store = _context.Stores.Where(s => s.User == user).FirstOrDefault();
            if (store == null) { return BadRequest("You do not own a store"); }

            if (file == null) { return BadRequest("No file was provided"); }
            if (file.Length > (2 * 1024 * 1024)) { return BadRequest("File size must be less than 2 MB"); }

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".jpg" && extension != ".png") { return BadRequest("File must be in jpg or png format"); }

            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Images", file.FileName);
            var stream = new FileStream(uploadPath, FileMode.Create);
            file.CopyTo(stream);
            try
            {
                await _context.Products.AddAsync(new Product
                {
                    Name = name,
                    Brand = brand,
                    Store = store,
                    User = user,
                    Price = price,
                    ImagePath = uploadPath
                });
                await _context.SaveChangesAsync();
                return Ok("Product has been added!");
            }
            catch (Exception e) { return BadRequest(e); }
        }

        [HttpPut("deleteProduct")]
        public async Task<ActionResult> DeleteProduct(string productName)
        {
            User? user = (User?)HttpContext.Items["User"];
            Store? store = _context.Stores.Where(s => s.User == user).FirstOrDefault();
            if (store == null) { return BadRequest("You do not own a store"); }
            Product? product = _context.Products.Where(p => p.Name == productName && p.Store == store && p.User == user).FirstOrDefault(); // REVIEW THIS TO SEE IF WORKS
            if (product == null) { return BadRequest("This product either doesn't exist, or you do not own it"); }
            try
            {
                product.deleted = true;
                _context.Products.Update(product);
                //_context.DeletedProducts.Add(product);
                await _context.SaveChangesAsync();
                return Ok("Product removed");
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpGet("getDeletedProducts")] // Test this
        public ActionResult GetDeletedProducts()
        {
            User? user = (User?)HttpContext.Items["User"];
            IEnumerable<Product> products = _context.Products.Where(p => p.deleted == true).ToArray();
            return Ok(products);
        }

        [HttpPut("recoverDeletedProduct")] 
        public async Task<ActionResult> RecoverDeletedProduct(string productName)
        {
            User? user = (User?)HttpContext.Items["User"];
            Store? store = _context.Stores.Where(s => s.User == user).FirstOrDefault();
            if (store == null) { return BadRequest("You do not own a store"); }
            Product? product = _context.Products.Where(p => p.deleted == true && p.Name == productName && p.Store == store && p.User == user).FirstOrDefault();
            if (product == null) { return BadRequest("This product either doesn't exist, or you do not own it"); }
            product.deleted = false;
            try
            {
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                return Ok($"Product {productName} has been recovered.");
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }
        [HttpPut("editProduct")] // TEST THIS
        public async Task<ActionResult> EditProduct(decimal newPrice, string newName, string oldName)
        {
            User? user = (User?)HttpContext.Items["User"];
            Store? store = _context.Stores.Where(s => s.User == user).FirstOrDefault();
            if (store == null) { return BadRequest("You do not own a store"); }
            Product? product = _context.Products.Where(p => p.Name == oldName && p.Store == store && p.User == user).FirstOrDefault(); // REVIEW THIS TO SEE IF WORKS
            if (product == null) { return BadRequest("This product either doesn't exist, or you do not own it"); }
            product.Name = newName;
            product.Price = newPrice;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return Ok("Product updated");
        }
    }
}
