using System.Web;

namespace Server
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
            if (context.Request.Url.LocalPath.EndsWith("sessions.info"))
            {
                var currentUsers = UpgradeHelpers.WebMap.Server.OnlineUsersModule.OnlineUsers;
                //First update sesssion size info
                foreach (var user in currentUsers)
                {
                    var sessionData = SessionUtils.GetSessionById(context.ApplicationInstance, user.SessionId);
                    if (sessionData != null)
                    {
                        try
                        {
                            user.SessionSize = SessionUtils.CalculateSessionSize(sessionData);
                        }
                        catch
                        {
                            user.SessionSize = -1;
                        }
                    }
                }
                //var template = new OnlineUserModule.CurrentSessions();
                //template.OnlineUsers = currentUsers;
                objResponse.AppendHeader("Cache-Control", "no-cache");
               // objResponse.Write(template.TransformText());
            }
            else if (context.Request.Url.LocalPath.EndsWith("pageinfo.info"))
            {
                int pageSize = 20;
                var sessionID = context.Request.QueryString["sessionID"];
                var sessionpages = SessionUtils.GetSessionInfoPagesCount(context.ApplicationInstance, sessionID, pageSize);
                var data = SessionUtils.GetSessionInfo(context.ApplicationInstance, sessionID, 1, pageSize);

                /*var template = new OnlineUserModule.Pager();
                template.PagesCount = sessionpages;
                template.PageSize = 20;
                template.SessionID = sessionID;
                template.Data = data;
                template.showButtons = true;
                objResponse.AppendHeader("Cache-Control", "no-cache");
                objResponse.Write(template.TransformText());
                */
            }
            else if (context.Request.Url.LocalPath.EndsWith("pageinfonext.info"))
            {
     
                var sessionID = context.Request.QueryString["sessionID"];
                var pageSize = int.Parse(context.Request.QueryString["pageSize"]);
                var pageIndex = int.Parse(context.Request.QueryString["pageIndex"]);

                var sessionpages = SessionUtils.GetSessionInfoPagesCount(context.ApplicationInstance, sessionID, pageSize);
                var data = SessionUtils.GetSessionInfo(context.ApplicationInstance, sessionID, pageIndex, pageSize);

               /* var template = new OnlineUserModule.Pager();
                template.showButtons = false;
                template.PagesCount = sessionpages;
                template.PageSize = pageSize;
                template.SessionID = sessionID;
                template.Data = data;
                objResponse.AppendHeader("Cache-Control", "no-cache");
                objResponse.Write(template.TransformText());*/
            }
            else if (context.Request.Url.LocalPath.EndsWith("dump.info"))
            {
                var sessionID = context.Request.QueryString["sessionID"];
                objResponse.Write(SessionUtils.DumpSession(context.ApplicationInstance,sessionID));
            }
            else
            {

               /* var template = new OnlineUserModule.DashboardMainTemplate();
                objResponse.Write(template.TransformText());*/
            }
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