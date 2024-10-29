using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TaskProject.Models;
using TaskProject.ViewModel;

namespace TaskProject.Controllers
{
    public class AdminUsersController : Controller
    {
        private taskEntities db = new taskEntities();

        // GET: AdminUsers/Register
        [HttpGet]
        public ActionResult AdminRegister()
        {
            return View(new AdminViewModel());
        }

        // POST: AdminUsers/Register
        [HttpPost]
        public JsonResult AdminRegister(AdminViewModel adminViewModel)
        {
            if (ModelState.IsValid)
            {
                if (db.AdminUser.Any(u => u.Email == adminViewModel.Email || u.TCKN == adminViewModel.TCKN))
                {
                    return Json(new { success = false, message = "E-posta veya TCKN zaten kullanılıyor." });
                }

                if (db.Users.Any(u => u.Email == adminViewModel.Email))
                {
                    return Json(new { success = false, message = "Bu e-posta adresi zaten bir normal kullanıcı tarafından kullanılıyor." });
                }

                if (string.IsNullOrWhiteSpace(adminViewModel.PasswordHash))
                {
                    return Json(new { success = false, message = "Şifre boş olamaz." });
                }

                var user = new AdminUser
                {
                    TCKN = adminViewModel.TCKN,
                    Email = adminViewModel.Email,
                    PasswordHash = HashPassword(adminViewModel.PasswordHash),
                    Fullname = adminViewModel.FullName,
                    PhoneNumber = adminViewModel.PhoneNumber,
                    CreateDate = DateTime.Now
                };

                db.AdminUser.Add(user);
                db.SaveChanges();
                return Json(new { success = true, redirectUrl = Url.Action("AdminLogin", "AdminUsers") });
            }
            return Json(new { success = false, message = "Geçersiz model." });
        }
        

        // GET: AdminUsers/Login
        [HttpGet]
        public ActionResult AdminLogin()
        {
            if (Session["AdminUserID"] != null)
            {
                return RedirectToAction("AdminDetails", "AdminUsers");
            }

            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "-1";

            return View(new AdminViewModel());
        }

        // POST: AdminUsers/Login
        [HttpPost]
        public JsonResult AdminLogin(AdminViewModel adminViewModel)
        {
            var user = db.AdminUser.SingleOrDefault(u => u.Email == adminViewModel.Email);
            if (user != null && BCrypt.Net.BCrypt.Verify(adminViewModel.Password, user.PasswordHash))
            {
                Session["AdminUserID"] = user.AdminID;
                return Json(new { success = true, redirectUrl = Url.Action("AdminUsersList", "AdminUsers") }); 
            }
            return Json(new { success = false, message = "Geçersiz e-posta veya şifre." });
        }

        // GET: AdminUsers/Details
        [HttpGet]
        public ActionResult AdminDetails()
        {
            if (Session["AdminUserID"] == null)
            {
                return RedirectToAction("AdminLogin", "AdminUsers");
            }

            int userId = (int)Session["AdminUserID"];
            var user = db.AdminUser.Find(userId);
            if (user == null)
            {
                return HttpNotFound();
            }

            var adminViewModel = new AdminViewModel
            {
                TCKN = user.TCKN,
                Email = user.Email,
                FullName = user.Fullname,
                PhoneNumber = user.PhoneNumber
            };

            return View(adminViewModel);
        }

        // GET: AdminUsers/Logout
        public ActionResult AdminLogout()
        {
            Session.Clear();
            Session.Abandon();

            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            Response.Cache.SetNoStore();

            return RedirectToAction("AdminLogin", "AdminUsers");
        }

        // POST: AdminUsers/UpdatePhone
        [HttpPost]
        public JsonResult AdminUpdatePhone(string phoneNumber)
        {
            int userId = (int)Session["AdminUserID"];
            var user = db.AdminUser.Find(userId);
            if (user != null)
            {
                user.PhoneNumber = phoneNumber;
                db.SaveChanges();
                return Json(new { success = true, message = "Telefon numarası güncellendi." });
            }
            return Json(new { success = false, message = "Kullanıcı bulunamadı." });
        }

