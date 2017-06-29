using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Server.RequestProcessors
{
    class Details : IRequestProcessor
    {
        public string key
        {
            get
            {
                return "details";
            }
        }

        public void Process(HttpContext context)
        {
            var objResponse = context.Response;
            objResponse.AppendHeader("Cache-Control", "no-cache");
           // objResponse.AppendHeader("Content-type", "application/json");
            var sessionID = context.Request.QueryString["sessionID"];
            int pageNumber, numItems;
            int.TryParse(context.Request.QueryString["page"], out pageNumber);
            int.TryParse(context.Request.QueryString["numItems"], out numItems);
            if (pageNumber < 1) return; //Exit if pages less that 1 requested
            var sessionpages = SessionUtils.GetSessionInfoPagesCount(context.ApplicationInstance, sessionID, numItems);
            if (!sessionpages.Invalid)
            {
                if (pageNumber > sessionpages.TotalPages) return; //Exit if pages over the top requested

                var data = SessionUtils.GetSessionInfo(context.ApplicationInstance, sessionID, pageNumber, numItems);
                var modelDetails = new { SessionID = sessionID,
                    data = data.data, PageSize = data.PageSize,
                    PageIndex = data.PageIndex, TotalPages = sessionpages.TotalPages, TotalItems = sessionpages.TotalItems, StartIndex = data.StartIndex };
                string resultDetails = SessionsInfoHandler.Parse("details", modelDetails);
                //var JavaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                //var detailsAsJson = JavaScriptSerializer.Serialize(data);
                objResponse.Output.WriteLine(resultDetails);
            }
        }
    }
}
