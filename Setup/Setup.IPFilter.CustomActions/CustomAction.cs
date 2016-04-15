namespace Setup.IPFilter.CustomActions
{
    using System;
    using Microsoft.Deployment.WindowsInstaller;

    public class CustomActions
    {
        [CustomAction]
        public static ActionResult UninstallClickOnce(Session session)
        {
            session.Log("Begin to uninstall ClickOnce deployment");

            var appName = session["CLICKONCEAPPNAME"];
            if (string.IsNullOrEmpty(appName))
            {
                session.Log("Please set property CLICKONCEAPPNAME.");
                return ActionResult.Failure;
            }

            try
            {
                var uninstallInfo = UninstallInfo.Find(appName);
                if (uninstallInfo == null)
                {
                    session.Log("No uninstall information found for " + appName);
                    return ActionResult.NotExecuted;
                }

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
