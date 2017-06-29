using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Web.Hosting;
using System.Web.SessionState;
using System.Linq;
using System.Text.RegularExpressions;

public class SessionItemPair
{
    public string key { get; set; }
    public object value { get; set; }
}
public class SessionInfoPage
{
    public string status { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int PageIndex { get; set; }
    public int StartIndex { get;  set; }
    public int EndIndex { get;set; }

    public List<SessionItemPair> data = new List<SessionItemPair>();
}
public static class SessionUtils
{



    public static string DumpSession(HttpApplication httpApplication, string sessionId)
    {
        

        var data = GetSessionById(httpApplication, sessionId);
		object temp = data.Items["WMCACHE@"];
		var f = temp.GetType().GetField("cache", BindingFlags.NonPublic | BindingFlags.Instance);
        var dataTable =  f.GetValue(temp) as IDictionary<string,object>;


        if (dataTable != null)
        {
            using (StreamWriter file = new StreamWriter(httpApplication.Server.MapPath("~/" + sessionId + "-" + DateTime.Now.ToString("dd-mm-yyyy-HH-mm-ss") + ".csv"))) 
            {

				file.Write("key,type,subtype,flagStatic,lengthStatic,flagEvent,lengthEvent,flagPointerToObject,Size,Value");
				file.WriteLine();

                var keys = new string[dataTable.Count];
                dataTable.Keys.CopyTo(keys, 0);
                Array.Sort(keys);
                foreach (var key in keys)
                {
					string _type = "DATA";
					string _subtype = "";
					int _lengthStatic = 0;
					int _flagStatic = 0;
					int _lengthEvent = 0;
					int _flagEvent = 0;
					int _flagPointerToObject = 0;

					string originalKey = key;
                    file.Write(key.Replace("\"", "░").Replace(",", "┤").Replace("\n", "▄"));

					if (key.StartsWith("!"))
					{
						_type = "SURROGATE";

						if (key.StartsWith("!Value"))
						{
							_subtype = "VALUE";
						} else {
							_subtype = "TOP"; ;
						}
					} if (key.StartsWith("%")) {

						_type = "EVENT";
						_lengthEvent = key.IndexOf("#");
						_flagEvent = 1;

					} if (key.StartsWith("@@")) {
						_type = "WEBMAP";
					} if (key.StartsWith("^")){
						if (key.StartsWith("^Rt"))
						{
							_type = "REFERENCE_TABLE";
						}
						else
						{
							_type = "PROMISE";
						}
					} if (key.StartsWith("->"))
					{
						_type = "POINTER";
					} if (key.StartsWith(">"))
					{
						_type = "SHAREDSTATE";

						_lengthStatic = key.Length;
						_flagStatic = 1;

					} if (key.StartsWith("KI"))
					{
						_type = "ITEM_LIST";
					} if (key.StartsWith("KP"))
					{
						_type = "PAGE";
					} if (key.StartsWith("X"))
					{
						_subtype = "ADOPTED";
					}

					int pos1 = key.IndexOf("#>");
					if (pos1 > 0 )
					{
						_flagStatic = 1;
						_lengthStatic = key.Length - pos1;
					}

					pos1 = key.IndexOf("#%");
					if (pos1 > 0)
					{
						_flagEvent = 1;
						var idtemp = key.Substring(pos1) ;
						var pos2 = idtemp.IndexOf("#");
						var pos3 = idtemp.Length;
						_lengthEvent = Math.Min(pos2 == -1 ? pos3 : pos2, pos3);
					}

					if (key.Contains("->PO"))
					{
						_flagPointerToObject = 1;
					}


                    file.Write(",");


					file.Write(_type); file.Write(",");
					file.Write(_subtype); file.Write(",");
					file.Write(_lengthStatic); file.Write(",");
					file.Write(_flagStatic); file.Write(",");
					file.Write(_lengthEvent); file.Write(",");
					file.Write(_flagEvent); file.Write(",");
					file.Write(_flagPointerToObject); file.Write(",");


                    //Write Value length
                    var value = dataTable[key];
					if (value is byte[])
                    {
                        var asArray = value as byte[];
                        if (asArray != null)
                        {
                            file.Write(asArray.Length);
                        }
                        else
                            file.Write(0);

						file.Write(",");
						file.Write(value);
                    }
                    else
                    {
                        value = value + "";
                        file.Write(((string)value).Length * 2);
						file.Write(",");
						if (((string)value).Length > 5000)
						{
							file.Write("... DATA ..");
						}
						else
						{
							file.Write(((value as string).Replace("\"", "░").Replace(",", "┤").Replace("\n", "▄")));
						}
                    }

                    //file.Write(",");
                    //Value representation
					//file.Write(result.Replace(",", "_") + "");


                    file.WriteLine();
                }
                file.Close();
                return "dumping";
            }
        }
        return "nodata";
    }
    public class SessionSize
    {
        public bool Invalid;
        public int TotalItems;
        public int TotalPages;
    }
    public static SessionSize GetSessionInfoPagesCount(HttpApplication httpApplication, string sessionId, int pageSize)
    {
        var res = new SessionSize() { Invalid = true, TotalPages = -1, TotalItems = -1 };
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return res;
        }
     

