using log4net;
using MVC.Twitter.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Web.Mvc;

namespace MVC.Twitter.Controllers
{
    /// <summary>
    ///     Controller for Home
    /// </summary>
    public class HomeController : Controller
    {
        private log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Action Index
        /// </summary>
        /// <returns> View with list tweets </returns>
        public ActionResult Index()
        {
            return View(TweetsModel.Tweets);
        }

        public ActionResult Refresh()
        {
            TweetsModel.Tweets = TwitterService.GetAllTweets();
            return RedirectToAction("Index");
        }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="tweetText"></param>
        /// <returns></returns>
        public ActionResult PostTweet(string tweetText)
        {
            if (string.IsNullOrEmpty(tweetText)) 
            {
                //TODO: send request out for user
                logger.Error("Null or empty string for post stt");
                return RedirectToAction("Index");
            }
            else
            {
                try     
                {
                    TweetModel tweet = TwitterService.PostTweet(tweetText);
                    TweetsModel.Tweets.Insert(0, tweet);
                }
                catch (Exception ex)
                {

                    logger.Error(ex.Message);
                    throw;
                }
                
                return RedirectToAction("Index");
            }
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            base.OnException(filterContext);
        }
    }
}