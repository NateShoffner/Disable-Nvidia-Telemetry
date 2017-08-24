﻿using System.Reflection;
using System.Security.Principal;
using Microsoft.Win32.TaskScheduler;

namespace DisableNvidiaTelemetry.Utilities
{
    internal class BootTaskUtilities
    {
        private const string TaskName = "Disable Nvidia Telemetry";

        public static Task GetTask()
        {
            return TaskService.Instance.FindTask(TaskName);
        }

        public static void Create()
        {
            var user = WindowsIdentity.GetCurrent().Name;

            using (var ts = new TaskService())
            {
                var td = ts.NewTask();
                td.RegistrationInfo.Description = "Disables Nvidia telemetry services and tasks on startup.";
                td.Principal.UserId = user;
                td.Principal.LogonType = TaskLogonType.InteractiveToken;
                td.Principal.RunLevel = TaskRunLevel.Highest;
                td.Triggers.Add(new LogonTrigger());
                td.Actions.Add(new ExecAction(Assembly.GetExecutingAssembly().Location, Program.StartupParamSilent));
                ts.RootFolder.RegisterTaskDefinition(TaskName, td);
            }
        }

        public static void Remove()
        {
            using (var ts = new TaskService())
            {
                ts.RootFolder.DeleteTask(TaskName, false);
            }
        }
    }
}