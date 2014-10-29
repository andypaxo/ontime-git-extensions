using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GitUI.CommandsDialogs;
using GitUI.SpellChecker;

namespace OnTimeTicket
{
    public class OnTimeTicketDropDownButton : ToolStripDropDownButton
    {
        public FormCommit CommitDialog { get; set; }
        private static string TitleText = "O&nTime tickets";

        public OnTimeTicketDropDownButton()
            : base(TitleText)
        {
            DropDown = new ToolStripDropDown();
        }

        public void OnFeaturesUpdated(object sender, OnTimeFeaturesEventArgs e)
        {
            EmptyDropdown();

            foreach (var feature in e.Features)
                DropDown.Items.Add(feature.ToString(), null, AddMessage);
            Text = string.Format("{0} ({1})", TitleText, e.Features.Count);
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