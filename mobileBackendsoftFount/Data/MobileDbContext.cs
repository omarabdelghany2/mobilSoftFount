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
    public DbSet<OilSupplier> OilSuppliers { get; set; } 
    public DbSet<Oil> Oils { get; set; } 
    public DbSet<OilSellProduct> OilSellProducts { get; set; } // Added SellProduct DbSet
    public DbSet<OilSellRecipe> OilSellRecipes { get; set; } // Added SellProduct DbSet
    public DbSet<OilBuyReceipt> OilBuyReceipts { get; set; }
    public DbSet<OilBuyProduct> OilBuyProducts { get; set; }

    

    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OilSellRecipe>()
            .HasIndex(r => r.Date)
            .IsUnique(); // 🔹 Ensure OilSellRecipe.Date is unique



        modelBuilder.Entity<OilBuyReceipt>()
            .HasIndex(r => r.Date)
            .IsUnique(); // 🔹 Ensure OilBuyReceipt.Date is unique ✅
            
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

        modelBuilder.Entity<OilSupplier>()
            .HasIndex(c => c.Name)
            .IsUnique();    

        modelBuilder.Entity<Oil>()
            .HasIndex(o => o.Name)
            .IsUnique();

        modelBuilder.Entity<Oil>()
            .HasIndex(o => o.Order)
            .IsUnique();

        modelBuilder.Entity<Oil>()
            .HasOne(o => o.Supplier)
            .WithMany(s => s.Oils)
            .HasForeignKey(o => o.SupplierId)
            .OnDelete(DeleteBehavior.Cascade); // Ensure cascade delete   

        modelBuilder.Entity<OilSellProduct>()
            .HasIndex(s => s.Name)
            .IsUnique(); // Ensure SellProduct.Name is unique     
    }



    

}
