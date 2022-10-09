using FWCore;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.WebAPICore
{
    public class CommonSign
    {
        public String sign(String url, String body, String secretKey)
        {
            Dictionary<String, String> param = getParamsFromUrl(url);

            // 1. 第一步，确保参数已经排序
            string[] keys = new string[param.Keys.Count];
            param.Keys.CopyTo(keys, 0);
            Array.Sort(keys);

            // 2. 第二步，把所有参数名和参数值拼接在一起(包含body体)
            String joinedParams = joinRequestParams(param, body, secretKey, keys);

            //FileLogUtil.WriteError("joinedParams:" + joinedParams);

            // 3. 第三步，使用加密算法进行加密（目前仅支持md5算法）
            String signMethod = param["sign_method"];
            if (!string.Equals("md5", signMethod))
            {
                //TODO
                return null;
            }
            byte[] abstractMesaage = digest(joinedParams);

            // 4. 把二进制转换成大写的十六进制
            String sign = byte2Hex(abstractMesaage);

            return sign;
        }

        private Dictionary<string, string> getParamsFromUrl(String url)
        {
            Dictionary<string, string> requestParams = new Dictionary<string, string>();
            try
            {
                String fullUrl = System.Web.HttpUtility.UrlDecode(url, System.Text.Encoding.UTF8);
                String[] urls = fullUrl.Split('?');
                if (urls.Length == 2)
                {
                    String[] paramArray = urls[1].Split('&');
                    foreach (String param in paramArray)
                    {
                        String[] values = param.Split('=');
                        if (values.Length == 2)
                        {
                            requestParams.Add(values[0], values[1]);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //FileLogUtil.WriteError(e.ToString());
                return null;
            }
            return requestParams;
        }
        private String byte2Hex(byte[] bytes)
        {
            char[] hexDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
            int j = bytes.Length;
            char[] str = new char[j * 2];
            int k = 0;
            foreach (byte byte0 in bytes)
            {
                str[k++] = hexDigits[byte0 >> 4 & 0xf];
                str[k++] = hexDigits[byte0 & 0xf];
            }
            return new String(str);
        }

        private byte[] digest(String message)
        {
            try
            {
                //获取加密服务  
                MD5CryptoServiceProvider md5CSP = new MD5CryptoServiceProvider();

                //获取要加密的字段，并转化为Byte[]数组  
                byte[] testEncrypt = System.Text.Encoding.UTF8.GetBytes(message);

                //加密Byte[]数组  
                byte[] resultEncrypt = md5CSP.ComputeHash(testEncrypt);
                return resultEncrypt;
            }
            catch (Exception e)
            {
                NLogUtil.WriteFileLog(LogLevel.Error,LogType.Web,"加密", e.ToString());
                return null;
            }
        }

        private String joinRequestParams(Dictionary<String, String> param, String body, String secretKey, String[] sortedKes)
        {
            StringBuilder sb = new StringBuilder(secretKey); // 前面加上secretKey

            foreach (String key in sortedKes)
            {
                if ("sign".Equals(key))
                {
                    continue; // 签名时不计算sign本身
                }
                else
                {
                    String value = param[key];
                    if (isNotEmpty(key) && isNotEmpty(value))
                    {
                        sb.Append(key).Append(value);
                    }
                }
            }
            sb.Append(body); // 拼接body体
            sb.Append(secretKey); // 最后加上secretKey
            return sb.ToString();
        }

        private bool isNotEmpty(String s)
        {
            return null != s && !"".Equals(s);
        }
    }
}
