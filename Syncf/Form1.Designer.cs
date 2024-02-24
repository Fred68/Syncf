namespace Syncf
	{
	partial class Form1
		{
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
			{
			if(disposing && (components != null))
				{
				components.Dispose();
				}
			base.Dispose(disposing);
			}

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			btRead = new Button();
			btWrite = new Button();
			btReadWrite = new Button();
			btExit = new Button();
			groupBox1 = new GroupBox();
			btStop = new Button();
			tbMsg = new TextBox();
			panel1 = new Panel();
			statusStrip1 = new StatusStrip();
			toolStripStatusLabel1 = new ToolStripStatusLabel();
			refreshTimer = new System.Windows.Forms.Timer(components);
			groupBox1.SuspendLayout();
			panel1.SuspendLayout();
			statusStrip1.SuspendLayout();
			SuspendLayout();
			// 
			// btRead
			// 
			btRead.Location = new Point(20,30);
			btRead.Name = "btRead";
			btRead.Size = new Size(110,27);
			btRead.TabIndex = 1;
			btRead.Text = "Leggi";
			btRead.UseVisualStyleBackColor = true;
			btRead.Click += btRead_Click;
			// 
			// btWrite
			// 
			btWrite.Location = new Point(20,63);
			btWrite.Name = "btWrite";
			btWrite.Size = new Size(110,27);
			btWrite.TabIndex = 2;
			btWrite.Text = "Scrivi";
			btWrite.UseVisualStyleBackColor = true;
			// 
			// btReadWrite
			// 
			btReadWrite.Location = new Point(20,96);
			btReadWrite.Name = "btReadWrite";
			btReadWrite.Size = new Size(110,27);
			btReadWrite.TabIndex = 3;
			btReadWrite.Text = "Leggi e scrivi";
			btReadWrite.UseVisualStyleBackColor = true;
			// 
			// btExit
			// 
			btExit.Location = new Point(32,312);
			btExit.Name = "btExit";
			btExit.Size = new Size(110,27);
			btExit.TabIndex = 4;
			btExit.Text = "Esci";
			btExit.UseVisualStyleBackColor = true;
			btExit.Click += btExit_Click;
			// 
			// groupBox1
			// 
			groupBox1.Controls.Add(btReadWrite);
			groupBox1.Controls.Add(btWrite);
			groupBox1.Controls.Add(btRead);
			groupBox1.Location = new Point(12,12);
			groupBox1.Name = "groupBox1";
			groupBox1.Size = new Size(149,142);
			groupBox1.TabIndex = 5;
			groupBox1.TabStop = false;
			groupBox1.Text = "Comandi";
			// 
			// btStop
			// 
			btStop.Location = new Point(32,160);
			btStop.Name = "btStop";
			btStop.Size = new Size(110,27);
			btStop.TabIndex = 6;
			btStop.Text = "Stop";
			btStop.UseVisualStyleBackColor = true;
			btStop.Click += btStop_Click;
			// 
			// tbMsg
			// 
			tbMsg.BorderStyle = BorderStyle.FixedSingle;
			tbMsg.Dock = DockStyle.Right;
			tbMsg.Location = new Point(181,0);
			tbMsg.Multiline = true;
			tbMsg.Name = "tbMsg";
			tbMsg.ReadOnly = true;
			tbMsg.ScrollBars = ScrollBars.Vertical;
			tbMsg.Size = new Size(331,351);
			tbMsg.TabIndex = 0;
			// 
			// panel1
			// 
			panel1.Controls.Add(tbMsg);
			panel1.Controls.Add(groupBox1);
			panel1.Controls.Add(btExit);
			panel1.Controls.Add(btStop);
			panel1.Dock = DockStyle.Top;
			panel1.Location = new Point(0,0);
			panel1.Name = "panel1";
			panel1.Size = new Size(512,351);
			panel1.TabIndex = 7;
			// 
			// statusStrip1
			// 
			statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1 });
			statusStrip1.Location = new Point(0,372);
			statusStrip1.Name = "statusStrip1";
			statusStrip1.Size = new Size(512,22);
			statusStrip1.TabIndex = 8;
			statusStrip1.Text = "statusStrip1";
			// 
			// toolStripStatusLabel1
			// 
			toolStripStatusLabel1.Name = "toolStripStatusLabel1";
			toolStripStatusLabel1.Size = new Size(118,17);
			toolStripStatusLabel1.Text = "toolStripStatusLabel1";
			// 
			// refreshTimer
			// 
			refreshTimer.Tick += refreshTimer_Tick;
			// 
			// Form1
			// 
			AutoScaleDimensions = new SizeF(7F,15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(512,394);
			Controls.Add(statusStrip1);
			Controls.Add(panel1);
			Name = "Form1";
			Text = "Form1";
			FormClosing += Form1_FormClosing;
			groupBox1.ResumeLayout(false);
			panel1.ResumeLayout(false);
			panel1.PerformLayout();
			statusStrip1.ResumeLayout(false);
			statusStrip1.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private Button btRead;
		private Button btWrite;
		private Button btReadWrite;
		private Button btExit;
		private GroupBox groupBox1;
		private Button btStop;
		private TextBox tbMsg;
		private Panel panel1;
		private StatusStrip statusStrip1;
		private ToolStripStatusLabel toolStripStatusLabel1;
		private System.Windows.Forms.Timer refreshTimer;
	}
	}
