namespace Setup.IPFilter.CustomActions
{
    using System;
    using System.Linq;
    using Microsoft.Win32;

    public class UninstallInfo
    {
        public const string UninstallRegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Uninstall";

        UninstallInfo() {}

        public static UninstallInfo Find(string appName)
        {
            var uninstall = Registry.CurrentUser.OpenSubKey(UninstallRegistryPath);
            if (uninstall == null) return null;

            return (from app in uninstall.GetSubKeyNames() let sub = uninstall.OpenSubKey(app)
                    where sub != null && sub.GetValue("DisplayName") as string == appName
                    select new UninstallInfo
                    {
                        Key = app,
                        UninstallString = sub.GetValue("UninstallString") as string,
                        ShortcutFolderName = sub.GetValue("ShortcutFolderName") as string,
                        ShortcutSuiteName = sub.GetValue("ShortcutSuiteName") as string,
                        ShortcutFileName = sub.GetValue("ShortcutFileName") as string,
                        SupportShortcutFileName = sub.GetValue("SupportShortcutFileName") as string,
                        Version = sub.GetValue("DisplayVersion") as string
                    }).FirstOrDefault();
        }

        public string Key { get; set; }

        public string UninstallString { get; private set; }

        public string ShortcutFolderName { get; set; }

        public string ShortcutSuiteName { get; set; }

        public string ShortcutFileName { get; set; }

        public string SupportShortcutFileName { get; set; }

        public string Version { get; set; }

        public string GetPublicKeyToken()
        {
            var token = UninstallString.Split(',')
                .First(s => s.Trim().StartsWith("PublicKeyToken=", StringComparison.Ordinal)).Substring(16);
            if (token.Length != 16) throw new ArgumentException();
            return token;
        }
    }
}
