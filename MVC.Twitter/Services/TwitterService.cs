using MVC.Twitter.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;

namespace MVC.Twitter
{
    public class TwitterService
    {
        private static TwitterHelper helper = new TwitterHelper(ConfigurationManager.AppSettings["OauthConsumerKey"],
                                                                ConfigurationManager.AppSettings["OauthConsumerKeySecret"],
                                                                ConfigurationManager.AppSettings["OauthAccessToken"],
                                                                ConfigurationManager.AppSettings["OauthAccessTokenSecret"]);

        /// <summary>
        ///     Get user timeline
        /// </summary>
        /// <param name="count"> num of tweets to get, max 200, must larger than 0 </param>
        /// <returns> the string response from Twitter:
        ///     success: return the list of tweets
        ///     fail: error code
        /// </returns>
        public static List<TweetInModel> GetTweets(int count, string max_id)
        {
            List<TweetInModel> tweets = new List<TweetInModel>();

            string resourceUrl = "https://api.twitter.com/1.1/statuses/user_timeline.json";
            var requestParameters = new Dictionary<string, string>();
            if (count > 0)
            {
                requestParameters.Add("count", count.ToString());
            }
            else
            {
                throw new ArgumentException("GetTweets, count must larger than zero");
            }

            if (!string.IsNullOrEmpty(max_id))
            {
                requestParameters.Add("max_id", max_id);
            }

            var request = helper.CreateRequest(resourceUrl, HttpMethod.Get, requestParameters);
            var response = helper.GetResponse(request);

            if (response.Contains("error"))
            {
                dynamic error = JsonConvert.DeserializeObject(response);

                throw new Exception(error.massage);
            }
            else
            {
                dynamic timeline = JsonConvert.DeserializeObject(response);

                foreach (var tweet in timeline)
                {
                    TweetInModel model = ConvertToTweetModel(tweet);

                    tweets.Add(model);
                }
            }
            return tweets;
        }

        /// <summary>
        ///     Get list tweets
        /// </summary>
        /// <returns> list all of tweets (less than 3200 tweets) </returns>
        public static List<TweetInModel> GetAllTweets()
        {
            List<TweetInModel> tweets = new List<TweetInModel>();
            string min_id = string.Empty;
            int count;

            do
            {
                List<TweetInModel> tweetsAPart = GetTweets(200, min_id);
                
                count = tweetsAPart.Count;

                //remove the last, don't remove when nothing in list (first time)
                if (tweets.Count > 0)
                {
                    tweets.Remove(tweets.LastOrDefault());
                }

                tweets.AddRange(tweetsAPart);

                min_id = tweets.LastOrDefault().Id;
            }
            while (count >= 200);

            return tweets;
        }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="tweetText"></param>
        /// <returns></returns>
        public static TweetInModel PostTweet(TweetOutModel outTweet)
        {
            if (outTweet == null)
            {
                throw new ArgumentNullException("outTweet");
            }
            if (string.IsNullOrEmpty(outTweet.Content))
            {
                throw new ArgumentException("PostTweet, text to post can not null or empty");
            }

            string resourceUrl = "https://api.twitter.com/1.1/statuses/update.json";
            var requestParameters = new Dictionary<string, string>();
            requestParameters.Add("status", outTweet.Content);

            var request = helper.CreateRequest(resourceUrl, HttpMethod.Post, requestParameters);
            var response = helper.GetResponse(request);

            if (response.Contains("error"))
            {
                dynamic error = JsonConvert.DeserializeObject(response);
                ErrorModel errorModel = new ErrorModel();

                foreach (var item in error.errors)
                {
                    errorModel.Code = ((dynamic)item).code;
                    errorModel.Message = ((dynamic)item).message;
                }

                throw new HttpException ("Twitter error return: " + errorModel.Message);
            }
            
            dynamic tweetIn = JsonConvert.DeserializeObject(response);

            TweetInModel model = ConvertToTweetModel(tweetIn);

            return model;
        }

        /// <summary>
        ///     convert from dynamic to tweet model
        /// </summary>
        /// <param name="tweet"></param>
        /// <returns></returns>
        private static TweetInModel ConvertToTweetModel(dynamic tweet)
        {
            if (tweet == null)
            {
                throw new ArgumentNullException("tweet");
            }

            TweetInModel model = new TweetInModel();

            model.Id = ((dynamic)tweet).id.ToString();
            model.AuthorName = ((dynamic)tweet).user.name;
            model.AuthorUrl = ((dynamic)tweet).user.url;
            model.Content = ((dynamic)tweet).text;
            string publishedDate = ((dynamic)tweet).created_at;
            publishedDate = publishedDate.Substring(0, 19);
            model.Published = DateTime.ParseExact(publishedDate, "ddd MMM dd HH:mm:ss", null);
            model.ProfileImage = ((dynamic)tweet).user.profile_image_url;

            return model;
        }
    }
}