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
        /// <param name="rootUrl"> Url to send resquest </param>
        /// <param name="methodName"> GET/POST <param>
        /// <param name="requestParameters"></param>
        /// <returns> The response string </returns>
        public string GetResponse(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new Exception("GetResponse, empty request");
            }

            HttpClient httpClient1 = new HttpClient();
            HttpResponseMessage response = httpClient1.SendAsync(request).Result;

            var result = response.Content.ReadAsStringAsync().Result;

            return result;
        }

        /// <summary>
        ///     Create the Twitter request, not support for any other API 
        /// </summary>
        /// <param name="rootUrl"></param>
        /// <param name="methodName"></param>
        /// <param name="requestParameters"></param>
        /// <returns></returns>
        public HttpRequestMessage CreateRequest(string rootUrl, HttpMethod httpMethod, SortedDictionary<string, string> requestParameters)
        {
            if (string.IsNullOrEmpty(rootUrl))
            {
                throw new Exception("CreateRequest, null or empty rootUrl");
            }
            //if (string.Compare(methodName, "GET", StringComparison.OrdinalIgnoreCase) != 0
            //    && string.Compare(methodName, "POST", StringComparison.OrdinalIgnoreCase) != 0)
            //{
            //    throw new Exception("CreateRequest, method is not suported");
            //}
            if (requestParameters == null)
            {
                //optional: parameter can be null, but need defined
                requestParameters = new SortedDictionary<string, string>();
            }

            //TODO: what does this do?
            ServicePointManager.Expect100Continue = false;

            string url = string.Empty;

            if (requestParameters.Count != 0)
            {
                url = rootUrl + "?" + requestParameters.ToWebString();
            }

            string oauthSignature = OauthSignature(url, httpMethod);
            // create the request header
            string authHeader = AuthHeader(oauthSignature);
            // make the request

            ServicePointManager.Expect100Continue = false;

            HttpRequestMessage request = new HttpRequestMessage(httpMethod, url);
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Authorization", authHeader);

            return request;
        }
        
        #region private methods

        private string AuthHeader(string oauthSignature)
        {
            const string headerFormat = "OAuth oauth_nonce=\"{0}\", oauth_signature_method=\"{1}\", " +
                                        "oauth_timestamp=\"{2}\", oauth_consumer_key=\"{3}\", " +
                                        "oauth_token=\"{4}\", oauth_signature=\"{5}\", " +
                                        "oauth_version=\"{6}\"";

            string authHeader = string.Format(headerFormat,
                Uri.EscapeDataString(oauthNonce),
                Uri.EscapeDataString(Hmacsha1SignatureType),
                Uri.EscapeDataString(oauthTimestamp),
                Uri.EscapeDataString(ConsumerKey),
                Uri.EscapeDataString(AccessToken),
                Uri.EscapeDataString(oauthSignature),
                Uri.EscapeDataString(OAuthVersion));
            return authHeader;
        }

        private string OauthSignature(string url, HttpMethod httpMethod)
        {
            string normalizeUrl;
            string normalizedString;
            string oauthSignature = GenerateSignature(new Uri(url), ConsumerKey, ConsumerKeySecret, AccessToken, AccessTokenSecret, httpMethod, oauthTimestamp, oauthNonce, out normalizeUrl, out normalizedString);
            return oauthSignature;
        }

        private static string CleanupQueryString(string querystring)
        {
            if (!string.IsNullOrEmpty(querystring))
            {
                if (querystring.IndexOf('&') == 0)
                    querystring = querystring.Remove(0, 1);
            }
            return querystring;
        }

        #endregion
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