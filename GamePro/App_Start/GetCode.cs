using System;
using System.Collections.Generic;
 
using System.Text;
using System.Net;
using System.IO;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Web;

namespace ecd_csharp_demo
{
    public class CloudInfDemo
    {
    	
    	
    	/// ========================================程序配置参数区开始
    	
        //接口生产地址(应用上线后正式环境必须使用该地址)
        //private  static String url = "http://www.etuocloud.com/gateway.action";
        
        //接口测试地址（未上线前测试环境使用）
        private static String url = "http://www.etuocloud.com/gatetest.action";
        
        //应用 app_key
         private static String APP_KEY = "TRa58x1YeoiDAfC6uyEqoUq77tlbCGRX";
        //应用 app_secret
        private static String APP_SECRET = "3hBJaDsy9EWxVg2PKsweIsCfV5ajMw54TXAhZ0TTl5KzTpM56yfkRWbJ2xDgMOJ5";
        
        //接口响应格式 json或xml
        private static String FORMAT = "json";
        
        /// ========================================程序配置参数区结束


        /// <summary>
        /// 发生短信验证码
        /// </summary>
        /// <param name="to">手机号</param>
        /// <param name="template">短信模板ID</param>
        /// <param name="smscode">验证码</param>
        /// <returns></returns>
        public static string sendSmsCode(string to, int template, string smscode)
        {

            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("app_key", APP_KEY);
            parameters.Add("view", FORMAT);
            parameters.Add("method", "cn.etuo.cloud.api.sms.simple");
            parameters.Add("out_trade_no", "");//商户订单号，可空
            parameters.Add("to", to);
            parameters.Add("template", template.ToString());
            parameters.Add("smscode", smscode);
            parameters.Add("sign", getsign(parameters));
            return HttpClient.HttpPost(url, parameters);

        }


        /// <summary>
        /// 发生模板短信
        /// </summary>
        /// <param name="to">手机号</param>
        /// <param name="template">短信模板ID</param>
        /// <param name="param">模板中的参数，以英文逗号分隔</param>
        /// <returns></returns>
        public static string sendSmsTpl(string to, int template, string param)
        {
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("app_key", APP_KEY);
            parameters.Add("view", FORMAT);
            parameters.Add("method", "cn.etuo.cloud.api.sms.template");
            parameters.Add("out_trade_no", "");//商户订单号，可空
            parameters.Add("to", to);
            parameters.Add("template", template.ToString());
            parameters.Add("params", param);
            parameters.Add("sign", getsign(parameters));
            return HttpClient.HttpPost(url, parameters);
        }



        /// <summary>
        /// 发送自定义短信
        /// </summary>
        /// <param name="to">手机号</param>
        /// <param name="content">短信内容</param>
        /// <param name="out_trade_no">商户订单号，可空</param>
        /// <returns></returns>
        public static string sendText(string to, string content, string out_trade_no)
        {
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("app_key", APP_KEY);
            parameters.Add("view", FORMAT);
            parameters.Add("method", "cn.etuo.cloud.api.sms.advance");
            parameters.Add("out_trade_no", out_trade_no);
            parameters.Add("to", to);
            parameters.Add("content", content);
            parameters.Add("sign", getsign(parameters));
            return HttpClient.HttpPost(url, parameters);
        }



        /// <summary>
        /// 发送语音验证码
        /// </summary>
        /// <param name="to">手机号</param>
        /// <param name="template">语音模板ID</param>
        /// <param name="verifyCode">验证码</param>
        /// <returns></returns>
        public static string sendVoiceCode(string to, int template, string verifyCode)
        {
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("app_key", APP_KEY);
            parameters.Add("method", "cn.etuo.cloud.api.voice.simple");
            parameters.Add("out_trade_no", "");//商户订单号，可空
            parameters.Add("to", to);
            parameters.Add("template", template.ToString());
            parameters.Add("verifyCode", verifyCode);
            parameters.Add("sign", getsign(parameters));
            return HttpClient.HttpPost(url, parameters);
        }


        /// <summary>
        /// 发送语音通知
        /// </summary>
        /// <param name="to">接收语音通知的手机号</param>
        /// <param name="template">语音通知模板ID</param>
        /// <returns></returns>
        public static string sendVoiceNotice(string to, int template)
        {
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("app_key", APP_KEY);
            parameters.Add("method", "cn.etuo.cloud.api.voice.message");
            parameters.Add("out_trade_no", "");//商户订单号，可空
            parameters.Add("to", to);
            parameters.Add("template", template.ToString());
            parameters.Add("sign", getsign(parameters));
            return HttpClient.HttpPost(url, parameters);
        }



