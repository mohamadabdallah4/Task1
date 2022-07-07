using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Task1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandController : ControllerBase
    {
        private readonly DataContext _context;

        public BrandController(DataContext context)
        {
            _context = context;
        }
        [HttpGet("getAllBrands")] // TESTED
        public IAsyncEnumerable<Brand> GetAllBrands()
        {
            return _context.Brands.Include(b => b.User).AsAsyncEnumerable();
        }
        [HttpPost ("addNewBrand")] // TESTED
        [Authorize]
        public async Task<IActionResult> AddBrand(string Name)
        {
            User? user = (User?) HttpContext.Items["User"];
            if (_context.Brands.Where(b => b.Name == Name).Any()) { return BadRequest("This brand name is already taken"); }
            try
            {
                await _context.Brands.AddAsync(new Brand { Name = Name, User = user });
                await _context.SaveChangesAsync();
                return Ok($"Brand {Name} added");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            
        }
    }
}
