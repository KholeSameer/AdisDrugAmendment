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
            var session = HttpContext.Current.Session;

            if (user != null)
            {
                return user.IsInRole(roles[role].Group);
            }

            return false;
        }

        public bool ControlAuthorizationIsInRole(string role)
        {
            WindowsPrincipal user = HttpContext.Current.User as WindowsPrincipal;
            var session = HttpContext.Current.Session;

            bool isInRole = false;
            if (user != null)
            {
                string key = user.Identity.Name + "-IsInRole-" + role;
                if (session[key] != null)
                {
                    isInRole = (bool)session[key];
                }
                else
                {
                    //heavy operation as it populates users group listing each time
                    isInRole = user.IsInRole(roles[role].Group);
                    session[key] = isInRole;
                }
            }
            //#if DEBUG
            //			isInRole = true;
            //#endif
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
            private readonly string _group;
            public string Group
            {
                get { return _group; }
            }
            public Role(string group) 
            {
                _group = group;
            }
        }
    }
}