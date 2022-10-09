using iWMS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iWMS.WebApplication;

namespace iWMS.BlueSkyAPI.Models
{
    public class COMSKURequest : IEvent<Outcome>
    {
        /// <summary>
        /// 物料编码(唯一)
        /// </summary>
        public string SkuId { get; set; }
        /// <summary>
        /// 货主代码
        /// </summary>
        public string StorerId { get; set; }
        /// <summary>
        /// 物料描述
        /// </summary>
        public string SkuDec { get; set; }
        /// <summary>
        /// 物料简称
        /// </summary>
        public string ShortName { get; set; }
        /// <summary>
        /// 物料英文名
        /// </summary>
        public string EnglishName { get; set; }
        /// <summary>
        /// 物料类型
        /// </summary>
        public string SkuType { get; set; }
        /// <summary>
        /// 物料规格
        /// </summary>
        public string Spec { get; set; }
        /// <summary>
        /// 颜色
        /// </summary>
        public string Color { get; set; }
        /// <summary>
        /// 尺码
        /// </summary>
        public string Size { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// 包装单位
        /// </summary>
        public string PackageCode { get; set; }
        /// <summary>
        /// 包装数量
        /// </summary>
        public int PackageQty { get; set; }
        /// <summary>
        /// 物料单位
        /// </summary>
        public string GoodsUnit { get; set; }
        /// <summary>
        /// 物料所属类别ID
        /// </summary>
        public string CategoryId { get; set; }
        /// <summary>
        /// 物料所属类别名称
        /// </summary>
        public string CategoryName { get; set; }
        /// <summary>
        /// 品牌ID
        /// </summary>
        public string BrandId { get; set; }
        /// <summary>
        /// 品牌名称
        /// </summary>
        public string BrandName { get; set; }
        /// <summary>
        /// 安全库存
        /// </summary>
        public int SafetyStock { get; set; }
        /// <summary>
        /// 是否SN管理
        /// </summary>
        public int IsSnMgt { get; set; }
        /// <summary>
        /// 生产日期
        /// </summary>
        public string ProductDate { get; set; }
        /// <summary>
        /// 过期日期
        /// </summary>
        public string ExpireDate { get; set; }
        /// <summary>
        /// 是否需要保质期管理
        /// </summary>
        public int IsLifeMgt { get; set; }
        /// <summary>
        /// 保质期禁售天数
        /// </summary>
        public int LifeCycle { get; set; }
        /// <summary>
        /// 保质期禁收天数
        /// </summary>
        public int RejectCycle { get; set; }
        /// <summary>
        /// 保质期天数
        /// </summary>
        public int ShelfLife { get; set; }
        /// <summary>
        /// 毛重
        /// </summary>
        public decimal GrossWeight { get; set; }
        /// <summary>
        /// 净重
        /// </summary>
        public decimal NetWeight { get; set; }
        /// <summary>
        /// 长
        /// </summary>
        public decimal Length { get; set; }
        /// <summary>
        /// 宽
        /// </summary>
        public decimal Width { get; set; }
        /// <summary>
        /// 高
        /// </summary>
        public decimal Height { get; set; }
        /// <summary>
        /// 物料热度
        /// </summary>
        public string ABCType { get; set; }
        /// <summary>
        /// 物料图片
        /// </summary>
        public string ImgUrl { get; set; }
        /// <summary>
        /// 供应商码
        /// </summary>
        public string ProviderCode { get; set; }
        /// <summary>
        /// 箱规
        /// </summary>
        public int Pcs { get; set; }
        /// <summary>
        /// 外箱长
        /// </summary>
        public decimal CartonLength { get; set; }
        /// <summary>
        /// 外箱宽
        /// </summary>
        public decimal CartonWidth { get; set; }
        /// <summary>
        /// 外箱高
        /// </summary>
        public decimal CartonHeight { get; set; }
        /// <summary>
        /// 外箱体积
        /// </summary>
        public decimal CartonVolume { get; set; }
        /// <summary>
        /// 外箱重量
        /// </summary>
        public decimal CartonWeight { get; set; }
        /// <summary>
        /// 是否有效
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Memo { get; set; }
        /// <summary>
        /// 条码明细
        /// </summary>
        public List<COM_SKUUPCDetail> Upclist { get; set; }
    }
    public class COM_SKUUPCDetail
    {
        /// <summary>
        /// 条码
        /// </summary>
        public string UpcCode { get; set; }
    }
    }