using System.Web;

namespace UpgradeHelpers.WebMap.Server
{
    /// <summary>
    /// Summary description for NewHandler.
    /// </summary>
    public class SessionsInfoHandler : IHttpHandler
    {
        public SessionsInfoHandler()
        {
            //
            // TODO: Add constructor logic here
            //
        }



        #region Implementation of IHttpHandler
        public void ProcessRequest(System.Web.HttpContext context)
        {
            HttpResponse objResponse = context.Response;
            var currentUsers = UpgradeHelpers.WebMap.Server.OnlineUsersModule.OnlineUsers;
            //First update sesssion size info
            foreach (var user in currentUsers)
            {
                var sessionData = SessionUtils.GetSessionById(context.ApplicationInstance, user.SessionId);
                user.SessionSize = SessionUtils.CalculateSessionSize(sessionData);
            }
            var template = new OnlineUserModule.OnlineUsersTemplate();
            template.OnlineUsers = currentUsers;
            objResponse.Write(template.TransformText());
        }
        public bool IsReusable
        {
            get
            {
                return true;
            }
        }
        #endregion
    }
}