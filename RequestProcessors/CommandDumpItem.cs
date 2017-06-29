using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Server.RequestProcessors
{
    class CommandDumpItem : IRequestProcessor
    {
        public string key
        {
            get
            {
                return "commandDumpItem";
            }
        }

        class DumpItemInfo
        {
            public string sessionID { get; set; }
            public string ID { get; set; }
        }

        public void Process(HttpContext context)
        {
            var objResponse = context.Response;
            string commandRequest;
            context.Request.InputStream.Position = 0;
            using (StreamReader inputStream = new StreamReader(context.Request.InputStream))
            {
                commandRequest = inputStream.ReadToEnd();
            }
            if (string.IsNullOrWhiteSpace(commandRequest)) {
                objResponse.Write("Invalid Request \r\n");
                return;
            }

            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var info = serializer.Deserialize<DumpItemInfo>(commandRequest);

    
            objResponse.AppendHeader("Cache-Control", "no-cache");
            var sessionID = info.sessionID;
            var itemID    = info.ID;
            try
            {
                bool statusOK=false;
                var value =  SessionUtils.TryGetItemByID(context.ApplicationInstance, sessionID, itemID,false,ref statusOK);
                if (statusOK)
                    objResponse.Write("" + value);
                else
                    objResponse.Write("ERROR retrieving value");
            }
            catch(Exception ex)
            {
                Trace.TraceError($"OnlineUserModel CommandDumpItem: {ex.Message}");
            }
        }
    }
}
