using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;

namespace RouterServices
{
	[RunInstaller(true)]
	public partial class ProjectInstaller : Installer
	{
		public ProjectInstaller()
		{
			InitializeComponent();
		}

        protected override void OnAfterInstall(System.Collections.IDictionary savedState)
        {
            base.OnAfterInstall(savedState);
            if (!EventLog.SourceExists("Avant.Core.Router"))
            {
                EventLog.CreateEventSource("Avant.Core.Router", "Application");
            }
        }
	}
}