using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.IO;
using System.Text;

namespace GamePro.App_Start
{
    public class HttpService
    {
        public static string Get(string uri)
        {
            string strLine = "", data = "";
            using (WebClient wc = new WebClient())
            {
                try
                {
                    using (Stream stream = wc.OpenRead(uri))
                    {
                        using (StreamReader sr = new StreamReader(stream))
                        {
                            while ((strLine = sr.ReadLine()) != null)
                            {
                                data += strLine;
                            }
                            sr.Close();
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    return ex.Message;
                }
                wc.Dispose();

            }
            return data;
        }
        public static string Post(string uri, string postData)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            HttpWebRequest webrequest =(HttpWebRequest) WebRequest.Create(new Uri(uri));
            webrequest.Method = "post";
            webrequest.ContentType = "application/x-www-form-urlencoded";
            webrequest.ContentLength = byteArray.Length;
            System.IO.Stream newstream = webrequest.GetRequestStream();
            newstream.Write(byteArray,0,byteArray.Length);
            newstream.Close();
            HttpWebResponse respone = (HttpWebResponse)webrequest.GetResponse();
            string data = new System.IO.StreamReader(respone.GetResponseStream(),Encoding.GetEncoding("utf-8")).ReadToEnd();
            return data;
        }
    }
}