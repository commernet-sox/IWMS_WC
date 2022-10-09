using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FWCore
{
    [DataContract]
    [Serializable]
    public class DE_OperationResult
    {
        #region 数据字段
        private String _ErrMsg = string.Empty;

        [DataMember]
        public String ErrMsg
        {
            get { return _ErrMsg; }
            set { _ErrMsg = value; }
        }

        private Boolean _IsSuccess = false;

        [DataMember]
        public Boolean IsSuccess
        {
            get { return _IsSuccess; }
            set { _IsSuccess = value; }
        }

        private String _SuccessMsg = string.Empty;

        [DataMember]
        public String SuccessMsg
        {
            get { return _SuccessMsg; }
            set { _SuccessMsg = value; }
        }

        private String _Ex1 = string.Empty;

        [DataMember]
        public String Ex1
        {
            get { return _Ex1; }
            set { _Ex1 = value; }
        }

        #endregion

    }
}
