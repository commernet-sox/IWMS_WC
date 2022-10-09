using FWCore;
using iWMS.WebAPICore;
using Microsoft.AspNetCore.Mvc;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class APIController : Controller
    {
        [HttpGet]
        public string Post(string type = null)
        {
            APIResponse response = new APIResponse();
            response.SetResponse(false, "S20", "无效请求,请用Post请求！");
            return response.GetResult();
        }

        [HttpPost]
        public string Post()
        {
            APIResponse response = new APIResponse();
            string body = string.Empty;
            using (var stream = new MemoryStream())
            {
                Request.Body.CopyTo(stream);
                stream.Position = 0;
                byte[] bytes = stream.ToArray();
                body = Encoding.UTF8.GetString(bytes);
            }
            string error = string.Empty;
            APIRequest request = new APIRequest();
            if (!request.ParseString(HttpContext.Request.Query, body, out error))
            {
                response.SetResponse(false, "S01", "非法的请求参数" + error);
                return response.GetResult();
            }
            if (!CheckRequestParameter(request, response))
            {
                return response.GetResult();
            }
            if (!request.CheckSign())
            {
                response.SetResponse(false, "S02", "签名错误");
                return response.GetResult();
            }
            switch (request.method.ToLower())
            {
                case "gz.wms.deliverytask.create":
                    response = new SyncDeliveryTaskInfo().ProcessRequest<APIResponse>(request);
                    break;
                default:
                    response.SetResponse(false, "S05", $"无法识别的method：{request.method}");
                    break;

            }
            return response.GetResult();
        }

        private bool CheckRequestParameter(APIRequest request, APIResponse response)
        {
            if (string.IsNullOrWhiteSpace(request.method))
            {
                response.SetResponse(false, "S03", "请求参数method不能为空");
                return false;
            }
            if (string.IsNullOrWhiteSpace(request.timestamp))
            {
                response.SetResponse(false, "S03", "请求参数timestamp不能为空");
                return false;
            }
            else
            {
                var result = DateTime.Now;
                if (!DateTime.TryParse(request.timestamp, out result))
                {
                    response.SetResponse(false, "S04", "请求参数timestamp不合法");
                    return false;
                }
            }
            if (string.IsNullOrWhiteSpace(request.format))
            {
                response.SetResponse(false, "S03", "请求参数format不能为空");
                return false;
            }
            if (string.IsNullOrWhiteSpace(request.app_key))
            {
                response.SetResponse(false, "S03", "请求参数app_key不能为空");
                return false;
            }
            if (string.IsNullOrWhiteSpace(request.v))
            {
                response.SetResponse(false, "S03", "请求参数v不能为空");
                return false;
            }
            if (string.IsNullOrWhiteSpace(request.sign))
            {
                response.SetResponse(false, "S03", "请求参数sign不能为空");
                return false;
            }
            if (string.IsNullOrWhiteSpace(request.sign_method))
            {
                response.SetResponse(false, "S03", "请求参数sign_method不能为空");
                return false;
            }
            if (string.IsNullOrWhiteSpace(request.customerid))
            {
                response.SetResponse(false, "S03", "请求参数customerid不能为空");
                return false;
            }
            return true;
        }
    }
}
