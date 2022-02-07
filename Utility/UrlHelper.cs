namespace OktaManualLoginFlow.Utility
{
    public static class UrlHelper
    {
        public static string EnsureTrailingSlash(this string url)
        {
            if(url == null)
            {
                return "/";
            }
            if (!url.EndsWith("/"))
            {
                return $"{url}/";
            }
            return url;
        }
    }
}
