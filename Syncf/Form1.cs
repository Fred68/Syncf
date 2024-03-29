using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;

using Fred68.CfgReader;
using static Fred68.CfgReader.CfgReader;

namespace Syncf
{

	public partial class Form1:Form
	{
		SyncFile sf;
		SyncfParams par;

		static CancellationTokenSource? cts = null;
		CancellationToken token = CancellationToken.None;
		List<Control> taskCtrl;
		List<ToolStripMenuItem> taskMenuitem;

		bool running = false;
		bool closeRequest = false;

		char[] ch = { '|','/','-','\\' };
		int ich = 0;
		string[] arguments;

		const string CMD_USR = "-usr";
		const string CMD_CFG = "-cfg";
		const string CMD_LST = "-lst";
		const string CMD_ALL = "-all";
		const string CMD_FLS = "-filterLst";
		const string CMD_NWR = "-noWrite";

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

			CrtlWithTask(out taskCtrl,out taskMenuitem);

			arguments = args;
			AnalyseArgs(arguments, ref par);
			par.fmsg = AddMessageAltTask;
			// Spostato sf = new SyncFile(par) in Form1_Load(), dopo creazione della finestra principale

			statusStrip1.MinimumSize = new System.Drawing.Size(0,30);
			toolStripStatusLabel1.Text = new string('-',80);
			
			this.Text = "Syncf";
			btStop.Visible = true;
			btStop.Enabled = false;

			this.MinimumSize = this.Size;

			panel1.MinimumSize = panel1.Size;
			panel1.AutoSize = false;
			panel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			
			rtbMsg.MinimumSize = rtbMsg.Size;
			rtbMsg.AutoSize = false;
			//rtbMsg.AutoSizeMode = AutoSizeMode.GrowAndShrink;

			ResumeLayout(false);
			PerformLayout();
		}

		/// <summary>
		/// Abilita o disabilita i controlli interessati dai task
		/// </summary>
		/// <param name="lc"></param>
		/// <param name="lm"></param>
		void CrtlWithTask(out List<Control> lc,out List<ToolStripMenuItem> lm)
		{
			List<Control> lstc = new List<Control>();
			lstc.Add(gbComandi);
			lstc.Add(gbLog);

			List<ToolStripMenuItem> lstm = new List<ToolStripMenuItem>();
			lstm.Add(leggiToolStripMenuItem);
			lstm.Add(scriviToolStripMenuItem);
			lstm.Add(leggiEScriviToolStripMenuItem);
			lstm.Add(vediListaToolStripMenuItem);
			lstm.Add(eliminaDuplicatiToolStripMenuItem);
			lstm.Add(cancellaListaToolStripMenuItem);
			lstm.Add(vediLogToolStripMenuItem);
			lstm.Add(cancellaLogToolStripMenuItem);

			lc = lstc;
			lm = lstm;
		}

		/// <summary>
		/// OnLoad...
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Form1_Load(object sender,EventArgs e)
		{
			SuspendLayout();
			sf = new SyncFile(ref par);
			rtbMsg.AppendText("\r\n");
			rtbMsg.AppendText("args[]=");
			for(int i = 1;i < arguments.Length;i++)
			{
				rtbMsg.AppendText($" {arguments[i]}");
			}
			rtbMsg.AppendText("\r\n");
			string msg = sf.MsgConfiguration;
			rtbMsg.AppendText(msg);          //MessageBox.Show(msg);
			rtbMsg.SelectionLength = 0;      // Deseleziona

#if DEBUG
			eliminaDuplicatiToolStripMenuItem.Visible = true;
#endif
			refreshTimer.Interval = 300;
			refreshTimer.Start();

			ResumeLayout(false);
			PerformLayout();

			EnableTaskCtrls(sf.IsEnabled);

			if(sf.IsEnabled)
			{
				FuncBkgnd? f = sf.SetStartFunction();
				if(f != null)
				{
					//taskCtrl = gbComandi;
					EseguiComando(f);
				}
			}
			else
			{
				MessageBox.Show($"Errori nella configurazione.\r\nIl programma non pu� essere eseguito");
				btRead.Enabled = btWrite.Enabled = btReadWrite.Enabled = btStop.Enabled = btClearLog.Enabled = false;
				refreshTimer.Stop();
			}

		}

		/// <summary>
		/// Aggiunge messaggio alla finestra
		/// '\n' esegue scroll
		/// </summary>
		/// <param name="msg"></param>
		private void AddMessage(string msg,MSG typ = MSG.Message,int newlines = 1)
		{
			int start = rtbMsg.TextLength;
			rtbMsg.AppendText(msg);
			if(newlines > 0) rtbMsg.AppendText(new string('\n',newlines));
			rtbMsg.Select(start,msg.Length);
			rtbMsg.SelectionColor = sf.txtCol[(int)typ];

			if(newlines > 0)    // Era: msg.Contains('\n')
			{
				rtbMsg.SelectionStart = rtbMsg.Text.Length;
				rtbMsg.ScrollToCaret();
			}

			rtbMsg.Invalidate();
		}

		/// <summary>
		/// Aggiunge messaggio alla finestra...
		/// ...chiamata da task differente dal quello principale (UI)
		/// </summary>
		/// <param name="msg"></param>
		public void AddMessageAltTask(string msg,MSG typ,int newlines)
		{
			rtbMsg.BeginInvoke(new Action(() => AddMessage(msg,typ,newlines)));
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
			EnableTaskCtrls(false);
			Task<bool> task = Task<bool>.Factory.StartNew(() => Esegui(f,token,cts),token);
			task.ContinueWith(AfterTask);
		}

