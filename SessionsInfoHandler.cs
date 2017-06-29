
using System.IO;
using System.Web;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Server
{
    /// <summary>
    /// Summary description for NewHandler.
    /// </summary>
    public class SessionsInfoHandler : IHttpHandler
    {

        IList<IRequestProcessor> processors;
        public SessionsInfoHandler()
        {
            //
            // TODO: Add constructor logic here
            //
            processors = new List<IRequestProcessor>();
            processors.Add(new Server.RequestProcessors.Sessions());
            processors.Add(new Server.RequestProcessors.Size());
            processors.Add(new Server.RequestProcessors.Console());
            processors.Add(new Server.RequestProcessors.ConsoleRequest());
            processors.Add(new Server.RequestProcessors.Details());
            processors.Add(new Server.RequestProcessors.CommandDumpItem());

            processors.Add(new Server.RequestProcessors.CommandDumpSession());
            processors.Add(new Server.RequestProcessors.CommandDumpBinItem());
            processors.Add(new Server.RequestProcessors.CommandDumpByPattern());

        }

        private string GetRequestIdentifier(HttpContext context)
        {
            var localPath = context.Request.Url.LocalPath;
            var pattern = ".axd/";
            var indexOf = localPath.IndexOf(pattern);
            if (indexOf!=-1)
            {
                if (localPath.IndexOf("|") != -1)
                {
                    var start = indexOf + pattern.Length;
                    var length = localPath.IndexOf('|') - start;
                    var requestIdentifier = localPath.Substring(start, length);
                    return requestIdentifier;
                }
                else
                {
                    var requestIdentifier = localPath.Substring(indexOf + pattern.Length);
                    return requestIdentifier;
                }
            }
            return string.Empty;
        }

 
        #region Implementation of IHttpHandler
        public void ProcessRequest(System.Web.HttpContext context)
        {
            HttpResponse objResponse = context.Response;
            var requestIdentifier = GetRequestIdentifier(context);
            var processor = processors.FirstOrDefault(x => x.key == requestIdentifier);
            if (processor != null)
                processor.Process(context);
          
            //if (context.Request.Url.LocalPath.EndsWith("sessions.info"))
            //{
            //   
            //    //var template = new OnlineUserModule.CurrentSessions();
            //    //template.OnlineUsers = currentUsers;
            //    objResponse.AppendHeader("Cache-Control", "no-cache");
            //   // objResponse.Write(template.TransformText());
            //}
            //else if (context.Request.Url.LocalPath.EndsWith("pageinfo.info"))
            //{
            //    int pageSize = 20;
            //    var sessionID = context.Request.QueryString["sessionID"];
            //    var sessionpages = SessionUtils.GetSessionInfoPagesCount(context.ApplicationInstance, sessionID, pageSize);
            //    var data = SessionUtils.GetSessionInfo(context.ApplicationInstance, sessionID, 1, pageSize);

            //    /*var template = new OnlineUserModule.Pager();
            //    template.PagesCount = sessionpages;
            //    template.PageSize = 20;
            //    template.SessionID = sessionID;
            //    template.Data = data;
            //    template.showButtons = true;
            //    objResponse.AppendHeader("Cache-Control", "no-cache");
            //    objResponse.Write(template.TransformText());
            //    */
            //}
            //else if (context.Request.Url.LocalPath.EndsWith("pageinfonext.info"))
            //{
     
            //    var sessionID = context.Request.QueryString["sessionID"];
            //    var pageSize = int.Parse(context.Request.QueryString["pageSize"]);
            //    var pageIndex = int.Parse(context.Request.QueryString["pageIndex"]);

            //    var sessionpages = SessionUtils.GetSessionInfoPagesCount(context.ApplicationInstance, sessionID, pageSize);
            //    var data = SessionUtils.GetSessionInfo(context.ApplicationInstance, sessionID, pageIndex, pageSize);

            //   /* var template = new OnlineUserModule.Pager();
            //    template.showButtons = false;
            //    template.PagesCount = sessionpages;
            //    template.PageSize = pageSize;
            //    template.SessionID = sessionID;
            //    template.Data = data;
            //    objResponse.AppendHeader("Cache-Control", "no-cache");
            //    objResponse.Write(template.TransformText());*/
            //}
            //else if (context.Request.Url.LocalPath.EndsWith("dump.info"))
            //{
            //    var sessionID = context.Request.QueryString["sessionID"];
            //    objResponse.Write(SessionUtils.DumpSession(context.ApplicationInstance,sessionID));
            //}
            //else
            //{

            //   /* var template = new OnlineUserModule.DashboardMainTemplate();
            //    objResponse.Write(template.TransformText());*/
            //}
        }

        internal static string Parse(string templateName, object modelDetails)
        {
            switch(templateName)
            {
                case "console":
                    return new Server.Templates.console() { Model = modelDetails }.TransformText();
                case "details":
                    return new Server.Templates.details() { Model = modelDetails }.TransformText();
                case "dumpbypattern":
                    return new Server.Templates.dumpbypattern() { Model = modelDetails }.TransformText();
                case "sessions":
                    return new Server.Templates.sessions() { Model = modelDetails }.TransformText();
                default:
                    throw new NotImplementedException($"Sorry no template for {templateName}");
            }
        }

        class ConsoleCommandRequest
        {
            public string Command { get; set; }
        }
        private object ProcessConsoleCommand(string commandRequest)
        {

            var JavaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var command = JavaScriptSerializer.Deserialize<ConsoleCommandRequest>(commandRequest);
            return "{status:'ok'}";
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