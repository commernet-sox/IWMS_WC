using FWCore.TaskManage;
using iWMS.WebAPICore;
using System;
using System.IO;

namespace iWMS.WebAPIConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var message = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "body.txt"));

            APIRequest request = new APIRequest();
            request.method = "gz.wms.deliverytask.create";
            request.format = "json";
            request.app_key = "Test";
            request.v = "1.0";
            request.sign_method = "md5";
            request.customerid = "Test";
            request.timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string requestString = request.GetRequestString();
            string url = "http://127.0.0.1:5000/api" + "?" + requestString;
            WebAPICore.CommonSign commonSign = new CommonSign();
            string md5 = commonSign.sign(url, message, request.appsecret);
            url = url + "&sign=" + md5;
            var httpresult = SyncHelper.Post(url, true, message);
            if (httpresult.IsSuccess)
            {
                var result = httpresult.SuccessMsg;
                Console.WriteLine(result);
            }
            else
            {
                var result = httpresult.ErrMsg;
                Console.WriteLine(result);
            }
            Console.Read();
        }
    }
}
