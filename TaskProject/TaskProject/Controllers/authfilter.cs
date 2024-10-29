using System;
using System.Web;
using System.Web.Mvc;

public class AuthFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        var userId = HttpContext.Current.Session["UserID"];
        if (userId == null)
        {
            filterContext.Result = new RedirectResult("~/Users/Login"); 
        }

        base.OnActionExecuting(filterContext);
    }
}
