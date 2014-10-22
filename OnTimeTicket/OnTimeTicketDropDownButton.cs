using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Windows.Forms;
using GitUI.CommandsDialogs;
using GitUI.SpellChecker;

namespace OnTimeTicket
{
    public class OnTimeTicketDropDownButton : ToolStripDropDownButton
    {
        public FormCommit CommitDialog { get; set; }

        public OnTimeTicketDropDownButton()
            : base("OnTime tickets")
        {
            DropDown = new ToolStripDropDown();
        }

        public void ShowTickets(string company, string accessToken, string userId)
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

                EmptyDropdown();

                foreach (var feature in featuresInProgress)
                    DropDown.Items.Add(feature.ToString(), null, AddMessage);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void EmptyDropdown()
        {
            var oldItems = new List<ToolStripItem>(DropDown.Items.Cast<ToolStripItem>());
            DropDown.Items.Clear();
            foreach (var item in oldItems)
                item.Dispose();
        }

        private void AddMessage(object sender, EventArgs e)
        {
            var message = ((ToolStripItem)sender).Text;
            Func<Control, bool> isMessageInput = x => x is EditNetSpell;
            var messageInput = WinFormUtils.FindControl(isMessageInput, CommitDialog);
            messageInput.Text += "\n\n" + message;
        }
    }
}