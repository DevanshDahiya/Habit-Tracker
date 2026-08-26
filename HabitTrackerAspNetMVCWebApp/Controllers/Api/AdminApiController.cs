using HabitTrackerAspNetMVCWebApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HabitTrackerAspNetMVCWebApp.Controllers.Api
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public AdminApiController(ApplicationDbContext context) => _context = context;

        [HttpGet("users")]
        public async Task<ActionResult<object>> GetUsers()
        {
            var users = await _context.Users.OrderBy(u => u.Email).Select(u => new { u.Id, u.Email, u.UserName, u.IsActive }).ToListAsync();
            var counts = await _context.Habits.GroupBy(h => h.UserId).Select(g => new { userId = g.Key, count = g.Count() }).ToListAsync();
            return new { users, habitCounts = counts };
        }

        [HttpPost("seed-demo")]
        public async Task<IActionResult> SeedDemo([FromServices] IServiceProvider sp)
        {
            await DemoDataSeeder.SeedAsync(sp);
            return Ok(new { message = "Demo data seeded" });
        }
    }
}
