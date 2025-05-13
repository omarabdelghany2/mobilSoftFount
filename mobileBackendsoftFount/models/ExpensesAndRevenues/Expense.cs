 using System;
using System.ComponentModel.DataAnnotations;

namespace mobileBackendsoftFount.Models
{
    public class Expense
    {
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }
        public string BankName{get;set;}="";
        public int Round{get;set;}=0;
        public string Comment{get;set;}="";
        public decimal Value{get;set;}=0.0m;


        // Foreign Key for ExpenseCategory
        public int ExpenseCategoryId { get; set; }

        // Navigation property to ExpenseCategory
        public ExpenseCategory ExpenseCategory { get; set; }
    }



    public class ExpenseCategory
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public bool DeductionFromProfit { get; set; }

    // Add the navigation property for Expenses
        public ICollection<Expense> Expenses { get; set; }  // <-- This line was missing
    }
}
