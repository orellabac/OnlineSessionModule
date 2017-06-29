using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Server.RequestProcessors
{
    class Size : IRequestProcessor
    {
        public string key
        {
            get
            {
                return "size";
            }
        }

        public void Process(HttpContext context)
        {
            var objResponse = context.Response;
            var sessionID = context.Request.QueryString["sessionID"];
            var sessionData = SessionUtils.GetSessionById(context.ApplicationInstance, sessionID);
            objResponse.AppendHeader("Cache-Control", "no-cache");
            var sizeResponse = "?";
            if (sessionData != null)
            {
                try
                {
                    sizeResponse = "" + SessionUtils.CalculateSessionSize(sessionData);
                }
                catch
                {
                    sizeResponse = "error??";
                }
            }
            objResponse.Write(sizeResponse);
        }
    }
}
