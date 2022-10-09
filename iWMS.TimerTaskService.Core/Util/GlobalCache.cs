using Common.DBCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FWCore;

namespace iWMS.TimerTaskService.Core.Util
{
    public class GlobalCache
    {
        private static Dictionary<string, int> _priorityDict = null;
        public static Dictionary<string, int> PriorityDict
        {
            get
            {
                if (_priorityDict == null || _priorityDict.Count < 1)
                {
                    Initialize();
                }
                return _priorityDict;
            }
        }
        private const string QueryPrioritySql = @"SELECT * FROM SYS_TaskPriority";

        static GlobalCache()
        {
            Initialize();
        }

        public static void Initialize()
        {
            _priorityDict = new Dictionary<string, int>();
            var priorityTable = BusinessDbUtil.GetDataTable(QueryPrioritySql);
            if (priorityTable != null && priorityTable.Rows.Count > 0)
            {
                foreach (DataRow row in priorityTable.Rows)
                {
                    var key = row["StorerId"].ToString() + row["WarehouseId"].ToString() + row["Category"].ToString() + row["TaskType"].ToString();
                    if (!_priorityDict.ContainsKey(key))
                    {
                        _priorityDict.Add(key, row["Priority"].ConvertInt32());
                    }
                }
            }
        }

        public static int GetPriorityValue(string key)
        {
            if (_priorityDict==null || _priorityDict.Count==0)
            {
                return 0;
            }
            if (_priorityDict.ContainsKey(key))
            {
                return _priorityDict[key];
            }
            return 0;
        }
    }
}
