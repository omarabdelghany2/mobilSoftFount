using Microsoft.EntityFrameworkCore;
using mobileBackendsoftFount.Models;

namespace mobileBackendsoftFount.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}

    public DbSet<User> Users { get; set; }
    public DbSet<Benzene> Benzenes{get;set; }
    public DbSet<BenzeneGunCounter> BenzeneGunCounters{ get; set; } 
    public DbSet<SellingReceipt> SellingReceipts { get; set; }

}
