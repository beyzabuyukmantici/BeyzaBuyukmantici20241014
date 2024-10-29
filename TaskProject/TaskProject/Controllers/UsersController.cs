using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TaskProject.Models;
using TaskProject.ViewModel;

namespace TaskProject.Controllers
{
    public class UsersController : Controller
    {
        private taskEntities db = new taskEntities();

        // GET: Users/Register
        [HttpGet]
        public ActionResult Register()
        {
            return View(new UserViewModel());
        }
        // POST: Users/Register
        [HttpPost]
        public JsonResult Register(UserViewModel userViewModel)
        {
            if (ModelState.IsValid)
            {
                if (db.Users.Any(u => u.Email == userViewModel.Email || u.TCKN == userViewModel.TCKN))
                {
                    return Json(new { success = false, message = "E-posta veya TCKN zaten kullanılıyor." });
                }

                if (string.IsNullOrWhiteSpace(userViewModel.PasswordHash))
                {
                    return Json(new { success = false, message = "Şifre boş olamaz." });
                }

                var user = new Users
                {
                    TCKN = userViewModel.TCKN,
                    Email = userViewModel.Email,
                    PasswordHash = HashPassword(userViewModel.PasswordHash),
                    FullName = userViewModel.FullName,
                    PhoneNumber = userViewModel.PhoneNumber,
                    CreatedDate = DateTime.Now
                };

                db.Users.Add(user);
                db.SaveChanges();
                return Json(new { success = true, redirectUrl = Url.Action("Login", "Users") }); 
            }
            return Json(new { success = false, message = "Geçersiz model." });
        }


        [HttpPost]
        public JsonResult CheckEmail(string email)
        {
            bool emailExists = db.Users.Any(u => u.Email == email);
            return Json(new { exists = emailExists });
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // GET: Users/Login
        [HttpGet]
        public ActionResult Login()
        {
            if (Session["UserID"] != null) 
            {
                return RedirectToAction("Index", "Home"); 
            }
            return View(new UserViewModel());
        }

        // POST: Users/Login
        [HttpPost]
        public JsonResult Login(UserViewModel userViewModel)
        {
            var user = db.Users.SingleOrDefault(u => u.Email == userViewModel.Email);
            if (user != null && BCrypt.Net.BCrypt.Verify(userViewModel.Password, user.PasswordHash))
            {
                Session["UserID"] = user.ID; 
                return Json(new { success = true, redirectUrl = Url.Action("Index", "TransferHistories") }); 
            }
            return Json(new { success = false, message = "Geçersiz e-posta veya şifre." });
        }

        // GET: Users/Details
        [HttpGet]
        public ActionResult Details()
        {
            if (Session["UserID"] == null) 
            {
                return RedirectToAction("Login", "Users");
            }

            int userId = (int)Session["UserID"];
            var user = db.Users.Find(userId);
            if (user == null)
            {
                return HttpNotFound();
            }

            var userViewModel = new UserViewModel
            {
                TCKN = user.TCKN,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber
            };

            return View(userViewModel);
        }

        // GET: Users/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon(); 

            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            Response.Cache.SetNoStore();

            return RedirectToAction("Login", "Users"); 
        }

        // POST: Users/UpdatePhone
        [HttpPost]
        public JsonResult UpdatePhone(string phoneNumber)
        {
            int userId = (int)Session["UserID"];
            var user = db.Users.Find(userId);
            if (user != null)
            {
                user.PhoneNumber = phoneNumber;
                db.SaveChanges();
                return Json(new { success = true, message = "Telefon numarası güncellendi." });
            }
            return Json(new { success = false, message = "Kullanıcı bulunamadı." });
        }

        // POST: Users/ChangePassword
        [HttpPost]
        public JsonResult ChangePassword(string currentPassword, string newPassword)
        {
            int userId = (int)Session["UserID"];
            var user = db.Users.Find(userId);
            if (user != null && BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            {
                user.PasswordHash = HashPassword(newPassword);
                db.SaveChanges();
                return Json(new { success = true, message = "Şifre değiştirildi." });
            }
            return Json(new { success = false, message = "Geçersiz mevcut şifre." });
        }
    }
}
