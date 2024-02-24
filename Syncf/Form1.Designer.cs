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
			button1 = new Button();
			button2 = new Button();
			button3 = new Button();
			button4 = new Button();
			groupBox1 = new GroupBox();
			button5 = new Button();
			tbMsg = new TextBox();
			panel1 = new Panel();
			statusStrip1 = new StatusStrip();
			toolStripStatusLabel1 = new ToolStripStatusLabel();
			groupBox1.SuspendLayout();
			panel1.SuspendLayout();
			statusStrip1.SuspendLayout();
			SuspendLayout();
			// 
			// button1
			// 
			button1.Location = new Point(20,30);
			button1.Name = "button1";
			button1.Size = new Size(110,27);
			button1.TabIndex = 1;
			button1.Text = "button1";
			button1.UseVisualStyleBackColor = true;
			// 
			// button2
			// 
			button2.Location = new Point(20,63);
			button2.Name = "button2";
			button2.Size = new Size(110,27);
			button2.TabIndex = 2;
			button2.Text = "button2";
			button2.UseVisualStyleBackColor = true;
			// 
			// button3
			// 
			button3.Location = new Point(20,96);
			button3.Name = "button3";
			button3.Size = new Size(110,27);
			button3.TabIndex = 3;
			button3.Text = "button3";
			button3.UseVisualStyleBackColor = true;
			// 
			// button4
			// 
			button4.Location = new Point(31,244);
			button4.Name = "button4";
			button4.Size = new Size(110,27);
			button4.TabIndex = 4;
			button4.Text = "Exit";
			button4.UseVisualStyleBackColor = true;
			// 
			// groupBox1
			// 
			groupBox1.Controls.Add(button3);
			groupBox1.Controls.Add(button2);
			groupBox1.Controls.Add(button1);
			groupBox1.Location = new Point(31,22);
			groupBox1.Name = "groupBox1";
			groupBox1.Size = new Size(149,154);
			groupBox1.TabIndex = 5;
			groupBox1.TabStop = false;
			groupBox1.Text = "Comandi";
			// 
			// button5
			// 
			button5.Location = new Point(31,193);
			button5.Name = "button5";
			button5.Size = new Size(110,27);
			button5.TabIndex = 6;
			button5.Text = "Stop";
			button5.UseVisualStyleBackColor = true;
			// 
			// tbMsg
			// 
			tbMsg.BorderStyle = BorderStyle.FixedSingle;
			tbMsg.Dock = DockStyle.Right;
			tbMsg.Location = new Point(225,0);
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
			panel1.Controls.Add(button4);
			panel1.Controls.Add(button5);
			panel1.Dock = DockStyle.Top;
			panel1.Location = new Point(0,0);
			panel1.Name = "panel1";
			panel1.Size = new Size(556,351);
			panel1.TabIndex = 7;
			// 
			// statusStrip1
			// 
			statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1 });
			statusStrip1.Location = new Point(0,375);
			statusStrip1.Name = "statusStrip1";
			statusStrip1.Size = new Size(556,22);
			statusStrip1.TabIndex = 8;
			statusStrip1.Text = "statusStrip1";
			// 
			// toolStripStatusLabel1
			// 
			toolStripStatusLabel1.Name = "toolStripStatusLabel1";
			toolStripStatusLabel1.Size = new Size(118,17);
			toolStripStatusLabel1.Text = "toolStripStatusLabel1";
			// 
			// Form1
			// 
			AutoScaleDimensions = new SizeF(7F,15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(556,397);
			Controls.Add(statusStrip1);
			Controls.Add(panel1);
			Name = "Form1";
			Text = "Form1";
			groupBox1.ResumeLayout(false);
			panel1.ResumeLayout(false);
			panel1.PerformLayout();
			statusStrip1.ResumeLayout(false);
			statusStrip1.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private Button button1;
		private Button button2;
		private Button button3;
		private Button button4;
		private GroupBox groupBox1;
		private Button button5;
		private TextBox tbMsg;
		private Panel panel1;
		private StatusStrip statusStrip1;
		private ToolStripStatusLabel toolStripStatusLabel1;
	}
	}
