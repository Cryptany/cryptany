/*
   Copyright 2006-2017 Cryptany, Inc.

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
namespace Cryptany.Core.ConnectorServices
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
            this.serviceInstaller_Beeline = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_Megafon_Khabarovsk = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_Megafon_Kavkaz = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_Megafon_Povolzhje = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_Sonic_Duo = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_BaikalWestCom = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_UralSvyazInform = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_Megafon_Novosibirsk = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_Megafon_Center = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_TELE2 = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_Nizhegorodskaya_Sotovaya_Set = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_Indigo_Saratov = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_NTS = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_Indigo_Arkhangelsk = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_Tsifrovaya_expansiya = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_SMARTS_Samara = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_Stack_GSM = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_Indigo_Murmansk = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_SMARTS_Penza = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_SMARTS_Volgograd = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_SMARTS_Astrakhan = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_SMARTS_Shupashkar = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_Enisey_Telekom = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_SMARTS_Orenburg = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_LuchsheNet = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_MTS = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_Tatinkom = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_SMARTS_Yaroslavl = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_KievStar = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_AKOS = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_Life = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_MTS_USSD = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_Beeline_Ukraina = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_Megafon_Piter = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_UMC = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_Bashsel = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_Megafon_Ural = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_SMARTS_Ivanovo = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_MOTIV = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller_Infon = new System.ServiceProcess.ServiceInstaller();
            // 
            // serviceProcessInstaller
            // 
            this.serviceProcessInstaller.Password = null;
            this.serviceProcessInstaller.Username = null;
            // 
            // serviceInstaller_Beeline
            // 
            this.serviceInstaller_Beeline.Description = "Beeline connector service";
            this.serviceInstaller_Beeline.DisplayName = "Cryptany.ConnectorService2";
            this.serviceInstaller_Beeline.ServiceName = "Cryptany.ConnectorService2";
            this.serviceInstaller_Beeline.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_Megafon_Khabarovsk
            // 
            this.serviceInstaller_Megafon_Khabarovsk.Description = "Megafon Khabarovsk connector service";
            this.serviceInstaller_Megafon_Khabarovsk.DisplayName = "Cryptany.ConnectorService4";
            this.serviceInstaller_Megafon_Khabarovsk.ServiceName = "Cryptany.ConnectorService4";
            this.serviceInstaller_Megafon_Khabarovsk.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_Megafon_Kavkaz
            // 
            this.serviceInstaller_Megafon_Kavkaz.Description = "Megafon Kavkaz connector service";
            this.serviceInstaller_Megafon_Kavkaz.DisplayName = "Cryptany.ConnectorService5";
            this.serviceInstaller_Megafon_Kavkaz.ServiceName = "Cryptany.ConnectorService5";
            this.serviceInstaller_Megafon_Kavkaz.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_Megafon_Povolzhje
            // 
            this.serviceInstaller_Megafon_Povolzhje.Description = "Megafon Povolzhje connector service";
            this.serviceInstaller_Megafon_Povolzhje.DisplayName = "Cryptany.ConnectorService6";
            this.serviceInstaller_Megafon_Povolzhje.ServiceName = "Cryptany.ConnectorService6";
            this.serviceInstaller_Megafon_Povolzhje.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_Sonic_Duo
            // 
            this.serviceInstaller_Sonic_Duo.Description = "Sonic Duo connector service";
            this.serviceInstaller_Sonic_Duo.DisplayName = "Cryptany.ConnectorService7";
            this.serviceInstaller_Sonic_Duo.ServiceName = "Cryptany.ConnectorService7";
            this.serviceInstaller_Sonic_Duo.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_BaikalWestCom
            // 
            this.serviceInstaller_BaikalWestCom.Description = "BaikalWestCom connector service";
            this.serviceInstaller_BaikalWestCom.DisplayName = "Cryptany.ConnectorService8";
            this.serviceInstaller_BaikalWestCom.ServiceName = "Cryptany.ConnectorService8";
            this.serviceInstaller_BaikalWestCom.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_UralSvyazInform
            // 
            this.serviceInstaller_UralSvyazInform.Description = "UralSvyazInform connector service";
            this.serviceInstaller_UralSvyazInform.DisplayName = "Cryptany.ConnectorService9";
            this.serviceInstaller_UralSvyazInform.ServiceName = "Cryptany.ConnectorService9";
            this.serviceInstaller_UralSvyazInform.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_Megafon_Novosibirsk
            // 
            this.serviceInstaller_Megafon_Novosibirsk.Description = "Megafon Novosibirsk connector service";
            this.serviceInstaller_Megafon_Novosibirsk.DisplayName = "Cryptany.ConnectorService10";
            this.serviceInstaller_Megafon_Novosibirsk.ServiceName = "Cryptany.ConnectorService10";
            this.serviceInstaller_Megafon_Novosibirsk.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_Megafon_Center
            // 
            this.serviceInstaller_Megafon_Center.Description = "Megafon Center connector service";
            this.serviceInstaller_Megafon_Center.DisplayName = "Cryptany.ConnectorService11";
            this.serviceInstaller_Megafon_Center.ServiceName = "Cryptany.ConnectorService11";
            this.serviceInstaller_Megafon_Center.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_TELE2
            // 
            this.serviceInstaller_TELE2.Description = "TELE2 connector service";
            this.serviceInstaller_TELE2.DisplayName = "Cryptany.ConnectorService12";
            this.serviceInstaller_TELE2.ServiceName = "Cryptany.ConnectorService12";
            this.serviceInstaller_TELE2.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_Nizhegorodskaya_Sotovaya_Set
            // 
            this.serviceInstaller_Nizhegorodskaya_Sotovaya_Set.Description = "Nizhegorodskaya Sotovaya Set connector service";
            this.serviceInstaller_Nizhegorodskaya_Sotovaya_Set.DisplayName = "Cryptany.ConnectorService17";
            this.serviceInstaller_Nizhegorodskaya_Sotovaya_Set.ServiceName = "Cryptany.ConnectorService17";
            this.serviceInstaller_Nizhegorodskaya_Sotovaya_Set.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_Indigo_Saratov
            // 
            this.serviceInstaller_Indigo_Saratov.Description = "Indigo Saratov connector service";
            this.serviceInstaller_Indigo_Saratov.DisplayName = "Cryptany.ConnectorService18";
            this.serviceInstaller_Indigo_Saratov.ServiceName = "Cryptany.ConnectorService18";
            this.serviceInstaller_Indigo_Saratov.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_NTS
            // 
            this.serviceInstaller_NTS.Description = "NTS connector service";
            this.serviceInstaller_NTS.DisplayName = "Cryptany.ConnectorService19";
            this.serviceInstaller_NTS.ServiceName = "Cryptany.ConnectorService19";
            this.serviceInstaller_NTS.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_Indigo_Arkhangelsk
            // 
            this.serviceInstaller_Indigo_Arkhangelsk.Description = "Indigo Arkhangelsk connector service";
            this.serviceInstaller_Indigo_Arkhangelsk.DisplayName = "Cryptany.ConnectorService20";
            this.serviceInstaller_Indigo_Arkhangelsk.ServiceName = "Cryptany.ConnectorService20";
            this.serviceInstaller_Indigo_Arkhangelsk.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_Tsifrovaya_expansiya
            // 
            this.serviceInstaller_Tsifrovaya_expansiya.Description = "Tsifrovaya expansiya connector service";
            this.serviceInstaller_Tsifrovaya_expansiya.DisplayName = "Cryptany.ConnectorService22";
            this.serviceInstaller_Tsifrovaya_expansiya.ServiceName = "Cryptany.ConnectorService22";
            this.serviceInstaller_Tsifrovaya_expansiya.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_SMARTS_Samara
            // 
            this.serviceInstaller_SMARTS_Samara.Description = "SMARTS Samara connector service";
            this.serviceInstaller_SMARTS_Samara.DisplayName = "Cryptany.ConnectorService23";
            this.serviceInstaller_SMARTS_Samara.ServiceName = "Cryptany.ConnectorService23";
            this.serviceInstaller_SMARTS_Samara.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_Stack_GSM
            // 
            this.serviceInstaller_Stack_GSM.Description = "Stack GSM connector service";
            this.serviceInstaller_Stack_GSM.DisplayName = "Cryptany.ConnectorService24";
            this.serviceInstaller_Stack_GSM.ServiceName = "Cryptany.ConnectorService24";
            this.serviceInstaller_Stack_GSM.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_Indigo_Murmansk
            // 
            this.serviceInstaller_Indigo_Murmansk.Description = "Indigo Murmansk connector service";
            this.serviceInstaller_Indigo_Murmansk.DisplayName = "Cryptany.ConnectorService25";
            this.serviceInstaller_Indigo_Murmansk.ServiceName = "Cryptany.ConnectorService25";
            this.serviceInstaller_Indigo_Murmansk.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_SMARTS_Penza
            // 
            this.serviceInstaller_SMARTS_Penza.Description = "SMARTS Penza connector service";
            this.serviceInstaller_SMARTS_Penza.DisplayName = "Cryptany.ConnectorService26";
            this.serviceInstaller_SMARTS_Penza.ServiceName = "Cryptany.ConnectorService26";
            this.serviceInstaller_SMARTS_Penza.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_SMARTS_Volgograd
            // 
            this.serviceInstaller_SMARTS_Volgograd.Description = "SMARTS Volgograd connector service";
            this.serviceInstaller_SMARTS_Volgograd.DisplayName = "Cryptany.ConnectorService27";
            this.serviceInstaller_SMARTS_Volgograd.ServiceName = "Cryptany.ConnectorService27";
            this.serviceInstaller_SMARTS_Volgograd.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_SMARTS_Astrakhan
            // 
            this.serviceInstaller_SMARTS_Astrakhan.Description = "SMARTS Astrakhan connector service";
            this.serviceInstaller_SMARTS_Astrakhan.DisplayName = "Cryptany.ConnectorService28";
            this.serviceInstaller_SMARTS_Astrakhan.ServiceName = "Cryptany.ConnectorService28";
            this.serviceInstaller_SMARTS_Astrakhan.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_SMARTS_Shupashkar
            // 
            this.serviceInstaller_SMARTS_Shupashkar.Description = "SMARTS Shupashkar connector service";
            this.serviceInstaller_SMARTS_Shupashkar.DisplayName = "Cryptany.ConnectorService29";
            this.serviceInstaller_SMARTS_Shupashkar.ServiceName = "Cryptany.ConnectorService29";
            this.serviceInstaller_SMARTS_Shupashkar.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_Enisey_Telekom
            // 
            this.serviceInstaller_Enisey_Telekom.Description = "Enisey Telekom connector service";
            this.serviceInstaller_Enisey_Telekom.DisplayName = "Cryptany.ConnectorService30";
            this.serviceInstaller_Enisey_Telekom.ServiceName = "Cryptany.ConnectorService30";
            this.serviceInstaller_Enisey_Telekom.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_SMARTS_Orenburg
            // 
            this.serviceInstaller_SMARTS_Orenburg.Description = "SMARTS Orenburg connector service";
            this.serviceInstaller_SMARTS_Orenburg.DisplayName = "Cryptany.ConnectorService31";
            this.serviceInstaller_SMARTS_Orenburg.ServiceName = "Cryptany.ConnectorService31";
            this.serviceInstaller_SMARTS_Orenburg.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_LuchsheNet
            // 
            this.serviceInstaller_LuchsheNet.Description = "LuchsheNet connector service";
            this.serviceInstaller_LuchsheNet.DisplayName = "Cryptany.ConnectorService32";
            this.serviceInstaller_LuchsheNet.ServiceName = "Cryptany.ConnectorService32";
            this.serviceInstaller_LuchsheNet.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_MTS
            // 
            this.serviceInstaller_MTS.Description = "MTS connector service";
            this.serviceInstaller_MTS.DisplayName = "Cryptany.ConnectorService33";
            this.serviceInstaller_MTS.ServiceName = "Cryptany.ConnectorService33";
            this.serviceInstaller_MTS.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_Tatinkom
            // 
            this.serviceInstaller_Tatinkom.Description = "Tatinkom connector service";
            this.serviceInstaller_Tatinkom.DisplayName = "Cryptany.ConnectorService34";
            this.serviceInstaller_Tatinkom.ServiceName = "Cryptany.ConnectorService34";
            this.serviceInstaller_Tatinkom.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_SMARTS_Yaroslavl
            // 
            this.serviceInstaller_SMARTS_Yaroslavl.Description = "SMARTS Yaroslavl connector service";
            this.serviceInstaller_SMARTS_Yaroslavl.DisplayName = "Cryptany.ConnectorService35";
            this.serviceInstaller_SMARTS_Yaroslavl.ServiceName = "Cryptany.ConnectorService35";
            this.serviceInstaller_SMARTS_Yaroslavl.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_KievStar
            // 
            this.serviceInstaller_KievStar.Description = "KievStar connector service";
            this.serviceInstaller_KievStar.DisplayName = "Cryptany.ConnectorService50";
            this.serviceInstaller_KievStar.ServiceName = "Cryptany.ConnectorService50";
            this.serviceInstaller_KievStar.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_AKOS
            // 
            this.serviceInstaller_AKOS.Description = "AKOS connector service";
            this.serviceInstaller_AKOS.DisplayName = "Cryptany.ConnectorService37";
            this.serviceInstaller_AKOS.ServiceName = "Cryptany.ConnectorService37";
            this.serviceInstaller_AKOS.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_Life
            // 
            this.serviceInstaller_Life.Description = "Life connector service";
            this.serviceInstaller_Life.DisplayName = "Cryptany.ConnectorService54";
            this.serviceInstaller_Life.ServiceName = "Cryptany.ConnectorService54";
            this.serviceInstaller_Life.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_MTS_USSD
            // 
            this.serviceInstaller_MTS_USSD.Description = "MTS USSD connector service";
            this.serviceInstaller_MTS_USSD.DisplayName = "Cryptany.ConnectorService55";
            this.serviceInstaller_MTS_USSD.ServiceName = "Cryptany.ConnectorService55";
            this.serviceInstaller_MTS_USSD.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_Beeline_Ukraina
            // 
            this.serviceInstaller_Beeline_Ukraina.Description = "Beeline Ukraina connector service";
            this.serviceInstaller_Beeline_Ukraina.DisplayName = "Cryptany.ConnectorService53";
            this.serviceInstaller_Beeline_Ukraina.ServiceName = "Cryptany.ConnectorService53";
            this.serviceInstaller_Beeline_Ukraina.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_Megafon_Piter
            // 
            this.serviceInstaller_Megafon_Piter.Description = "Megafon Piter connector service";
            this.serviceInstaller_Megafon_Piter.DisplayName = "Cryptany.ConnectorService101";
            this.serviceInstaller_Megafon_Piter.ServiceName = "Cryptany.ConnectorService101";
            this.serviceInstaller_Megafon_Piter.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_UMC
            // 
            this.serviceInstaller_UMC.Description = "UMC connector service";
            this.serviceInstaller_UMC.DisplayName = "Cryptany.ConnectorService52";
            this.serviceInstaller_UMC.ServiceName = "Cryptany.ConnectorService52";
            this.serviceInstaller_UMC.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_Bashsel
            // 
            this.serviceInstaller_Bashsel.Description = "Bashsel connector service";
            this.serviceInstaller_Bashsel.DisplayName = "Cryptany.ConnectorService36";
            this.serviceInstaller_Bashsel.ServiceName = "Cryptany.ConnectorService36";
            this.serviceInstaller_Bashsel.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_Megafon_Ural
            // 
            this.serviceInstaller_Megafon_Ural.Description = "Megafon Ural connector service";
            this.serviceInstaller_Megafon_Ural.DisplayName = "Cryptany.ConnectorService102";
            this.serviceInstaller_Megafon_Ural.ServiceName = "Cryptany.ConnectorService102";
            this.serviceInstaller_Megafon_Ural.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_SMARTS_Ivanovo
            // 
            this.serviceInstaller_SMARTS_Ivanovo.Description = "SMARTS Ivanovo connector service";
            this.serviceInstaller_SMARTS_Ivanovo.DisplayName = "Cryptany.ConnectorService200";
            this.serviceInstaller_SMARTS_Ivanovo.ServiceName = "Cryptany.ConnectorService200";
            this.serviceInstaller_SMARTS_Ivanovo.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_MOTIV
            // 
            this.serviceInstaller_MOTIV.Description = "MOTIV connector service";
            this.serviceInstaller_MOTIV.DisplayName = "Cryptany.ConnectorService103";
            this.serviceInstaller_MOTIV.ServiceName = "Cryptany.ConnectorService103";
            this.serviceInstaller_MOTIV.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // serviceInstaller_Infon
            // 
            this.serviceInstaller_Infon.Description = "Infon connector service";
            this.serviceInstaller_Infon.DisplayName = "Cryptany.ConnectorService229";
            this.serviceInstaller_Infon.ServiceName = "Cryptany.ConnectorService229";
            this.serviceInstaller_Infon.ServicesDependedOn = new string[] {
        "MSMQ",
        "MSMQTriggers"};
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.serviceProcessInstaller,
            this.serviceInstaller_Beeline,
            this.serviceInstaller_Megafon_Khabarovsk,
            this.serviceInstaller_Megafon_Kavkaz,
            this.serviceInstaller_Megafon_Povolzhje,
            this.serviceInstaller_Sonic_Duo,
            this.serviceInstaller_BaikalWestCom,
            this.serviceInstaller_UralSvyazInform,
            this.serviceInstaller_Megafon_Novosibirsk,
            this.serviceInstaller_Megafon_Center,
            this.serviceInstaller_TELE2,
            this.serviceInstaller_Nizhegorodskaya_Sotovaya_Set,
            this.serviceInstaller_Indigo_Saratov,
            this.serviceInstaller_NTS,
            this.serviceInstaller_Indigo_Arkhangelsk,
            this.serviceInstaller_Tsifrovaya_expansiya,
            this.serviceInstaller_SMARTS_Samara,
            this.serviceInstaller_Stack_GSM,
            this.serviceInstaller_Indigo_Murmansk,
            this.serviceInstaller_SMARTS_Penza,
            this.serviceInstaller_SMARTS_Volgograd,
            this.serviceInstaller_SMARTS_Astrakhan,
            this.serviceInstaller_SMARTS_Shupashkar,
            this.serviceInstaller_Enisey_Telekom,
            this.serviceInstaller_SMARTS_Orenburg,
            this.serviceInstaller_LuchsheNet,
            this.serviceInstaller_MTS,
            this.serviceInstaller_Tatinkom,
            this.serviceInstaller_SMARTS_Yaroslavl,
            this.serviceInstaller_KievStar,
            this.serviceInstaller_AKOS,
            this.serviceInstaller_Life,
            this.serviceInstaller_MTS_USSD,
            this.serviceInstaller_Beeline_Ukraina,
            this.serviceInstaller_Megafon_Piter,
            this.serviceInstaller_UMC,
            this.serviceInstaller_Bashsel,
            this.serviceInstaller_Megafon_Ural,
            this.serviceInstaller_SMARTS_Ivanovo,
            this.serviceInstaller_MOTIV,
            this.serviceInstaller_Infon});

		}

		#endregion

		private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_Beeline;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_Megafon_Khabarovsk;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_Megafon_Kavkaz;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_Megafon_Povolzhje;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_Sonic_Duo;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_BaikalWestCom;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_UralSvyazInform;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_Megafon_Novosibirsk;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_Megafon_Center;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_TELE2;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_Nizhegorodskaya_Sotovaya_Set;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_Indigo_Saratov;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_NTS;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_Indigo_Arkhangelsk;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_Tsifrovaya_expansiya;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_SMARTS_Samara;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_Stack_GSM;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_Indigo_Murmansk;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_SMARTS_Penza;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_SMARTS_Volgograd;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_SMARTS_Astrakhan;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_SMARTS_Shupashkar;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_Enisey_Telekom;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_SMARTS_Orenburg;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_LuchsheNet;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_MTS;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_Tatinkom;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_SMARTS_Yaroslavl;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_KievStar;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_AKOS;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_Life;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_MTS_USSD;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_Beeline_Ukraina;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_Megafon_Piter;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_UMC;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_Bashsel;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_Megafon_Ural;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_SMARTS_Ivanovo;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_MOTIV;
        private System.ServiceProcess.ServiceInstaller serviceInstaller_Infon;
	}
}