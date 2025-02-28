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
    public DbSet<BenzeneBuyReceipt> BenzeneBuyReceipts { get; set; } // New model
    public DbSet<BenzeneRecipeProduct> BenzeneRecipeProducts { get; set; } 

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BenzeneRecipeProduct>()
            .HasOne(p => p.BenzeneBuyReceipt)
            .WithMany(r => r.Products)
            .HasForeignKey(p => p.BenzeneBuyReceiptId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Benzene>()
            .HasIndex(b => b.Name)
            .IsUnique(); // 🔹 Ensure Benzene.Name is unique  

        modelBuilder.Entity<SellingReceipt>()
            .HasIndex(r => r.Date)
            .IsUnique(); // 🔹 Ensure SellingReceipt.Date is unique  
    }



    

}
