using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BIE.Core.DBRepository
{
    public class DBHelper
    {
        public static T Convert<T>(string text)
        {
            T result;
            try
            {
                if (text.Length > 0)
                {
                    result = JsonSerializer.Deserialize<T>(text);
                    return result;
                }
            }
            catch (JsonException je)
            {

            }

            return default(T);
        }
    }
}

