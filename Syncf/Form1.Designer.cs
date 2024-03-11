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
			gbComandi = new GroupBox();
			btClearLog = new Button();
			btStop = new Button();
			rtbMsg = new RichTextBox();
			panel1 = new Panel();
			btLogFolder = new Button();
			btTest = new Button();
			tbTest = new TextBox();
			statusStrip1 = new StatusStrip();
			toolStripStatusLabel1 = new ToolStripStatusLabel();
			refreshTimer = new System.Windows.Forms.Timer(components);
			gbComandi.SuspendLayout();
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
			btWrite.Click += btWrite_Click;
			// 
			// btReadWrite
			// 
			btReadWrite.Location = new Point(20,96);
			btReadWrite.Name = "btReadWrite";
			btReadWrite.Size = new Size(110,27);
			btReadWrite.TabIndex = 3;
			btReadWrite.Text = "Leggi e scrivi";
			btReadWrite.UseVisualStyleBackColor = true;
			btReadWrite.Click += btReadWrite_Click;
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
			// gbComandi
			// 
			gbComandi.Controls.Add(btClearLog);
			gbComandi.Controls.Add(btReadWrite);
			gbComandi.Controls.Add(btWrite);
			gbComandi.Controls.Add(btRead);
			gbComandi.Location = new Point(12,12);
			gbComandi.Name = "gbComandi";
			gbComandi.Size = new Size(149,170);
			gbComandi.TabIndex = 5;
			gbComandi.TabStop = false;
			gbComandi.Text = "Comandi";
			// 
			// btClearLog
			// 
			btClearLog.Location = new Point(20,129);
			btClearLog.Name = "btClearLog";
			btClearLog.Size = new Size(110,27);
			btClearLog.TabIndex = 4;
			btClearLog.Text = "Cancella log";
			btClearLog.UseVisualStyleBackColor = true;
			btClearLog.Click += btClearLog_Click;
			// 
			// btStop
			// 
			btStop.Location = new Point(32,188);
			btStop.Name = "btStop";
			btStop.Size = new Size(110,27);
			btStop.TabIndex = 6;
			btStop.Text = "Stop";
			btStop.UseVisualStyleBackColor = true;
			btStop.Click += btStop_Click;
			// 
			// rtbMsg
			// 
			rtbMsg.BackColor = SystemColors.Control;
			rtbMsg.BorderStyle = BorderStyle.FixedSingle;
			rtbMsg.Dock = DockStyle.Right;
			rtbMsg.Font = new Font("Segoe UI",8.25F,FontStyle.Regular,GraphicsUnit.Point,0);
			rtbMsg.Location = new Point(167,0);
			rtbMsg.Name = "rtbMsg";
			rtbMsg.ScrollBars = RichTextBoxScrollBars.Vertical;
			rtbMsg.Size = new Size(295,351);
			rtbMsg.TabIndex = 9;
			rtbMsg.Text = "";
			// 
			// panel1
			// 
			panel1.Controls.Add(btLogFolder);
			panel1.Controls.Add(rtbMsg);
			panel1.Controls.Add(btTest);
			panel1.Controls.Add(gbComandi);
			panel1.Controls.Add(btExit);
			panel1.Controls.Add(btStop);
			panel1.Dock = DockStyle.Top;
			panel1.Location = new Point(0,0);
			panel1.Name = "panel1";
			panel1.Size = new Size(462,351);
			panel1.TabIndex = 7;
			// 
			// btLogFolder
			// 
			btLogFolder.Location = new Point(32,230);
			btLogFolder.Name = "btLogFolder";
			btLogFolder.Size = new Size(110,27);
			btLogFolder.TabIndex = 10;
			btLogFolder.Text = "Cartella di log";
			btLogFolder.UseVisualStyleBackColor = true;
			btLogFolder.Click += btLogFolder_Click;
			// 
			// btTest
			// 
			btTest.Location = new Point(32,260);
			btTest.Name = "btTest";
			btTest.Size = new Size(110,27);
			btTest.TabIndex = 8;
			btTest.Text = "Test";
			btTest.UseVisualStyleBackColor = true;
			btTest.Click += btTest_Click;
			// 
			// tbTest
			// 
			tbTest.Location = new Point(0,357);
			tbTest.Name = "tbTest";
			tbTest.Size = new Size(462,23);
			tbTest.TabIndex = 7;
			// 
			// statusStrip1
			// 
			statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1 });
			statusStrip1.Location = new Point(0,382);
			statusStrip1.Name = "statusStrip1";
			statusStrip1.Size = new Size(462,22);
			statusStrip1.TabIndex = 8;
			statusStrip1.Text = "statusStrip1";
			// 
			// toolStripStatusLabel1
			// 
			toolStripStatusLabel1.Font = new Font("Courier New",9F,FontStyle.Regular,GraphicsUnit.Point,0);
			toolStripStatusLabel1.Name = "toolStripStatusLabel1";
			toolStripStatusLabel1.Size = new Size(154,17);
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
			ClientSize = new Size(462,404);
			Controls.Add(statusStrip1);
			Controls.Add(tbTest);
			Controls.Add(panel1);
			HelpButton = true;
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "Form1";
			HelpButtonClicked += Form1_HelpButtonClicked;
			FormClosing += Form1_FormClosing;
			Load += Form1_Load;
			gbComandi.ResumeLayout(false);
			panel1.ResumeLayout(false);
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
		private GroupBox gbComandi;
		private Button btStop;
		private Panel panel1;
		private StatusStrip statusStrip1;
		private ToolStripStatusLabel toolStripStatusLabel1;
		private System.Windows.Forms.Timer refreshTimer;
		private Button btClearLog;
		private Button btTest;
		private TextBox tbTest;
		private RichTextBox rtbMsg;
		private Button btLogFolder;
	}
	}
