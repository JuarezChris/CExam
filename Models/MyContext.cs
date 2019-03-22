using Microsoft.EntityFrameworkCore;

namespace Dojo_Activity.Models
{
    public class MyContext : DbContext
    {
        // base() calls the parent class' constructor passing the "options" parameter along
        public MyContext(DbContextOptions options) : base(options) { }
         public DbSet<User> users { get; set; }
        public DbSet<Active> activities { get; set; }
        public DbSet<Join> joinTable { get; set; }
    }
}