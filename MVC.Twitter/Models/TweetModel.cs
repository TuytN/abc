using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC.Twitter.Models
{
    /// <summary>
    ///     Model for a Tweet
    /// </summary>
    public class TweetModel
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public string AuthorName { get; set; }
        public string AuthorUrl { get; set; }
        public string ProfileImage { get; set; }
        public DateTime? Published { get; set; }
    }
}