using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Windows.Forms;
using GitUI.CommandsDialogs;
using GitUI.SpellChecker;

namespace OnTimeTicket
{
    public partial class FormShowTickets : Form
    {
        public FormShowTickets()
        {
            InitializeComponent();
        }

        public FormCommit CommitDialog { get; set; }

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

                var features = (FeaturesResponse)new DataContractJsonSerializer(typeof(FeaturesResponse)).ReadObject(webResponse.GetResponseStream());
                var featuresInProgress = features.data.Where(x => x.workflow_step.name == "In Progress");

                listBox.Items.Clear();
                foreach (var feature in featuresInProgress)
                    listBox.Items.Add(feature);
            }
            catch (Exception ex) {
                MessageBox.Show(this, ex.ToString());
            }
        }

        private void listBox_DoubleClick(object sender, EventArgs e)
        {
            if (listBox.SelectedItem == null || CommitDialog == null)
                return;

            Func<Control, bool> isMessageInput = x => x is EditNetSpell;
            var messageInput = FindControl(isMessageInput, CommitDialog);
            messageInput.Text += "\n\n" + listBox.SelectedItem;
        }

        private Control FindControl(Func<Control, bool> predicate, Control parent)
        {
            foreach (var control in parent.Controls.Cast<Control>())
            {
                if (predicate(control))
                    return control;

                var foundChildControl = FindControl(predicate, control);
                if (foundChildControl != null)
                    return foundChildControl;
            }

            return null;
        }
    }
}
