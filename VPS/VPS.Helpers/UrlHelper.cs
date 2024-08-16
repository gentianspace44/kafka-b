namespace VPS.Helpers
{
    public static class UrlHelper
    {
        public static string CombineUrls(string baseUrl, string endpoint)
        {
            // Ensure the base URL ends with a trailing slash
            if (!baseUrl.EndsWith('/'))
            {
                baseUrl += "/";
            }

            // Remove any leading slash from the endpoint
            endpoint = endpoint.TrimStart('/');

            // Combine the base URL and endpoint with a single slash
            return new Uri(new Uri(baseUrl), endpoint).AbsoluteUri;
        }


    }
}
