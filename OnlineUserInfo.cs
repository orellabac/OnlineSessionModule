using System;
using System.Security.Principal;
using System.Text;

public class OnlineUserInfo
{
    public String UserAgent { get; set; }
    public String SessionId { get; set; }
    public String IPAddress { get; set; }
    public String UrlReferrer { get; set; }
    public DateTime SessionStarted { get; set; }
    public IPrincipal CurrentUser { get; set; }
    public long SessionSize { get; set; }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("UserAgent = {0} | ", UserAgent);
        sb.AppendFormat("SessionId = {0} | ", SessionId);
        sb.AppendFormat("IPAddress = {0} | ", IPAddress);
        sb.AppendFormat("UrlReferrer = {0} | ", UrlReferrer);
        sb.AppendFormat("SessionStarted = {0}", SessionStarted);
        sb.AppendFormat("SessionSize = {0}", SessionSize);
        return sb.ToString();
    }
}