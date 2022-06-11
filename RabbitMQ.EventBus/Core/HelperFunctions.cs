using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQ.EventBus.Core
{
    public static class HelperFunctions
    {
        public static int GetRetryCount(IBasicProperties messageProperties, string countHeader)
        {
            IDictionary<string, object> headers = messageProperties.Headers;
            int count = 0;
            if (headers != null)
            {
                if (headers.ContainsKey(countHeader))
                {
                    string countAsString = Convert.ToString(headers[countHeader]);
                    count = Convert.ToInt32(countAsString);
                }
            }

            return count;
        }

        public static IDictionary<string, object> CopyHeaders(IBasicProperties originalProperties)
        {
            IDictionary<string, object> dict = new Dictionary<string, object>();
            IDictionary<string, object> headers = originalProperties.Headers;
            if (headers != null)
            {
                foreach (KeyValuePair<string, object> kvp in headers)
                {
                    dict[kvp.Key] = kvp.Value;
                }
            }

            return dict;
        }
    }
}
