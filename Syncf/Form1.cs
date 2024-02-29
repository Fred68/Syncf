using System;
using System.Collections.Generic;
using System.DirectoryServices;
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
		enum CMD { USR, CFG, None };
		string usrName, cfgFile;

		/// <summary>
		/// Ctor
		/// </summary>
		public Form1(string[] args)
		{
			InitializeComponent();
			SuspendLayout();
			usrName = cfgFile = string.Empty;
			arguments = args;
			AnalyseArgs(args);
			sf = new SyncFile(AddMessageAltTask,usrName,cfgFile);
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

			tbMsg.AppendText("\r\nMESSAGGI:\r\n");
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
					default:
						{
							switch(cmd)
							{
								case CMD.USR:
									{
										usrName = s;
									}
									break;
								case CMD.CFG:
									{
										cfgFile = s;
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


		#region HANDLERS

		private void refreshTimer_Tick(object sender,EventArgs e)
		{
			toolStripStatusLabel1.Text = GetRotChar().ToString();
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
			MessageBox.Show("Help");
		}

		#endregion
	}
}