        var data = GetSessionById(httpApplication, sessionId);
        if (data != null)
        {
            res.Invalid = false;
            
            if (data.Items["WM@"] != null) //Is it a webmap session
            {
                var dataTable = data.Items["WM@"] as IDictionary<string, object>;
                if (dataTable != null)
                {
                    res.TotalItems = dataTable.Count;
                    res.TotalPages = (dataTable.Count / pageSize) + (dataTable.Count % pageSize > 0 ? pageSize : 1);
                }
                else
                {
                    res.Invalid = true; 
                }
            }
            else
            {
                res.TotalItems = data.Items.Count;
                res.TotalPages = data.Items.Count / pageSize;
            }
        }
        return res;
    }

    public static object TryGetItemByID(HttpApplication httpApplication, string sessionId, string itemID,bool returnRaw, ref bool statusOK)
    {
        statusOK = false;
        var data = GetSessionById(httpApplication, sessionId);
        if (data == null)
        {
            return null;
        }
        IEnumerable keys = null;
        Func<string, object> getter = null;

        //Trying to get information
        if (data.Items["WM@"] != null)
        {
            IDictionary<string, object> dataTable;
            dataTable = (IDictionary<string, object>)((SessionStateStoreData)data).Items["WM@"];
            keys = dataTable.Keys;
            getter = (string x) =>
            {
                object value = null;
                dataTable.TryGetValue(x, out value);
                if (returnRaw)
                    return value;
                else 
                    return "" + value;
            };
        }
        else
        {
            keys = data.Items.Keys;
            getter = (string x) =>
            {
                if (returnRaw)
                    return data.Items[x];
                else
                    return "" + data.Items[x];
            };

        }
        if (keys != null && getter != null)
        {
            statusOK = true;
            return getter(itemID);
        }
        else
        {
            //Something is wrong
            statusOK = false;
            return null;
        }
    }

    public static SessionInfoPage GetSessionInfoByPattern(HttpApplication httpApplication, string sessionId, int lastIndex,int pageSize,Regex regex)
    {
        var result = new SessionInfoPage() { PageSize = pageSize, StartIndex = lastIndex,PageIndex=-1, status = "ok" };

        var data = GetSessionById(httpApplication, sessionId);
        if (data == null)
        {
            result.status = "error";
            return result;
        }
        IEnumerable keys = null;
        Func<string, string> getter = null;

        //Trying to get information
        if (data.Items["WM@"] != null)
        {
            IDictionary<string, object> dataTable;
            dataTable = (IDictionary<string, object>)((SessionStateStoreData)data).Items["WM@"];
            keys = dataTable.Keys;
            getter = (string x) => "" + dataTable[x];
        }
        else
        {
            keys = data.Items.Keys;
            getter = (string x) => "" + data.Items[x];
        }
        if (keys != null && getter != null)
        {
            result.StartIndex = lastIndex;
            //result.EndIndex = pageSize * ((pageIndex - 1) + 1);

            var enumerator = keys.GetEnumerator();
            int index = 0;
            bool lastMoveNext = false;
            while ((lastMoveNext = enumerator.MoveNext()))
            {
                index++;
                if (index >= result.StartIndex) break;
            }
            if (lastMoveNext)
            {
                do
                {
                    var key = "" + enumerator.Current;
                    if (regex.IsMatch(key))
                    {
                        result.data.Add(new SessionItemPair() { key = key, value = getter(key) });
                    }
                    index++;
                    if (result.data.Count >= result.PageSize)
                    {
                        result.EndIndex = index;
                        break;
                    }
                } while (enumerator.MoveNext());
            }

        }
        else
        {
            //Something is wrong
            result.status = "error";
        }
        return result;
    }

