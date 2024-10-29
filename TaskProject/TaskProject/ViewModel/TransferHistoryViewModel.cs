using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaskProject.ViewModel
{
    public class TransferHistoryViewModel
    {
        public int TransferID { get; set; }
        public int FromAccountID { get; set; }
        public string FromAccountNumber { get; set; } 
        public int ToAccountID { get; set; }
        public string ToAccountNumber { get; set; } 
        public decimal Amount { get; set; }
        public DateTime TransferDate { get; set; }
        public string Description { get; set; }
        public decimal RemainingBalance { get; set; }
        public decimal RecipientRemainingBalance { get; set; } 



    }

}