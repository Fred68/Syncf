using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;

using Fred68.CfgReader;
using static Fred68.CfgReader.CfgReader;

namespace Syncf
{
	public delegate bool FuncBkgnd(CancellationToken tk);

	public partial class Form1:Form
	{
		SyncFile sf;
		SyncfParams par;

		static CancellationTokenSource? cts = null;
		CancellationToken token = CancellationToken.None;
		Control? taskCtrl = null;
		bool running = false;
		bool closeRequest = false;

		char[] ch = { '|','/','-','\\' };
		int ich = 0;
		string[] arguments;

		const string CMD_USR = "-usr";
		const string CMD_CFG = "-cfg";
		const string CMD_LST = "-lst";
		const string CMD_ALL = "-all";
		const string CMD_NFL = "-lstnofilter";

		enum CMD { USR, CFG, LST, ALL, None };

		//string usrName, cfgFile, lstFileNoExt;
		//FLS fls;

		/// <summary>
		/// Ctor
		/// </summary>
		public Form1(string[] args)
		{
			InitializeComponent();
			SuspendLayout();
			par = new SyncfParams();

			//usrName = cfgFile = lstFileNoExt = string.Empty;
			//fls = FLS.None;

			arguments = args;
			AnalyseArgs(args);
			par.fmsg = AddMessageAltTask;

			//par = new SyncfParams(AddMessageAltTask,usrName,cfgFile,lstFileNoExt,fls);
			//sf = new SyncFile(AddMessageAltTask,usrName,cfgFile,lstFileNoExt,fls);
			sf = new SyncFile(par);
			statusStrip1.MinimumSize = new System.Drawing.Size(0,30);
			toolStripStatusLabel1.Text = new string('-',80);
			this.MinimumSize = this.Size;
			this.Text = "Syncf";
			btStop.Visible = true;
			btStop.Enabled = false;
			ResumeLayout(false);
			PerformLayout();
		}

		/// <summary>
		/// OnLoad...
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Form1_Load(object sender,EventArgs e)
		{
			SuspendLayout();

			tbMsg.AppendText("\r\n");
			string msg = sf.msgConfiguration;
			tbMsg.AppendText(msg);          //MessageBox.Show(msg);
			tbMsg.SelectionLength = 0;      // Deseleziona

			refreshTimer.Interval = 300;
			refreshTimer.Start();

			ResumeLayout(false);
			PerformLayout();

			if(sf.isEnabled)
			{
				FuncBkgnd? f = sf.SetStartFunction();
				if(f != null)
				{
					taskCtrl = gbComandi;
					EseguiComando(f);
				}
			}
			else
			{
				MessageBox.Show($"Errori nella configurazione.\r\nIl programma non può essere eseguito");
				btRead.Enabled = btWrite.Enabled = btReadWrite.Enabled = btStop.Enabled = btClearLog.Enabled = false;
				refreshTimer.Stop();
			}

		}

		/// <summary>
		/// Aggiunge messaggio alla finestra
		/// </summary>
		/// <param name="msg"></param>
		private void AddMessage(string msg)
		{
			tbMsg.AppendText(msg);
			tbMsg.Invalidate();
		}

		/// <summary>
		/// Aggiunge messaggio alla finestra...
		/// ...chiamata da task differente dal quello principale (UI)
		/// </summary>
		/// <param name="msg"></param>
		public void AddMessageAltTask(string msg)
		{
			tbMsg.BeginInvoke(new Action(() => AddMessage(msg)));
		}

		/// <summary>
		/// Registra e chiama la funzione per eseguire un comando
		/// </summary>
		/// <param name="f">Funzione di background</param>
		/// <param name="token"></param>
		/// <param name="ctsrc"></param>
		/// <returns></returns>
		bool Esegui(FuncBkgnd? f,CancellationToken token,CancellationTokenSource ctsrc)
		{
			bool ok = false;

			if((token != CancellationToken.None) && (ctsrc != null))
			{
				token.Register(() =>
								{
									//MessageBox.Show("Operazione annullata");
									ctsrc.Dispose();
								}

				);

				running = true;
				if(f != null)
				{
					ok = f(token);      // Esegue le operazioni
				}
			}

			return ok;
		}

