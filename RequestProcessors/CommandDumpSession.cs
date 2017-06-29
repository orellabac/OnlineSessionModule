using ClosedXML.Excel;
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
    class CommandDumpSession : IRequestProcessor
    {
        public string key
        {
            get
            {
                return "commandDumpSession";
            }
        }

        public void Process(HttpContext context)
        {

            var sessionID = context.Request.QueryString["sessionID"];

            var sessionpages = SessionUtils.GetSessionInfoPagesCount(context.ApplicationInstance, sessionID, 20);


            if (string.IsNullOrWhiteSpace(sessionID)) return;
            var objResponse = context.Response;

            objResponse.AppendHeader("Cache-Control", "no-cache");
            objResponse.AppendHeader("content-disposition", "attachment; filename=session" + sessionID + ".xlsx");
            objResponse.BufferOutput = false;
            objResponse.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Session" + sessionID);
            worksheet.Cell("A1").Value = "Key";
            worksheet.Cell("B1").Value = "Value";
            worksheet.Cell("C1").Value = "Size";
            try
            {
                var data = SessionUtils.GetSessionInfo(context.ApplicationInstance, sessionID, 1, sessionpages.TotalItems);
                if (data!=null && data.data!=null)
                {
                    int start = 2;
                    for(var i=0;i<data.data.Count;i++)
                    {
                        var item = data.data[i];
                        worksheet.Cell($"A{start + i}" ).Value = item.key;
                        var value = item.value;

                        worksheet.Cell($"B{start + i}").Value = "" + item.value;
                        if (value is string)
                        {
                            
                            worksheet.Cell($"C{start + i}").Value = Encoding.Default.GetByteCount((string)value);
                        }
                        else if (value is byte[])
                        {
                            worksheet.Cell($"C{start + i}").Value = ((byte[])value).LongLength;
                        }
                        
                    }
                }
                var tempFile = Path.GetTempFileName() + ".xlsx";

                workbook.SaveAs(tempFile);
                objResponse.WriteFile(tempFile);
                objResponse.Flush();
                objResponse.Close();

            }
            catch(Exception ex)
            {
                Trace.TraceError($"Problem while creating excel file. Error {ex.Message}");
            }
        }
    }
}
