using DeliveryServiceApi.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace DeliveryServiceApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
    }
}
