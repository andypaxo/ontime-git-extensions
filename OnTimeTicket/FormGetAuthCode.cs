using System;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Windows.Forms;

namespace OnTimeTicket
{
    public partial class FormGetAuthCode : Form
    {

        public string OnTimeApiKey { get; set; }
        public string OnTimeApiSecret { get; set; }
        public string OnTimeCompanyName { get; set; }
        public string OnTimeAuthCode { get; private set; }
        public string OnTimeAccessToken { get; private set; }
        public string OnTimeUserId { get; private set; }

        public FormGetAuthCode()
        {
            InitializeComponent();
        }

        private void FormGetAuthCode_Load(object sender, EventArgs e)
        {
            webBrowser.Url = new Uri(string.Format(
                "https://{0}.ontimenow.com/auth?response_type=code&client_id={1}&redirect_uri=http%3A%2F%2Fecompliance.com/robots.txt",
                OnTimeCompanyName,
                OnTimeApiKey));
        }

        private void webBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            if (e.Url.AbsoluteUri.StartsWith("http://ecompliance.com"))
            {
                try
                {
                    // TODO : We're assuming success here, also need to handle failure
                    OnTimeAuthCode = e.Url.AbsoluteUri.Split('=')[1];

                    var webRequest = WebRequest.Create(string.Format(
                        "https://{0}.ontimenow.com/api/oauth2/token?grant_type=authorization_code&client_id={1}&client_secret={2}&code={3}&redirect_uri=http%3A%2F%2Fecompliance.com/robots.txt",
                        OnTimeCompanyName,
                        OnTimeApiKey,
                        OnTimeApiSecret,
                        OnTimeAuthCode));
                    var webResponse = webRequest.GetResponse();
                    var tokenData = (OAuthTokenResponse)new DataContractJsonSerializer(typeof (OAuthTokenResponse)).ReadObject(webResponse.GetResponseStream());

                    OnTimeAccessToken = tokenData.access_token;
                    OnTimeUserId = tokenData.data.id;

                    Close();
                }
                catch (Exception exception)
                {
                    MessageBox.Show(this, exception.ToString());
                }
            }
        }
    }
}
