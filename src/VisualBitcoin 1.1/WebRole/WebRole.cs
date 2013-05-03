﻿using System.Diagnostics;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Storage;

namespace WebRole
{
    public class WebRole : RoleEntryPoint
    {
        public override void Run()
        {
			Trace.WriteLine("Entry point called",
				"VisualBitcoin.WebRole.WebRole Information");

            while (true)
			{
                Thread.Sleep(10000);
				Trace.WriteLine("Working",
					"VisualBitcoin.WebRole.WebRole Information");
            }
        }

		public override bool OnStart()
		{
			Trace.WriteLine("On start",
				"VisualBitcoin.WebRole.WebRole Information");

			// Storage configuration and start.
			var connectionString = CloudConfigurationManager.GetSetting("StorageConnectionString");
			var resetBlobBlocksEnable = CloudConfigurationManager.GetSetting("ResetBlobBlocksEnable");
			var resetQueueMessagesEnable = CloudConfigurationManager.GetSetting("ResetBlobBlocksEnable");
			WindowsAzure.Start(connectionString, resetBlobBlocksEnable, resetQueueMessagesEnable);

			return base.OnStart();
		}
    }
}
