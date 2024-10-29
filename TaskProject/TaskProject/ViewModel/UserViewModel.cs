using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TaskProject.ViewModel
{
    public class UserViewModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "TCKN gerekli.")]
        [StringLength(11, ErrorMessage = "TCKN 11 karakter olmalıdır.")]
        public string TCKN { get; set; }

        [Required(ErrorMessage = "Email gerekli.")]
        [EmailAddress(ErrorMessage = "Geçersiz Email adresi.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Ad Soyad gerekli.")]
        public string FullName { get; set; }

        [Phone(ErrorMessage = "Geçersiz telefon numarası.")]
        public string PhoneNumber { get; set; }
        public string PasswordHash {  get; set; }

        public DateTime CreatedDate { get; set; }

        public string Password { get; set; } 

    }

}