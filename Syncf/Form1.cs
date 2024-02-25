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

		/// <summary>
		/// Ctor
		/// </summary>
		public Form1()
		{
			InitializeComponent();
			SuspendLayout();
			sf = new SyncFile(AddMessageAltTask);
			statusStrip1.MinimumSize = new System.Drawing.Size(0,30);
			toolStripStatusLabel1.Text = new string('-',80);
			this.MinimumSize = this.Size;
			this.Text = "Syncf";
			btStop.Visible = true;
			btStop.Enabled = false;
			ResumeLayout(false);
			PerformLayout();
		}

		private void Form1_Load(object sender,EventArgs e)
		{
			SuspendLayout();

			tbMsg.AppendText("\r\nMESSAGGI:\r\n");
			string msg = sf.CfgMsgs();
			tbMsg.AppendText(msg);			//MessageBox.Show(msg);
			tbMsg.SelectionLength = 0;      // Deseleziona

			refreshTimer.Interval = 300;
			refreshTimer.Start();

			ResumeLayout(false);
			PerformLayout();


			if(sf.isEnabled)
			{
				FuncBkgnd? f = sf.StartFunc();
				if(f!=null)
				{
					taskCtrl = gbComandi;
					EseguiComando(f);
				}
				
				// EseguiComando(sf.ReadWriteFile);
			}
			else
			{
				MessageBox.Show($"Errori nella configurazione.\r\nIl programma non può essere eseguito");
				btRead.Enabled = btWrite.Enabled = btReadWrite.Enabled = btStop.Enabled = false;
				refreshTimer.Stop();
			}

		}

		private void AddMessage(string msg)
		{
			tbMsg.AppendText(msg);
			tbMsg.Invalidate();
		}

		public void AddMessageAltTask(string msg)
		{
			tbMsg.BeginInvoke(new Action(() => AddMessage(msg)));
		}

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
				ok = f(token);                      // Esegue le operazioni

			}

			return ok;
		}

		void EseguiComando(FuncBkgnd f)
		{
			cts = new CancellationTokenSource();
			token = cts.Token;
			btStop.Enabled = true;
			EnableTaskCtrl(false);
			Task<bool> task = Task<bool>.Factory.StartNew(() => Esegui(f,token,cts),token);
			task.ContinueWith(AfterTask);
		}

		void EnableTaskCtrl(bool enabled)
		{
			if(taskCtrl != null)
			{
				taskCtrl.Enabled = enabled;
			}
		}

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
				this.BeginInvoke(new Action(() => Close()));
			}
		}

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


		public char GetRotChar()
		{
			ich++;
			if(ich >= ch.Length) ich = 0;
			return ch[ich];
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
			if(!running)
			{
				if(!closeRequest)
				{
					if(MessageBox.Show("Uscire ?","Confermare chiusura programma",MessageBoxButtons.OKCancel) != DialogResult.OK)
					{
						e.Cancel = true;
					}
				}
			}
			else
			{
				AddMessage("\r\nArresto operazione in corso...\r\n");
				e.Cancel = true;			// Annulla chiusura
				closeRequest = true;		// Richiede chiusura al termine del task
				StopTask();					// Richiede arresto del task
			}

			if(!e.Cancel)		// Chiusura confermata
			{
				refreshTimer.Stop();
				sf.ReleaseBusy();
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
				//taskCtrl = btRead;
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
				//taskCtrl = btWrite;
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
				//taskCtrl = btReadWrite;
				taskCtrl = gbComandi;
				EseguiComando(sf.ReadWriteFile);
			}
			else
			{
				MessageBox.Show("Operazione in corso");
			}
		}

		#endregion

		
	}
}
