using System;
using System.Collections.Generic;
using System.Linq;
using GitUI;
using GitUI.CommandsDialogs;
using GitUIPluginInterfaces;
using System.Windows.Forms;

namespace OnTimeTicket
{
    public class OnTimeTicketPlugin : GitPluginBase
    {
        // This is used as a hack to wait until the commit window is shown
        // before we show our own interface
        private Timer timer;

        private const string OnTimeApiKey = "0c14512c-5536-47bc-8881-57edd63f1ae6";
        private const string OnTimeApiSecret = "4857d80a-177a-4c4f-a0c0-c51d9de060c7";
        private readonly StringSetting accessTokenSettingName = new StringSetting("OnTime access token", "");
        private readonly StringSetting organizationSettingName = new StringSetting("OnTime organization", "");
        private readonly StringSetting userIdSettingName = new StringSetting("OnTime user ID", "");
        private OnTimeTicketDropDownButton dropDown;

        public override bool Execute(GitUIBaseEventArgs gitUiCommands)
        {
            return false;
        }

        public override void Register(IGitUICommands gitUiCommands)
        {
            base.Register(gitUiCommands);

            new UserAgentSwitcher().ChangeActualBrowser();

            gitUiCommands.PreCommit += GitUICommandsOnPreCommit;
        }

        public override void Unregister(IGitUICommands gitUiCommands)
        {
            base.Unregister(gitUiCommands);

            gitUiCommands.PreCommit -= GitUICommandsOnPreCommit;

            if (dropDown != null)
            {
                dropDown.Dispose();
                dropDown = null;
            }

            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }
        }

        void GitUICommandsOnPreCommit(object sender, GitUIBaseEventArgs e)
        {
            if (dropDown == null)
            {
                timer = new Timer();
                timer.Tick += DelayedStartTimerDone;
                timer.Interval = 200;
            }

            // TODO : Have to renew access token when it expires (every 30 days)
            if (string.IsNullOrEmpty(accessTokenSettingName[Settings]))
                ObtainAccessToken(e);

            timer.Start();
        }

        private void ObtainAccessToken(GitUIBaseEventArgs e)
        {
            var companyName = organizationSettingName[Settings];
            var authForm = new FormGetAuthCode
            {
                OnTimeApiKey = OnTimeApiKey,
                OnTimeApiSecret = OnTimeApiSecret,
                OnTimeCompanyName = companyName
            };
            using (authForm)
            {
                authForm.ShowDialog(e.OwnerForm);
                accessTokenSettingName[Settings] = authForm.OnTimeAccessToken;
                userIdSettingName[Settings] = authForm.OnTimeUserId;
            }
        }
        
        private void DelayedStartTimerDone(object sender, EventArgs eventArgs)
        {
            timer.Stop();

            if (dropDown == null)
                dropDown = new OnTimeTicketDropDownButton();

            var commitDialog = (FormCommit) Application.OpenForms.Cast<Form>().FirstOrDefault(x => x is FormCommit);
            if (commitDialog != null)
            {
                var toolbarCommit =
                    (ToolStripEx) WinFormUtils.FindControl(c => c.Name == "toolbarCommit", commitDialog);
                dropDown.CommitDialog = commitDialog;
                toolbarCommit.Items.Add(dropDown);
                commitDialog.Focus();
            }

            dropDown.ShowTickets(
                organizationSettingName[Settings],
                accessTokenSettingName[Settings],
                userIdSettingName[Settings]);
        }

        public override string Description
        {
            get { return "OnTime Tickets"; }
        }

        public override IEnumerable<ISetting> GetSettings()
        {
            yield return organizationSettingName;
            yield return userIdSettingName;
            yield return accessTokenSettingName;
        }
    }
}
