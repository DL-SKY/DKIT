using System;

namespace Modules.Utils.Scripts.Extensions
{
    public static class DateTimeExtension
    {
        public static long ToUnixMs(this DateTime dateTime)
        {
            return new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();
        }
    }
}
