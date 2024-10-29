using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TaskProject.Models;
using TaskProject.ViewModel;
using static TaskProject.ViewModel.AccountViewModel;

namespace TaskProject.Controllers
{
    [AuthFilter]
    public class AccountsController : Controller
    {
        private taskEntities db = new taskEntities();

        public ActionResult Index()
        {
            int userId = (int)Session["UserID"];
            var accounts = db.Accounts
                             .Where(a => a.UserID == userId && (a.IsActive == true)) 
                             .ToList();

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
        public ActionResult Details(int id)
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
        // GET: Accounts/Create
        public ActionResult Create()
        {
            var accountViewModel = new AccountViewModel
            {
                AccountTypes = db.AccountTypes.Select(a => new SelectListItem
                {
                    Value = a.AccountTypeID.ToString(),
                    Text = a.AccountTypeName
                }).ToList(),
                AccountNumber = GenerateUniqueAccountNumber()
            };
            return View(accountViewModel);
        }

        // POST: Accounts/Create
        [HttpPost]
        public JsonResult Create(AccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                string newAccountNumber = GenerateUniqueAccountNumber();

                if (Session["UserID"] == null)
                {
                    return Json(new { success = false, message = "Kullanıcı oturumu geçerli değil." });
                }
                int userId = (int)Session["UserID"];

                var account = new Accounts
                {
                    AccountNumber = newAccountNumber,
                    AccountTypeID = model.AccountTypeID,
                    Balance = model.Balance,
                    CreatedDate = DateTime.Now,
                    UserID = userId,
                    IsActive = true
                };

                db.Accounts.Add(account);
                db.SaveChanges();

                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Geçersiz veri girdiniz." });
        }

        private string GenerateUniqueAccountNumber()
        {
            var lastAccount = db.Accounts
                .OrderByDescending(a => a.AccountNumber)
                .FirstOrDefault();

            if (lastAccount != null && long.TryParse(lastAccount.AccountNumber.Substring(2), out long lastNumber))
            {
                return "TR" + (lastNumber + 1).ToString("D6");
            }
            return "TR000001";
        }


        // GET: Accounts/Edit/5
        public ActionResult Edit(int id)
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
                AccountTypes = db.AccountTypes.Select(a => new SelectListItem
                {
                    Value = a.AccountTypeID.ToString(),
                    Text = a.AccountTypeName
                }).ToList()
            };

            return View(accountViewModel);
        }

        // POST: Accounts/Edit
        [HttpPost]
        public JsonResult Edit(AccountViewModel accountViewModel)
        {
            if (ModelState.IsValid)
            {
                var account = db.Accounts.Find(accountViewModel.AccountID);
                if (account != null)
                {
                    account.AccountNumber = accountViewModel.AccountNumber;
                    account.AccountTypeID = accountViewModel.AccountTypeID;
                    db.SaveChanges();
                    return Json(new { success = true, redirectUrl = Url.Action("Index", "Accounts") });
                }
                return Json(new { success = false, message = "Hesap bulunamadı." });
            }
            return Json(new { success = false, message = "Geçersiz model." });
        }

        // POST: Accounts/Delete/5
        [HttpPost]
        public JsonResult Delete(int id)
        {
            var account = db.Accounts.Find(id);
            if (account != null)
            {
                if (account.Balance > 0)
                {
                    return Json(new { success = false, message = "Bu hesabı silmek için önce bakiyenin sıfır olması gerekir." });
                }

                account.IsActive = false; 
                db.SaveChanges(); 

                return Json(new { success = true, message = "Hesap başarıyla silindi" });
            }
            return Json(new { success = false, message = "Hesap bulunamadı." });
        }

    }
}
