using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DrugAmmendment.Exceptions
{
    public class JsonReply : Dictionary<string, object>
    {
        public bool Success
        {
            get { return (bool)this["success"]; }
            set { this["success"] = value; }
        }

        public string Reason
        {
            get { return (string)this["reason"]; }
            set { this["reason"] = value; }
        }

        public static JsonReply Create(bool success, string failureReason)
        {
            JsonReply returnVal = new JsonReply();
            returnVal.Reason = failureReason;
            returnVal.Success = success;

            return returnVal;
        }

        public static JsonReply Successful
        {
            get { return JsonReply.Create(true, ""); }
        }
    }
}