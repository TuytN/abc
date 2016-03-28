using System.Web;
using System.Web.Mvc;

namespace MVC.Twitter
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new CustomHandleErrorAttribute());
            filters.Add(new HandleErrorAttribute());
        }
    }
}
