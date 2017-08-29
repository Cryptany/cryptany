using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;

namespace Cryptany.Core.Router.RouterServices
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
            if (!EventLog.SourceExists("Cryptany.Core.Router"))
            {
                EventLog.CreateEventSource("Cryptany.Core.Router", "Application");
            }
        }
	}
}