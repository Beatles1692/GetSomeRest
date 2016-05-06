using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace NH.WebApi.Client
{
    public class WebApiClient
    {
        private readonly string _address;

        public WebApiClient(string address)
        {
            if (!address.EndsWith("/"))
                address += "/";
            _address = address;
        }

        private string GetUrl(string command)
        {
            return $"{_address}{command}";

        }

        private TResult UseWebClient<TResult>(Func<System.Net.WebClient, TResult> func)
        {
            using (var client = new System.Net.WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                client.Encoding = Encoding.UTF8;
                return func(client);
            }
        }
        public TResult Post<T, TResult>(string command, T args)
        {
            return UseWebClient(client =>
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var textWriter = new JsonTextWriter(new StreamWriter(memoryStream, Encoding.UTF8)))
                    {
                        var jsonSerializer = new JsonSerializer();
                        jsonSerializer.Serialize(textWriter, args);
                        textWriter.Flush();
                        var data = Encoding.UTF8.GetString(memoryStream.ToArray(), 0, (int)memoryStream.Length);
                        var result = client.UploadString(GetUrl(command),"POST", data);


                        return jsonSerializer.Deserialize<TResult>(new JsonTextReader(new StringReader(result)));
                    }

                }
            });
        }

        public TResult Get<TResult>(string command, IEnumerable<KeyValuePair<string, string>> args)
        {
            return UseWebClient(client =>
            {
                var queryString = String.Join("&", args.Select(x => $"{x.Key}={x.Value}").ToArray());
                var result = client.DownloadString($"{GetUrl(command)}?{queryString}");
                return new JsonSerializer().Deserialize<TResult>(new JsonTextReader(new StringReader(result)));
            });
        }
    }
}