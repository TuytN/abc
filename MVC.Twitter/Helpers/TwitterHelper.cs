using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Net.Http.Headers;

namespace MVC.Twitter
{
    /// <summary>
    ///     Provice methods to commnunicate with Twitter
    /// </summary>
    public class TwitterHelper
    {
        private const string OauthVersion = "1.0";
        private const string OauthSignatureMethod = "HMAC-SHA1";

        private string ConsumerKey { set; get; }
        private string ConsumerKeySecret { set; get; }
        private string AccessToken { set; get; }
        private string AccessTokenSecret { set; get; }

        /// <summary>
        ///     Create class
        /// </summary>
        /// <param name="consumerKey"></param>
        /// <param name="consumerKeySecret"></param>
        /// <param name="accessToken"></param>
        /// <param name="accessTokenSecret"></param>
        public TwitterHelper(string consumerKey, string consumerKeySecret, string accessToken, string accessTokenSecret)
        {
            this.ConsumerKey = consumerKey;
            this.ConsumerKeySecret = consumerKeySecret;
            this.AccessToken = accessToken;
            this.AccessTokenSecret = accessTokenSecret;
        }

        /// <summary>
        ///     Get response from Twitter
        /// </summary>
        /// <param name="resourceUrl"> Url to send resquest </param>
        /// <param name="methodName"> GET/POST <param>
        /// <param name="requestParameters"></param>
        /// <returns> The response string </returns>
        public string GetResponse(WebRequest request)
        {
            if (request == null)
            {
                throw new Exception("GetResponse, empty request");
            }

            string resultString = string.Empty;

            try
            {
                var response = request.GetResponse();
                using (var sd = new StreamReader(response.GetResponseStream()))
                {
                    resultString = sd.ReadToEnd();
                    response.Close();
                }
                
            }
            catch (Exception ex)
            {
                throw;
            }

            return resultString;
        }

        /// <summary>
        ///     Create the Twitter request, not support for any other API 
        /// </summary>
        /// <param name="resourceUrl"></param>
        /// <param name="methodName"></param>
        /// <param name="requestParameters"></param>
        /// <returns></returns>
        public WebRequest CreateRequest(string resourceUrl, HttpMethod methodName, SortedDictionary<string, string> requestParameters)
        {
            if (string.IsNullOrEmpty(resourceUrl))
            {
                throw new Exception("CreateRequest, null or empty resourceUrl");
            }
            //if (string.Compare(methodName, "GET", StringComparison.OrdinalIgnoreCase) != 0 
            //    && string.Compare(methodName, "POST", StringComparison.OrdinalIgnoreCase) != 0)
            //{
            //    throw new Exception("CreateRequest, method is not suported");
            //}TODO
            if (requestParameters == null)
            {
                //optional: parameter can be null, but need defined
                requestParameters = new SortedDictionary<string, string>();
            }

            //TODO: what does this do?
            ServicePointManager.Expect100Continue = false;

            WebRequest request = null;

            if (requestParameters.Count != 0)
            {
                request = (HttpWebRequest)WebRequest.Create(resourceUrl + "?" + requestParameters.ToWebString());
            }
            else
            {
                request = (HttpWebRequest)WebRequest.Create(resourceUrl);

            }
            
            request.Method = methodName.ToString();
            request.ContentType = "application/x-www-form-urlencoded";

            var authHeader = CreateHeader(resourceUrl, methodName, requestParameters);
            request.Headers.Add("Authorization", authHeader);

            return request;
        }

        //TODO: what does this do?
        /// <summary>
        ///     
        /// </summary>
        /// <returns></returns>
        private string CreateOauthNonce()
        {
            return Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
        }

