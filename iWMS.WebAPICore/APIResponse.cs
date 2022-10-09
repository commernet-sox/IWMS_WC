using Newtonsoft.Json;

namespace iWMS.WebAPICore
{
    public class APIResponse
    {
        public string flag = string.Empty;
        public string code = string.Empty;
        public string message = string.Empty;

        public void SetResponse(bool success, string code, string message)
        {
            this.flag = success ? "success" : "failure";
            this.code = code;
            this.message = message;
        }

        public virtual string GetResult()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
