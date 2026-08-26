using System.Security.Claims;
using HabitTrackerAspNetMVCWebApp.Data;
using HabitTrackerAspNetMVCWebApp.DTOs;
using HabitTrackerAspNetMVCWebApp.Models;
using HabitTrackerAspNetMVCWebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HabitTrackerAspNetMVCWebApp.Controllers.Api
{
    [Route("api/calendar")]
    [ApiController]
    [Authorize]
    public class CalendarApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly HabitScheduleService _schedule = new HabitScheduleService();

        public CalendarApiController(ApplicationDbContext context) => _context = context;

        // GET: api/calendar?year=2026&month=8
        [HttpGet]
        public async Task<ActionResult<object>> GetCalendar([FromQuery] int? year, [FromQuery] int? month)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var today = DateTime.Today;
            int y = year ?? today.Year;
            int m = month ?? today.Month;
            var first = new DateTime(y, m, 1);
            var last = first.AddMonths(1).AddDays(-1);

            var habits = await _context.Habits.Where(h => h.UserId == uid).ToListAsync();
            var logs = await _context.HabitLogs
                .Include(hl => hl.Habit)
                .Where(hl => hl.Habit != null && hl.Habit.UserId == uid && hl.LogDate.Date >= first.Date && hl.LogDate.Date <= last.Date)
                .ToListAsync();

            var days = new List<object>();
            int startOffset = ((int)first.DayOfWeek + 6) % 7;
            var calStart = first.AddDays(-startOffset);
            int endOffset = 6 - (((int)last.DayOfWeek + 6) % 7);
            var calEnd = last.AddDays(endOffset);

            for (var d = calStart; d <= calEnd; d = d.AddDays(1))
            {
                var planned = habits.Where(h => _schedule.IsHabitPlannedForDate(h, d)).Select(h =>
                {
                    var log = logs.FirstOrDefault(l => l.HabitId == h.Id && l.LogDate.Date == d.Date);
                    return new { habitId = h.Id, title = h.Title, status = log?.Status.ToString() };
                }).ToList();
                days.Add(new { date = d.ToString("yyyy-MM-dd"), isCurrentMonth = d.Month == first.Month, isToday = d.Date == today, planned });
            }

            return new { year = y, month = m, monthName = first.ToString("MMMM yyyy"), days };
        }

        // POST: api/calendar/log
        [HttpPost("log")]
        public async Task<IActionResult> SetLog(SetHabitLogDto dto)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var habit = await _context.Habits.FirstOrDefaultAsync(h => h.Id == dto.HabitId && h.UserId == uid);
            if (habit == null) return NotFound("Habit not found.");

            var date = dto.Date.Date;
            var log = await _context.HabitLogs.FirstOrDefaultAsync(l => l.HabitId == dto.HabitId && l.LogDate.Date == date);
            if (log == null)
            {
                log = new HabitLog { HabitId = dto.HabitId, LogDate = date, Status = dto.Status };
                _context.HabitLogs.Add(log);
            }
            else log.Status = dto.Status;

            if (date == DateTime.Today)
            {
                habit.KanbanStatus = dto.Status == HabitLogStatus.Completed ? KanbanStatus.Done : dto.Status == HabitLogStatus.PartiallyCompleted ? KanbanStatus.InProgress : KanbanStatus.Todo;
            }
            await _context.SaveChangesAsync();
            return Ok(new { habitId = dto.HabitId, date = date.ToString("yyyy-MM-dd"), status = dto.Status.ToString() });
        }

        // DELETE: api/calendar/log?habitId=1&date=2026-08-26
        [HttpDelete("log")]
        public async Task<IActionResult> ClearLog([FromQuery] int habitId, [FromQuery] DateTime date)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var habit = await _context.Habits.FirstOrDefaultAsync(h => h.Id == habitId && h.UserId == uid);
            if (habit == null) return NotFound();
            var log = await _context.HabitLogs.FirstOrDefaultAsync(l => l.HabitId == habitId && l.LogDate.Date == date.Date);
            if (log != null) _context.HabitLogs.Remove(log);
            if (date.Date == DateTime.Today) habit.KanbanStatus = KanbanStatus.Todo;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
