using Microsoft.AspNetCore.Mvc;

namespace Task1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly DataContext _context;
        private const int maxFileSize = 2 * 1024 * 1024;

        public ProductController(DataContext dataContext)
        {
            _context = dataContext;
        }
        [AllowWithoutAuthorization]
        [HttpGet("getProductByName")]
        public async Task<ActionResult> GetProductByName(string name)
        {
            Product? product = await _context.Products
                .Include(p => p.Store)
                .Include(p => p.Brand)
                .Include(p => p.User)
                .Where(p => p.Name == name)
                .FirstOrDefaultAsync();
            return product != null? Ok(product): NotFound("We did not find a product with this name");
        }

        [HttpPost("addProduct")] // TESTED
        public async Task<ActionResult> AddProduct(string name, string brandName, decimal price, [Required] [AllowedExtensions(new string[] { ".jpg", ".png" })] [MaxFileSize(maxFileSize)] IFormFile file)
        {
            User? user = (User?)HttpContext.Items["User"];
            Brand? brand = await _context.Brands.FindAsync(brandName);
            if (brand == null) { return BadRequest("No such brand exists"); }
            Store? store = _context.Stores.Where(s => s.User == user).FirstOrDefault();
            if (store == null) { return BadRequest("You do not own a store"); }
            if (_context.Products.Where(p => p.Name == name).Any()) { return BadRequest("This product name was registered for another product"); }

            var extension = Path.GetExtension(file.FileName);
            Product product = new Product
            {
                Name = name,
                Brand = brand,
                Store = store,
                User = user,
                Price = price
            };
            string imgName = $"{name}{extension}";
            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Images", imgName);
            using (var stream = new FileStream(uploadPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            product.ImagePath = uploadPath;
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return Ok("Product has been added!");
        }

        [HttpPut("deleteProduct")] // TESTED
        public async Task<ActionResult> DeleteProduct(string productName)
        {
            User? user = (User?)HttpContext.Items["User"];
            Store? store = _context.Stores.Where(s => s.User == user).FirstOrDefault();
            if (store == null) { return BadRequest("You do not own a store"); }
            Product? product = _context.Products.Where(p => p.Name == productName && p.Store == store && p.User == user).FirstOrDefault(); // REVIEW THIS TO SEE IF WORKS
            if (product == null) { return BadRequest("This product either doesn't exist, or you do not own it"); }
            if (product.deleted == true) { return BadRequest("This product has already been deleted"); } 
            product.deleted = true;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return Ok("Product removed");
        }

        [HttpGet("getDeletedProducts")] // TESTED
        public ActionResult GetDeletedProducts()
        {
            User? user = (User?)HttpContext.Items["User"];
            IEnumerable<Product> products = _context.Products.Where(p => p.deleted == true).ToArray();
            return Ok(products);
        }

        [HttpPut("recoverDeletedProduct")] // TESTED
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
        [HttpPut("editProduct")] // TESTED
        public async Task<ActionResult> EditProduct(decimal newPrice, string newName, string oldName, [AllowedExtensions(new string[] { ".jpg", ".png" })][MaxFileSize(maxFileSize)] IFormFile? file)
        {
            User? user = (User?)HttpContext.Items["User"];
            Store? store = _context.Stores.Where(s => s.User == user).FirstOrDefault();
            if (store == null) { return BadRequest("You do not own a store"); }
            Product? product = _context.Products.Where(p => p.Name == oldName && p.Store == store && p.User == user).FirstOrDefault(); // REVIEW THIS TO SEE IF WORKS
            if (product == null) { return BadRequest("This product either doesn't exist, or you do not own it"); }
            product.Name = newName.Length == 0 ? oldName : newName;
            product.Price = newPrice != 0? newPrice : product.Price;
            if (file != null)
            {
                var extension = Path.GetExtension(file.FileName);
                string imgName = $"{product.Name}{extension}";
                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Images", imgName);
                if (System.IO.File.Exists(product.ImagePath))
                {
                    System.IO.File.Delete(product.ImagePath);
                }
                using (var stream = new FileStream(uploadPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                product.ImagePath = uploadPath;
            }
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return Ok("Product updated");
        }
    }
}
