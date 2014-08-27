using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace OnTimeTicket
{
    public class UserAgentSwitcher
    {
        [DllImport("urlmon.dll", CharSet = CharSet.Ansi)]
        private static extern int UrlMkSetSessionOption(
            int dwOption, string pBuffer, int dwBufferLength, int dwReserved);

        const int URLMON_OPTION_USERAGENT = 0x10000001;

        public void ChangeUserAgent()
        {
            var ua = "Mozilla/5.0 (Windows NT 6.3; WOW64; Trident/7.0; rv:11.0) like Gecko";

            UrlMkSetSessionOption(URLMON_OPTION_USERAGENT, ua, ua.Length, 0);
        }

        public void ChangeActualBrowser()
        {
            // OnTime doesn't like old IE, so this horrible hack changes the WebBrowser control to use IE11
            Registry.CurrentUser.SetValue(@"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION\GitExtensions.exe", 0x2EDF, RegistryValueKind.DWord);
        }
    }
}