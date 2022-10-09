﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FWCore
{
    /// <summary>
    ///     日志实体
    /// </summary>
    public class LogInfo
    {
        public DateTime Datetime { get; set; }
        public string Content { get; set; }
        public string IP { get; set; }
        public string LogColor { get; set; }
        public int Import { get; set; } = 0;
    }
}
