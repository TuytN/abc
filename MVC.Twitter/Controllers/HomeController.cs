using log4net;
using MVC.Twitter.Models;
using MVC.Twitter.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace MVC.Twitter.Controllers
{
    /// <summary>
    ///     Controller for Home
    /// </summary>
    public class HomeController : Controller
    {
        private log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        //private static TweetsModel Tweets = new TweetsModel(TwitterService.GetAllTweets());
        /// <summary>
        ///     Action Index
        /// </summary>
        /// <returns> View with list tweets </returns>
        public ActionResult Index()
        {
            return View(TweetsModel.ToViewModel());
        }

        /// <summary>
        ///     get list tweets again and show by Index view
        /// </summary>
        /// <returns> Index view with new list tweets </returns>
        public ActionResult Refresh()
        {
            TweetsModel.Tweets = TwitterService.GetAllTweets();
            return RedirectToAction("Index");
        }

        /// <summary>
        ///     Post a tweet to Twitter
        /// </summary>
        /// <param name="tweetText"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        public ActionResult PostTweet(string tweetText)
        {
            if (string.IsNullOrEmpty(tweetText))
            {
                logger.Error("Null or empty string for post stt");
                TempData["Error"] = "Null or empty string for post stt";
                return RedirectToAction("Index");
            }
            else
            {
                TweetOutModel tweetOut = new TweetOutModel()
                {
                    Content = tweetText
                };

                try
                {
                    TweetInModel tweetIn = TwitterService.PostTweet(tweetOut);
                    TweetsModel.Tweets.Insert(0, tweetIn);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Twitter"))
                    {
                        logger.Error(ex.Message);
                        TempData["Error"] = ex.Message;
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
        }
    }
}