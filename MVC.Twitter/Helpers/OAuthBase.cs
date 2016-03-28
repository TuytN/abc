using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace MVC.Twitter
{
    public class OAuthBase
    {
        /// <summary>
        ///     Provides a predefined set of algorithms that are supported officially by the protocol
        /// </summary>
        public enum SignatureTypes
        {
            Hmacsha1,
            Plaintext,
            Rsasha1
        }

        protected const string OAuthVersion = "1.0";
        protected const string OAuthParameterPrefix = "oauth_";

        // List of know and used oauth parameters' names
        protected const string OAuthConsumerKeyKey = "oauth_consumer_key";
        protected const string OAuthCallbackKey = "oauth_callback";
        protected const string OAuthVersionKey = "oauth_version";
        protected const string OAuthSignatureMethodKey = "oauth_signature_method";
        protected const string OAuthSignatureKey = "oauth_signature";
        protected const string OAuthTimestampKey = "oauth_timestamp";
        protected const string OAuthNonceKey = "oauth_nonce";
        protected const string OAuthTokenKey = "oauth_token";
        protected const string OAuthTokenSecretKey = "oauth_token_secret";

        protected const string Hmacsha1SignatureType = "HMAC-SHA1";
        protected const string PlainTextSignatureType = "PLAINTEXT";
        protected const string Rsasha1SignatureType = "RSA-SHA1";

        protected const string UnreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";
        //protected readonly Random Random = new Random();

        /// <summary>
        ///     Helper function to compute a hash value
        /// </summary>
        /// <param name="hashAlgorithm">
        ///     The hashing algoirhtm used. If that algorithm needs some initialization, like HMAC and its
        ///     derivatives, they should be initialized prior to passing it to this function
        /// </param>
        /// <param name="data">The data to hash</param>
        /// <returns>a Base64 string of the hash value</returns>
        private static string ComputeHash(HashAlgorithm hashAlgorithm, string data)
        {
            if (hashAlgorithm == null)
                throw new ArgumentNullException("hashAlgorithm");

            if (string.IsNullOrEmpty(data))
                throw new ArgumentNullException("data");

            byte[] dataBuffer = Encoding.ASCII.GetBytes(data);
            byte[] hashBytes = hashAlgorithm.ComputeHash(dataBuffer);

            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        ///     Generate a nonce
        /// </summary>
        /// <returns></returns>
        protected static string GenerateNonce()
        {
            // Just a simple implementation of a random number between 123400 and 9999999
            return Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
        }

        /// <summary>
        ///     Generates a signature using the HMAC-SHA1 algorithm
        /// </summary>
        /// <param name="url">The full url that needs to be signed including its non OAuth url parameters</param>
        /// <param name="consumerKey">The consumer key</param>
        /// <param name="consumerSecret">The consumer seceret</param>
        /// <param name="token">The token, if available. If not available pass null or an empty string</param>
        /// <param name="tokenSecret">The token secret, if available. If not available pass null or an empty string</param>
        /// <param name="httpMethod">The http method used. Must be a valid HTTP method verb (POST,GET,PUT, etc)</param>
        /// <param name="nonce"></param>
        /// <param name="normalizedUrl"></param>
        /// <param name="normalizedRequestParameters"></param>
        /// <param name="timeStamp"></param>
        /// <returns>A base64 string of the hash value</returns>
        protected string GenerateSignatureHmacsha1Alg(Uri url, IDictionary<string, string> requestParameters, string consumerKey, string consumerSecret, string token, string tokenSecret, HttpMethod httpMethod, string timeStamp, string nonce, out string normalizedUrl,
            out string normalizedRequestParameters)
        {
            return GenerateSignature(url, requestParameters, consumerKey, consumerSecret, token, tokenSecret, httpMethod, timeStamp, nonce, SignatureTypes.Hmacsha1, out normalizedUrl, out normalizedRequestParameters);
        }

        /// <summary>
        ///     Generates a signature using the specified signatureType
        /// </summary>
        /// <param name="url">The full url that needs to be signed including its non OAuth url parameters</param>
        /// <param name="consumerKey">The consumer key</param>
        /// <param name="consumerSecret">The consumer seceret</param>
        /// <param name="token">The token, if available. If not available pass null or an empty string</param>
        /// <param name="tokenSecret">The token secret, if available. If not available pass null or an empty string</param>
        /// <param name="httpMethod">The http method used. Must be a valid HTTP method verb (POST,GET,PUT, etc)</param>
        /// <param name="nonce"></param>
        /// <param name="signatureType">The type of signature to use</param>
        /// <param name="normalizedUrl"></param>
        /// <param name="normalizedRequestParameters"></param>
        /// <param name="timeStamp"></param>
        /// <returns>A base64 string of the hash value</returns>
        private string GenerateSignature(Uri url, IDictionary<string, string> requestParameters, string consumerKey, string consumerSecret, string token, string tokenSecret, HttpMethod httpMethod, string timeStamp, string nonce, SignatureTypes signatureType, out string normalizedUrl,
            out string normalizedRequestParameters)
        {
            normalizedUrl = null;
            normalizedRequestParameters = null;

            switch (signatureType)
            {
                case SignatureTypes.Plaintext:
                    return HttpUtility.UrlEncode(string.Format("{0}&{1}", consumerSecret, tokenSecret));
                case SignatureTypes.Hmacsha1:
                    string signatureBase = GenerateSignatureBase(url, requestParameters, consumerKey, token, tokenSecret, httpMethod, timeStamp, nonce, Hmacsha1SignatureType, out normalizedUrl, out normalizedRequestParameters);

                    using (HMACSHA1 hmacsha1 = new HMACSHA1())
                    {
                        hmacsha1.Key = Encoding.ASCII.GetBytes(string.Format("{0}&{1}", UrlEncode(consumerSecret), string.IsNullOrEmpty(tokenSecret) ? string.Empty : UrlEncode(tokenSecret)));
                        return GenerateSignatureUsingHash(signatureBase, hmacsha1);
                    }
                case SignatureTypes.Rsasha1:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentException("Unknown signature type", "signatureType");
            }
        }

        /// <summary>
        ///     Generate the signature base that is used to produce the signature
        /// </summary>
        /// <param name="url">The full url that needs to be signed including its non OAuth url parameters</param>
        /// <param name="consumerKey">The consumer key</param>
        /// <param name="token">The token, if available. If not available pass null or an empty string</param>
        /// <param name="tokenSecret">The token secret, if available. If not available pass null or an empty string</param>
        /// <param name="httpMethod">The http method used. Must be a valid HTTP method verb (POST,GET,PUT, etc)</param>
        /// <param name="nonce"></param>
        /// <param name="signatureType">
        ///     The signature type. To use the default values use
        ///     <see cref="OAuthBase.SignatureTypes">OAuthBase.SignatureTypes</see>.
        /// </param>
        /// <param name="normalizedUrl"></param>
        /// <param name="normalizedRequestParameters"></param>
        /// <param name="timeStamp"></param>
        /// <returns>The signature base</returns>
        private static string GenerateSignatureBase(Uri url, IDictionary<string, string> requestParameters, string consumerKey, string token, string tokenSecret, HttpMethod httpMethod, string timeStamp, string nonce, string signatureType, out string normalizedUrl,
            out string normalizedRequestParameters)
        {
            if (token == null)
                token = string.Empty;

            if (string.IsNullOrEmpty(consumerKey))
                throw new ArgumentNullException("consumerKey");

            if (httpMethod == null)
                throw new ArgumentNullException("httpMethod");

            if (string.IsNullOrEmpty(signatureType))
                throw new ArgumentNullException("signatureType");

            requestParameters.Add(OAuthVersionKey, OAuthVersion);
            requestParameters.Add(OAuthNonceKey, nonce);
            requestParameters.Add(OAuthTimestampKey, timeStamp);
            requestParameters.Add(OAuthSignatureMethodKey, signatureType);
            requestParameters.Add(OAuthConsumerKeyKey, consumerKey);

            if (!string.IsNullOrEmpty(token))
            {
                requestParameters.Add(OAuthTokenKey, token);
            }

            IDictionary<string, string> sortedRequestParameters = new SortedDictionary<string, string>(requestParameters);
            
            normalizedUrl = string.Format("{0}://{1}", url.Scheme, url.Host);
            if (!((url.Scheme == "http" && url.Port == 80) || (url.Scheme == "https" && url.Port == 443)))
                normalizedUrl += ":" + url.Port;
            normalizedUrl += url.AbsolutePath;
            normalizedRequestParameters = NormalizeRequestParameters(sortedRequestParameters);

            StringBuilder signatureBase = new StringBuilder();
            signatureBase.AppendFormat("{0}&", httpMethod.ToString().ToUpper());
            signatureBase.AppendFormat("{0}&", UrlEncode(normalizedUrl));
            signatureBase.AppendFormat("{0}", UrlEncode(normalizedRequestParameters));

            return signatureBase.ToString();
        }

        /// <summary>
        ///     Generate the signature value based on the given signature base and hash algorithm
        /// </summary>
        /// <param name="signatureBase">The signature based as produced by the GenerateSignatureBase method or by any other means</param>
        /// <param name="hash">
        ///     The hash algorithm used to perform the hashing. If the hashing algorithm requires initialization or
        ///     a key it should be set prior to calling this method
        /// </param>
        /// <returns>A base64 string of the hash value</returns>
        private static string GenerateSignatureUsingHash(string signatureBase, HashAlgorithm hash)
        {
            return ComputeHash(hash, signatureBase);
        }

        /// <summary>
        ///     Generate the timestamp for the signature
        /// </summary>
        /// <returns></returns>
        protected static string GenerateTimeStamp()
        {
            // Default implementation of UNIX time of the current UTC time
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

        /// <summary>
        ///     Normalizes the request parameters according to the spec
        /// </summary>
        /// <param name="parameters">The list of parameters already sorted</param>
        /// <returns>a string representing the normalized parameters</returns>
        protected static string NormalizeRequestParameters(IDictionary<string, string> parameters)
        {
            var body = new StringBuilder();
            foreach (var requestParameter in parameters)
            {
                body.Append(requestParameter.Key);
                body.Append("=");
                body.Append(Uri.EscapeUriString(requestParameter.Value));
                body.Append("&");
            } //remove trailing '&' 
            body.Remove(body.Length - 1, 1);
            return body.ToString();
        }

        /// <summary>
        ///     This is a different Url Encode implementation since the default .NET one outputs the percent encoding in lower
        ///     case.
        ///     While this is not a problem with the percent encoding spec, it is used in upper case throughout OAuth
        /// 
        ///     JS: I am not sure why, but for comman the encoding this was generating was %2C, but Oauth is expecting %252C
        /// </summary>
        /// <param name="value">The value to Url encode</param>
        /// <returns>Returns a Url encoded string</returns>
        protected static string UrlEncode(string value)
        {
            StringBuilder result = new StringBuilder();

            foreach (char symbol in value)
            {
                if (UnreservedChars.IndexOf(symbol) != -1)
                    result.Append(symbol);
                else
                {
                    if (symbol == ',')
                        result.Append("%252C");
                    else
                        result.Append('%' + String.Format("{0:X2}", (int)symbol));
                }
            }

            return result.ToString();
        }
    }
}