using System.Security.Claims;
using HabitTrackerAspNetMVCWebApp.Data;
using HabitTrackerAspNetMVCWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HabitTrackerAspNetMVCWebApp.Controllers.Api
{
    [Route("api/kanban")]
    [ApiController]
    [Authorize]
    public class KanbanApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public KanbanApiController(ApplicationDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<object>> GetBoard()
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var habits = await _context.Habits.Where(h => h.UserId == uid).ToListAsync();
            return new
            {
                todo = habits.Where(h => h.KanbanStatus == KanbanStatus.Todo).Select(h => new { h.Id, h.Title, h.Description, h.Frequency, h.Status }),
                inProgress = habits.Where(h => h.KanbanStatus == KanbanStatus.InProgress).Select(h => new { h.Id, h.Title, h.Description, h.Frequency, h.Status }),
                done = habits.Where(h => h.KanbanStatus == KanbanStatus.Done).Select(h => new { h.Id, h.Title, h.Description, h.Frequency, h.Status })
            };
        }

        [HttpPost("{id}/move")]
        public async Task<IActionResult> Move(int id, [FromQuery] KanbanStatus status)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var habit = await _context.Habits.FirstOrDefaultAsync(h => h.Id == id && h.UserId == uid);
            if (habit == null) return NotFound();
            habit.KanbanStatus = status;
            await _context.SaveChangesAsync();
            return Ok(new { habit.Id, kanbanStatus = habit.KanbanStatus.ToString() });
        }
    }
}
