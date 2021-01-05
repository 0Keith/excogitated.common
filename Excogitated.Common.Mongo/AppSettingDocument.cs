using System;

namespace Excogitated.Common.Mongo
{
    public class AppSettingDocument
    {
        public Guid Id { get; set; }
        public string Key { get; set; }
        public object Value { get; set; }
    }
}