        /// <summary>
        /// 获取流量产品列表
        /// </summary>
        /// <returns></returns>
        public static string getFlowProductList()
        {
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("app_key", APP_KEY);
            parameters.Add("method", "cn.etuo.cloud.api.flow.list");
            parameters.Add("sign", getsign(parameters));
            return HttpClient.HttpPost(url, parameters);
        }

        /// <summary>
        /// 充值流量
        /// </summary>
        /// <param name="to">手机号</param>
        /// <param name="flowcode">流量产品编码</param>
        /// <param name="out_trade_no">商户订单号，可空</param>
        /// <returns></returns>
        private static string rechargeFlow(string to, string flowcode, string out_trade_no)
        {
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("app_key", APP_KEY);
            parameters.Add("method", "cn.etuo.cloud.api.flow.charge");
            parameters.Add("out_trade_no", out_trade_no);
            parameters.Add("mobile", to);
            parameters.Add("flowcode", flowcode);
            parameters.Add("sign", getsign(parameters));
            return HttpClient.HttpPost(url, parameters);
        }


        /// <summary>
        /// 获取param签名
        /// </summary>
        /// <param name="sParams"></param>
        /// <returns></returns>
        private static string getsign(NameValueCollection parameters)
        {
            SortedDictionary<string, string> sParams = new SortedDictionary<string, string>();
            foreach (string key in parameters.Keys)
            {
                sParams.Add(key, parameters[key]);
            }

            string sign = string.Empty;
            StringBuilder codedString = new StringBuilder();
            foreach (KeyValuePair<string, string> temp in sParams)
            {
                if (temp.Value == "" || temp.Value == null || temp.Key.ToLower() == "sign")
                {
                    continue;
                }

                if (codedString.Length > 0)
                {
                    codedString.Append("&");
                }
                codedString.Append(temp.Key.Trim());
                codedString.Append("=");
                codedString.Append(temp.Value.Trim());
            }

            // 应用key
            codedString.Append(APP_SECRET);
            string signkey = codedString.ToString();
            sign = GetMD5(signkey, "utf-8");

            return sign;
        }




        //md5
        private static string GetMD5(string encypStr, string charset)
        {
            string retStr;
            MD5CryptoServiceProvider m5 = new MD5CryptoServiceProvider();

            //创建md5对象
            byte[] inputBye;
            byte[] outputBye;

            //使用XXX编码方式把字符串转化为字节数组．
            try
            {
                inputBye = Encoding.GetEncoding(charset).GetBytes(encypStr);
            }
            catch (Exception)
            {
                inputBye = System.Text.Encoding.UTF8.GetBytes(encypStr);
            }
            outputBye = m5.ComputeHash(inputBye);

            retStr = System.BitConverter.ToString(outputBye);
            retStr = retStr.Replace("-", "").ToUpper();

            //  return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(ConvertString, "MD5").ToLower(); ;

            return retStr;
        }

    }




    public class HttpClient
    {
        /// <summary>
        /// POST请求与获取结果  
        /// </summary>
        /// <param name="Url"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string HttpPost(string Url, NameValueCollection parameters)
        {
            return HttpPost(Url, toParaData(parameters));
        }



        //调用http接口,接口编码为utf-8
        private static string toParaData(NameValueCollection parameters)
        {

            //设置参数，并进行URL编码
            StringBuilder codedString = new StringBuilder();
            foreach (string key in parameters.Keys)
            {
                // codedString.Append(HttpUtility.UrlEncode(key));
                codedString.Append(key);
                codedString.Append("=");
                codedString.Append(HttpUtility.UrlEncode(parameters[key], System.Text.Encoding.UTF8));
                codedString.Append("&");
            }
            string paraUrlCoded = codedString.Length == 0 ? string.Empty : codedString.ToString().Substring(0, codedString.Length - 1);


            return paraUrlCoded;
        }


        /// <summary>  
        /// POST请求与获取结果  
        /// </summary>  
        public static string HttpPost(string Url, string postDataStr)
        {

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded;charset=utf-8";

            //request.ContentLength = postDataStr.Length;
            //StreamWriter writer = new StreamWriter(request.GetRequestStream(), System.Text.Encoding.UTF8);
            // writer.Write(postDataStr);
            // writer.Flush();


            //将URL编码后的字符串转化为字节
            byte[] payload = System.Text.Encoding.UTF8.GetBytes(postDataStr);
            request.ContentLength = payload.Length;
            Stream writer = request.GetRequestStream();
            writer.Write(payload, 0, payload.Length);
            writer.Close();

            //获得响应流
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string encoding = response.ContentEncoding;
            if (encoding == null || encoding.Length < 1)
            {
                encoding = "UTF-8"; //默认编码  
            }
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(encoding));

            string retString = reader.ReadToEnd();
            return retString;
        }



    }

}
