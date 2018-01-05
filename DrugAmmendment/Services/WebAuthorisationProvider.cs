using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Xml;

namespace DrugAmmendment.Services
{
    public class WebAuthorisationProvider : IAuthorizationProvider
    {
        private readonly Dictionary<string, Role> roles;

        public WebAuthorisationProvider()
        {
            roles = new Dictionary<string, Role>();

            XmlDocument x = new XmlDocument();
            x.Load(HttpContext.Current.Request.MapPath("~/Auth.config"));
            XmlNodeList nl = x.SelectNodes("//role");
            foreach (XmlNode xn in nl)
            {
                string name = xn.Attributes["name"].Value;
                string group = xn.Attributes["group"].Value;
                //string password = xn.Attributes["password"].Value;
                Role r = new Role(group); //name, group, password);
                roles.Add(name, r);
            }
        }

        public string Username
        {
            get
            {
                string username = HttpContext.Current.User.Identity.Name;
                if (username != null)
                {
                    int i = username.LastIndexOf(@"\");
                    if (i >= 0 && i < username.Length - 1)
                        username = username.Substring(i + 1);
                }
                return username;
            }
        }

        public bool IsInRole(string role)
        {
            WindowsPrincipal user = HttpContext.Current.User as WindowsPrincipal;

            bool isInRole = false;
            if (user != null)
            {
                string key = "IsInRole-" + role;
                if (HttpContext.Current.Session[key] != null)
                {
                    isInRole = (bool)HttpContext.Current.Session[key];
                }
                else
                {
                    //heavy operation as it populates users group listing each time
                    isInRole = user.IsInRole(roles[role].Group);
                    HttpContext.Current.Session[key] = isInRole;
                }
            }
            return isInRole;
        }

        public bool IsAdmin
        {
            get { return IsInRole("admin"); }
        }

        public bool IsUser
        {
            get { return IsInRole("user"); }
        }

        public bool IsEditor
        {
            get { return IsAdmin || IsUser; }
        }

        private class Role
        {
            //			private readonly string _name;
            private readonly string _group;
            //			private readonly string _password;

            //			public string Name
            //			{
            //				 get { return _name; }
            //			}
            public string Group
            {
                get { return _group; }
            }
            //			public string Password
            //			{
            //				get { return _password; }
            //			}
            public Role(string group) //string name, string group, string password)
            {
                //_name = name;
                _group = group;
                //_password = password;
            }
        }
    }
}