using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using TaskProject.Models;
using TaskProject.ViewModel;

namespace TaskProject.Controllers
{
    public class TransferHistoriesController : Controller
    {
        private taskEntities db = new taskEntities();

        // GET: TransferHistories
        public ActionResult Index()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Users");
            }

            int userId = (int)Session["UserID"];
            var openAccounts = db.Accounts
                .Where(a => a.UserID == userId && a.IsActive) // Aktif hesapları kontrol et
                .Select(a => a.AccountID)
                .ToList();

            var incomingTransfers = db.TransferHistory
                .Where(th => openAccounts.Contains(th.ToAccountID))
                .OrderByDescending(th => th.TransferDate)
                .ToList();

            var incomingViewModels = incomingTransfers.Select(th => new TransferHistoryViewModel
            {
                FromAccountNumber = db.Accounts
                    .Where(a => a.AccountID == th.FromAccountID)
                    .Select(a => a.AccountNumber)
                    .FirstOrDefault(),
                ToAccountNumber = db.Accounts
                    .Where(a => a.AccountID == th.ToAccountID)
                    .Select(a => a.AccountNumber)
                    .FirstOrDefault(),
                Amount = th.Amount,
                TransferDate = th.TransferDate,
                Description = th.Description,
                RecipientRemainingBalance = th.RecipientRemainingBalance
            }).ToList();

            // Get outgoing transfers
            var outgoingTransfers = db.TransferHistory
                .Where(th => openAccounts.Contains(th.FromAccountID))
                .OrderByDescending(th => th.TransferDate)
                .ToList();

            var outgoingViewModels = outgoingTransfers.Select(th => new TransferHistoryViewModel
            {
                FromAccountNumber = db.Accounts
                    .Where(a => a.AccountID == th.FromAccountID)
                    .Select(a => a.AccountNumber)
                    .FirstOrDefault(),
                ToAccountNumber = db.Accounts
                    .Where(a => a.AccountID == th.ToAccountID)
                    .Select(a => a.AccountNumber)
                    .FirstOrDefault(),
                Amount = th.Amount,
                TransferDate = th.TransferDate,
                Description = th.Description,
                RemainingBalance = th.RemainingBalance
            }).ToList();

            var transferHistoryViewModel = new TransferHistoryCombinedViewModel
            {
                IncomingTransfers = incomingViewModels,
                OutgoingTransfers = outgoingViewModels
            };

            if (!incomingViewModels.Any())
            {
                ViewBag.IncomingMessage = "Bu kullanıcı için gelen transfer bulunmamaktadır.";
            }
            if (!outgoingViewModels.Any())
            {
                ViewBag.OutgoingMessage = "Bu kullanıcı için giden transfer bulunmamaktadır.";
            }

            return View(transferHistoryViewModel);
        }

        public ActionResult Create()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Users");
            }

            int currentUserId = (int)Session["UserID"];
            PopulateAccountDropDownLists(currentUserId);
            return View();
        }

        [HttpPost]
        public ActionResult Create([Bind(Include = "FromAccountID,ToAccountID,Amount,Description")] TransferHistory transferHistory)
        {
            if (Session["UserID"] == null)
            {
                return Json(new { success = false, redirectUrl = Url.Action("Login", "Users") });
            }

            int currentUserId = (int)Session["UserID"];

            if (ModelState.IsValid)
            {
                var fromAccount = db.Accounts.FirstOrDefault(a => a.AccountID == transferHistory.FromAccountID && a.UserID == currentUserId && a.IsActive);
                var toAccount = db.Accounts.FirstOrDefault(a => a.AccountID == transferHistory.ToAccountID && a.IsActive);

                if (fromAccount == null || toAccount == null)
                {
                    ModelState.AddModelError("", "Gönderen veya alıcı hesap bulunamadı veya hesap aktif değil.");
                }
                else if (transferHistory.Amount <= 0)
                {
                    ModelState.AddModelError("Amount", "Gönderilecek miktar sıfırdan büyük olmalıdır.");
                }
                else if (fromAccount.Balance < transferHistory.Amount)
                {
                    ModelState.AddModelError("Amount", "Gönderen hesap bakiyesinden fazla tutar gönderemezsiniz.");
                }
                else
                {
                    fromAccount.Balance -= transferHistory.Amount;
                    toAccount.Balance += transferHistory.Amount;

                    transferHistory.TransferDate = DateTime.Now;
                    transferHistory.RemainingBalance = fromAccount.Balance;
                    transferHistory.RecipientRemainingBalance = toAccount.Balance;

                    db.TransferHistory.Add(transferHistory);
                    db.SaveChanges();

                    return Json(new { success = true, message = $"Transfer başarılı! Kalan bakiye: {transferHistory.RemainingBalance}" });
                }
            }

            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        private void PopulateAccountDropDownLists(int userId, int? selectedFromAccount = null, int? selectedToAccount = null)
        {
            var userAccounts = db.Accounts
                .Where(a => a.UserID == userId && a.Balance > 0 && a.IsActive)
                .ToList();

            ViewBag.FromAccountID = new SelectList(userAccounts, "AccountID", "AccountNumber", selectedFromAccount);

            var allAccounts = db.Accounts
                .Where(a => a.UserID != userId && a.IsActive)
                .ToList();

            ViewBag.ToAccountID = new SelectList(allAccounts, "AccountID", "AccountNumber", selectedToAccount);
        }

        [HttpPost]
        public JsonResult CreateTransfer([Bind(Include = "FromAccountID,ToAccountID,Amount,Description")] TransferHistory transferHistory)
        {
            if (Session["UserID"] == null)
            {
                return Json(new { success = false, message = "Kullanıcı oturumu kapalı." });
            }

            int currentUserId = (int)Session["UserID"];

            if (ModelState.IsValid)
            {
                var fromAccount = db.Accounts.FirstOrDefault(a => a.AccountID == transferHistory.FromAccountID && a.UserID == currentUserId && a.IsActive);
                var toAccount = db.Accounts.FirstOrDefault(a => a.AccountID == transferHistory.ToAccountID && a.IsActive);

                if (fromAccount == null || toAccount == null)
                {
                    return Json(new { success = false, message = "Gönderen veya alıcı hesap bulunamadı veya hesap aktif değil." });
                }
                else if (transferHistory.Amount <= 0)
                {
                    return Json(new { success = false, message = "Gönderilecek miktar sıfırdan büyük olmalıdır." });
                }
                else if (fromAccount.Balance < transferHistory.Amount)
                {
                    return Json(new { success = false, message = "Gönderen hesap bakiyesinden fazla tutar gönderemezsiniz." });
                }
                else
                {
                    fromAccount.Balance -= transferHistory.Amount;
                    toAccount.Balance += transferHistory.Amount;

                    transferHistory.TransferDate = DateTime.Now;
                    transferHistory.RemainingBalance = fromAccount.Balance;
                    transferHistory.RecipientRemainingBalance = toAccount.Balance;

                    db.TransferHistory.Add(transferHistory);
                    db.SaveChanges();

                    return Json(new { success = true, message = $"Transfer başarılı! Kalan bakiye: {transferHistory.RemainingBalance}" });
                }
            }

            return Json(new { success = false, message = "Geçersiz model durumu." });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}
