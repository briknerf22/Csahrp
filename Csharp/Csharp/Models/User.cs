namespace Csharp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public bool ConsentForAITraining { get; set; }

        public List<Note> Notes { get; set; } = new();
    }
}
