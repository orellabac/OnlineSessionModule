using System;
using System.Collections.Generic;
using System.Web;
using System.Web.SessionState;

namespace UpgradeHelpers.WebMap.Server
{
    public class OnlineUsersModule : IHttpModule
    {
        private static Int32 _sessionTimeOut = 20; // Set Default to 20 Minutes
        private static List<OnlineUserInfo> _onlineUsers = new List<OnlineUserInfo>();




        public static List<OnlineUserInfo> OnlineUsers
        {
            get
            {
                CleanExpiredSessions();
                return _onlineUsers;
            }
        }

        private static void CleanExpiredSessions()
        {
            _onlineUsers.RemoveAll(user => user.SessionStarted.AddMinutes(_sessionTimeOut) < DateTime.Now);
            //{

        }

        private void Session_Start(object sender, EventArgs e)
        {
            HttpRequest Request = HttpContext.Current.Request;
            HttpApplicationState Application = HttpContext.Current.Application;
            HttpSessionState Session = HttpContext.Current.Session;

            // Get Session TimeOut
            _sessionTimeOut = HttpContext.Current.Session.Timeout;

            Application.Lock();

            OnlineUserInfo user = new OnlineUserInfo();

            user.SessionId = Session.SessionID;
            user.SessionStarted = DateTime.Now;
            user.UserAgent = !String.IsNullOrEmpty(Request.UserAgent)
            ? Request.UserAgent : String.Empty;
            user.IPAddress = !String.IsNullOrEmpty(Request.UserHostAddress)
            ? Request.UserHostAddress : String.Empty;
            if (Request.UrlReferrer != null)
            {
                user.UrlReferrer = !String.IsNullOrEmpty(Request.UrlReferrer.OriginalString)
                ? Request.UrlReferrer.OriginalString : String.Empty;
            }
            else
            {
                user.UrlReferrer = String.Empty;
            }
            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                user.CurrentUser = HttpContext.Current.User;
            }

            // Add the New User to Collection
            _onlineUsers.Add(user);
            Application.UnLock();
        }

        public void Dispose()
        {
            if (module != null)
                module.Start -= new EventHandler(Session_Start);
        }

        SessionStateModule module;
        public void Init(HttpApplication context)
        {

            // Get the Current Session State Module
            module = context.Modules["Session"] as SessionStateModule;

            module.Start += new EventHandler(Session_Start);
        }
    }
}