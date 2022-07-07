using Microsoft.AspNetCore.Mvc;

namespace Task1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StoreController : ControllerBase
    {
        private readonly DataContext _context;
        private const int maxFileSize = 2 * 1024 * 1024;

        public StoreController(DataContext context)
        {
            _context = context;
        }
        [HttpPost("createStore")] // TESTED
        public async Task<ActionResult> CreateStore(NewStoreRequest request)
        {
            User? user = (User?)HttpContext.Items["User"];
            if (_context.Stores.Where(s => s.Name == request.storeName).Any()) { return BadRequest("The provided store name is taken"); }
            if (_context.Stores.Where(s => s.User == user).Any()) { return BadRequest("You already own a store (only 1 store per user is allowed)"); }
            Brand? brand = await _context.Brands.FindAsync(request.brandName);
            if (brand == null) { return BadRequest("No such brand exists"); }
            Store store = new() { Name = request.storeName, Brand = brand, User = user };
            await _context.Stores.AddAsync(store);
            await _context.StoreAddresses.AddAsync(new StoreAddress { AddressName = request.initialStoreAddress, Store = store, Status = request.initialAddressStatus });
            await _context.SaveChangesAsync();
            return Ok("Store created successfully");
        }
        
        [HttpPatch("setStoreLogo")]
        public async Task<ActionResult> SetStoreLogo([Required][AllowedExtensions(new string[] { ".jpg", ".png" })][MaxFileSize(maxFileSize)] IFormFile file)
        {
            User? user = (User?)HttpContext.Items["User"];
            Store? store = _context.Stores.Where(s => s.User == user).FirstOrDefault();
            if (store == null) { return BadRequest("You do not own a store"); }
            var extension = Path.GetExtension(file.FileName).ToLower();
            string imgName = $"{store.Name}{extension}";
            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "General\\Logos", imgName);
            if (System.IO.File.Exists(uploadPath))
            {
                System.IO.File.Delete(uploadPath);
            }
            using (var stream = new FileStream(uploadPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            store.ImagePath = uploadPath;
            _context.Stores.Update(store);
            await _context.SaveChangesAsync();

            return Ok($"Logo for store {store.Name} has been added");
        }
        
        [HttpGet("getStore")] // TESTED
        public ActionResult GetStore()
        {
            User? user = (User?)HttpContext.Items["User"];
            Store? store = _context.Stores
                .Include(s => s.Brand)
                .Include(s => s.Products.Where(p => p.deleted == false))
                .ThenInclude(brand => brand.User)
                .Include(s => s.Products).Include(s => s.Addresses)
                .Where(s => s.User == user)
                .FirstOrDefault();
            return store != null? Ok(store) : NotFound("You do not own a store");
        }

        
        [HttpPost("addStoreAddress")] // TESTED
        public async Task<ActionResult> AddStoreAddress(string address, string status)
        {
            User? user = (User?)HttpContext.Items["User"];
            var store = _context.Stores.Where(s => s.User == user).FirstOrDefault();
            if (store == null) { return BadRequest("You do not own a store"); }
            if (_context.StoreAddresses.Where(sa => sa.AddressName == address && sa.Store == store).Any()) { { return BadRequest("You already have an address set for this location"); } }
            _context.StoreAddresses.Add(new StoreAddress { AddressName = address, Store = store, Status = status });
            await _context.SaveChangesAsync();
            return Ok($"Address: ({address}) for store {store.Name} has been registered");
        }

        [HttpPatch("editStoreAddress")] // TESTED
        public async Task<ActionResult> EditStoreAddress(string oldAddress, string newAddress)
        {
            User? user = (User?)HttpContext.Items["User"];
            Store? store = _context.Stores.Where(s => s.User == user).FirstOrDefault();
            if (store == null) { return BadRequest("You do not own a store"); }
            StoreAddress? storeAddress = _context.StoreAddresses.Where(sa => sa.AddressName == oldAddress && sa.Store == store).FirstOrDefault();
            if (storeAddress == null) { return NotFound("Address was not found!"); }
            storeAddress.AddressName = newAddress;
            _context.StoreAddresses.Update(storeAddress);
            await _context.SaveChangesAsync();
            return Ok($"Address ({oldAddress}) has been updated to ({newAddress}) for store ({store.Name})");
        }

        [HttpGet("getAddresses")] // TESTED, BUT MUST REVISIT
        public ActionResult GetStoreAddresses()
        {
            User? user = (User?)HttpContext.Items["User"];
            Store? store = _context.Stores.Where(s => s.User == user).FirstOrDefault();
            if (store == null) { return BadRequest("You do not own a store"); }
            IEnumerable<StoreAddress> addresses = _context.StoreAddresses.Where(sa => sa.Store == store).ToArray();
            /*
            foreach(StoreAddress address in addresses)
            {
                if (address.Store != null)
                {
                    address.Store.Addresses = null;
                }
            }
            */
            return Ok(addresses);
        }

        [HttpPatch("setStoreAddressStatus")] // TESTED
        public async Task<ActionResult> SetStoreAddressStatus(string address, string newStatus)
        {
            User? user = (User?)HttpContext.Items["User"];
            Store? store = _context.Stores.Where(s => s.User == user).FirstOrDefault();
            if (store == null) { return BadRequest("You do not own a store"); }
            StoreAddress? storeAddress = _context.StoreAddresses.Where(sa => sa.Store == store && sa.AddressName == address).FirstOrDefault();
            if (storeAddress == null) { return BadRequest("Your store does not have a branch at this address"); }
            storeAddress.Status = newStatus;
            try
            {
                _context.StoreAddresses.Update(storeAddress);
                await _context.SaveChangesAsync();
                return Ok($"Successfully updated store address {address} with status {newStatus}");
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPatch("unassignStoreBrand")] // TESTED
        public async Task<ActionResult> RemoveStoreBrand()
        {
            User? user = (User?)HttpContext.Items["User"];
            Store? store = _context.Stores.Where(s => s.User == user).FirstOrDefault();
            if (store == null) { return BadRequest("You do not own a store"); }
            store.BrandName = null;
            _context.Stores.Update(store);
            await _context.SaveChangesAsync();
            return Ok("Your store does not have a brand anymore");
        }

        [HttpPatch("setStoreBrand")] // TESTED
        public async Task<ActionResult> SetStoreBrand(string brandName)
        {
            User? user = (User?)HttpContext.Items["User"];
            Store? store = _context.Stores.Where(s => s.User == user).FirstOrDefault();
            if (store == null) { return BadRequest("You do not own a store"); }
            Brand? brand = _context.Brands.Where(b => b.Name == brandName).FirstOrDefault();
            if (brand == null) { return BadRequest("This brand does not exist"); }
            try
            {
                store.Brand = brand;
                _context.Stores.Update(store);
                await _context.SaveChangesAsync();
                return Ok($"Brand for your store has been changed to {brand.Name}");
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }


        [HttpDelete("deleteStoreAddress")] // PAY ATTENTION TO OSSIT L INITIAL ADDRESS
        public async Task<ActionResult> deleteStoreAddress(string address)
        {
            User? user = (User?)HttpContext.Items["User"];
            Store? store = _context.Stores.Where(s => s.User == user).FirstOrDefault();
            if (store == null) { return BadRequest("You do not own a store"); }
            StoreAddress? storeAddress = _context.StoreAddresses.Where(sa => sa.Store == store && sa.AddressName == address).FirstOrDefault();
            if (storeAddress == null) { return BadRequest("Your store does not have a branch in this address"); }
            try
            {
                _context.StoreAddresses.Remove(storeAddress);
                await _context.SaveChangesAsync();
                return Ok($"Successfully deleted this store address: {address}");
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }
    }
}