		/// <summary>
		/// Apre un task per eseguire una funzione
		/// </summary>
		/// <param name="f">funzione da eseguire</param>
		void EseguiComando(FuncBkgnd f)
		{
			cts = new CancellationTokenSource();
			token = cts.Token;
			btStop.Enabled = true;
			EnableTaskCtrl(false);
			Task<bool> task = Task<bool>.Factory.StartNew(() => Esegui(f,token,cts),token);
			task.ContinueWith(AfterTask);
		}

		/// <summary>
		/// Abilita o disabilita un controllo sul Form
		/// </summary>
		/// <param name="enabled"></param>
		void EnableTaskCtrl(bool enabled)
		{
			if(taskCtrl != null)
			{
				taskCtrl.Enabled = enabled;
			}
		}

		/// <summary>
		/// Operazioni eseguite al termien del task
		/// </summary>
		/// <param name="t"></param>
		void AfterTask(Task<bool> t)
		{
			bool s = t.Result;
			btStop.BeginInvoke(new Action(() => btStop.Enabled = false));
			running = false;

			string msg;
			if(s)
			{
				msg = "Operazione completata\r\n";
			}
			else
			{
				msg = "\r\nOperazione fallita o interrotta\r\n";
			}

			tbMsg.BeginInvoke(new Action(() => AddMessage(msg)));
			this.BeginInvoke(new Action(() => EnableTaskCtrl(true)));

			if(closeRequest)
			{
				tbMsg.BeginInvoke(new Action(() => AddMessage("Task arrestato, chiusura programma...")));
				Thread.Sleep(2000);
				this.BeginInvoke(new Action(() => Close()));
			}
		}

		/// <summary>
		/// Richiede l'arresto del tak
		/// </summary>
		void StopTask()
		{
			if(!token.IsCancellationRequested)
			{
				if(cts != null)
				{
					cts.Cancel();
				}
			}
		}

		/// <summary>
		/// Carattere per animazione
		/// </summary>
		/// <returns></returns>
		public char GetRotChar()
		{
			ich++;
			if(ich >= ch.Length) ich = 0;
			return ch[ich];
		}

		bool AnalyseArgs(string[] args)
		{
			bool ok = true;
			CMD cmd = CMD.None;
			for(int i = 0;i < args.Length;i++)
			{
				string s = args[i];
				switch(s)
				{
					case CMD_USR:
						{
							cmd = CMD.USR;
						}
						break;
					case CMD_CFG:
						{
							cmd = CMD.CFG;
						}
						break;
					case CMD_LST:
						{
							cmd = CMD.LST;
						}
						break;
					case CMD_ALL:
						{
							par.fls = FLS.ALL;                      // Legge tutti i file
							par.lstFile = string.Empty;
							cmd = CMD.None;
						}
						break;
					case CMD_NFL:
						{
							par.noFilterLst = true;
							cmd = CMD.None;
						}
						break;
					default:
						{
							switch(cmd)
							{
								case CMD.USR:
									{
										par.usrName = s;
									}
									break;
								case CMD.CFG:
									{
										par.cfgFile = s;
									}
									break;
								case CMD.LST:
									{
										if(par.fls != FLS.ALL)              // Se non c'é l'opzione -all...
										{
											if(s == "*")
											{
												par.fls = FLS.ALL_LST;      // Legge tutti i file di lista
											}
											else
											{
												par.lstFile = s;
												par.fls = FLS.LST;          // Legge solo il file di lista specificato
											}
										}
									}
									break;
								default:
									{
										cmd = CMD.None;
									}
									break;
							}
						}
						break;
				}
			}

			return ok;
		}

		public static string Version()
		{
			StringBuilder strb = new StringBuilder();
			strb.AppendLine("Informazioni sul programma" + System.Environment.NewLine);
			try
			{
				//FileInfo fi = new FileInfo(Assembly.GetExecutingAssembly().Location);
				strb.AppendLine("Versione: " + Assembly.GetEntryAssembly().GetName().Version.ToString());
				// strb.AppendLine("FullName: " + Assembly.GetEntryAssembly().GetName().FullName.ToString() + System.Environment.NewLine );
				strb.AppendLine("Assembly name: " + Assembly.GetEntryAssembly().GetName().Name.ToString());
				strb.AppendLine("Product name: " + Application.ProductName);
				// strb.AppendLine("ProductVersion: " + Application.ProductVersion);
				// strb.AppendLine("Startup path: " + Application.StartupPath);
				strb.AppendLine("Copyright: " + Application.CompanyName);
				strb.AppendLine("Build: " + Build());
				strb.AppendLine("Executable path: " + Application.ExecutablePath);
				// strb.AppendLine("Eseguibile: " + fi.FullName);
			}
			catch
			{
				MessageBox.Show("Errore in Version()");
			}
			return strb.ToString();
		}

