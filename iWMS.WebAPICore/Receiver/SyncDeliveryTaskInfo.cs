using iWMS.WebAPICore.Model;
using System;

namespace iWMS.WebAPICore
{
    public class SyncDeliveryTaskInfo : SyncBaseListener<DeliveryTaskRequest, APIResponse>
    {
        protected override APIResponse HandleProcessEntity(DeliveryTaskRequest request)
        {
            APIResponse response = new APIResponse();
            try
            {
                //业务判断

                //保存数据库

                response.SetResponse(true, "", "");
            }
            catch (Exception ex)
            {
                response.SetResponse(false, "", ex.ToString());
            }
            return response;
        }
    }
}
