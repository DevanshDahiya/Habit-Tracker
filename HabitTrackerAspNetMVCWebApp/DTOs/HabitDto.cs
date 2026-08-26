using System.ComponentModel.DataAnnotations;
using HabitTrackerAspNetMVCWebApp.Models;

namespace HabitTrackerAspNetMVCWebApp.DTOs
{
    public class HabitDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Frequency Frequency { get; set; }
        public HabitStatus Status { get; set; }
        public KanbanStatus KanbanStatus { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? UserEmail { get; set; }
    }

    public class CreateHabitDto
    {
        [Required, StringLength(100)]
        public string Title { get; set; } = string.Empty;
        [StringLength(500)]
        public string? Description { get; set; }
        [Required]
        public Frequency Frequency { get; set; }
        [Required]
        public HabitStatus Status { get; set; }
        [Required]
        public KanbanStatus KanbanStatus { get; set; } = KanbanStatus.Todo;
        [Required]
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class UpdateHabitDto : CreateHabitDto
    {
        [Required]
        public int Id { get; set; }
    }

    public class HabitLogDto
    {
        public int Id { get; set; }
        public int HabitId { get; set; }
        public DateTime LogDate { get; set; }
        public HabitLogStatus Status { get; set; }
    }

    public class SetHabitLogDto
    {
        [Required]
        public int HabitId { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public HabitLogStatus Status { get; set; }
    }
}