    public static SessionInfoPage GetSessionInfo(HttpApplication httpApplication, string sessionId, int pageIndex, int pageSize)
    {
        var result = new SessionInfoPage() { PageSize = pageSize, PageIndex = pageIndex, status="ok" };
       
        var data = GetSessionById(httpApplication, sessionId);
        if (data==null)
        {
            result.status = "error";
            return result;
        }
        IEnumerable keys=null;
        Func<string, string> getter = null;

        //Trying to get information
        if (data.Items["WM@"]!=null)
        {
            IDictionary<string,object> dataTable;
            dataTable = (IDictionary<string, object>) ((SessionStateStoreData)data).Items["WM@"];
            keys = dataTable.Keys;
            getter = (string x) => "" + dataTable[x];
        }
        else
        {
            keys = data.Items.Keys;
            getter = (string x) => "" + data.Items[x];
        }
        if (keys!=null && getter!=null)
        {
            result.StartIndex = pageSize * (pageIndex - 1);
            result.EndIndex =   pageSize * ((pageIndex - 1) + 1);

            var enumerator = keys.GetEnumerator();
            int index = 0;
            bool lastMoveNext = false;
            while ((lastMoveNext = enumerator.MoveNext()))
            {
                index++;
                if (index >= result.StartIndex) break;
            }
            if (lastMoveNext)
            {
                do
                {
                    var key = "" + enumerator.Current;
                    result.data.Add(new SessionItemPair() { key = key, value = getter(key) });
                    index++;
                    if (index >= result.EndIndex) break;
                } while (enumerator.MoveNext());
            }

        }
        else
        {
            //Something is wrong
            result.status = "error";
        }
        
        return result;
    }

    public static SessionStateStoreData GetSessionById(HttpApplication httpApplication, string sessionId)
    {
        // Black magic #1: getting to SessionStateModule
        HttpModuleCollection httpModuleCollection = httpApplication.Modules;
        SessionStateModule sessionHttpModule = httpModuleCollection["Session"] as SessionStateModule;
        if (sessionHttpModule == null)
        {
            // Couldn't find Session module
            return null;
        }

        // Black magic #2: getting to SessionStateStoreProviderBase through reflection
        FieldInfo fieldInfo = typeof(SessionStateModule).GetField("_store", BindingFlags.NonPublic | BindingFlags.Instance);
        SessionStateStoreProviderBase sessionStateStoreProviderBase = fieldInfo.GetValue(sessionHttpModule) as SessionStateStoreProviderBase;
        if (sessionStateStoreProviderBase == null)
        {
            // Couldn't find sessionStateStoreProviderBase
            return null;
        }

        // Black magic #3: generating dummy HttpContext out of the thin air. sessionStateStoreProviderBase.GetItem in #4 needs it.
        SimpleWorkerRequest request = new SimpleWorkerRequest("dummy.html", null, new StringWriter());
        HttpContext context = new HttpContext(request);

        // Black magic #4: using sessionStateStoreProviderBase.GetItem to fetch the data from session with given Id.
        bool locked;
        TimeSpan lockAge;
        object lockId;
        SessionStateActions actions;
        SessionStateStoreData sessionStateStoreData = sessionStateStoreProviderBase.GetItem(
            context, sessionId, out locked, out lockAge, out lockId, out actions);
        return sessionStateStoreData;
    }

    internal static long CalculateSessionSize(SessionStateStoreData Session)
    {
        long totalSessionBytes = 0;
        BinaryFormatter b = new BinaryFormatter();
        MemoryStream m;
        foreach (var key in Session.Items.Keys)
        {
            m = new MemoryStream();
            if (key != null)
            {
                b.Serialize(m, key);
                var value = Session.Items[key.ToString()];
                if (value != null)
                {
                    b.Serialize(m, value);
                }
            }
            totalSessionBytes += m.Length;
        }
        return totalSessionBytes;
    }
}