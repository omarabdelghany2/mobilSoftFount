using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace mobileBackendsoftFount.Models{

    public class BenzeneCalibration{

        [Key]
        public int id {get;set;}
        public double amount92{get;set;}
        public double TotalMoney92{get;set;}
        public double amount95{get;set;}
        public double TotalMoney95{get;set;}
        public DateTime date{get;set;}

    }
}