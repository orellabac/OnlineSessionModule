using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Server.RequestProcessors
{
    class Console : IRequestProcessor
    {
        public string key
        {
            get
            {
                return "console";
            }
        }

        public void Process(HttpContext context)
        {
            var localPath = context.Request.Url.LocalPath;
            var index = localPath.IndexOf("|");
            if (index == -1) return;
            var sessionId = localPath.Substring(index+1);
            var objResponse = context.Response;
            var modelConsole = new { SessionID = sessionId};
            string resultConsole = SessionsInfoHandler.Parse("console", modelConsole);
            objResponse.AppendHeader("Cache-Control", "no-cache");
            objResponse.Write(resultConsole);
        }
    }
}
