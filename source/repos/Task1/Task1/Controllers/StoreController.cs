using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Task1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoreController : ControllerBase
    {
        private readonly DataContext _context;

        public StoreController(DataContext context)
        {
            _context = context;
        }
        [HttpPost("createStore")]
        public async Task<IActionResult> CreateStore(string storeName, string brandName, string initialStoreAddress, string initialAddressStatus)
        {
            User? user = (User?)HttpContext.Items["User"];
            if (user == null) { return Unauthorized(); }
            if (_context.Stores.Where(s => s.Name == storeName).Any()) { return BadRequest("The provided store name is taken"); }
            Brand? brand = await _context.Brands.FindAsync(brandName);
            if (brand == null) { return BadRequest("No such brand exists"); }
            Store store = new Store { Name = storeName, Brand = brand, User = user };
            await _context.Stores.AddAsync(store);
            await _context.StoreAddresses.AddAsync(new StoreAddress { AddressName = initialStoreAddress, Store = store, Status = initialAddressStatus});
            await _context.SaveChangesAsync();
            return Ok("Store created successfully");
        }

        [HttpPost("addStoreAddress")]
        public async Task<IActionResult> AddStoreAddress(string address, string status)
        {
            User? user = (User?)HttpContext.Items["User"];
            if (user == null) { return Unauthorized(); }
            var store = _context.Stores.Where(s => s.User == user).FirstOrDefault();
            if (store == null) { return BadRequest("You do not own a store"); }

            try
            {
                await _context.StoreAddresses.AddAsync(new StoreAddress { AddressName = address, Store = store, Status = status });
                await _context.SaveChangesAsync();
                return Ok($"Address: ({address}) for store {store.Name} has been registered");
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost("editStoreAddress")]
        public IActionResult EditStoreAddress(string oldAddress, string newAddress)
        {
            User? user = (User?)HttpContext.Items["User"];
            if (user == null) { return Unauthorized(); }
            return Ok();
        }

        [HttpGet("getAddresses")]
        public IActionResult GetStoreAddresses()
        {
            User? user = (User?)HttpContext.Items["User"];
            if (user == null) { return Unauthorized(); }
            Store? store = _context.Stores.Where(s => s.User == user).FirstOrDefault();
            if (store == null) { return BadRequest("You do not own a store"); }
            IEnumerable<StoreAddress> addresses = _context.StoreAddresses.Where(sa => sa.Store == store).ToArray();
            foreach(StoreAddress address in addresses)
            {
                if (address.Store != null)
                {
                    address.Store.Addresses = null;
                }
            }

            return Ok(addresses);
        }

        [HttpPatch("setStoreAddressStatus")]
        public IActionResult SetStoreAddressStatus(string address, string newStatus)
        {
            User? user = (User?)HttpContext.Items["User"];
            if (user == null) { return Unauthorized(); }
            Store? store = _context.Stores.Where(s => s.User == user).FirstOrDefault();
            if (store == null) { return BadRequest("You do not own a store"); }
            StoreAddress? storeAddress = _context.StoreAddresses.Where(sa => sa.Store == store && sa.AddressName == address).FirstOrDefault();
            if (storeAddress == null) { return BadRequest("Your store does not have a branch in this address"); }
            storeAddress.Status = newStatus;
            try
            {
                _context.StoreAddresses.Update(storeAddress);
                _context.SaveChanges();
                return Ok($"Successfully updated store address {address} with status {newStatus}");
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPatch("removeStoreBrand")]
        public IActionResult RemoveStoreBrand()
        {
            User? user = (User?)HttpContext.Items["User"];
            if (user == null) { return Unauthorized(); }
            Store? store = _context.Stores.Where(s => s.User == user).FirstOrDefault();
            if (store == null) { return BadRequest("You do not own a store"); }
            try
            {
                store.BrandName = null;
                _context.Stores.Update(store);
                _context.SaveChanges();
                return Ok("Your store does not have a brand anymore");
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPatch("setStoreBrand")]
        public IActionResult SetStoreBrand(string brandName)
        {
            User? user = (User?)HttpContext.Items["User"];
            if (user == null) { return Unauthorized(); }
            Store? store = _context.Stores.Where(s => s.User == user).FirstOrDefault();
            if (store == null) { return BadRequest("You do not own a store"); }
            if (!_context.Brands.Where(b => b.Name == brandName).Any()) { return BadRequest("This brand does not exist"); }
            try
            {
                store.BrandName = brandName;
                _context.Stores.Update(store);
                _context.SaveChanges();
                return Ok($"Your store's brand is now {brandName}");
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }


        [HttpDelete("deleteStoreAddress")]
        public IActionResult deleteStoreAddress(string address)
        {
            User? user = (User?)HttpContext.Items["User"];
            if (user == null) { return Unauthorized(); }
            Store? store = _context.Stores.Where(s => s.User == user).FirstOrDefault();
            if (store == null) { return BadRequest("You do not own a store"); }
            StoreAddress? storeAddress = _context.StoreAddresses.Where(sa => sa.Store == store && sa.AddressName == address).FirstOrDefault();
            if (storeAddress == null) { return BadRequest("Your store does not have a branch in this address"); }
            try
            {
                _context.StoreAddresses.Remove(storeAddress);
                _context.SaveChanges();
                return Ok($"Successfully deleted this store address: {address}");
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }
    }
}
