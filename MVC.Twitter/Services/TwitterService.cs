using MVC.Twitter.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace MVC.Twitter.Services
{
    public class TwitterService
    {
        //TODO: private log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //private TwitterMethods twitterMethod = new TwitterMethods();

        /// <summary>
        ///     Get list tweets
        /// </summary>
        /// <returns> list of tweets </returns>
        public static List<TweetModel> GetAllTweets()
        {
            List<TweetModel> lstTweets = new List<TweetModel>();

            var response = TwitterMethods.GetTweets(200);

            dynamic timeline = JsonConvert.DeserializeObject(response);
            int count;

            do
            {
                count = timeline.Count;

                foreach (var tweet in timeline)
                {
                    TweetModel model = ConvertToTweetModel(tweet);
                    
                    lstTweets.Add(model);
                }

                response = TwitterMethods.GetTweets(200, lstTweets.LastOrDefault().Id);
                timeline = JsonConvert.DeserializeObject(response);

                if (count >= 200)
                {
                    lstTweets.Remove(lstTweets.LastOrDefault());
                }
                
            } while (count >= 200);

            return lstTweets;
        }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="tweetText"></param>
        /// <returns></returns>
        public static TweetModel PostTweet(string tweetText)
        {
            try
            {
                string response = TwitterMethods.PostTweet(tweetText);

                dynamic tweet = JsonConvert.DeserializeObject(response);

                TweetModel model = ConvertToTweetModel(tweet);

                return model;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tweet"></param>
        /// <returns></returns>
        private static TweetModel ConvertToTweetModel(dynamic tweet)
        {
            TweetModel model = new TweetModel();

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