        // POST: AdminUsers/ChangePassword
        [HttpPost]
        public JsonResult AdminChangePassword(string currentPassword, string newPassword)
        {
            int userId = (int)Session["AdminUserID"];
            var user = db.AdminUser.Find(userId);
            if (user != null && BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            {
                user.PasswordHash = HashPassword(newPassword);
                db.SaveChanges();
                return Json(new { success = true, message = "Şifre değiştirildi." });
            }
            return Json(new { success = false, message = "Geçersiz mevcut şifre." });
        }

        // POST: AdminUsers/CheckEmail
        [HttpPost]
        public JsonResult AdminCheckEmail(string email)
        {
            bool emailExists = db.AdminUser.Any(u => u.Email == email);
            return Json(new { exists = emailExists });
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }


        [HttpGet]
        public ActionResult AdminUsersList()
        {
            // Giriş kontrolü
            if (Session["AdminUserID"] == null)
            {
                return RedirectToAction("AdminLogin", "AdminUsers");
            }

            var users = db.Users.Select(u => new UserViewModel
            {
                ID = u.ID,
                TCKN = u.TCKN,
                Email = u.Email,
                FullName = u.FullName,
                PhoneNumber = u.PhoneNumber,
                CreatedDate = u.CreatedDate
            }).ToList();

            return View(users);
        }
        [HttpGet]
        public JsonResult GetUsers()
        {
            if (Session["AdminUserID"] == null)
            {
                return Json(new { success = false, message = "Giriş yapmalısınız." }, JsonRequestBehavior.AllowGet);
            }

            var users = db.Users.Select(u => new
            {
                ID = u.ID,
                TCKN = u.TCKN,
                Email = u.Email,
                FullName = u.FullName,
                PhoneNumber = u.PhoneNumber ?? "-",
                CreatedDate = u.CreatedDate
            }).ToList();

            return Json(users, JsonRequestBehavior.AllowGet);
        }
























        public ActionResult AdminAccountIndex(int userId)
        {
            if (Session["AdminUserID"] == null)
            {
                return RedirectToAction("AdminLogin", "AdminUsers");
            }

            var accounts = db.Accounts
                             .Where(a => a.UserID == userId && (a.IsActive))
                             .ToList();

            var user = db.Users.Find(userId); 
            if (user == null)
            {
                ViewBag.UserName = "Kullanıcı Bulunamadı";
            }
            else
            {
                ViewBag.UserName = user.FullName; 
            }

            var accountViewModels = accounts.Select(a => new AccountViewModel
            {
                AccountID = a.AccountID,
                UserID = a.UserID,
                AccountNumber = a.AccountNumber,
                AccountTypeID = a.AccountTypeID,
                Balance = a.Balance,
                CreatedDate = a.CreatedDate,
                UpdatedDate = a.UpdatedDate,
                AccountTypeName = db.AccountTypes
                    .Where(at => at.AccountTypeID == a.AccountTypeID)
                    .Select(at => at.AccountTypeName)
                    .FirstOrDefault()
            }).ToList();

            return View(accountViewModels);
        }


        // AdminUsersController.cs
        public ActionResult AdminAccountDetails(int id)
        {
            var account = db.Accounts.Find(id);
            if (account == null)
            {
                return HttpNotFound();
            }

            var accountViewModel = new AccountViewModel
            {
                AccountID = account.AccountID,
                UserID = account.UserID,
                AccountNumber = account.AccountNumber,
                AccountTypeID = account.AccountTypeID,
                Balance = account.Balance,
                CreatedDate = account.CreatedDate,
                UpdatedDate = account.UpdatedDate,
                AccountTypeName = db.AccountTypes
                    .Where(at => at.AccountTypeID == account.AccountTypeID)
                    .Select(at => at.AccountTypeName)
                    .FirstOrDefault()
            };

            var viewModel = new AccountDetailsViewModel
            {
                Account = accountViewModel,
                IncomingMovements = new List<AccountViewModel.TransferHistoryDTO>(), 
                OutgoingMovements = new List<AccountViewModel.TransferHistoryDTO>() 
            };

            return View(viewModel);
        }

        public JsonResult GetTransferHistory(int accountId)
        {
            var incomingMovements = db.TransferHistory
                .Where(m => m.ToAccountID == accountId)
                .Select(m => new AccountViewModel.TransferHistoryDTO
                {
                    TransferID = m.TransferID,
                    FromUserID = m.FromAccountID,
                    FromAccountNumber = db.Accounts
                        .Where(a => a.AccountID == m.FromAccountID)
                        .Select(a => a.AccountNumber)
                        .FirstOrDefault(),
                    ToUserID = m.ToAccountID,
                    ToAccountNumber = db.Accounts
                        .Where(a => a.AccountID == m.ToAccountID)
                        .Select(a => a.AccountNumber)
                        .FirstOrDefault(),
                    Amount = m.Amount,
                    TransferDate = m.TransferDate,
                    Description = m.Description,
                    RemainingBalance = m.RemainingBalance,
                    RecipientRemainingBalance = m.RecipientRemainingBalance
                })
                .ToList();

            var outgoingMovements = db.TransferHistory
                .Where(m => m.FromAccountID == accountId)
                .Select(m => new AccountViewModel.TransferHistoryDTO
                {
                    TransferID = m.TransferID,
                    FromUserID = m.FromAccountID,
                    FromAccountNumber = db.Accounts
                        .Where(a => a.AccountID == m.FromAccountID)
                        .Select(a => a.AccountNumber)
                        .FirstOrDefault(),
                    ToUserID = m.ToAccountID,
                    ToAccountNumber = db.Accounts
                        .Where(a => a.AccountID == m.ToAccountID)
                        .Select(a => a.AccountNumber)
                        .FirstOrDefault(),
                    Amount = m.Amount,
                    TransferDate = m.TransferDate,
                    Description = m.Description,
                    RemainingBalance = m.RemainingBalance,
                    RecipientRemainingBalance = db.TransferHistory
                        .Where(th => th.ToAccountID == m.ToAccountID && th.TransferDate <= m.TransferDate)
                        .OrderByDescending(th => th.TransferDate)
                        .Select(th => th.RemainingBalance)
                        .FirstOrDefault()
                })
                .ToList();

            return Json(new { incomingMovements, outgoingMovements }, JsonRequestBehavior.AllowGet);
        }









        [HttpPost]
        public JsonResult CancelTransfer(int transferId)
        {
            var transfer = db.TransferHistory.Find(transferId);
            if (transfer != null)
            {
                if (transfer.IsCanceled) 
                {
                    return Json(new { success = false, message = "Bu transfer zaten geri alındı." });
                }

                var fromAccount = db.Accounts.FirstOrDefault(a => a.AccountID == transfer.ToAccountID); // Alıcı hesap
                var toAccount = db.Accounts.FirstOrDefault(a => a.AccountID == transfer.FromAccountID); // Gönderen hesap

                if (fromAccount == null || toAccount == null)
                {
                    return Json(new { success = false, message = "Gönderen veya alıcı hesap bulunamadı." });
                }

                if (!(fromAccount?.IsActive ?? false) || !(toAccount?.IsActive ?? false))
                {
                    return Json(new { success = false, message = "Gönderen veya alıcı hesap pasif durumda." });
                }

                if (fromAccount.Balance < transfer.Amount)
                {
                    return Json(new { success = false, message = "Alıcı hesap bakiyesinden fazla tutar geri alınamaz." });
                }

                // Hesap bakiyelerini güncelle
                fromAccount.Balance -= transfer.Amount; 
                toAccount.Balance += transfer.Amount; 

                // Yeni transfer kaydını oluştur
                var reverseTransfer = new TransferHistory
                {
                    FromAccountID = transfer.ToAccountID, // Alıcı 
                    ToAccountID = transfer.FromAccountID, // Gönderen 
                    Amount = transfer.Amount,
                    TransferDate = DateTime.Now,
                    Description = "Geri Alındı",
                    RemainingBalance = fromAccount.Balance, // Alıcının yeni bakiyesi
                    RecipientRemainingBalance = toAccount.Balance // Gönderenin yeni bakiyesi
                };

                db.TransferHistory.Add(reverseTransfer);
                transfer.IsCanceled = true; // Geri alındı 

                db.SaveChanges(); 

                return Json(new { success = true, message = "Transfer başarıyla geri alındı." });
            }
            return Json(new { success = false, message = "Transfer bulunamadı." });
        }

        [HttpPost]
        public JsonResult AdminAccountDelete(int id)
        {
            var account = db.Accounts.Find(id);
            if (account != null)
            {
                if (account.Balance > 0)
                {
                    return Json(new { success = false, message = "Bu hesabı silmek için önce bakiyenin sıfır olması gerekir." });
                }

                db.Accounts.Remove(account); 
                db.SaveChanges();

                return Json(new { success = true, message = "Hesap başarıyla silindi." });
            }
            return Json(new { success = false, message = "Hesap bulunamadı." });
        }





    }

}