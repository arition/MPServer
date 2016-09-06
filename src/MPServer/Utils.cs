using System;
using System.Linq;
using System.Reflection;

namespace MPServer
{
    public static class Utils
    {
        public static int ProcessInvalidPages(int? expectedPage, int maxPage)
        {
            if (expectedPage == null)
            {
                expectedPage = 1;
            }
            if (expectedPage < 1)
            {
                expectedPage = 1;
            }
            if (expectedPage > maxPage)
            {
                expectedPage = maxPage;
            }
            return expectedPage.Value;
        }

        public static TDst MapObject<TSrc, TDst>(TSrc srcObject)
        {
            var dstObject = (TDst)Activator.CreateInstance(typeof(TDst));
            return MapObject(srcObject, dstObject);
        }

        public static TDst MapObject<TSrc, TDst>(TSrc srcObject, TDst dstObject)
        {
            foreach (var dstProperty in typeof(TDst).GetProperties())
            {
                var srcProperty = typeof(TSrc).GetProperty(dstProperty.Name);
                if (srcProperty == null) continue;
                var srcValue = srcProperty.GetValue(srcObject);
                try
                {
                    dstProperty.SetValue(dstObject, srcValue);
                }
                catch
                {
                    // ignored
                }
            }
            return dstObject;
        }

        public static IQueryable<T> Page<T>(this IQueryable<T> o, int page)
        {
            return o.Skip((page - 1)*Variables.ItemPerPage).Take(Variables.ItemPerPage);
        }
    }

    public static class Variables
    {
        public const int ItemPerPage = 30;
    }
}
