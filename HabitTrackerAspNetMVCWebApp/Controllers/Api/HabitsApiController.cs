using System.Security.Claims;
using HabitTrackerAspNetMVCWebApp.Data;
using HabitTrackerAspNetMVCWebApp.DTOs;
using HabitTrackerAspNetMVCWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HabitTrackerAspNetMVCWebApp.Controllers.Api
{
    [Route("api/habits")]
    [ApiController]
    [Authorize]
    public class HabitsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HabitsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        private bool IsAdmin() => User.IsInRole("Admin");

        private static HabitDto ToDto(Habit h) => new HabitDto
        {
            Id = h.Id,
            Title = h.Title,
            Description = h.Description,
            Frequency = h.Frequency,
            Status = h.Status,
            KanbanStatus = h.KanbanStatus,
            StartDate = h.StartDate,
            EndDate = h.EndDate,
            UserId = h.UserId,
            UserEmail = h.User?.Email
        };

        // GET: api/habits
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HabitDto>>> GetHabits([FromQuery] string? userId, [FromQuery] bool all = false)
        {
            var currentUserId = GetUserId();
            var isAdmin = IsAdmin();
            IQueryable<Habit> query = _context.Habits.Include(h => h.User);

            if (isAdmin)
            {
                if (all) { /* all */ }
                else if (!string.IsNullOrEmpty(userId)) query = query.Where(h => h.UserId == userId);
                else query = query.Where(h => h.UserId == currentUserId);
            }
            else
            {
                query = query.Where(h => h.UserId == currentUserId);
            }

            var habits = await query.OrderBy(h => h.StartDate).ToListAsync();
            return habits.Select(ToDto).ToList();
        }

        // GET: api/habits/5
        [HttpGet("{id}")]
        public async Task<ActionResult<HabitDto>> GetHabit(int id)
        {
            var habit = await _context.Habits.Include(h => h.User).FirstOrDefaultAsync(h => h.Id == id);
            if (habit == null) return NotFound();
            if (!IsAdmin() && habit.UserId != GetUserId()) return Forbid();
            return ToDto(habit);
        }

        // POST: api/habits
        [HttpPost]
        public async Task<ActionResult<HabitDto>> CreateHabit(CreateHabitDto dto, [FromQuery] string? ownerUserId)
        {
            var currentUserId = GetUserId();
            var isAdmin = IsAdmin();
            string targetUserId = currentUserId;

            if (isAdmin && !string.IsNullOrEmpty(ownerUserId))
            {
                var exists = await _context.Users.AnyAsync(u => u.Id == ownerUserId && u.IsActive);
                if (!exists) return BadRequest("Selected user not found.");
                targetUserId = ownerUserId;
            }

            var habit = new Habit
            {
                Title = dto.Title,
                Description = dto.Description,
                Frequency = dto.Frequency,
                Status = dto.Status,
                KanbanStatus = dto.KanbanStatus,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                UserId = targetUserId
            };

            _context.Habits.Add(habit);
            await _context.SaveChangesAsync();
            await _context.Entry(habit).Reference(h => h.User).LoadAsync();

            return CreatedAtAction(nameof(GetHabit), new { id = habit.Id }, ToDto(habit));
        }

        // PUT: api/habits/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHabit(int id, UpdateHabitDto dto, [FromQuery] string? ownerUserId)
        {
            if (id != dto.Id) return BadRequest("Id mismatch.");
            var habit = await _context.Habits.FirstOrDefaultAsync(h => h.Id == id);
            if (habit == null) return NotFound();
            if (!IsAdmin() && habit.UserId != GetUserId()) return Forbid();

            if (IsAdmin() && !string.IsNullOrEmpty(ownerUserId))
            {
                var exists = await _context.Users.AnyAsync(u => u.Id == ownerUserId && u.IsActive);
                if (!exists) return BadRequest("Selected user not found.");
                habit.UserId = ownerUserId;
            }

            habit.Title = dto.Title;
            habit.Description = dto.Description;
            habit.Frequency = dto.Frequency;
            habit.Status = dto.Status;
            habit.KanbanStatus = dto.KanbanStatus;
            habit.StartDate = dto.StartDate;
            habit.EndDate = dto.EndDate;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/habits/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHabit(int id)
        {
            var habit = await _context.Habits.FirstOrDefaultAsync(h => h.Id == id);
            if (habit == null) return NotFound();
            if (!IsAdmin() && habit.UserId != GetUserId()) return Forbid();

            _context.Habits.Remove(habit);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // GET: api/habits/stats
        [HttpGet("stats")]
        public async Task<ActionResult<object>> GetStats()
        {
            var uid = GetUserId();
            var total = await _context.Habits.CountAsync(h => h.UserId == uid);
            var active = await _context.Habits.CountAsync(h => h.UserId == uid && h.Status == HabitStatus.Active);
            var completed = await _context.Habits.CountAsync(h => h.UserId == uid && h.Status == HabitStatus.Completed);
            var todo = await _context.Habits.CountAsync(h => h.UserId == uid && h.KanbanStatus == KanbanStatus.Todo);
            var inProgress = await _context.Habits.CountAsync(h => h.UserId == uid && h.KanbanStatus == KanbanStatus.InProgress);
            var done = await _context.Habits.CountAsync(h => h.UserId == uid && h.KanbanStatus == KanbanStatus.Done);
            return new { total, active, completed, todo, inProgress, done };
        }
    }
}
