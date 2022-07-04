using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Task1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly DataContext _context;

        public ProductController(DataContext dataContext)
        {
            _context = dataContext;
        }

        [HttpPost("addProduct")]
        public IActionResult AddProduct([FromBody] Product product)
        {
            User? user = (User?)HttpContext.Items["User"];
            if (user == null) { return Unauthorized(); } 
            try
            {
                _context.Products.Add(product);
                _context.SaveChanges();
                return Ok("Product has been added!");
            }
            catch (Exception e) { return BadRequest(e); }
        }
    }
}
