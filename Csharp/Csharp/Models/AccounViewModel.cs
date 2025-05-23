using Microsoft.Win32;
using Csharp.Models;

namespace Csharp.Models
{
    public class AccountViewModel
    {
        public Register Register { get; set; } = new Register();
        public Login Login { get; set; } = new Login();
    }
}
