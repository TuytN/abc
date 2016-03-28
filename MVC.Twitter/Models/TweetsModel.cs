using MVC.Twitter.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC.Twitter.Models
{
    /// <summary>
    ///     Class for the list of tweets
    /// </summary>
    public class TweetsModel
    {
        public static List<TweetInModel> Tweets { get; set; }

        public TweetsModel(List<TweetInModel> tweets)
        {
            Tweets = tweets;
        }

        public static List<TweetViewModel> ToViewModel()
        {
            List<TweetViewModel> tweetsViewModel = new List<TweetViewModel>();

            foreach (TweetInModel item in Tweets)
            {
                TweetViewModel viewModel = new TweetViewModel()
                {
                    Id = item.Id,
                    AuthorName = item.AuthorName,
                    AuthorUrl = item.AuthorUrl,
                    Content = item.Content,
                    ProfileImage = item.ProfileImage,
                    Published = item.Published,
                };

                tweetsViewModel.Add(viewModel);
            }

            return tweetsViewModel;
        }
    }
}