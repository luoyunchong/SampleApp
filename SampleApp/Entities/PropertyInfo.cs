using Humanizer;

namespace SampleApp.Entities
{
    /// <summary>
    /// 属性
    /// </summary>
    public class PropertyInfo
    {
        /// <summary>
        /// 是否必填
        /// </summary>
        public bool Required { get; set; }
        /// <summary>
        /// 属性类型
        /// </summary>
        public string Type { get; set; }
        public string NamePascalize => Name.Pascalize();

        /// <summary>
        /// 属性名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }


        /// <summary>
        /// 属性名转下划线
        /// </summary>
        public string NameUnderscore => Name.Underscore();

    }
}
