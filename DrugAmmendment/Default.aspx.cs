using DrugAmmendment.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DrugAmmendment
{
    public partial class Default : System.Web.UI.Page
    {
        private string roles = "admin,editor";
        private WebAuthorisationProvider _authProvider = null;
        protected void Page_Load(object sender, EventArgs e)
        {
            _authProvider = new WebAuthorisationProvider();
        }

        protected void DrugAmendment_Click(object sender, EventArgs e)
        {
            if (SplitString(roles).Any(role => _authProvider.IsInRole(role)))
            {
                string siteUrl = System.Configuration.ConfigurationManager.AppSettings["Dashboard"] as string;
                Response.Redirect(siteUrl);
            }
            else
            {
                Response.Redirect("UnauthorizePage.aspx");
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