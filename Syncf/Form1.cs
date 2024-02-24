using System;
using System.Collections.Generic;
using System.Text;

using Fred68.CfgReader;
using static Fred68.CfgReader.CfgReader;

namespace Syncf
{
	public partial class Form1:Form
	{
		delegate bool FuncBkgnd(CancellationToken tk);

		SyncFile sf;

		static CancellationTokenSource? cts = null;
		CancellationToken token = CancellationToken.None;

		bool running = false;

		char[] ch = { '|','/','-','\\' };
		int ich = 0;

		public Form1()
		{
			InitializeComponent();
			SuspendLayout();

			tbMsg.AppendText("MESSAGGI:\r\n\r\n");
			sf = new SyncFile();
			tbMsg.AppendText(sf.CfgMsgs());
			tbMsg.SelectionLength = 0;      // Deseleziona
			statusStrip1.MinimumSize = new System.Drawing.Size(0,30);
			toolStripStatusLabel1.Text = new string('-',80);
			this.MinimumSize = this.Size;
			this.Text = "Syncf";
			btStop.Visible = true;
			btStop.Enabled = false;

			refreshTimer.Interval = 200;
			refreshTimer.Start();

			ResumeLayout(false);
			PerformLayout();


			if(!sf.isEnabled)
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

		bool Esegui(FuncBkgnd f,CancellationToken token,CancellationTokenSource ctsrc)
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

		private void Form1_FormClosing(object sender,FormClosingEventArgs e)
		{
			if(!running)
			{
				if(MessageBox.Show("Uscire ?","Confermare chiusura programma",MessageBoxButtons.OKCancel) != DialogResult.OK)
				{
					e.Cancel = true;
				}
			}
			else
			{
				MessageBox.Show("Operazione in corso...");
				e.Cancel = true;
			}
		if(!e.Cancel)
			{
			refreshTimer.Stop();
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
				cts = new CancellationTokenSource();
				token = cts.Token;
				btStop.Enabled = true;
				Task<bool> task = Task<bool>.Factory.StartNew(() => Esegui(sf.ReadFile,token,cts),token);
				task.ContinueWith(ShowResult);
			}
			else
			{
				MessageBox.Show("Operazione in corso");
			}
		}

		void ShowResult(Task<bool> t)
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
				msg = "Operazione fallita o interrotta\r\n";
			}

			tbMsg.BeginInvoke(new Action(() => AddMessage(msg)));
		}

		private void btStop_Click(object sender,EventArgs e)
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
			if(ich>=ch.Length) ich=0;
			return ch[ich];
			}

		private void refreshTimer_Tick(object sender,EventArgs e)
		{
			toolStripStatusLabel1.Text = GetRotChar().ToString();
		}
	}
}
