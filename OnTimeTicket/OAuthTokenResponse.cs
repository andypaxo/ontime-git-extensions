namespace OnTimeTicket
{
    public class OAuthTokenResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public OAuthTokenData data { get; set; }
    }

    public class OAuthTokenData
    {
        public string id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
    }
}