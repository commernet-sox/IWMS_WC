using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iWMS.WebAPICore.Model
{
    [Serializable]
    public class IRequestEntity
    {
        string WarehouseId { get; set; }

        string StorerId { get; set; }

        string OrderId { get; set; }
    }
}
