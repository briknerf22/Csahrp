using System.ComponentModel.DataAnnotations;

namespace Csharp.Models
{
    public class Note
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [Required]
        public string Title { get; set; } = null!;

        [Required]
        public string Content { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
        public bool IsImportant { get; set; }
    }
}
