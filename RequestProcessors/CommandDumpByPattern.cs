using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Server.RequestProcessors
{
    class CommandDumpByPattern : IRequestProcessor
    {
        public string key
        {
            get
            {
                return "commandDumpByPattern";
            }
        }

        class RequestInfo
        {
            public string sessionID { get; set; }
            public int page { get; set; }
            public int numItems { get; set; }
            public int index { get; set; }
            public string regex { get;  set; }
        }

        public void Process(HttpContext context)
        {
            var objResponse = context.Response;
            objResponse.AppendHeader("Cache-Control", "no-cache");
                // objResponse.AppendHeader("Content-type", "application/json");



                string commandRequest;
                context.Request.InputStream.Position = 0;
                using (StreamReader inputStream = new StreamReader(context.Request.InputStream))
                {
                    commandRequest = inputStream.ReadToEnd();
                }
                if (string.IsNullOrWhiteSpace(commandRequest))
                {
                    objResponse.Write("Invalid Request \r\n");
                    return;
                }

                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                var info = serializer.Deserialize<RequestInfo>(commandRequest);

            var sessionID = info.sessionID;
            int pageNumber=info.page, numItems=info.numItems;
            var regex = info.regex;
            int index = info.index;
            //??if (pageNumber < 1) return; //Exit if pages less that 1 requested
           

            Regex regularExpression;
            try
            {
                regularExpression = new Regex(regex);
            }
            catch (Exception regexExp)
            {
                Trace.TraceError($"Invalid regular expression {regexExp.Message}");
                objResponse.Write($"Invalid regular expression [{regex}]\r\n");
                return;
            }
            var sessionpages = SessionUtils.GetSessionInfoPagesCount(context.ApplicationInstance, sessionID, numItems);
            if (!sessionpages.Invalid)
            {
                var data = SessionUtils.GetSessionInfoByPattern(context.ApplicationInstance, sessionID, index,numItems,regularExpression);
                var modelDetails = new
                {
                    SessionID = sessionID,
                    data = data.data,
                    PageSize = data.PageSize,
                    PageIndex = data.PageIndex,
                    TotalPages = sessionpages.TotalPages,
                    TotalItems = sessionpages.TotalItems,
                    StartIndex = data.StartIndex,
                    Pattern = regex
                };
                string resultDetails = SessionsInfoHandler.Parse("dumpbypattern", modelDetails);
                //var JavaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                //var detailsAsJson = JavaScriptSerializer.Serialize(data);
                objResponse.Output.WriteLine(resultDetails);
            }
        }
    }
}
