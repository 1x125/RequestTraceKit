using System;
using System.ComponentModel;

namespace RequestTraceKit
{
    public enum DeviceType : short
    {
        [Description("PC")]
        Desktop = 1,

        [Description("Mobile")]
        Mobile = 2,

        [Description("Pad")]
        Tablet = 3,

        [Description("TV")]
        TV = 4,
    }

    public enum StorageDataBaseType
    {
        MongoDB = 1,
        MySql = 2,
        Custom=3
    }

    public class Geography
    {
        public float Latitude { get; set; }

        public float Longitude { get; set; }
    }
}