		public static string Build()
		{
			StringBuilder strb = new StringBuilder();
			try
			{
				string[] d_t;
				string[] dd;
				string[] hh;

				d_t = (Resource.BDT).Trim().Split(' ',StringSplitOptions.RemoveEmptyEntries);

				int n = d_t.Length;
				if(n == 2)
				{
					dd = d_t[0].Split('/',StringSplitOptions.RemoveEmptyEntries);
					hh = d_t[1].Split(new char[] { ':',',' },StringSplitOptions.RemoveEmptyEntries);
					if((dd.Length == 3) && (hh.Length == 4))
					{
						strb.Append($"{dd[2]}.{dd[1]}.{dd[0]}.");
						strb.Append($"{hh[0]}.{hh[1]}.{hh[2]}.{hh[3]}");
					}
					else
					{
						throw new Exception($"Numero di elementi {dd.Length} o {hh.Length} errato.");
					}
				}
				else
				{
					throw new Exception($"Numero {n} di elementi in Resources.DT_txt {Resource.BDT} errato.");
				}

			}
			catch(Exception ex)
			{
				MessageBox.Show($"Errore in Build(): {ex.Message}");
			}
			return strb.ToString();
		}

		#region HANDLERS

		private void refreshTimer_Tick(object sender,EventArgs e)
		{
			toolStripStatusLabel1.Text = GetRotChar().ToString() + (running ? " ...elaborazione..." : "");
		}

		private void btStop_Click(object sender,EventArgs e)
		{
			StopTask();
		}

		private void Form1_FormClosing(object sender,FormClosingEventArgs e)
		{
			if(!closeRequest)   // Chiede conferma, se non c'è già una richiesta di chiusura
			{
				if(MessageBox.Show("Uscire ?","Confermare chiusura programma",MessageBoxButtons.OKCancel) != DialogResult.OK)
				{
					e.Cancel = true;
				}
			}

			if(!e.Cancel)       // Se chiusura confermata...
			{
				if(running)     // Se task in corso...
				{
					e.Cancel = true;            // Annulla chiusura
					closeRequest = true;        // Richiede chiusura al termine del task
					StopTask();                 // Richiede arresto del task
					AddMessage("\r\nArresto operazione in corso...\r\n");
				}
				else
				{               // ...se nessun task, prosegue con la chiusura
					refreshTimer.Stop();
					sf.ReleaseFiles();
				}
			}
		}

		private void btExit_Click(object sender,EventArgs e)
		{
			Close();
		}

		private void btRead_Click(object sender,EventArgs e)
		{
			if(!running)
			{
				taskCtrl = gbComandi;
				EseguiComando(sf.ReadFile);
			}
			else
			{
				MessageBox.Show("Operazione in corso");
			}
		}

		private void btWrite_Click(object sender,EventArgs e)
		{
			if(!running)
			{
				taskCtrl = gbComandi;
				EseguiComando(sf.WriteFile);
			}
			else
			{
				MessageBox.Show("Operazione in corso");
			}
		}

		private void btReadWrite_Click(object sender,EventArgs e)
		{
			if(!running)
			{
				taskCtrl = gbComandi;
				EseguiComando(sf.ReadWriteFile);
			}
			else
			{
				MessageBox.Show("Operazione in corso");
			}
		}

		private void btClearLog_Click(object sender,EventArgs e)
		{
			if(MessageBox.Show("Cancellare il file di log ?","Cancellazione log",MessageBoxButtons.OKCancel) == DialogResult.OK)
			{
				sf.ClearLog();
			}
		}



		private void Form1_HelpButtonClicked(object sender,System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = true;
			MessageBox.Show(Version());
		}

		#endregion

		private void btTest_Click(object sender,EventArgs e)
		{
			string txt = tbTest.Text;
			bool x;
			StringBuilder sb = new StringBuilder();

			x = sf.FilterExt(in txt);
			sb.AppendLine("Extension: " + (x ? "yes" : "no"));
			
			x = sf.FilterMatch(in txt);
			sb.AppendLine("Match: " + (x ? "yes" : "no"));

			MessageBox.Show(sb.ToString());	
		}
	}
}
