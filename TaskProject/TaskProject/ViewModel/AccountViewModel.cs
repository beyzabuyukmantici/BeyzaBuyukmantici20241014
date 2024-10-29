using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace TaskProject.ViewModel
{
    public class AccountViewModel
    {
        public int AccountID { get; set; }

        [Required(ErrorMessage = "Kullanıcı ID gerekli.")]
        public int UserID { get; set; }

        [Required(ErrorMessage = "Hesap numarası gerekli.")]
        public string AccountNumber { get; set; }

        [Required(ErrorMessage = "Hesap türü gerekli.")]
        public int AccountTypeID { get; set; } 
        public string AccountTypeName { get; set; } 

        public decimal Balance { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UserName { get; set; } 
        public bool IsActive { get; set; } 


        public IEnumerable<SelectListItem> AccountTypes { get; set; }


        public class TransferHistoryDTO
        {
                public int TransferID { get; set; }
                public int? FromUserID { get; set; }
                public string FromAccountNumber { get; set; } 
                public int? ToUserID { get; set; }
                public string ToAccountNumber { get; set; } 
                public decimal Amount { get; set; }
                public DateTime TransferDate { get; set; }
                public string Description { get; set; }
                public decimal RemainingBalance { get; set; }
                public decimal RecipientRemainingBalance { get; set; } 



        }
        public UserViewModel User { get; set; }
    }
}
