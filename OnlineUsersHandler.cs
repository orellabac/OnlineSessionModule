using System.Web;

namespace UpgradeHelpers.WebMap.Server
{
    /// <summary>
    /// Summary description for NewHandler.
    /// </summary>
    public class NewHandler : IHttpHandler
    {
        public NewHandler()
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

            objResponse.Write("<html>");
            objResponse.Write("<body>");
            objResponse.Write("<h1> Current Online Users </h1>");
            objResponse.Write("<ul>");
            foreach (var user in currentUsers)
            {
                objResponse.Write("<li>");

                var sessionData = SessionUtils.GetSessionById(context.ApplicationInstance,user.SessionId);
                user.SessionSize = SessionUtils.CalculateSessionSize(sessionData);
                objResponse.Write(user.ToString());

                objResponse.Write("</li>");
            }
            objResponse.Write("</ul>");
            objResponse.Write("</body>");
            objResponse.Write("</html>");
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