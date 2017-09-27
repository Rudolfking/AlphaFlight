using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaConfigurator.Utils
{
    public static class ExtensionMethods
    {
        public static string ZeroesUp(this int item, int count)
        {
            string ret = item.ToString();
            if (ret.Length >= count)
                return ret;

            while(ret.Length < count)
            {
                ret = "0" + ret;
            }
            return ret;
        }
    }
}
