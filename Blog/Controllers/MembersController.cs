using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Blog.Data;
using Blog.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Mail;

namespace Blog.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public MembersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: api/Members
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Member>>> GetMembers()
        {
            return await _context.Members.ToListAsync();
        }

        // GET: api/Members/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Member>> GetMember(string id)
        {
            var member = await _context.Members.FindAsync(id);

            if (member == null)
            {
                return NotFound();
            }

            return member;
        }

        // PUT: api/Members/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMember(string id, Member member)
        {
            if (id != member.Id)
            {
                return BadRequest();
            }

            _context.Entry(member).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MemberExists(id))
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

        // POST: api/Members
        
        [HttpPost]
        public async Task<ActionResult<Member>> PostMember(Member member)
        {
            if (_context.Members == null)
            {
                return Problem("Entity set 'ApplicationContext.Members' is null.");
            }

            // Yeni kullanıcı oluşturma
            var result = await _userManager.CreateAsync(member.ApplicationUser!, member.ApplicationUser!.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // Role atama
            var roleResult = await _userManager.AddToRoleAsync(member.ApplicationUser, "Member");
            if (!roleResult.Succeeded)
            {
                return BadRequest(roleResult.Errors);
            }

            // Member kaydını veritabanına ekleme
            member.Id = member.ApplicationUser!.Id;
            member.ApplicationUser = null;
            _context.Members.Add(member);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (MemberExists(member.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetMember", new { id = member.Id }, member);
        }

        // DELETE: api/Members/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMember(string id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            _context.Members.Remove(member);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Members/Login
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

        // POST: api/Members/Logout
        [Authorize(Roles = "Member")]
        [HttpPost("Logout")]
        public async Task<ActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }

        // POST: api/Members/ForgetPassword
        [HttpPost("ForgetPassword")]
        public async Task<ActionResult<string>> ForgetPassword(string userName)
        {
            var applicationUser = await _userManager.FindByNameAsync(userName);
            if (applicationUser == null)
            {
                return NotFound();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(applicationUser);

            // For demonstration purposes, we're returning the token directly.
            // In a real application, you would send the token via email.
            var mailMessage = new MailMessage("abc@abc.com", applicationUser.Email, "Şifre Sıfırlama", token);
            var smtpClient = new SmtpClient("smtp.domain.com");
            smtpClient.Send(mailMessage);

            return Ok(token);
        }

        // POST: api/Members/ResetPassword
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

        private bool MemberExists(string id)
        {
            return _context.Members.Any(e => e.Id == id);
        }
    }
}
