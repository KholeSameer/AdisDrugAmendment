using DrugAmmendment.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DrugAmmendment.Attributes
{
    public class AdisAuthoriseAttribute : FilterAttribute, IAuthorizationFilter
    {
        private string _roles;
        private string _users;

        public string Roles
        {
            get { return _roles ?? String.Empty; }
            set { _roles = value; }
        }

        public string Users
        {
            get { return _users ?? String.Empty; }
            set { _users = value; }
        }
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if ((filterContext.Controller as ISecureController) == null)
            {
                throw new InvalidOperationException("AdisAuthoriseAttribute used on a controller ("
                    + filterContext.Controller.GetType().Name + ") that doesn't implement ISecureController");
            }
            var authProvider = (filterContext.Controller as ISecureController).AuthorisationProvider;
            if (authProvider == null)
            {
                throw new InvalidOperationException("AuthorisationProvider is null for controller " + filterContext.Controller.GetType().Name);
            }


            if (SplitString(Users).Any(user => authProvider.Username == user)
                || SplitString(Roles).Any(role => authProvider.IsInRole(role)))
            {
                return;
            }

            filterContext.HttpContext.Response.StatusCode = 401; //unauthorised return code
                                                                 //use this to throw an exception that can get handled by the HandleAttribute
            throw new Exception(string.Format("You are not allowed to perform this action (User: {0}).", authProvider.Username));
            {
                //This will return a 401 status code and will autoforward to a login page if forms authorisation is setup
                //filterContext.Cancel = true;
                //filterContext.Result = new HttpUnauthorizedResult();
            }
            {
                //This method can be used to return an error page instead. It will be a "successful" request status-wise 
                ////////////////////////////////////////////////////////////////////////////////////////////
                //var errorInfo = new HandleErrorInfo(new InvalidUserException("you are bad. never come here again"),
                //  filterContext.Controller.GetType().Name, filterContext.ActionMethod.Name);
                //var result = new ViewResult()
                //{
                //  ViewName = "Error",
                //  MasterName = "",
                //  TempData = new TempDataDictionary(),
                //  ViewData = new ViewDataDictionary() { Model = errorInfo },
                //  ViewEngine = new WebFormViewEngine()
                //};
                //filterContext.Result = result;
            }
        }
        private static IEnumerable<string> SplitString(string original)
        {
            return from piece in original.Split(',')
                   let trimmed = piece.Trim()
                   where !String.IsNullOrEmpty(trimmed)
                   select trimmed;
        }
    }
}