        /// <summary>
        ///     Create header for request
        /// </summary>
        /// <param name="resourceUrl"> Url to send resquest </param>
        /// <param name="methodName"> GET/POST <param>
        /// <param name="requestParameters"></param>
        /// <returns> The OAuth header</returns>
        private string CreateHeader(string resourceUrl, HttpMethod methodName, SortedDictionary<string, string> requestParameters)
        {
            if (string.IsNullOrEmpty(resourceUrl))
            {
                throw new Exception("CreateRequest, null or empty resourceUrl");
            }
            //if (string.Compare(methodName, "GET", StringComparison.OrdinalIgnoreCase) != 0 && string.Compare(methodName, "POST", StringComparison.OrdinalIgnoreCase) != 0)
            //{
            //    throw new Exception("CreateRequest, method is not suported");
            //}
            if (requestParameters == null)
            {
                //optional: parameter can be null, but need defined
                requestParameters = new SortedDictionary<string, string>();
            }

            var oauthNonce = CreateOauthNonce();
            var oauthTimestamp = CreateOAuthTimestamp();
            var oauthSignature = CreateOauthSignature(resourceUrl, methodName, oauthNonce, oauthTimestamp, requestParameters);

            const string HeaderFormat = "OAuth oauth_nonce=\"{0}\", oauth_signature_method=\"{1}\", "
                + "oauth_timestamp=\"{2}\", oauth_consumer_key=\"{3}\", "
                + "oauth_token=\"{4}\", oauth_signature=\"{5}\", "
                + "oauth_version=\"{6}\"";

            var authHeader = string.Format(
                HeaderFormat,
                Uri.EscapeDataString(oauthNonce),
                Uri.EscapeDataString(OauthSignatureMethod),
                Uri.EscapeDataString(oauthTimestamp),
                Uri.EscapeDataString(ConsumerKey),
                Uri.EscapeDataString(AccessToken),
                Uri.EscapeDataString(oauthSignature),
                Uri.EscapeDataString(OauthVersion));

            return authHeader;
        }

        /// <summary>
        ///     Create Oauth signature, work fine in Oauth 1.0
        /// </summary>
        /// <param name="resourceUrl"></param>
        /// <param name="method"></param>
        /// <param name="oauthNonce"></param>
        /// <param name="oauthTimestamp"></param>
        /// <param name="requestParameters"></param>
        /// <returns></returns>
        private string CreateOauthSignature(string resourceUrl, HttpMethod methodName, string oauthNonce, string oauthTimestamp, SortedDictionary<string, string> requestParameters)
        {
            if (string.IsNullOrEmpty(resourceUrl))
            {
                throw new Exception("CreateRequest, null or empty resourceUrl");
            }
            //if (string.Compare(methodName, "GET", StringComparison.OrdinalIgnoreCase) != 0 && string.Compare(methodName, "POST", StringComparison.OrdinalIgnoreCase) != 0)
            //{
            //    throw new Exception("CreateRequest, method is not suported");
            //} TODO
            if (requestParameters == null)
            {
                //optional: parameter can be null, but need defined
                requestParameters = new SortedDictionary<string, string>();
            }

            //add the standard oauth parameters to the sorted list 
            requestParameters.Add("oauth_consumer_key", ConsumerKey);
            requestParameters.Add("oauth_nonce", oauthNonce);
            requestParameters.Add("oauth_signature_method", OauthSignatureMethod);
            requestParameters.Add("oauth_timestamp", oauthTimestamp);
            requestParameters.Add("oauth_token", AccessToken);
            requestParameters.Add("oauth_version", OauthVersion);

            var sigBaseString = requestParameters.ToWebString();
            var signatureBaseString = string.Concat(methodName.ToString(), "&", Uri.EscapeDataString(resourceUrl), "&", Uri.EscapeDataString(sigBaseString.ToString()));

            var compositeKey = string.Concat(Uri.EscapeDataString(ConsumerKeySecret), "&", Uri.EscapeDataString(AccessTokenSecret));
            string oauthSignature;
            using (var hasher = new HMACSHA1(Encoding.ASCII.GetBytes(compositeKey)))
            {
                oauthSignature = Convert.ToBase64String(hasher.ComputeHash(Encoding.ASCII.GetBytes(signatureBaseString)));
            }
            return oauthSignature;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static string CreateOAuthTimestamp()
        {
            var nowUtc = DateTime.UtcNow;
            var timeSpan = nowUtc - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var timestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();
            return timestamp;
        }
    }

    /// <summary>
    ///     Extension methods support for send request to Twitter
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        ///     parse list of parameter to web parameter url format
        /// </summary>
        /// <param name="source"> list of parameter </param>
        /// <returns> web parameter url format </returns>
        public static string ToWebString(this SortedDictionary<string, string> source)
        {
            var body = new StringBuilder();
            if (source.Count != 0)
            {
                foreach (var requestParameter in source)
                {
                    body.Append(requestParameter.Key);
                    body.Append("=");
                    body.Append(Uri.EscapeUriString(requestParameter.Value));
                    body.Append("&");
                }
                //remove the last '&' 
                body.Remove(body.Length - 1, 1);
            }
            return body.ToString();
        }
    }
}