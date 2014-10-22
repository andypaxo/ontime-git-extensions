using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;

namespace OnTimeTicket
{
    public class OnTimeConnector
    {
        public event EventHandler OnFailedAuthentication;
        public event EventHandler<MessageEventArgs> OnCommunicationError;
        public event EventHandler<OnTimeFeaturesEventArgs> OnFeaturesUpdated; 

        private readonly string company;
        private readonly string accessToken;
        private readonly string userId;

        public OnTimeConnector(string company, string accessToken, string userId)
        {
            this.company = company;
            this.accessToken = accessToken;
            this.userId = userId;
        }

        public void GetTickets()
        {
            try
            {
                var webRequest = WebRequest.Create(string.Format(
                    "https://{0}.ontimenow.com/api/v2/features?page_size=1000&assigned_to_id={1}&columns=id,name,workflow_step",
                    company,
                    userId));
                webRequest.Headers["Authorization"] = string.Format("Bearer {0}", accessToken);
                var webResponse = webRequest.GetResponse();

                var responseStream = webResponse.GetResponseStream();
                if (responseStream == null)
                    throw new Exception("No response from OnTime");

                var features =
                    (FeaturesResponse)
                        new DataContractJsonSerializer(typeof (FeaturesResponse)).ReadObject(responseStream);
                var featuresInProgress = features.data.Where(x => x.workflow_step.name == "In Progress");

                if (OnFeaturesUpdated != null)
                    OnFeaturesUpdated(this, new OnTimeFeaturesEventArgs {Features = featuresInProgress});
            }
            catch (WebException wex)
            {
                if (((HttpWebResponse) wex.Response).StatusCode == HttpStatusCode.Unauthorized)
                {
                    if (OnFailedAuthentication != null)
                        OnFailedAuthentication(this, EventArgs.Empty);
                }
                else
                    Handle(wex);
            }
            catch (Exception ex)
            {
                Handle(ex);
            }
        }

        private void Handle(Exception ex)
        {
            if (OnCommunicationError != null)
                OnCommunicationError(this, new MessageEventArgs {Message = ex.ToString()});
        }
    }

    public class OnTimeFeaturesEventArgs : EventArgs
    {
        public IEnumerable<Feature> Features { get; set; }
    }

    public class MessageEventArgs : EventArgs
    {
        public string Message { get; set; }
    }
}