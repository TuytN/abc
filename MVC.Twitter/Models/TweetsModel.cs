using MVC.Twitter.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC.Twitter.Models
{
    //TODO: delete?
    /// <summary>
    ///     Class for the list of tweets
    /// </summary>
    public class TweetsModel
    {
        public static List<TweetModel> Tweets { get; set; }
    }
}