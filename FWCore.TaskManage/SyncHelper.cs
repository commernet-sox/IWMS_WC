using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace FWCore.TaskManage
{
    public static class SyncHelper
    {
        public static string MD5Hash(string inputString)
        {
            byte[] textBytes = Encoding.UTF8.GetBytes(inputString);

            var cryptHandler = new MD5CryptoServiceProvider();
            byte[] hash = cryptHandler.ComputeHash(textBytes);
            StringBuilder sb = new StringBuilder(32);
            foreach (byte a in hash)
            {
                sb.Append(a.ToString("x2"));
            }

            return sb.ToString();
        }

        public static string Encode(string value)
        {
            return HttpUtility.UrlEncode(value, Encoding.UTF8);
        }

        public static string Decode(string value)
        {
            return HttpUtility.UrlDecode(value, Encoding.UTF8);
        }

        public static DE_OperationResult PostAsync(string url, bool isPost, string param)
        {
            Task<DE_OperationResult> tk = Task<DE_OperationResult>.Factory.StartNew(() =>
            {
                return Post(url, isPost, param);
            });
            return tk.Result;
        }

        public static DE_OperationResult PostText(string url, string param)
        {
            DE_OperationResult result = new DE_OperationResult();
            try
            {
                #region ---- 完成 HTTP POST 请求----
                var req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "POST";    //默认值是 GET
                req.KeepAlive = true;
                req.UserAgent = "iWMS";
                req.Timeout = 300000;
                req.ContentType = "text/plain";

                if (null != param)
                {
                    byte[] postData = Encoding.UTF8.GetBytes(param);
                    Stream reqStream = req.GetRequestStream();
                    reqStream.Write(postData, 0, postData.Length);
                    reqStream.Close();
                }
                using (HttpWebResponse rsp = (HttpWebResponse)req.GetResponse())
                {
                    Encoding encoding = System.Text.Encoding.UTF8;
                    using (Stream stream = rsp.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream, encoding);
                        var htmlContext = reader.ReadToEnd();
                        //获取的内容先进行一次Html实体解码
                        result.SuccessMsg = HttpUtility.HtmlDecode(htmlContext);
                        result.IsSuccess = true;

                        if (reader != null) reader.Close();
                        if (stream != null) stream.Close();
                    }
                    if (rsp != null) rsp.Close();
                }
                #endregion
            }
            catch (UriFormatException uriExp)
            {
                result.IsSuccess = false;
                result.ErrMsg = string.Format("Invalid URI：[{0}] {1}{2}", url, Environment.NewLine, uriExp.StackTrace);
            }
            catch (WebException wex)
            {
                result.IsSuccess = false;
                result.ErrMsg = wex.Message + Environment.NewLine + wex.Status.ToString() +
                    Environment.NewLine + url + Environment.NewLine + wex.StackTrace;

                HttpWebResponse tempresp = wex.Response as HttpWebResponse;
                if (null != tempresp)
                {
                    Encoding encoding = System.Text.Encoding.UTF8;
                    StreamReader sr = new StreamReader(tempresp.GetResponseStream(), encoding);
                    string strHtml = sr.ReadToEnd();
                    result.ErrMsg += Environment.NewLine + strHtml;
                }
            }
            catch (Exception exp)
            {
                result.IsSuccess = false;
                //自定错误消息格式
                result.ErrMsg = exp.Message + Environment.NewLine + exp.StackTrace;
            }

            return result;
        }

        public static DE_OperationResult Post(string url, bool isPost, string param)
        {
            DE_OperationResult result = new DE_OperationResult();
            try
            {
                #region ---- 完成 HTTP POST 请求----
                var req = (HttpWebRequest)WebRequest.Create(url);
                if (isPost)
                {
                    req.Method = "POST";    //默认值是 GET
                }
                req.KeepAlive = true;
                req.UserAgent = "iWMS";
                req.Timeout = 300000;
                req.ContentType = "application/x-www-form-urlencoded;charset=utf-8";

                if (null != param)
                {
                    byte[] postData = Encoding.UTF8.GetBytes(param);
                    Stream reqStream = req.GetRequestStream();
                    reqStream.Write(postData, 0, postData.Length);
                    reqStream.Close();
                }
                using (HttpWebResponse rsp = (HttpWebResponse)req.GetResponse())
                {
                    Encoding encoding = System.Text.Encoding.UTF8;
                    using (Stream stream = rsp.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream, encoding);
                        var htmlContext = reader.ReadToEnd();
                        //获取的内容先进行一次Html实体解码
                        result.SuccessMsg = HttpUtility.HtmlDecode(htmlContext);
                        result.IsSuccess = true;

                        if (reader != null) reader.Close();
                        if (stream != null) stream.Close();
                    }
                    if (rsp != null) rsp.Close();
                }
                #endregion
            }
            catch (UriFormatException uriExp)
            {
                result.IsSuccess = false;
                result.ErrMsg = string.Format("Invalid URI：[{0}] {1}{2}", url, Environment.NewLine, uriExp.StackTrace);
            }
            catch (WebException wex)
            {
                result.IsSuccess = false;
                result.ErrMsg = wex.Message + Environment.NewLine + wex.Status.ToString() +
                    Environment.NewLine + url + Environment.NewLine + wex.StackTrace;

                HttpWebResponse tempresp = wex.Response as HttpWebResponse;
                if (null != tempresp)
                {
                    Encoding encoding = System.Text.Encoding.UTF8;
                    StreamReader sr = new StreamReader(tempresp.GetResponseStream(), encoding);
                    string strHtml = sr.ReadToEnd();
                    result.ErrMsg += Environment.NewLine + strHtml;
                }
            }
            catch (Exception exp)
            {
                result.IsSuccess = false;
                //自定错误消息格式
                result.ErrMsg = exp.Message + Environment.NewLine + exp.StackTrace;
            }

            return result;
        }

        /// <summary>
        /// 发送请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="isPost"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static DE_OperationResult Post(string url, bool isPost, IDictionary<string, string> param)
        {
            string paramstr = PostData(param);
            return Post(url, isPost, paramstr);
        }

        public static DE_OperationResult Post(string url, string body)
        {
            return Post(url, true, body);
        }

        /// <summary>
        /// 组装普通文本请求参数。
        /// </summary>
        /// <param name="parameters">Key-Value形式请求参数字典</param>
        /// <returns>URL编码后的请求数据</returns>
        public static string PostData(IDictionary<string, string> parameters)
        {
            StringBuilder postData = new StringBuilder();
            bool hasParam = false;

            IEnumerator<KeyValuePair<string, string>> dem = parameters.GetEnumerator();
            while (dem.MoveNext())
            {
                string name = dem.Current.Key;
                string value = dem.Current.Value;
                // 忽略参数名或参数值为空的参数
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
                {
                    if (hasParam)
                    {
                        postData.Append("&");
                    }
                    postData.Append(name);
                    postData.Append("=");
                    postData.Append(Uri.EscapeDataString(value));
                    hasParam = true;
                }
            }

            return postData.ToString();
        }

    }
}
