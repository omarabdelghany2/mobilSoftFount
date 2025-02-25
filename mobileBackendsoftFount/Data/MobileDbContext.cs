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
            .HasForeignKey(p => p.BenzeneBuyReceiptId) // Now it exists!
            .OnDelete(DeleteBehavior.Cascade); // Ensures products are deleted with the receipt





        modelBuilder.Entity<Benzene>()
            .HasIndex(b => b.Name)
            .IsUnique(); // 🔹 Ensure unique Name    
    }


    

}
