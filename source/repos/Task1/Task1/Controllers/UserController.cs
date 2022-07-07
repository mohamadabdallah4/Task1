using Microsoft.AspNetCore.Mvc;

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
        private const int maxFileSize = 2 * 1024 * 1024;

        public UserController(DataContext context, IUserService userService, IEmailService emailService, IJwtUtils jwtUtils)
        {
            _context = context;
            _userService = userService;
            _emailService = emailService;
            _jwtUtils = jwtUtils;
        }

        [HttpPost("changePassword")] // TESTED
        public ActionResult ChangePassword([FromBody] NewPasswordRequest request)
        {
            User? user = _context.Users.Where(x => x.Email == request.Email).FirstOrDefault();
            if (user == null) { return BadRequest("User not found!"); }
            if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.Password)) { return BadRequest("Incorrect password"); }
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            if (_context.UserPasswords.AsEnumerable().Where(x => { return x.Email == request.Email && BCrypt.Net.BCrypt.Verify(request.NewPassword,x.Password); }).Any()) { return BadRequest("You cannot use one of your old passwords"); }
            user.Password = hashedPassword;
            user.LastPasswordChange = DateTime.UtcNow.ToString(); 
            _context.Users.Update(user);
            _context.UserPasswords.Add(new UserPassword { Email = user.Email, Password = user.Password });
            _context.SaveChanges();
            return Ok("Password changed");
        }

        [HttpPost("register")] // TESTED
        public async Task<ActionResult> Register([FromBody] UserRegisterRequest request)
        {
            User? AlreadyExistingUser = _context.Users.Where(u => u.Email == request.Email).FirstOrDefault();
            if (AlreadyExistingUser != null) { return BadRequest("Email already registered"); }

            string RandomPassword = _userService.GeneratePassword();

            User user = new User { 
                FirstName = request.FirstName, 
                LastName = request.LastName, 
                Email = request.Email, 
                Password = BCrypt.Net.BCrypt.HashPassword(RandomPassword),
                LastPasswordChange = DateTime.UtcNow.ToString() 
            };
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
                    return BadRequest(new { message = "Something went wrong with your email" });
                }
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
            
        }

        [HttpPost("authenticate")] // TESTED
        public ActionResult Authenticate([FromBody] UserLoginRequest request)
        {
            User? user = _context.Users.Where(x => x.Email == request.Email).FirstOrDefault();
            if (user == null) { return BadRequest("You are not registered!"); }
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password)) { return BadRequest("Incorrect password"); }
            string token = _jwtUtils.GenerateToken(user);
            return Ok(new { message = "You have been logged in!", token = token });
        }

        [HttpPut("confirmNewEmail")] // TESTED
        public async Task<ActionResult> ConfirmNewEmail([Required] string confirmationCode, [Required] string newEmail)
        {
            User? unconfirmedUser = _context.Users.Where(x => x.Email == newEmail).FirstOrDefault();
            if (unconfirmedUser == null) { return BadRequest("You do not own an account! (You must register first)"); }
            if (confirmationCode != unconfirmedUser.ConfirmationCode) { return BadRequest("Incorrect confirmation code, please check your email to get the correct one"); }
            unconfirmedUser.ConfirmationCode = null;
            unconfirmedUser.Confirmed = true;
            await _context.SaveChangesAsync();
            return Ok("Your new email has been confirmed!");
        }
        [Authorize]
        [HttpPatch("setProfilePicture")] // TESTED
        public async Task<ActionResult> PostProfilePicture([Required] [AllowedExtensions(new string[] { ".jpg", ".png" })][MaxFileSize(maxFileSize)] IFormFile file)
        {
            User? user = (User?) HttpContext.Items["User"];
            try
            {
                var extension = Path.GetExtension(file.FileName).ToLower();
                string imgName = $"{user.Email}{extension}";
                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "General\\ProfilePictures", imgName);
                if (System.IO.File.Exists(uploadPath))
                {
                    System.IO.File.Delete(uploadPath);
                }
                using (var stream = new FileStream(uploadPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }  
                user.ImagePath = uploadPath;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return Ok("Profile picture successfully uploaded");
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
            
        }
        [Authorize]
        [HttpPut("updateUser")] // TESTED
        public async Task<ActionResult> UpdateUser([FromBody] UserUpdateRequest request)
        {
            User? user = (User?)HttpContext.Items["User"];
            try
            {
                user.FirstName = request.FirstName.Length == 0 ? user.FirstName : request.FirstName;
                user.LastName = request.LastName.Length == 0 ? user.LastName : request.LastName;
                if (request.Email != user.Email)
                {
                    if (_context.Users.Where(u => u.Email == request.Email).Any()) { return BadRequest("This email is already taken by someone else!"); }
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
