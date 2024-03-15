﻿namespace Syncf
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
			toDisable = new GroupBox();
			groupBox1 = new GroupBox();
			btLogFolder = new Button();
			statusStrip1 = new StatusStrip();
			toolStripStatusLabel1 = new ToolStripStatusLabel();
			refreshTimer = new System.Windows.Forms.Timer(components);
			gbComandi.SuspendLayout();
			panel1.SuspendLayout();
			toDisable.SuspendLayout();
			groupBox1.SuspendLayout();
			statusStrip1.SuspendLayout();
			SuspendLayout();
			// 
			// btRead
			// 
			btRead.Location = new Point(20,22);
			btRead.Name = "btRead";
			btRead.Size = new Size(110,27);
			btRead.TabIndex = 1;
			btRead.Text = "Leggi";
			btRead.UseVisualStyleBackColor = true;
			btRead.Click += btRead_Click;
			// 
			// btWrite
			// 
			btWrite.Location = new Point(20,55);
			btWrite.Name = "btWrite";
			btWrite.Size = new Size(110,27);
			btWrite.TabIndex = 2;
			btWrite.Text = "Scrivi";
			btWrite.UseVisualStyleBackColor = true;
			btWrite.Click += btWrite_Click;
			// 
			// btReadWrite
			// 
			btReadWrite.Location = new Point(20,88);
			btReadWrite.Name = "btReadWrite";
			btReadWrite.Size = new Size(110,27);
			btReadWrite.TabIndex = 3;
			btReadWrite.Text = "Leggi e scrivi";
			btReadWrite.UseVisualStyleBackColor = true;
			btReadWrite.Click += btReadWrite_Click;
			// 
			// btExit
			// 
			btExit.Location = new Point(32,370);
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
			gbComandi.Location = new Point(0,9);
			gbComandi.Name = "gbComandi";
			gbComandi.Size = new Size(149,166);
			gbComandi.TabIndex = 5;
			gbComandi.TabStop = false;
			gbComandi.Text = "Comandi";
			// 
			// btClearLog
			// 
			btClearLog.Location = new Point(20,121);
			btClearLog.Name = "btClearLog";
			btClearLog.Size = new Size(110,27);
			btClearLog.TabIndex = 4;
			btClearLog.Text = "Cancella log";
			btClearLog.UseVisualStyleBackColor = true;
			btClearLog.Click += btClearLog_Click;
			// 
			// btStop
			// 
			btStop.Location = new Point(32,337);
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
			rtbMsg.BorderStyle = BorderStyle.None;
			rtbMsg.Font = new Font("Segoe UI",8.25F,FontStyle.Regular,GraphicsUnit.Point,0);
			rtbMsg.Location = new Point(246,17);
			rtbMsg.Name = "rtbMsg";
			rtbMsg.ScrollBars = RichTextBoxScrollBars.Vertical;
			rtbMsg.Size = new Size(295,347);
			rtbMsg.TabIndex = 9;
			rtbMsg.Text = "";
			// 
			// panel1
			// 
			panel1.Controls.Add(toDisable);
			panel1.Controls.Add(rtbMsg);
			panel1.Controls.Add(btExit);
			panel1.Controls.Add(btStop);
			panel1.Dock = DockStyle.Top;
			panel1.Location = new Point(0,0);
			panel1.Name = "panel1";
			panel1.Size = new Size(553,400);
			panel1.TabIndex = 7;
			// 
			// toDisable
			// 
			toDisable.Controls.Add(gbComandi);
			toDisable.Controls.Add(groupBox1);
			toDisable.FlatStyle = FlatStyle.Flat;
			toDisable.Location = new Point(3,3);
			toDisable.Margin = new Padding(0);
			toDisable.Name = "toDisable";
			toDisable.Size = new Size(158,309);
			toDisable.TabIndex = 12;
			toDisable.TabStop = false;
			// 
			// groupBox1
			// 
			groupBox1.Controls.Add(btLogFolder);
			groupBox1.Location = new Point(3,181);
			groupBox1.Name = "groupBox1";
			groupBox1.Size = new Size(149,100);
			groupBox1.TabIndex = 11;
			groupBox1.TabStop = false;
			groupBox1.Text = "Log";
			// 
			// btLogFolder
			// 
			btLogFolder.Location = new Point(20,22);
			btLogFolder.Name = "btLogFolder";
			btLogFolder.Size = new Size(110,27);
			btLogFolder.TabIndex = 10;
			btLogFolder.Text = "Cartella di log";
			btLogFolder.UseVisualStyleBackColor = true;
			btLogFolder.Click += btLogFolder_Click;
			// 
			// statusStrip1
			// 
			statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1 });
			statusStrip1.Location = new Point(0,433);
			statusStrip1.Name = "statusStrip1";
			statusStrip1.Size = new Size(553,22);
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
			ClientSize = new Size(553,455);
			Controls.Add(statusStrip1);
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
			toDisable.ResumeLayout(false);
			groupBox1.ResumeLayout(false);
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
		private RichTextBox rtbMsg;
		private Button btClearLog;
		private GroupBox groupBox1;
		private Button btLogFolder;
		private GroupBox toDisable;
	}
	}
