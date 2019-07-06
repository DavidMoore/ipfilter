using System;
using System.Diagnostics;

namespace IPFilter.Commands
{
    class ScheduledTaskCommand
    {
        public enum _TASK_LOGON_TYPE
        {
            TASK_LOGON_NONE,
            TASK_LOGON_PASSWORD,
            TASK_LOGON_S4U,
            TASK_LOGON_INTERACTIVE_TOKEN,
            TASK_LOGON_GROUP,
            TASK_LOGON_SERVICE_ACCOUNT,
            TASK_LOGON_INTERACTIVE_TOKEN_OR_PASSWORD,
        }

        public enum _TASK_TRIGGER_TYPE2
        {
            TASK_TRIGGER_EVENT = 0,
            TASK_TRIGGER_TIME = 1,
            TASK_TRIGGER_DAILY = 2,
            TASK_TRIGGER_WEEKLY = 3,
            TASK_TRIGGER_MONTHLY = 4,
            TASK_TRIGGER_MONTHLYDOW = 5,
            TASK_TRIGGER_IDLE = 6,
            TASK_TRIGGER_REGISTRATION = 7,
            TASK_TRIGGER_BOOT = 8,
            TASK_TRIGGER_LOGON = 9,
            TASK_TRIGGER_SESSION_STATE_CHANGE = 11,
            TASK_TRIGGER_CUSTOM_TRIGGER_01 = 12
        }

        public enum _TASK_CREATION
        {
            TASK_VALIDATE_ONLY = 1,
            TASK_CREATE = 2,
            TASK_UPDATE = 4,
            TASK_CREATE_OR_UPDATE = 6,
            TASK_DISABLE = 8,
            TASK_DONT_ADD_PRINCIPAL_ACE = 16,
            TASK_IGNORE_REGISTRATION_TRIGGERS = 32
        }

        public enum _TASK_ACTION_TYPE
        {
            TASK_ACTION_EXEC = 0,
            TASK_ACTION_COM_HANDLER = 5,
            TASK_ACTION_SEND_EMAIL = 6,
            TASK_ACTION_SHOW_MESSAGE = 7,
        }

        public enum _TASK_RUNLEVEL
        {
            TASK_RUNLEVEL_LUA,
            TASK_RUNLEVEL_HIGHEST,
        }

        const string taskPath = "IPFilter";

        public static void Execute()
        {
            var type = Type.GetTypeFromProgID("Schedule.Service");
            dynamic service = Activator.CreateInstance(type);

            service.Connect();
            
            Trace.TraceInformation("Setting up the automatic schedule...");
            var task = service.NewTask(0);
            //using (var task = service.NewTask())
            {
                task.RegistrationInfo.Description = "Updates the IP Filter for bit torrent clients";

                task.Triggers.Clear();

                // Schedule to run daily at 4am
                var now = DateTime.Now.AddDays(-1);
                var date = new DateTime(now.Year, now.Month, now.Day, 4, 0, 0);
                
                var trigger = task.Triggers.Create(_TASK_TRIGGER_TYPE2.TASK_TRIGGER_DAILY);
                trigger.DaysInterval = 1;
                trigger.StartBoundary = date.ToString("s");
                trigger.RandomDelay = "PT15M"; // Delay randomly by 15 minutes to stagger the amount of requests hitting list servers
                
                // Execute silently
                //var action = (IExecAction) task.Actions.Create(_TASK_ACTION_TYPE.TASK_ACTION_EXEC);
                var action = task.Actions.Create(_TASK_ACTION_TYPE.TASK_ACTION_EXEC);
                action.Path = Process.GetCurrentProcess().MainModule.FileName;
                action.Arguments = "/silent";

                task.Settings.RunOnlyIfNetworkAvailable = true;
                task.Settings.StartWhenAvailable = true;
                task.Settings.WakeToRun = false;

                task.Principal.RunLevel = _TASK_RUNLEVEL.TASK_RUNLEVEL_LUA;
                task.Principal.LogonType = _TASK_LOGON_TYPE.TASK_LOGON_INTERACTIVE_TOKEN;
                //task.Principal.UserId = identity.Name;

                var folder = service.GetFolder("\\");
                
                var registered = folder.RegisterTaskDefinition(taskPath, task, (int)_TASK_CREATION.TASK_CREATE_OR_UPDATE, null,null,_TASK_LOGON_TYPE.TASK_LOGON_INTERACTIVE_TOKEN);
                Trace.TraceInformation("Finished creating / updating scheduled task");
            }
        }
    }
}