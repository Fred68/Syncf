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
			btViewTodo = new Button();
			btClearLog = new Button();
			btStop = new Button();
			rtbMsg = new RichTextBox();
			panel1 = new Panel();
			gbLog = new GroupBox();
			btLogFolder = new Button();
			statusStrip1 = new StatusStrip();
			toolStripStatusLabel1 = new ToolStripStatusLabel();
			refreshTimer = new System.Windows.Forms.Timer(components);
			toolTip1 = new ToolTip(components);
			menuStrip1 = new MenuStrip();
			fileToolStripMenuItem = new ToolStripMenuItem();
			leggiToolStripMenuItem = new ToolStripMenuItem();
			scriviToolStripMenuItem = new ToolStripMenuItem();
			leggiEScriviToolStripMenuItem = new ToolStripMenuItem();
			esciToolStripMenuItem = new ToolStripMenuItem();
			listaToolStripMenuItem = new ToolStripMenuItem();
			vediListaToolStripMenuItem = new ToolStripMenuItem();
			eliminaDuplicatiToolStripMenuItem = new ToolStripMenuItem();
			cancellaListaToolStripMenuItem = new ToolStripMenuItem();
			logToolStripMenuItem = new ToolStripMenuItem();
			vediLogToolStripMenuItem = new ToolStripMenuItem();
			cancellaLogToolStripMenuItem = new ToolStripMenuItem();
			cartellaDiLogToolStripMenuItem = new ToolStripMenuItem();
			toolStripMenuItem1 = new ToolStripMenuItem();
			aboutToolStripMenuItem = new ToolStripMenuItem();
			gbComandi.SuspendLayout();
			panel1.SuspendLayout();
			gbLog.SuspendLayout();
			statusStrip1.SuspendLayout();
			menuStrip1.SuspendLayout();
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
			btExit.Location = new Point(32,354);
			btExit.Name = "btExit";
			btExit.Size = new Size(110,27);
			btExit.TabIndex = 4;
			btExit.Text = "Esci";
			btExit.UseVisualStyleBackColor = true;
			btExit.Click += btExit_Click;
			// 
			// gbComandi
			// 
			gbComandi.Controls.Add(btViewTodo);
			gbComandi.Controls.Add(btReadWrite);
			gbComandi.Controls.Add(btWrite);
			gbComandi.Controls.Add(btRead);
			gbComandi.Location = new Point(12,12);
			gbComandi.Name = "gbComandi";
			gbComandi.Size = new Size(149,160);
			gbComandi.TabIndex = 5;
			gbComandi.TabStop = false;
			gbComandi.Text = "Comandi";
			// 
			// btViewTodo
			// 
			btViewTodo.Location = new Point(20,121);
			btViewTodo.Name = "btViewTodo";
			btViewTodo.Size = new Size(110,27);
			btViewTodo.TabIndex = 4;
			btViewTodo.Text = "Vedi lista";
			btViewTodo.UseVisualStyleBackColor = true;
			btViewTodo.Click += btViewTodo_Click;
			// 
			// btClearLog
			// 
			btClearLog.Location = new Point(20,55);
			btClearLog.Name = "btClearLog";
			btClearLog.Size = new Size(110,27);
			btClearLog.TabIndex = 4;
			btClearLog.Text = "Vedi log";
			btClearLog.UseVisualStyleBackColor = true;
			btClearLog.Click += vediLogToolStripMenuItem_Click;
			// 
			// btStop
			// 
			btStop.Location = new Point(32,321);
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
			rtbMsg.Location = new Point(167,26);
			rtbMsg.Name = "rtbMsg";
			rtbMsg.ReadOnly = true;
			rtbMsg.ScrollBars = RichTextBoxScrollBars.Vertical;
			rtbMsg.Size = new Size(383,355);
			rtbMsg.TabIndex = 9;
			rtbMsg.Text = "";
			rtbMsg.WordWrap = false;
			// 
			// panel1
			// 
			panel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			panel1.BackColor = SystemColors.Control;
			panel1.Controls.Add(gbComandi);
			panel1.Controls.Add(gbLog);
			panel1.Controls.Add(rtbMsg);
			panel1.Controls.Add(btExit);
			panel1.Controls.Add(btStop);
			panel1.Location = new Point(0,25);
			panel1.Name = "panel1";
			panel1.Size = new Size(550,390);
			panel1.TabIndex = 7;
			// 
			// gbLog
			// 
			gbLog.Controls.Add(btClearLog);
			gbLog.Controls.Add(btLogFolder);
			gbLog.Location = new Point(12,178);
			gbLog.Name = "gbLog";
			gbLog.Size = new Size(149,100);
			gbLog.TabIndex = 11;
			gbLog.TabStop = false;
			gbLog.Text = "Log";
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
			statusStrip1.Location = new Point(0,427);
			statusStrip1.Name = "statusStrip1";
			statusStrip1.Size = new Size(571,22);
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
			// menuStrip1
			// 
			menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem,listaToolStripMenuItem,logToolStripMenuItem,toolStripMenuItem1 });
			menuStrip1.Location = new Point(0,0);
			menuStrip1.Name = "menuStrip1";
			menuStrip1.Size = new Size(571,24);
			menuStrip1.TabIndex = 9;
			menuStrip1.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { leggiToolStripMenuItem,scriviToolStripMenuItem,leggiEScriviToolStripMenuItem,esciToolStripMenuItem });
			fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			fileToolStripMenuItem.Size = new Size(37,20);
			fileToolStripMenuItem.Text = "File";
			// 
			// leggiToolStripMenuItem
			// 
			leggiToolStripMenuItem.Name = "leggiToolStripMenuItem";
			leggiToolStripMenuItem.Size = new Size(142,22);
			leggiToolStripMenuItem.Text = "Leggi";
			leggiToolStripMenuItem.Click += btRead_Click;
			// 
			// scriviToolStripMenuItem
			// 
			scriviToolStripMenuItem.Name = "scriviToolStripMenuItem";
			scriviToolStripMenuItem.Size = new Size(142,22);
			scriviToolStripMenuItem.Text = "Scrivi";
			scriviToolStripMenuItem.Click += btWrite_Click;
			// 
			// leggiEScriviToolStripMenuItem
			// 
			leggiEScriviToolStripMenuItem.Name = "leggiEScriviToolStripMenuItem";
			leggiEScriviToolStripMenuItem.Size = new Size(142,22);
			leggiEScriviToolStripMenuItem.Text = "Leggi e scrivi";
			leggiEScriviToolStripMenuItem.Click += btReadWrite_Click;
			// 
			// esciToolStripMenuItem
			// 
			esciToolStripMenuItem.Name = "esciToolStripMenuItem";
			esciToolStripMenuItem.Size = new Size(142,22);
			esciToolStripMenuItem.Text = "Esci";
			esciToolStripMenuItem.Click += btExit_Click;
			// 
			// listaToolStripMenuItem
			// 
			listaToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { vediListaToolStripMenuItem,eliminaDuplicatiToolStripMenuItem,cancellaListaToolStripMenuItem });
			listaToolStripMenuItem.Name = "listaToolStripMenuItem";
			listaToolStripMenuItem.Size = new Size(43,20);
			listaToolStripMenuItem.Text = "Lista";
			// 
			// vediListaToolStripMenuItem
			// 
			vediListaToolStripMenuItem.Name = "vediListaToolStripMenuItem";
			vediListaToolStripMenuItem.Size = new Size(162,22);
			vediListaToolStripMenuItem.Text = "Vedi lista";
			vediListaToolStripMenuItem.Click += btViewTodo_Click;
			// 
			// eliminaDuplicatiToolStripMenuItem
			// 
			eliminaDuplicatiToolStripMenuItem.Name = "eliminaDuplicatiToolStripMenuItem";
			eliminaDuplicatiToolStripMenuItem.Size = new Size(162,22);
			eliminaDuplicatiToolStripMenuItem.Text = "Elimina duplicati";
			eliminaDuplicatiToolStripMenuItem.Visible = false;
			eliminaDuplicatiToolStripMenuItem.Click += eliminaDuplicatiToolStripMenuItem_Click;
			// 
			// cancellaListaToolStripMenuItem
			// 
			cancellaListaToolStripMenuItem.Name = "cancellaListaToolStripMenuItem";
			cancellaListaToolStripMenuItem.Size = new Size(162,22);
			cancellaListaToolStripMenuItem.Text = "Azzera lista";
			cancellaListaToolStripMenuItem.Click += btClearTodo_Click;
			// 
			// logToolStripMenuItem
			// 
			logToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { vediLogToolStripMenuItem,cancellaLogToolStripMenuItem,cartellaDiLogToolStripMenuItem });
			logToolStripMenuItem.Name = "logToolStripMenuItem";
			logToolStripMenuItem.Size = new Size(39,20);
			logToolStripMenuItem.Text = "Log";
			// 
			// vediLogToolStripMenuItem
			// 
			vediLogToolStripMenuItem.Name = "vediLogToolStripMenuItem";
			vediLogToolStripMenuItem.Size = new Size(147,22);
			vediLogToolStripMenuItem.Text = "Vedi log";
			vediLogToolStripMenuItem.Click += vediLogToolStripMenuItem_Click;
			// 
			// cancellaLogToolStripMenuItem
			// 
			cancellaLogToolStripMenuItem.Name = "cancellaLogToolStripMenuItem";
			cancellaLogToolStripMenuItem.Size = new Size(147,22);
			cancellaLogToolStripMenuItem.Text = "Azzera log";
			cancellaLogToolStripMenuItem.Click += btClearLog_Click;
			// 
			// cartellaDiLogToolStripMenuItem
			// 
			cartellaDiLogToolStripMenuItem.Name = "cartellaDiLogToolStripMenuItem";
			cartellaDiLogToolStripMenuItem.Size = new Size(147,22);
			cartellaDiLogToolStripMenuItem.Text = "Cartella di log";
			cartellaDiLogToolStripMenuItem.Click += btLogFolder_Click;
			// 
			// toolStripMenuItem1
			// 
			toolStripMenuItem1.DropDownItems.AddRange(new ToolStripItem[] { aboutToolStripMenuItem });
			toolStripMenuItem1.Name = "toolStripMenuItem1";
			toolStripMenuItem1.Size = new Size(24,20);
			toolStripMenuItem1.Text = "?";
			// 
			// aboutToolStripMenuItem
			// 
			aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			aboutToolStripMenuItem.Size = new Size(107,22);
			aboutToolStripMenuItem.Text = "About";
			aboutToolStripMenuItem.Click += aboutToolStripMenuItem_Click;
			// 
			// Form1
			// 
			AutoScaleDimensions = new SizeF(7F,15F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = SystemColors.Control;
			ClientSize = new Size(571,449);
			Controls.Add(statusStrip1);
			Controls.Add(menuStrip1);
			Controls.Add(panel1);
			MainMenuStrip = menuStrip1;
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "Form1";
			HelpButtonClicked += Form1_HelpButtonClicked;
			FormClosing += Form1_FormClosing;
			Load += Form1_Load;
			ResizeEnd += Form1_ResizeEnd;
			gbComandi.ResumeLayout(false);
			panel1.ResumeLayout(false);
			gbLog.ResumeLayout(false);
			statusStrip1.ResumeLayout(false);
			statusStrip1.PerformLayout();
			menuStrip1.ResumeLayout(false);
			menuStrip1.PerformLayout();
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
		private GroupBox gbLog;
		private Button btLogFolder;
		private Button btViewTodo;
		private ToolTip toolTip1;
		private MenuStrip menuStrip1;
		private ToolStripMenuItem fileToolStripMenuItem;
		private ToolStripMenuItem leggiToolStripMenuItem;
		private ToolStripMenuItem scriviToolStripMenuItem;
		private ToolStripMenuItem leggiEScriviToolStripMenuItem;
		private ToolStripMenuItem esciToolStripMenuItem;
		private ToolStripMenuItem toolStripMenuItem1;
		private ToolStripMenuItem aboutToolStripMenuItem;
		private ToolStripMenuItem listaToolStripMenuItem;
		private ToolStripMenuItem vediListaToolStripMenuItem;
		private ToolStripMenuItem eliminaDuplicatiToolStripMenuItem;
		private ToolStripMenuItem cancellaListaToolStripMenuItem;
		private ToolStripMenuItem logToolStripMenuItem;
		private ToolStripMenuItem cartellaDiLogToolStripMenuItem;
		private ToolStripMenuItem cancellaLogToolStripMenuItem;
		private ToolStripMenuItem vediLogToolStripMenuItem;
	}
	}
