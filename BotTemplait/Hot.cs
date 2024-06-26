using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace BotTemplait
{
    public class Hot
    {
        public static string StringInsert(string template, params object[] propertyValues)
        {
            string result = template.Replace("","");
            for (int i =0; i<propertyValues.Length; i++)
                result = result.Replace($"{i}", propertyValues[i].ToString());
            return result;
        }
    }
}
