using System.ComponentModel.DataAnnotations;

namespace Csharp.Models
{
    public class Register
    {
        [Required]
        public string Username { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = null!;

        [Required]
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the AI training consent.")]
        public bool AiTrainingConsent { get; set; }
    }
}
