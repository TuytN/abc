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
    public class TwitterHelper : OAuthBase
    {
        private const string OauthVersion = "1.0";
        private const string OauthSignatureMethod = "HMAC-SHA1";

        private string ConsumerKey { set; get; }
        private string ConsumerKeySecret { set; get; }
        private string AccessToken { set; get; }
        private string AccessTokenSecret { set; get; }

        private static string oauthNonce = GenerateNonce();
        private static string oauthTimestamp = GenerateTimeStamp();

        private const string HeaderFormat = "OAuth oauth_nonce=\"{0}\", oauth_signature_method=\"{1}\", " +
                                        "oauth_timestamp=\"{2}\", oauth_consumer_key=\"{3}\", " +
                                        "oauth_token=\"{4}\", oauth_signature=\"{5}\", " +
                                        "oauth_version=\"{6}\"";

        /// <summary>
        ///     Create class
        /// </summary>
        /// <param name="consumerKey"></param>
        /// <param name="consumerKeySecret"></param>
        /// <param name="accessToken"></param>
        /// <param name="accessTokenSecret"></param>
        public TwitterHelper(string consumerKey, string consumerKeySecret, string accessToken, string accessTokenSecret)
        {
            if (string.IsNullOrEmpty(consumerKey))
            {
                throw new ArgumentException("Create TwitterHelper, null or empty consumerKey");
            }
            if (string.IsNullOrEmpty(consumerKeySecret))
            {
                throw new ArgumentException("Create TwitterHelper, null or empty consumerKeySecret");
            }
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new ArgumentException("Create TwitterHelper, null or empty accessToken");
            }
            if (string.IsNullOrEmpty(accessTokenSecret))
            {
                throw new ArgumentException("Create TwitterHelper, null or empty accessTokenSecret");
            }

            this.ConsumerKey = consumerKey;
            this.ConsumerKeySecret = consumerKeySecret;
            this.AccessToken = accessToken;
            this.AccessTokenSecret = accessTokenSecret;
        }

        /// <summary>
        ///     Get response from Twitter
        /// </summary>
        /// <param name="rootUrl"> Url to send resquest </param>
        /// <param name="methodName"> GET/POST <param>
        /// <param name="requestParameters"></param>
        /// <returns> The response string </returns>
        public string GetResponse(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            HttpClient httpClient1 = new HttpClient();
            HttpResponseMessage response = httpClient1.SendAsync(request).Result;
            
            string result = response.Content.ReadAsStringAsync().Result;
            
            return result;
        }

        /// <summary>
        ///     Create the Twitter request, not support for any other API 
        /// </summary>
        /// <param name="rootUrl"></param>
        /// <param name="methodName"></param>
        /// <param name="requestParameters"></param>
        /// <returns></returns>
        public HttpRequestMessage CreateRequest(string rootUrl, HttpMethod httpMethod, IDictionary<string, string> requestParameters)
        {
            if (string.IsNullOrEmpty(rootUrl))
            {
                throw new ArgumentException("CreateRequest, null or empty rootUrl");
            }
            if (httpMethod != HttpMethod.Get && httpMethod != HttpMethod.Post)
            {
                throw new ArgumentException("CreateRequest, method is not suported");
            }
            //TODO: IIDictionary<string, string> sortedList = new SortedIDictionary<string, string>(requestParameters);
            ServicePointManager.Expect100Continue = false;

            string url = string.Empty;

            if (requestParameters.Count != 0)
            {
                url = rootUrl + "?" + requestParameters.ToWebString();
            }

            string oauthSignature = OauthSignature(url, httpMethod);
            
            string authHeader = AuthHeader(oauthSignature);

            ServicePointManager.Expect100Continue = false;

            HttpRequestMessage request = new HttpRequestMessage(httpMethod, url);
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Authorization", authHeader);

            return request;
        }

        /// <summary>
        ///     Create an Auth header with oauthSignature and others parameter as 
        ///     oauthNonce, oauthTimestamp, Hmacsha1SignatureType, ConsumerKey, AccessToken,
        ///     OAuthVersion
        /// </summary>
        /// <param name="oauthSignature"></param>
        /// <returns></returns>
        private string AuthHeader(string oauthSignature)
        {
            if (string.IsNullOrEmpty(oauthSignature))
            {
                throw new ArgumentException("AuthHeader, null or empty oauthSignature");
            }

            string authHeader = string.Format(HeaderFormat,
                Uri.EscapeDataString(oauthNonce),
                Uri.EscapeDataString(Hmacsha1SignatureType),
                Uri.EscapeDataString(oauthTimestamp),
                Uri.EscapeDataString(ConsumerKey),
                Uri.EscapeDataString(AccessToken),
                Uri.EscapeDataString(oauthSignature),
                Uri.EscapeDataString(OAuthVersion));
            return authHeader;
        }

        /// <summary>
        ///     create a Oauth Sign on url, with httpMethod
        /// </summary>
        /// <param name="url"></param>
        /// <param name="httpMethod"></param>
        /// <returns></returns>
        private string OauthSignature(string url, HttpMethod httpMethod)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("OauthSignature, null or empty url");
            }
            if (httpMethod != HttpMethod.Get && httpMethod != HttpMethod.Post)
            {
                throw new ArgumentException("OauthSignature, method is not suported");
            }

            string normalizeUrl;
            string normalizedString;
            string oauthSignature = GenerateSignatureHmacsha1Alg(new Uri(url), ConsumerKey, ConsumerKeySecret, AccessToken, AccessTokenSecret, httpMethod, oauthTimestamp, oauthNonce, out normalizeUrl, out normalizedString);
            return oauthSignature;
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
        public static string ToWebString(this IDictionary<string, string> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

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