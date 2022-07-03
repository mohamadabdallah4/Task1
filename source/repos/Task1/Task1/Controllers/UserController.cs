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

        [HttpPost("setPassword")]
        public ActionResult SetPassword([FromBody] NewPasswordRequest request)
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

            bool emailSent = _emailService.SendEmail(new EmailRequest { To = user.Email, Subject = "Registration successful", Body = $"Successfully registered {user.Email} with temporary password {RandomPassword}" });
            if (emailSent)
            {
                await _context.Users.AddAsync(user);
                await _context.UserPasswords.AddAsync(new UserPassword { Email = user.Email, Password = user.Password });
                await _context.SaveChangesAsync();
                return Ok(new { message = "Successfully registered" });
            }
            else
            {
                return BadRequest(new { message = "Could not register user!" });
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
        public ActionResult ConfirmNewEmail([FromBody] EmailConfirmationRequest request)
        {
            UnconfirmedUser? unconfirmedUser = _context.UnconfirmedUsers.Where(x => x.Email == request.NewEmail).FirstOrDefault();
            if (unconfirmedUser == null) { return BadRequest("This email is not unconfirmed"); }
            if (request.ConfirmationCode != unconfirmedUser.ConfirmationCode) { return BadRequest("Incorrect confirmation code, please check your email to get the correct one"); }
            _context.Users.Add(new User
            {
                Email = request.NewEmail,
                FirstName = unconfirmedUser.FirstName,
                LastName = unconfirmedUser.LastName,
                Password = unconfirmedUser.Password
            });
            _context.UnconfirmedUsers.Remove(unconfirmedUser);
            _context.SaveChanges();
            return Ok("Your new email has been confirmed!");
        }

        [HttpPost("profilePicture")]
        public ActionResult PostProfilePicture()
        {
            IFormFile file = Request.Form.Files.FirstOrDefault();
            if (file == null) { return BadRequest("No file"); }
            return Ok($"Received file {file.FileName} with size in bytes {file.Length}");
        }

        [HttpPut("update")]
        public ActionResult Update([FromBody] UserUpdateRequest request)
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token == null) { return Unauthorized(); }
            int? id = _jwtUtils.GetIdFromToken(token);
            if (id != null)
            {
                User? user = _userService.GetById(id.Value);
                if (user == null) { return BadRequest("User not found!"); }
                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                if (request.Email != user.Email)
                {
                    string code = random.Next(0, 99999).ToString("D6");
                    _context.UnconfirmedUsers.Add(new UnconfirmedUser
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = request.Email,
                        ConfirmationCode = code,
                        Password = user.Password
                    });
                    _context.Users.Remove(user);
                    bool emailSent = _emailService.SendEmail(new EmailRequest { To = request.Email, Subject = "Email change request", Body = $"Please confirm your new email by sending us this confirmation code: {code}" });
                    if (emailSent)
                    {
                        _context.SaveChanges();
                        return Ok("Update done successfully, you need to confirm your new email");
                    }
                    else
                    {
                        return BadRequest(new { message = "Your new email is invalid!" });
                    }
                    
                }
                else
                {
                    _context.Users.Update(user);
                    _context.SaveChanges();
                    return Ok("Update done successfully");
                }
            }
            return Unauthorized(); ;
        }
    }
}
