using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace MVC.Twitter.Services
{
    public class TwitterMethods
    {
        private static TwitterHelper helper = new TwitterHelper(ConfigurationManager.AppSettings["OauthConsumerKey"],
                                                            ConfigurationManager.AppSettings["OauthConsumerKeySecret"],
                                                            ConfigurationManager.AppSettings["OauthAccessToken"],
                                                            ConfigurationManager.AppSettings["OauthAccessTokenSecret"]);

        /// <summary>
        ///     Get tweet by hashtag
        /// </summary>
        /// <param name="twitterHashTag"> must have a format "#..." </param>
        /// <param name="count"> num of tweets to get, max 200 </param>
        /// <returns> the string response from Twitter
        ///     success: return the list of tweets have the hash tag
        ///     fail: error code
        /// </returns>
        public static string GetTweets(string twitterHashTag, int count)
        {
            string resourceUrl = string.Format("https://api.twitter.com/1.1/statuses/user_timeline.json");
            var requestParameters = new SortedDictionary<string, string>();
            requestParameters.Add("q", twitterHashTag);
            requestParameters.Add("count", count.ToString());
            requestParameters.Add("result_type", "mixed");
            var request = helper.CreateRequest(resourceUrl, "GET", requestParameters);
            var response = helper.GetResponse(request);
            return response;
        }

        /// <summary>
        ///     Get user timeline
        /// </summary>
        /// <param name="count"> num of tweets to get, max 200 </param>
        /// <returns> the string response from Twitter:
        ///     success: return the list of tweets
        ///     fail: error code
        /// </returns>
        public static string GetTweets(int count)
        {
            string resourceUrl = string.Format("https://api.twitter.com/1.1/statuses/user_timeline.json");
            var requestParameters = new SortedDictionary<string, string>();
            requestParameters.Add("count", count.ToString());
            //var request = helper.CreateRequest(resourceUrl, "GET", requestParameters);
            var response = helper.GetHomeTimeLine("count=" + count.ToString());
            return response;
        }

        /// <summary>
        ///     Get user timeline
        /// </summary>
        /// <param name="count"> num of tweets to get, max 200 </param>
        /// <returns> the string response from Twitter:
        ///     success: return the list of tweets
        ///     fail: error code
        /// </returns>
        public static string GetTweets(int count, string max_id)
        {
            string resourceUrl = string.Format("https://api.twitter.com/1.1/statuses/user_timeline.json");
            var requestParameters = new SortedDictionary<string, string>();
            requestParameters.Add("count", count.ToString());
            requestParameters.Add("max_id", max_id);
            var request = helper.CreateRequest(resourceUrl, "GET", requestParameters);
            var response = helper.GetResponse(request);
            return response;
        }

        /// <summary>
        ///     post status to Twitter
        /// </summary>
        /// <param name="stt">suport just for string</param>
        /// <returns> the string response from Twitter 
        ///     success: return the json of tweet just post
        ///     fail: error code
        /// </returns>
        public static string PostTweet(string stt)
        {
            string resourceUrl = string.Format("https://api.twitter.com/1.1/statuses/update.json");
            var requestParameters = new SortedDictionary<string, string>();
            requestParameters.Add("status", stt);
            //requestParameters.Add("display_coordinates", "false");
            //var request = helper.CreateRequest(resourceUrl, "POST", requestParameters);
            var response = helper.UpdateStatus("status=" + stt);
            return response;
        }
    }
}