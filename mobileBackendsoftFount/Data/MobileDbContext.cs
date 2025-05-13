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

    public DbSet<OilWorker> OilWorkers { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<SubCategory> SubCategories { get; set; }
    public DbSet<ServiceSellReceipt> ServiceSellReceipts { get; set; }
    public DbSet<ServiceSellProduct> ServiceSellProducts { get; set; }
    public DbSet<ClientService> ClientServices { get; set; }


    public DbSet<benzeneAdjustment> BenzeneAdjustments { get; set; }
    public DbSet<benzeneDeposit> BenzeneDeposits { get; set; }
    public DbSet<AccountInvestigationReport> AccountInvestigationReports { get; set; }
    public DbSet<AccountInvestigationMember> AccountInvestigationMembers { get; set; }
    public DbSet<oilAdjustment> OilAdjustments { get; set; }



    public DbSet<Balance> Balances { get; set; } // Balance table to store balances over time
    public DbSet<oilAccountBalance> oilAccountBalances { get; set; } // Balance table to store balances over time


    public DbSet<MinistryOfSupplyLetter> MinistryOfSupplyLetters { get; set; }
    public DbSet<MinistryOfSupplyLetterMember> MinistryOfSupplyLetterMembers { get; set; }
    

    public DbSet<BenzeneTank> BenzeneTanks { get; set; }
    public DbSet<BenzeneCalibration> BenzeneCalibrations { get; set; }


    //disabaility report
    public DbSet<DisabilityAndIncreaseReport> DisabilityAndIncreaseReports { get; set; }
    public DbSet<ReportTable1> ReportTables1 { get; set; }
    public DbSet<ReportTable2> ReportTables2 { get; set; }
    public DbSet<ReportTable3> ReportTables3 { get; set; }
    public DbSet<ReportTable4> ReportTables4 { get; set; }


    public DbSet<OilStorageBalanceReport> OilStorageBalanceReports { get; set; }
    public DbSet<OilBalanceProduct> OilBalanceProducts { get; set; }


    //oil deposits
    public DbSet<oilDeposit> OilDeposits { get; set; }



        // EXPESNSE AND Revenues
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<ExpenseCategory> ExpenseCategories { get; set; }

    public DbSet<Revenue> Revenues { get; set; }
    public DbSet<RevenueCategory> RevenueCategories { get; set; }



    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<BenzeneCalibration>()
            .HasIndex(r => r.date)
            .IsUnique(); // 🔹 Ensure OilSellRecipe.Date is unique

        modelBuilder.Entity<BenzeneTank>()
            .HasIndex(r => r.date)
            .IsUnique(); // 🔹 Ensure OilSellRecipe.Date is unique

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

        // modelBuilder.Entity<OilSellProduct>()
        //     .HasIndex(s => s.Name)
        //     .IsUnique(); // Ensure SellProduct.Name is unique     


        modelBuilder.Entity<OilSellProduct>()
            .HasOne(p => p.OilSellRecipe) // ✅ Use singular property name
            .WithMany(r => r.OilSellProducts)
            .HasForeignKey(p => p.OilSellRecipeId)
            .OnDelete(DeleteBehavior.Cascade);
        // modelBuilder.Entity<SubCategory>()
        //     .HasOne(s => s.Category)
        //     .WithMany()
        //     .HasForeignKey(s => s.CategoryId)
        //     .OnDelete(DeleteBehavior.Cascade);



        modelBuilder.Entity<SubCategory>()
            .HasMany(sc => sc.Categories)
            .WithMany(c => c.SubCategories);  // ✅ Many-to-Many Relationship





        modelBuilder.Entity<ServiceSellProduct>()
            .HasOne(p => p.ServiceSellReceipt) // Each product has one receipt
            .WithMany(r => r.ServiceSellProducts) // Each receipt has many products
            .HasForeignKey(p => p.ServiceSellReceiptId); // Foreign key is ServiceSellReceiptId



        // Unique constraint for BenzeneAdjustments.Date
        modelBuilder.Entity<benzeneAdjustment>()
            .HasIndex(a => a.date)
            .IsUnique(); // Ensure BenzeneAdjustment.Date is unique

        // Unique constraint for BenzeneDeposits.Date
        modelBuilder.Entity<benzeneDeposit>()
            .HasIndex(d => d.date)
            .IsUnique(); // Ensure BenzeneDeposit.Date is unique


           

    // Configure cascading delete for the relationship between AccountInvestigationReport and AccountInvestigationMember
        modelBuilder.Entity<AccountInvestigationReport>()
            .HasMany(r => r.AccountInvestigationMembers)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade); // This ensures that when a report is deleted, its members are also deleted

        modelBuilder.Entity<BenzeneBuyReceipt>()
            .HasIndex(r => r.MobilReceiptDate)
            .IsUnique(); // 🔹 Ensure BenzeneBuyReceipt.MobilReceiptDate is unique



        modelBuilder.Entity<MinistryOfSupplyLetter>()
            .HasMany(l => l.Members)
            .WithOne(m => m.MinistryOfSupplyLetter)
            .HasForeignKey(m => m.MinistryOfSupplyLetterId)
            .OnDelete(DeleteBehavior.Cascade);


        modelBuilder.Entity<MinistryOfSupplyLetter>()
            .HasIndex(l => l.MonthlyDate)
            .IsUnique();



        // Optional: index on ReportDate
        modelBuilder.Entity<DisabilityAndIncreaseReport>()
            .HasIndex(r => r.ReportDate);

        // Relationships
        modelBuilder.Entity<DisabilityAndIncreaseReport>()
            .HasMany(r => r.Table1)
            .WithOne(t => t.DisabilityAndIncreaseReport)
            .HasForeignKey(t => t.DisabilityAndIncreaseReportId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DisabilityAndIncreaseReport>()
            .HasMany(r => r.Table2)
            .WithOne(t => t.DisabilityAndIncreaseReport)
            .HasForeignKey(t => t.DisabilityAndIncreaseReportId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DisabilityAndIncreaseReport>()
            .HasMany(r => r.Table3)
            .WithOne(t => t.DisabilityAndIncreaseReport)
            .HasForeignKey(t => t.DisabilityAndIncreaseReportId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DisabilityAndIncreaseReport>()
            .HasMany(r => r.Table4)
            .WithOne(t => t.DisabilityAndIncreaseReport)
            .HasForeignKey(t => t.DisabilityAndIncreaseReportId)
            .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<ReportTable1>()
                .HasIndex(t => t.DisabilityAndIncreaseReportId);

            modelBuilder.Entity<ReportTable2>()
                .HasIndex(t => t.DisabilityAndIncreaseReportId);

            modelBuilder.Entity<ReportTable3>()
                .HasIndex(t => t.DisabilityAndIncreaseReportId);

            modelBuilder.Entity<ReportTable4>()
                .HasIndex(t => t.DisabilityAndIncreaseReportId);
                


            modelBuilder.Entity<OilStorageBalanceReport>()
                .HasMany(r => r.Products)
                .WithOne(p => p.OilStorageBalanceReport)
                .HasForeignKey(p => p.OilStorageBalanceReportId)
                .OnDelete(DeleteBehavior.Cascade); // << Cascade delete




            // Unique constraint for oilDeposit.Date
            modelBuilder.Entity<oilDeposit>()
                .HasIndex(d => d.date)
                .IsUnique(); // Ensure oilDeposit.Date is unique


        // Unique constraint for oilAdjustment.Date
        modelBuilder.Entity<oilAdjustment>()
            .HasIndex(a => a.date)
            .IsUnique(); // Ensure BenzeneAdjustment.Date is unique
        modelBuilder.Entity<ExpenseCategory>()
            .HasIndex(e => e.Name)
            .IsUnique();

        modelBuilder.Entity<Expense>()
            .HasOne(e => e.ExpenseCategory)
            .WithMany(c => c.Expenses)
            .HasForeignKey(e => e.ExpenseCategoryId)
            .OnDelete(DeleteBehavior.Cascade);  // Cascade delete when ExpenseCategory is deleted


        modelBuilder.Entity<Revenue>()
            .HasOne(r => r.RevenueCategory)
            .WithMany(rc => rc.Revenues)
            .HasForeignKey(r => r.RevenueCategoryId)
            .OnDelete(DeleteBehavior.Cascade);

    
    }



    

}
