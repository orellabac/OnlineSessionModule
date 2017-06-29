using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Server.RequestProcessors
{
    class ConsoleRequest : IRequestProcessor
    {
        public string key
        {
            get
            {
                return "consoleRequest";
            }
        }

        public void Process(HttpContext context)
        {
            string commandRequest;
            context.Request.InputStream.Position = 0;
            using (StreamReader inputStream = new StreamReader(context.Request.InputStream))
            {
                commandRequest = inputStream.ReadToEnd();
            }
           // var commandResponse = ProcessConsoleCommand(commandRequest);
 
        }
    }
}
