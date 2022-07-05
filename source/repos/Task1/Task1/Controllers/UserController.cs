using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Task1.Data;
using Task1.Models;
using BCrypt.Net;
using Task1.Authorization;

namespace Task1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly IJwtUtils _jwtUtils;
        private Random random = new Random();


        public UserController(DataContext context, IUserService userService, IEmailService emailService, IJwtUtils jwtUtils)
        {
            _context = context;
            _userService = userService;
            _emailService = emailService;
            _jwtUtils = jwtUtils;
        }

        [HttpPost("changePassword")]
        public ActionResult ChangePassword([FromBody] NewPasswordRequest request)
        {
            User? user = _context.Users.Where(x => x.Email == request.Email).FirstOrDefault();
            if (user == null) { return BadRequest("User not found!"); }
            if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.Password)) { return BadRequest("Incorrect password"); }
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            if (_context.UserPasswords.AsEnumerable().Where(x => { return x.Email == request.Email && BCrypt.Net.BCrypt.Verify(request.NewPassword,x.Password); }).Any()) { return BadRequest("You cannot use one of your old passwords"); }
            user.Password = hashedPassword;
            _context.Users.Update(user);
            _context.UserPasswords.Add(new UserPassword { Email = user.Email, Password = user.Password });
            _context.SaveChanges();
            return Ok("Password changed");
        }

        [HttpPost("register")] 
        public async Task<ActionResult> Register([FromBody] UserRegisterRequest request)
        {
            User? AlreadyExistingUser = _context.Users.Where(u => u.Email == request.Email).FirstOrDefault();
            if (AlreadyExistingUser != null) { return BadRequest("Email already registered"); }

            string RandomPassword = _userService.GeneratePassword();

            User user = new User { FirstName = request.FirstName, LastName = request.LastName, Email = request.Email, Password = BCrypt.Net.BCrypt.HashPassword(RandomPassword) };
            try
            {
                _context.Users.Add(user);
                _context.UserPasswords.Add(new UserPassword { Email = user.Email, Password = user.Password });
                await _context.SaveChangesAsync();

                bool emailSent = _emailService.SendEmail(new EmailRequest { To = user.Email, Subject = "Registration successful", Body = $"Successfully registered {user.Email} with temporary password {RandomPassword}" });
                if (emailSent)
                {
                    return Ok(new { message = "Successfully registered" });
                }
                else
                {
                    return BadRequest(new { message = "Could not register user!" });
                }
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
            
        }

        [HttpPost("authenticate")]
        public ActionResult Authenticate([FromBody] UserLoginRequest request)
        {
            User? user = _context.Users.Where(x => x.Email == request.Email).FirstOrDefault();
            if (user == null) { return BadRequest("You are not registered!"); }
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password)) { return BadRequest("Incorrect password"); }
            string token = _jwtUtils.GenerateToken(user);
            return Ok(new { message = "You have been logged in!", token = token });
        }

        [HttpPut("confirmNewEmail")]
        public async Task<ActionResult> ConfirmNewEmail([Required] string confirmationCode, [Required] string newEmail)
        {
            User? unconfirmedUser = _context.Users.Where(x => x.Email == newEmail).FirstOrDefault();
            if (unconfirmedUser == null) { return BadRequest("This email is not unconfirmed"); }
            if (confirmationCode != unconfirmedUser.ConfirmationCode) { return BadRequest("Incorrect confirmation code, please check your email to get the correct one"); }
            unconfirmedUser.ConfirmationCode = null;
            unconfirmedUser.Confirmed = true;
            await _context.SaveChangesAsync();
            return Ok("Your new email has been confirmed!");
        }

        [HttpPost("setProfilePicture")]
        public ActionResult PostProfilePicture(IFormFile file)
        {
            User? user = (User?) HttpContext.Items["User"];
            if (user == null)
            {
                return Unauthorized();
            }
            try
            {
                if (file == null) { return BadRequest("No file was provided"); }
                if (file.Length > (2 * 1024 * 1024)) { return BadRequest("File size must be less than 2 MB"); }

                var extension = Path.GetExtension(file.FileName).ToLower();
                if (extension != ".jpg" && extension != ".png") { return BadRequest("File must be in jpg or png format"); }

                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Images", file.FileName);
                var stream = new FileStream(uploadPath, FileMode.Create);
                file.CopyTo(stream);
                user.ImagePath = uploadPath;
                _context.Users.Update(user);
                _context.SaveChanges();

                return Ok($"Received file {file.FileName} with size in bytes {file.Length}");
            }
            catch (Exception)
            {
                return BadRequest("An error occured with the file");
            }
            
        }

        [HttpPut("updateUser")] // NOT DONE
        public async Task<ActionResult> UpdateUser([FromBody] UserUpdateRequest request)
        {
            User? user = (User?)HttpContext.Items["User"];
            if (user == null)
            {
                return Unauthorized();
            }
            var oldEmail = user.Email;
            try
            {
                user.FirstName = request.FirstName.Length == 0 ? user.FirstName : request.FirstName;
                user.LastName = request.LastName.Length == 0 ? user.LastName : request.LastName;
                if (request.Email != user.Email)
                {
                    string code = random.Next(0, 99999).ToString("D6");
                    user.ConfirmationCode = code;
                    user.Confirmed = false;
                    user.Email = request.Email;
                    _context.Users.Update(user);
                    bool emailSent = _emailService.SendEmail(new EmailRequest { To = request.Email, Subject = "Email change request", Body = $"Please confirm your new email by sending us this confirmation code: {code}" });
                    if (emailSent)
                    {
                        await _context.SaveChangesAsync();
                        return Ok("Update done successfully, we sent you a confirmation code to allow you to confirm your new email.");
                    }
                    else
                    {
                        return BadRequest(new { message = "Your new email is invalid! Please use a valid email" });
                    }
                }
                else
                {
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();
                    return Ok("Update done successfully");
                }
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
            
        }
    }
}
