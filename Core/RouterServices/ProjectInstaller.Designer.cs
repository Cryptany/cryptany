namespace Cryptany.Router.RouterServices
{
	partial class ProjectInstaller
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.serviceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.serviceInstaller_1 = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_2 = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_3 = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_main = new System.ServiceProcess.ServiceInstaller();
            // 
            // serviceProcessInstaller
            // 
            this.serviceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.NetworkService;
            this.serviceProcessInstaller.Password = null;
            this.serviceProcessInstaller.Username = null;
            // 
            // serviceInstaller_1
            // 
            this.serviceInstaller_1.Description = "Router service 1";
            this.serviceInstaller_1.DisplayName = "Avant.RouterService1";
            this.serviceInstaller_1.ServiceName = "Avant.RouterService1";
            this.serviceInstaller_1.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_2
            // 
            this.serviceInstaller_2.Description = "Router service 2";
            this.serviceInstaller_2.DisplayName = "Avant.RouterService2";
            this.serviceInstaller_2.ServiceName = "Avant.RouterService2";
            this.serviceInstaller_2.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_3
            // 
            this.serviceInstaller_3.Description = "Router service 3";
            this.serviceInstaller_3.ServiceName = "Avant.RouterService3";
            this.serviceInstaller_3.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_main
            // 
            this.serviceInstaller_main.Description = "Router main input queue service";
            this.serviceInstaller_main.DisplayName = "Cryptany.Router.RouterMainService";
            this.serviceInstaller_main.ServiceName = "Cryptany.Router.RouterMainService";
            this.serviceInstaller_main.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.serviceProcessInstaller,
            this.serviceInstaller_1,
            this.serviceInstaller_2,
            this.serviceInstaller_3,
            this.serviceInstaller_main});

		}

		#endregion

		private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstaller;
		private System.ServiceProcess.ServiceInstaller serviceInstaller_1;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_2;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_3;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_main;
	}
}