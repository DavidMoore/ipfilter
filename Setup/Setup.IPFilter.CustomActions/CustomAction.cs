namespace IPFilter.Setup.CustomActions
{
    using System;
    using System.Threading;
    using Microsoft.Deployment.WindowsInstaller;

    public class CustomActions
    {
        [CustomAction]
        public static ActionResult UninstallClickOnce(Session session)
        {
            session.Log("Begin to uninstall ClickOnce deployment");

            var appName = "IPFilter Updater";

            try
            {
                var uninstallInfo = UninstallInfo.Find(appName);
                if (uninstallInfo == null)
                {
                    session.Log("No uninstall information found for " + appName);
                    return ActionResult.NotExecuted;
                }

                session.Log("Waiting for files to become free...");
                Thread.Sleep(TimeSpan.FromSeconds(10));

                session.Log("Uninstalling " + appName);
                var uninstaller = new Uninstaller();
                uninstaller.Uninstall(uninstallInfo);
            }
            catch (Exception ex)
            {
                session.Log("ERROR in ClickOnceUninstaller custom action:\n {0}", ex.ToString());
                return ActionResult.Failure;
            }

            return ActionResult.Success;
        }
    }
}
