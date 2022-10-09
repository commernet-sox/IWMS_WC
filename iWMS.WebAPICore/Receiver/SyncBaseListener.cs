using FWCore;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace iWMS.WebAPICore
{
    public abstract class SyncBaseListener<TRequest, TResponse>
    {
        protected static object LockObject = new object();

        public virtual APIResponse ProcessRequest<TResponseEntity>(APIRequest apiRequest) where TResponseEntity : APIResponse
        {
            APIResponse response = new APIResponse();
            response.SetResponse(true, string.Empty, string.Empty);
            string bizdata = apiRequest.body;
            TRequest request = default(TRequest);
            try
            {
                request = JsonConvert.DeserializeObject<TRequest>(bizdata);
            }
            catch (Exception ex)
            {
                NLogUtil.WriteFileLog(NLog.LogLevel.Error, LogType.ApiRequest, "格式解析", "非法的JSON格式", ex);
                response.SetResponse(false, "", "非法的JSON格式");
                return response;
            }
            try
            {
                response = this.HandleProcessEntity(request);
            }
            catch (Exception ex)
            {
                NLogUtil.WriteFileLog(NLog.LogLevel.Error, LogType.ApiRequest, "业务提示", "业务服务异常", ex);
                response.SetResponse(false, "", "业务服务异常");
                return response;
            }
            return response;
        }

        protected abstract APIResponse HandleProcessEntity(TRequest request);
    }
}
