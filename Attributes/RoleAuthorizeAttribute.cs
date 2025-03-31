using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Learn_Auth.Models;

namespace Learn_Auth.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RoleAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly int[] _allowedRoleIds;

        public RoleAuthorizeAttribute(params int[] allowedRoleIds)
        {
            _allowedRoleIds = allowedRoleIds;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContextBase)
        {
            // First, check if the user is logged in
            if (!httpContextBase.User.Identity.IsAuthenticated)
                return false;

            // Extract the current username (email) from the Identity
            var email = httpContextBase.User.Identity.Name;
            if (string.IsNullOrEmpty(email))
                return false;

            // Access the database to get the user’s RoleId
            using (var db = new AuthDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Email == email);
                if (user == null)
                    return false;

                return _allowedRoleIds.Contains(user.RoleId);
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // If not logged in, redirect to Login
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new RedirectResult("~/Account/Login");
            }
            else
            {
                // If logged in but not in correct role, redirect to AccessDenied
                filterContext.Result = new RedirectResult("~/Account/AccessDenied");
            }
        }
    }
}