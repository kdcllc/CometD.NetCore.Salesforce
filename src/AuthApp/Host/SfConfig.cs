namespace AuthApp.Host
{
    public class SfConfig
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string LoginUrl { get; set; } = @"https://test.salesforce.com";
    }
}
