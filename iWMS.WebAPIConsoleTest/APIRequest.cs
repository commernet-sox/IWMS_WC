using FWCore.TaskManage;
using iWMS.WebAPICore;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.WebAPIConsoleTest
{
    public class APIRequest
    {
        public string method { get; set; }//接口类型 WMS提供 
        public string timestamp { get; set; }//yyyy-MM-dd HH:mm:ss
        public string format { get; set; }//固定值xml
        public string app_key { get; set; }//WMS提供 SuNing
        public string v { get; set; }//固定值2.0
        public string sign { get; set; }//奇门加签算法 app_secret WMS提供
        public string sign_method { get; set; }//md5
        public string customerid { get; set; }//WMS提供 
        public string appsecret { get; set; }
        public string body { get; set; }

        public APIRequest()
        {
            appsecret = "Test";
        }

        public bool ParseString(IQueryCollection query, string body, out string error)
        {
            error = string.Empty;
            try
            {
                if (string.IsNullOrWhiteSpace(body))
                {
                    return false;
                }
                this.body = body;
                if (query != null)
                {
                    foreach (var key in query.Keys)
                    {
                        switch (key.ToLower())
                        {
                            case "method":
                                this.method = SyncHelper.Decode(query[key].ToString());
                                break;
                            case "timestamp":
                                this.timestamp = SyncHelper.Decode(query[key].ToString());
                                break;
                            case "format":
                                this.format = SyncHelper.Decode(query[key].ToString());
                                break;
                            case "app_key":
                                this.app_key = SyncHelper.Decode(query[key].ToString());
                                break;
                            case "v":
                                this.v = SyncHelper.Decode(query[key].ToString());
                                break;
                            case "sign":
                                this.sign = SyncHelper.Decode(query[key].ToString());
                                break;
                            case "sign_method":
                                this.sign_method = SyncHelper.Decode(query[key].ToString());
                                break;
                            case "customerid":
                                this.customerid = SyncHelper.Decode(query[key].ToString());
                                break;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                error = ex.ToString();
                return false;
            }
        }

        public string GetRequestString()
        {
            return string.Format(@"method={0}&timestamp={1}&format={2}&app_key={3}&v={4}&sign_method={5}&customerId={6}"
               , SyncHelper.Encode(this.method), SyncHelper.Encode(this.timestamp), SyncHelper.Encode(this.format),
               SyncHelper.Encode(this.app_key), SyncHelper.Encode(this.v), SyncHelper.Encode(this.sign_method),
               SyncHelper.Encode(this.customerid));
        }

        public bool CheckSign()
        {
            CommonSign commonsign = new CommonSign();
            var url = "http://localhost:5000/api/post";
            string requestString = GetRequestString();
            url = url + "?" + requestString;
            string md5 = commonsign.sign(url, body, appsecret);
            return md5 == sign;
        }
    }
}
