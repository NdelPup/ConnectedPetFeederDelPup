using Microsoft.Azure.Devices.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace TwinExtension
{
    public static class TwinExtension
    {
        public static string DesiredProperty(this Twin that, string name)
        {
            if (that == null) return string.Empty;
            if (!that.Properties.Desired.Contains(name)) return string.Empty;
            return that.Properties.Desired[name];
        }

        public static bool IsDesiredPropertyEmpty(this Twin that, string name)
        {
            return string.IsNullOrWhiteSpace(DesiredProperty(that, name));
        }
    }
}
