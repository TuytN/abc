using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVC.Twitter.ViewModels
{
    public class TweetViewModel
    {
        [Required]
        [Display(Name = "Id")]
        public string Id { get; set; }

        [Required]
        [Display(Name = "Content")]
        public string Content { get; set; }

        [Display(Name = "Author")]
        public string AuthorName { get; set; }

        [Display(Name = "Author Url")]
        public string AuthorUrl { get; set; }

        [Display(Name = "Profile Image")]
        public string ProfileImage { get; set; }

        [Display(Name = "Published")]
        public DateTime? Published { get; set; }
    }
}