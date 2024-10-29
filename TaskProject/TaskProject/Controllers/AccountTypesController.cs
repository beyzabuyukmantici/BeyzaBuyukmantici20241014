using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TaskProject.Models;
using TaskProject.ViewModel;

namespace TaskProject.Controllers
{
    [AdminAuthFilter]
    public class AccountTypesController : Controller
    {
        private taskEntities db = new taskEntities();

        // GET: AccountTypes
        public ActionResult Index()
        {
            var accountTypes = db.AccountTypes.ToList();
            return View(accountTypes);
        }

        [HttpGet]
        public JsonResult GetAll()
        {
            var accountTypes = db.AccountTypes
                .Select(a => new AccountsTypesViewModel
                {
                    AccountTypeID = a.AccountTypeID,
                    AccountTypeName = a.AccountTypeName
                }).ToList();
            return Json(accountTypes, JsonRequestBehavior.AllowGet);
        }

        // GET: AccountTypes/Create
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Create(AccountsTypesViewModel accountTypeViewModel)
        {
            if (ModelState.IsValid)
            {
                if (db.AccountTypes.Any(a => a.AccountTypeName == accountTypeViewModel.AccountTypeName))
                {
                    return Json(new { success = false, message = "Hesap türü adı zaten kayıtlı. Lütfen farklı bir ad deneyin." });
                }

                var accountType = new AccountTypes
                {
                    AccountTypeName = accountTypeViewModel.AccountTypeName
                };

                db.AccountTypes.Add(accountType);
                db.SaveChanges();
                return Json(new { success = true, message = "Hesap türü başarıyla oluşturuldu." });
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(new { success = false, message = "Hatalı giriş: " + string.Join(", ", errors) });
        }

        // GET: AccountTypes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var accountType = db.AccountTypes.FirstOrDefault(a => a.AccountTypeID == id);
            if (accountType == null)
            {
                return HttpNotFound("Hesap türü bulunamadı.");
            }

            var viewModel = new AccountsTypesViewModel
            {
                AccountTypeID = accountType.AccountTypeID,
                AccountTypeName = accountType.AccountTypeName
            };

            return View(viewModel);
        }

        [HttpPost]
        public JsonResult Edit(AccountsTypesViewModel accountTypeViewModel)
        {
            if (ModelState.IsValid)
            {
                var accountType = db.AccountTypes.FirstOrDefault(a => a.AccountTypeID == accountTypeViewModel.AccountTypeID);
                if (accountType == null)
                {
                    return Json(new { success = false, message = "Güncellenmek istenen hesap türü bulunamadı." });
                }

                if (db.AccountTypes.Any(a => a.AccountTypeName == accountTypeViewModel.AccountTypeName && a.AccountTypeID != accountTypeViewModel.AccountTypeID))
                {
                    return Json(new { success = false, message = "Bu isimde başka bir hesap türü mevcut. Lütfen başka bir isim deneyin." });
                }

                accountType.AccountTypeName = accountTypeViewModel.AccountTypeName;
                db.Entry(accountType).State = EntityState.Modified;

                try
                {
                    db.SaveChanges();
                    return Json(new { success = true, message = "Hesap türü başarıyla güncellendi." });
                }
                catch (DbUpdateException)
                {
                    return Json(new { success = false, message = "Güncelleme sırasında bir hata oluştu. Lütfen tekrar deneyin." });
                }
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(new { success = false, message = "Hatalı giriş: " + string.Join(", ", errors) });
        }

        // GET: AccountTypes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var accountType = db.AccountTypes.Find(id);
            if (accountType == null)
            {
                return HttpNotFound("Silinmek istenen hesap türü bulunamadı.");
            }

            return View(accountType);
        }

        // POST: AccountTypes/Delete
        [HttpPost]
        public JsonResult Delete(int id)
        {
            var accountType = db.AccountTypes.Find(id);
            if (accountType == null)
            {
                return Json(new { success = false, message = "Silinmek istenen hesap türü bulunamadı." });
            }

            var relatedAccounts = db.Accounts.Where(a => a.AccountTypeID == id).ToList();

            if (relatedAccounts.Any())
            {
                if (relatedAccounts.All(a => a.IsActive == false))
                {
                    db.Accounts.RemoveRange(relatedAccounts);
                }
                else
                {
                    return Json(new { success = false, message = "Bu hesap türü, aktif hesaplarla ilişkilidir. Silme işlemi gerçekleştirilemiyor." });
                }
            }

            db.AccountTypes.Remove(accountType);
            try
            {
                db.SaveChanges();
                return Json(new { success = true, message = "Hesap türü başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Silme işlemi sırasında bir hata oluştu: " + ex.Message });
            }
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
