using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DrugAmmendment.Exceptions;

namespace DrugAmmendment.Attributes
{
    public class HandleJsonErrorAttribute : FilterAttribute, IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            HttpResponseBase response = filterContext.HttpContext.Response;

            response.ContentType = "application/json";

            //exceptions thrown from inside the action will end up in the InnerException but exceptions thrown from 
            //the AdisAuthoriseAttribute will be the main exception
            Exception exception = filterContext.Exception;
            while (exception.InnerException != null)
            {
                exception = exception.InnerException;
            }

            filterContext.HttpContext.Response.StatusCode = 500; //=Server Error

            var reply = JsonReply.Create(false, HttpUtility.HtmlEncode(exception.Message));
            if (exception.Data != null)
            {
                foreach (var key in exception.Data.Keys.Cast<string>())
                {
                    reply[key] = exception.Data[key];
                }
            }

            reply["exceptionType"] = exception.GetType().Name;
            reply["RouteData"] = filterContext.RouteData.Values.Select(c => string.Format("  {0} : {1}", c.Key.ToString(), c.Value.ToString()));

            // The JavaScriptSerializer type was marked as obsolete prior to .NET Framework 3.5 SP1
#pragma warning disable 0618
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            response.Write(serializer.Serialize(reply));
#pragma warning restore 0618


            filterContext.ExceptionHandled = true;
        }
    }
}