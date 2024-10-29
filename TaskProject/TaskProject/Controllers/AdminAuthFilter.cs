using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TaskProject.Models;

public class AdminAuthFilter : ActionFilterAttribute
{
    private readonly taskEntities db = new taskEntities(); 

    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
    
        var adminUserID = HttpContext.Current.Session["AdminUserID"];

        if (adminUserID == null)
        {
            filterContext.Result = new RedirectResult("~/AdminUsers/AdminLogin");
            return;
        }

        bool adminUserExists = db.AdminUser.Any(u => u.AdminID == (int)adminUserID);

        if (!adminUserExists)
        {
            filterContext.Result = new RedirectResult("~/AdminUsers/AdminLogin");
        }

        base.OnActionExecuting(filterContext);
    }
}