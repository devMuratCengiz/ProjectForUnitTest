using Microsoft.EntityFrameworkCore;

namespace ProjectForUnitTest.Web.Models.Context
{
    public class SqlDbContext : DbContext
    {
        public SqlDbContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<Product> Products { get; set; }
    }
}
