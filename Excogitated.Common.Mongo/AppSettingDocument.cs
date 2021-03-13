using System;

namespace Excogitated.Mongo
{
    /// <summary>
    /// Document for app settings stored in Mongo
    /// </summary>
    public class AppSettingDocument
    {
        /// <summary>
        /// 
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Unique key for Document
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public object Value { get; set; }
    }
}
