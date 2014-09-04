// TODO : Extension crashes if main form is closed! (ObjectDisposedException)

using System;
using System.Collections.Generic;
using System.Linq;
using GitUI.CommandsDialogs;
using GitUIPluginInterfaces;
using System.Windows.Forms;

namespace OnTimeTicket
{
    public class OnTimeTicketPlugin : GitPluginBase, IGitPlugin
    {
        private FormShowTickets ticketForm;
        // This is used as a hack to wait until the commit window is shown
        // before we show our own interface
        private Timer timer;
        private IWin32Window ownerForm;

        private const string OnTimeApiKey = "0c14512c-5536-47bc-8881-57edd63f1ae6";
        private const string OnTimeApiSecret = "4857d80a-177a-4c4f-a0c0-c51d9de060c7";
        private StringSetting AccessTokenSettingName = new StringSetting("OnTime access token", "");
        private StringSetting OrganizationSettingName = new StringSetting("OnTime organization", "");
        private StringSetting UserIdSettingName = new StringSetting("OnTime user ID", "");

        public override bool Execute(GitUIBaseEventArgs gitUiCommands)
        {
            return false;
        }

        public override void Register(IGitUICommands gitUiCommands)
        {
            base.Register(gitUiCommands);

            new UserAgentSwitcher().ChangeActualBrowser();

            gitUiCommands.PreCommit += GitUICommandsOnPreCommit;
            gitUiCommands.PostCommit += GitUiCommandsOnPostCommit;
        }

        public override void Unregister(IGitUICommands gitUiCommands)
        {
            base.Unregister(gitUiCommands);

            gitUiCommands.PreCommit -= GitUICommandsOnPreCommit;
            gitUiCommands.PostCommit -= GitUiCommandsOnPostCommit;

            if (ticketForm != null)
                ticketForm.Dispose();

            if (timer != null)
                timer.Dispose();
        }

        void GitUICommandsOnPreCommit(object sender, GitUIBaseEventArgs e)
        {
            if (ticketForm == null)
            {
                ticketForm = new FormShowTickets();

                timer = new Timer();
                timer.Tick += DelayedStartTimerDone;
                timer.Interval = 500;
            }

            // TODO : Have to renew access token when it expires (every 30 days)
            if (string.IsNullOrEmpty(AccessTokenSettingName[Settings]))
                ObtainAccessToken(e);

            ownerForm = e.OwnerForm;
            timer.Start();
        }

        private void ObtainAccessToken(GitUIBaseEventArgs e)
        {
            var companyName = OrganizationSettingName[Settings];
            var authForm = new FormGetAuthCode
            {
                OnTimeApiKey = OnTimeApiKey,
                OnTimeApiSecret = OnTimeApiSecret,
                OnTimeCompanyName = companyName
            };
            using (authForm)
            {
                authForm.ShowDialog(e.OwnerForm);
                AccessTokenSettingName[Settings] = authForm.OnTimeAccessToken;
                UserIdSettingName[Settings] = authForm.OnTimeUserId;
            }
        }
        
        private void DelayedStartTimerDone(object sender, EventArgs eventArgs)
        {
            timer.Stop();

            ticketForm.Visible = false;
            ticketForm.ShowTickets(
                OrganizationSettingName[Settings],
                AccessTokenSettingName[Settings],
                UserIdSettingName[Settings]);
            ticketForm.Show(ownerForm);

            var commitDialog = (FormCommit)Application.OpenForms.Cast<Form>().FirstOrDefault(x => x is FormCommit);
            if (commitDialog != null)
            {
                ticketForm.CommitDialog = commitDialog;
                // This fixes an existing problem in GitExtensions!
                commitDialog.Left = Math.Max(0, commitDialog.Left);

                ticketForm.Left = commitDialog.Right;
                ticketForm.Top = commitDialog.Top;
                ticketForm.Height = commitDialog.Height;

                commitDialog.Focus();
            }

            ticketForm.Visible = true;
        }

        private void GitUiCommandsOnPostCommit(object sender, GitUIPostActionEventArgs gitUiPostActionEventArgs)
        {
            ticketForm.Hide();
        }

        public override string Description
        {
            get { return "OnTime Tickets"; }
        }

        public override IEnumerable<ISetting> GetSettings()
        {
            yield return OrganizationSettingName;
            yield return UserIdSettingName;
            yield return AccessTokenSettingName;
        }
    }
}
