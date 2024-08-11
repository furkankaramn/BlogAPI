using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Blog.Data;
using Blog.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LikesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public LikesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Likes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Like>>> GetLikes()
        {
            return await _context.Likes.ToListAsync();
        }

        // GET: api/Likes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Like>> GetLike(int id)
        {
            var like = await _context.Likes.FindAsync(id);

            if (like == null)
            {
                return NotFound();
            }

            return like;
        }

        // PUT: api/Likes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLike(int id, Like like)
        {
            if (id != like.Id)
            {
                return BadRequest();
            }

            _context.Entry(like).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LikeExists(id))
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

        // POST: api/Likes
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Like>> PostLike(Like like)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            like.MembersId = userId;

            var existingLike = await _context.Likes.FirstOrDefaultAsync(l =>
                l.MembersId == userId &&
                (l.PostsId == like.PostsId || l.CommentsId == like.CommentsId));

            if (existingLike != null)
            {
                return BadRequest("Like already exists for this post/comment by the same user.");
            }

            _context.Likes.Add(like);

            // If the like is for a comment, update the comment's like count
            if (like.CommentsId.HasValue)
            {
                var comment = await _context.Comments.FindAsync(like.CommentsId.Value);
                if (comment != null)
                {
                    comment.LikeCount++;
                }
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLike", new { id = like.Id }, like);
        }


        // DELETE: api/Likes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLike(int id)
        {
            var like = await _context.Likes.FindAsync(id);
            if (like == null)
            {
                return NotFound();
            }

            _context.Likes.Remove(like);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LikeExists(int id)
        {
            return _context.Likes.Any(e => e.Id == id);
        }
    }
}
