using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FWCore
{
    /// <summary>
    ///     全局配置字典值内容
    /// </summary>
    public class DictionaryKeyValues
    {
        /// <summary>
        ///     key
        /// </summary>
        [Required(ErrorMessage = "key不能为空")]
        public string sKey { get; set; }

        /// <summary>
        ///     值
        /// </summary>
        public string sValue { get; set; }
    }
}
