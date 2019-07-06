using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;


namespace NetLib.WebAPI
{
    public enum WebMethod
    {
        GET = 0,
        POST = 1
    };

    public enum Service
    {
        None,
        Login,
        Data,
        Register,
        ValidateToken,
        UserDeck
    };


    public struct RequestResult
    {
        public RequestResult(uint statusCode, string content)
        {
            this.statusCode = statusCode;
            this.content = content;
        }

        public uint StatusCode
        {
            get => statusCode;
            set => statusCode = value;
        }

        public string Content
        {
            get => content;
            set => content = value;
        }

        private uint statusCode;
        private string content;
    };

    public struct RequestData
    {
        public RequestData(string key, string value)
        {
            this.key = key;
            this.value = value;
        }

        public string Key
        {
            get => key;
            set => key = value;
        }

        public string Value
        {
            get => value;
            set => this.value = value;
        }

        private string key;
        private string value;
    };

    internal struct API
    {
        internal string url;
        internal uint port;
        internal string cert;
        internal uint projectId;
        internal Dictionary<Service, string> services;
    }

    public static class APIRequest
    {
        public static void Create(string url, uint port, uint projectId = 1,string cert = "./ca-bundle.crt")
        {
            api = new API();
            api.url = url;
            api.port = port;
            api.cert = cert;
            api.projectId = projectId;
            api.services = new Dictionary<Service, string>();
        }

        public static void AddService(Service service, string serviceDomain)
        {
            api.services.Add(service, serviceDomain);
        }

        public static string Certificate
        {
            set => api.cert = value;
        }

        public static uint ProjectId => api.projectId;

        public static RequestResult Request(Service service, WebMethod method, List<RequestData> arguments)
        {
            WebRequest request = WebRequest.Create(GetServiceUrl(service));
            request.Method = method.ToString();
            string argumentString = ConvertArgumentsToString(method, arguments);
            X509Certificate cert = X509Certificate.CreateFromCertFile(api.cert);
            HttpWebRequest req = (HttpWebRequest) request;
            req.ClientCertificates.Add(cert);

            byte[] byteArray = Encoding.UTF8.GetBytes(argumentString);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(byteArray, 0, byteArray.Length);
            requestStream.Close();
            try
            {
                using (WebResponse response = request.GetResponse())
                {
                    using (requestStream = response.GetResponseStream())
                    {
                        if (requestStream != null)
                        {
                            // Open the stream using a StreamReader for easy access.  
                            StreamReader reader = new StreamReader(requestStream);
                            // Read the content.  
                            string responseFromServer = reader.ReadToEnd();
                            // Display the content.  
                            return new RequestResult((uint) ((HttpWebResponse) response).StatusCode,
                                responseFromServer);
                        }

                        return new RequestResult(500, "API Error");
                    }
                }
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    if (response != null)
                    {
                        Console.WriteLine("FAIL WITH RESPONSE");
                    }
                }

                Console.WriteLine("FAIL");
            }

            return new RequestResult(500, "API Error");
        }

        private static string GetServiceUrl(Service service)
        {
            return api.url + api.services[service];
        }

        private static string ConvertArgumentsToString(WebMethod method, List<RequestData> data)
        {
            if (data.Count == 0) return "";

            string returnString = "";
            if (method == WebMethod.GET)
            {
                returnString += "?";
            }

            var listSize = data.Count;

            for (var i = 0; i < listSize; ++i)
            {
                returnString += data[i].Key + "=" + data[i].Value;
                if (i != listSize - 1)
                {
                    returnString += "&";
                }
            }

            return returnString;
        }

        public static bool CheckConnection()
        {
            WebRequest request = WebRequest.Create(api.url);
            request.Method = "GET";
            try
            {
                using (WebResponse response = request.GetResponse())
                {
                    Console.WriteLine("Won't get here");
                }
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    if (response != null)
                    {
                        return true;
                    }
                }

                return false;
            }

            return false;
        }

        private static API api;
    }
}