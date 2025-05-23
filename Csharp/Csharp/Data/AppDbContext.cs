using Csharp.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Csharp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Note> Notes { get; set; }
    }
}
