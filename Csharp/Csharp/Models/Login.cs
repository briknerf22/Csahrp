using System.ComponentModel.DataAnnotations;

namespace Csharp.Models
{
    public class Login
    {
        [Required]
        public string Username { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
    }
}
