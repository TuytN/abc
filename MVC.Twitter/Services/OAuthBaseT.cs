//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Cryptography;
//using System.Text;
//using System.Web;

//namespace MVC.Twitter
//{
//    public class OAuthBaseT
//    {
//        protected const string OAuthVersion = "1.0";

//        //Can provide other Signature Types 
//        protected const string Hmacsha1SignatureType = "HMAC-SHA1";
//        protected string OauthSignatureMethod = Hmacsha1SignatureType;

//        private string ConsumerKey { set; get; }
//        private string ConsumerKeySecret { set; get; }
//        private string AccessToken { set; get; }
//        private string AccessTokenSecret { set; get; }

//        protected const string UnreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";

//        /// <summary>
//        ///     Generate a nonce
//        /// </summary>
//        /// <returns></returns>
//        protected static string CreateOauthNonce()
//        {
//            // Just a simple implementation of a random number between 123400 and 9999999
//            return Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
//        }

//        /// <summary>
//        ///     Generates a signature using the HMAC-SHA1 algorithm
//        /// </summary>
//        /// <param name="url">The full url that needs to be signed including its non OAuth url parameters</param>
//        /// <param name="consumerKey">The consumer key</param>
//        /// <param name="consumerSecret">The consumer seceret</param>
//        /// <param name="token">The token, if available. If not available pass null or an empty string</param>
//        /// <param name="tokenSecret">The token secret, if available. If not available pass null or an empty string</param>
//        /// <param name="httpMethod">The http method used. Must be a valid HTTP method verb (POST,GET,PUT, etc)</param>
//        /// <param name="nonce"></param>
//        /// <param name="normalizedUrl"></param>
//        /// <param name="normalizedRequestParameters"></param>
//        /// <param name="timeStamp"></param>
//        /// <returns>A base64 string of the hash value</returns>
//        protected string GenerateSignature(Uri url, string consumerKey, string consumerSecret, string token, string tokenSecret, string httpMethod, string timeStamp, string nonce, out string normalizedUrl,
//            out string normalizedRequestParameters)
//        {
//            return GenerateSignature(url, consumerKey, consumerSecret, token, tokenSecret, httpMethod, timeStamp, nonce, SignatureTypes.Hmacsha1, out normalizedUrl, out normalizedRequestParameters);
//        }

//        /// <summary>
//        ///     Generates a signature using the specified signatureType
//        /// </summary>
//        /// <param name="url">The full url that needs to be signed including its non OAuth url parameters</param>
//        /// <param name="consumerKey">The consumer key</param>
//        /// <param name="consumerSecret">The consumer seceret</param>
//        /// <param name="token">The token, if available. If not available pass null or an empty string</param>
//        /// <param name="tokenSecret">The token secret, if available. If not available pass null or an empty string</param>
//        /// <param name="httpMethod">The http method used. Must be a valid HTTP method verb (POST,GET,PUT, etc)</param>
//        /// <param name="nonce"></param>
//        /// <param name="signatureType">The type of signature to use</param>
//        /// <param name="normalizedUrl"></param>
//        /// <param name="normalizedRequestParameters"></param>
//        /// <param name="timeStamp"></param>
//        /// <returns>A base64 string of the hash value</returns>
//        private string GenerateSignature(Uri url, string consumerKey, string consumerSecret, string token, string tokenSecret, string httpMethod, string timeStamp, string nonce, SignatureTypes signatureType, out string normalizedUrl,
//            out string normalizedRequestParameters)
//        {
//            normalizedUrl = null;
//            normalizedRequestParameters = null;

//            string signatureBase = GenerateSignatureBase(url, consumerKey, token, tokenSecret, httpMethod, timeStamp, nonce, Hmacsha1SignatureType, out normalizedUrl, out normalizedRequestParameters);

//            using (HMACSHA1 hmacsha1 = new HMACSHA1())
//            {
//                hmacsha1.Key = Encoding.ASCII.GetBytes(string.Format("{0}&{1}", UrlEncode(consumerSecret), string.IsNullOrEmpty(tokenSecret) ? "" : UrlEncode(tokenSecret)));
//                return GenerateSignatureUsingHash(signatureBase, hmacsha1);
//            }
//        }
//    }
//}