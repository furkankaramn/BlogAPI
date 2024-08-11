using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Blog.Data;
using Blog.Models;

namespace Blog.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AdminsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: api/Admins
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Admin>>> GetAdmins()
        {
            return await _context.Admins.ToListAsync();
        }

        // GET: api/Admins/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Admin>> GetAdmin(string id)
        {
            var admin = await _context.Admins.FindAsync(id);

            if (admin == null)
            {
                return NotFound();
            }

            return admin;
        }

        // PUT: api/Admins/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAdmin(string id, Admin admin)
        {
            if (id != admin.Id)
            {
                return BadRequest();
            }

            _context.Entry(admin).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AdminExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Admins
        
        [HttpPost]
        public async Task<ActionResult<Admin>> PostAdmin(Admin admin)
        {
            if (_context.Admins == null)
            {
                return Problem("Entity set 'ApplicationContext.Admins' is null.");
            }

            // Yeni kullanıcı oluşturma
            var result = await _userManager.CreateAsync(admin.ApplicationUser!, admin.ApplicationUser!.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // Role atama
            var roleResult = await _userManager.AddToRoleAsync(admin.ApplicationUser, "Admin");
            if (!roleResult.Succeeded)
            {
                return BadRequest(roleResult.Errors);
            }

            // Admin kaydını veritabanına ekleme
            admin.Id = admin.ApplicationUser!.Id;
            admin.ApplicationUser = null;
            _context.Admins.Add(admin);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (AdminExists(admin.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetAdmin", new { id = admin.Id }, admin);
        }

        // DELETE: api/Admins/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdmin(string id)
        {
            var admin = await _context.Admins.FindAsync(id);
            if (admin == null)
            {
                return NotFound();
            }

            _context.Admins.Remove(admin);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AdminExists(string id)
        {
            return _context.Admins.Any(e => e.Id == id);
        }

        [HttpPost("Login")]
        public async Task<ActionResult> Login(string userName, string password)
        {
            var applicationUser = await _userManager.FindByNameAsync(userName);
            if (applicationUser == null)
            {
                return Unauthorized();
            }

            var signInResult = await _signInManager.PasswordSignInAsync(applicationUser, password, false, false);
            if (signInResult.Succeeded)
            {
                return Ok();
            }

            return Unauthorized();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("Logout")]
        public async Task<ActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }

        [HttpPost("ForgetPassword")]
        public async Task<ActionResult<string>> ForgetPassword(string userName)
        {
            var applicationUser = await _userManager.FindByNameAsync(userName);
            if (applicationUser == null)
            {
                return NotFound();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(applicationUser);
            var mailMessage = new System.Net.Mail.MailMessage("abc@abc", applicationUser.Email, "Şifre sıfırlama", token);
            var smtpClient = new System.Net.Mail.SmtpClient("http://smtp.domain.com");
            smtpClient.Send(mailMessage);
            return token;
        }

        [HttpPost("ResetPassword")]
        public async Task<ActionResult> ResetPassword(string userName, string token, string newPassword)
        {
            var applicationUser = await _userManager.FindByNameAsync(userName);
            if (applicationUser == null)
            {
                return NotFound();
            }

            var result = await _userManager.ResetPasswordAsync(applicationUser, token, newPassword);
            if (result.Succeeded)
            {
                return Ok();
            }

            return BadRequest(result.Errors);
        }
    }
}
