using System;
using System.Collections.Generic;
using System.Text;
using Fred68.CfgReader;
using static Fred68.CfgReader.CfgReader;

namespace Syncf
{
	public partial class Form1:Form
	{
		
		SyncFile sf;

		public Form1()
		{
			InitializeComponent();
			tbMsg.AppendText("MESSAGGI:\r\n\r\n");
			sf = new SyncFile();
			tbMsg.AppendText(sf.CfgMsgs());
			tbMsg.SelectionLength = 0;		// Deseleziona
			statusStrip1.MinimumSize=new System.Drawing.Size(0,30);
			toolStripStatusLabel1.Text = new string('-',80);
		}

		
	}
}
