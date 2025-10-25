using System.Linq;
using System.Web.Mvc;

namespace frontend.App_Start
{
    public class CustomViewEngine : RazorViewEngine
    {
        public CustomViewEngine()
        {
            // {0} = View Name ("Amoxicillin")
            // {1} = Controller Name ("TrangChu")

            var newViewLocations = new[]
            {
            // Add your custom path first
            "~/Views/{1}/Thuoc/Thuoc-Khong-Ke-Don/{0}.cshtml",
            "~/Views/{1}/Thuoc/Thuc-Pham-Bo-Sung/{0}.cshtml",
            "~/Views/{1}/Thuoc/Vitamin-va-KhoangChat/{0}.cshtml",
            "~/Views/{1}/San-pham-lam-dep/{0}.cshtml"
        };
            //combine new locations with the default ones
            ViewLocationFormats = newViewLocations.Concat(base.ViewLocationFormats).ToArray();
            PartialViewLocationFormats = newViewLocations.Concat(base.PartialViewLocationFormats).ToArray();
            MasterLocationFormats = newViewLocations.Concat(base.MasterLocationFormats).ToArray();
        }
    }
}