		/// <summary>
		/// Abilita o disabilita un controllo sul Form
		/// </summary>
		/// <param name="enabled"></param>
		void EnableTaskCtrls(bool enabled)
		{
			foreach(Control c in taskCtrl)
			{
				c.Enabled = enabled;
			}
			foreach(ToolStripMenuItem c in taskMenuitem)
			{
				c.Enabled = enabled;
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
				msg = "Operazione completata";
			}
			else
			{
				msg = "Operazione fallita o interrotta";
			}

			rtbMsg.BeginInvoke(new Action(() => AddMessage(msg,MSG.Warning,1)));
			this.BeginInvoke(new Action(() => EnableTaskCtrls(true)));

			if(closeRequest)
			{
				rtbMsg.BeginInvoke(new Action(() => AddMessage("Task arrestato, chiusura programma...",MSG.Error,1)));
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

		/// <summary>
		/// Interpreta gli argomenti della chiamata
		/// </summary>
		/// <param name="args"></param>
		/// <param name="par"></param>
		/// <returns></returns>
		bool AnalyseArgs(string[] args, ref SyncfParams par)
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
					case CMD_FLS:
						{
							par.filterLst = true;
							cmd = CMD.None;
						}
						break;
					case CMD_NWR:
						{
							par.noWrite = true;
							cmd = CMD.None;
						}
						break;

					default:
						{
							switch(cmd)
							{
								case CMD.USR:                       // Utente non standard
									{
										par.usrName = s;
									}
									break;
								case CMD.CFG:
									{
										par.cfgFile = s;            // Configurazione non standard
									}
									break;
								case CMD.LST:                       // File con lista
									{
										if(par.fls != FLS.ALL)      // Se non c'� l'opzione -all...
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
				Assembly? assembly = Assembly.GetExecutingAssembly();
				if(assembly != null)
				{
					//FileInfo fi = new FileInfo(Assembly.GetExecutingAssembly().Location);
					System.Version? v = assembly.GetName().Version;
					if(v != null) strb.AppendLine("Versione: " + v.ToString());
					string? n = assembly.GetName().Name;
					if(n != null) strb.AppendLine("Assembly name: " + n);
				}
				// strb.AppendLine("FullName: " + Assembly.GetEntryAssembly().GetName().FullName.ToString() + System.Environment.NewLine );
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

		#region EVENT HANDLERS

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
			if(!closeRequest)   // Chiede conferma, se non c'� gi� una richiesta di chiusura
			{
				string msg = (running ? "Operazione in corso...\r\n\r\n" : "") + "Uscire ?";
				if(MessageBox.Show(msg,"Confermare chiusura programma",MessageBoxButtons.OKCancel) != DialogResult.OK)
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
					AddMessage("\r\nArresto operazione in corso...\r\n",MSG.Error,1);
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
				if(!running)
				{
					sf.ClearLog();
				}
			}
		}

		private void Form1_HelpButtonClicked(object sender,System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = true;
			MessageBox.Show(Version());
		}

		private void btLogFolder_Click(object sender,EventArgs e)
		{
			if(Directory.Exists(sf.LogPath))
			{
				Process.Start("explorer.exe",sf.LogPath);
			}
			else
			{
				MessageBox.Show($"Cartella di log '{sf.LogPath}' non trovata.");
			}
		}

		private void btViewTodo_Click(object sender,EventArgs e)
		{
			if(!running)
			{
				string s = sf.TodoFile;
				if(File.Exists(s))
				{
					if(sf.GetExt(in s) == ".txt")
					{
						Process.Start("explorer.exe",sf.TodoFile);
					}
				}
			}

		}

		private void btClearTodo_Click(object sender,EventArgs e)
		{
			if(!running)
			{
				if(File.Exists(sf.TodoFile))
				{
					if(MessageBox.Show($"Cancellare la lista dei file da copiare:\r\n{sf.TodoFile} ?","Cancellazione lista",MessageBoxButtons.OKCancel) == DialogResult.OK)
					{
						try
						{
							File.Delete(sf.TodoFile);
							sf.Log($"Cancellata lista: '{sf.TodoFile}'");
						}
						catch(Exception ex)
						{
							MessageBox.Show($"Errore nella cancellazione della lista '{sf.LogPath}'\r\n{ex.Message}");
						}

					}
				}
			}
		}

		private void aboutToolStripMenuItem_Click(object sender,EventArgs e)
		{
			MessageBox.Show(Version());
		}


		private void eliminaDuplicatiToolStripMenuItem_Click(object sender,EventArgs e)
		{
			if(!running)
			{
				EseguiComando(sf.RemoveTodoDuplicates);
			}
			else
			{
				MessageBox.Show("Operazione in corso");
			}
		}

		private void vediLogToolStripMenuItem_Click(object sender,EventArgs e)
		{
			if(!running)
			{
				string s = sf.LogFile;
				if(File.Exists(s))
				{
					if(sf.GetExt(in s) == ".txt")
					{
						Process.Start("explorer.exe",sf.LogFile);
					}
				}
			}
		}

		#endregion

		private void Form1_ResizeEnd(object sender,EventArgs e)
		{
			//sf.Log("Resized");
			int dx = this.Size.Width - this.MinimumSize.Width;
			int dy = this.Size.Height - this.MinimumSize.Height;
			Size incsz = new Size(dx, dy);
			panel1.Size = panel1.MinimumSize + incsz;
			rtbMsg.Size = rtbMsg.MinimumSize + incsz;
		}
	}
}
