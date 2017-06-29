using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Server.RequestProcessors
{
    class Sessions : IRequestProcessor
    {
        public string key
        {
            get
            {
                return "sessions";
            }
        }

        public void Process(HttpContext context)
        {
            var objResponse = context.Response;
            var currentUsers = OnlineUsersModule.OnlineUsers;
            var model = new { users = currentUsers };
            string result = SessionsInfoHandler.Parse("sessions", model);
            //First update sesssion size info

            objResponse.AppendHeader("Cache-Control", "no-cache");
            objResponse.Write(result);
        }
    }
}
