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
        private OnTimeConnector connector;

        public OnTimeTicketDropDownButton(OnTimeConnector connector)
            : base("OnTime tickets")
        {
            this.connector = connector;
            DropDown = new ToolStripDropDown();
            connector.OnFeaturesUpdated += OnFeaturesUpdated;
        }

        private void OnFeaturesUpdated(object sender, OnTimeFeaturesEventArgs e)
        {
            EmptyDropdown();

            foreach (var feature in e.Features)
                DropDown.Items.Add(feature.ToString(), null, AddMessage);
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

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (connector != null) 
                connector.OnFeaturesUpdated -= OnFeaturesUpdated;
        }
    }
}