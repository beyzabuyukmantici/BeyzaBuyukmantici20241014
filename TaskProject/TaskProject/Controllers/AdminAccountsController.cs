using System.Linq;
using System.Web.Mvc;
using TaskProject.Models;
using TaskProject.ViewModel;

namespace TaskProject.Controllers
{
    [AdminAuthFilter] 
    public class AdminAccountsController : Controller
    {
        private taskEntities db = new taskEntities();

        // GET: AdminAccounts/Index
        public ActionResult Index()
        {
            var accounts = db.Accounts.ToList(); 
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
                    .FirstOrDefault(),
                UserName = db.Users 
                    .Where(u => u.ID == a.UserID)
                    .Select(u => u.FullName) 
                    .FirstOrDefault() 
            }).ToList();

            return View(accountViewModels);
        }

        // GET: AdminAccounts/Details/5
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

            return View(accountViewModel); 
        }
    }
}