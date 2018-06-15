using System;
using System.Reflection;
using System.Security.Principal;
using DisableNvidiaTelemetry.Properties;
using Microsoft.Win32.TaskScheduler;

namespace DisableNvidiaTelemetry.Utilities
{
    internal class TaskSchedulerUtilities
    {
        public enum TaskTrigger
        {
            WindowsLogin = 0,
            Daily = 1
        }

        public static Task GetTask()
        {
            return TaskService.Instance.FindTask(Resources.Disable_Nvidia_Telemetry);
        }

        public static void Create(TaskTrigger trigger)
        {
            var user = WindowsIdentity.GetCurrent().Name;

            using (var ts = new TaskService())
            {
                var td = ts.NewTask();
                td.RegistrationInfo.Description = Resources.Task_scheduler_description;
                td.Principal.UserId = user;
                td.Principal.LogonType = TaskLogonType.InteractiveToken;
                td.Principal.RunLevel = TaskRunLevel.Highest;
                if (trigger == TaskTrigger.WindowsLogin)
                    td.Triggers.Add(new LogonTrigger());
                if (trigger == TaskTrigger.Daily)
                {
                    var now = DateTime.Today;
                    var startDateTime = new DateTime(now.Year, now.Month, now.Day, 12, 0, 0);
                    var dt = new DailyTrigger
                    {
                        StartBoundary = startDateTime,
                        Enabled = true,
                        DaysInterval = 1,
                        Repetition = {Interval = TimeSpan.FromHours(24)}
                    };

                    td.Triggers.Add(dt);
                }
                td.Actions.Add(new ExecAction(Assembly.GetExecutingAssembly().Location, AppUtils.StartupParamSilent));
                ts.RootFolder.RegisterTaskDefinition(Resources.Disable_Nvidia_Telemetry, td);
            }
        }

        public static void Remove()
        {
            using (var ts = new TaskService())
            {
                ts.RootFolder.DeleteTask(Resources.Disable_Nvidia_Telemetry, false);
            }
        }
    }
}