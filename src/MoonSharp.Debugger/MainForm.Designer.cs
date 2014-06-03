namespace MoonSharp.Debugger
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.scriptCodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.bytecodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.stepOverToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.stepInToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.stepOutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
			this.toggleBreakpointToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
			this.gOToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			this.codeView = new MoonSharp.Debugger.SourceCodeDebugControl();
			this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
			this.menuStrip1.SuspendLayout();
			this.toolStrip1.SuspendLayout();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.debugToolStripMenuItem,
            this.helpToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(1094, 24);
			this.menuStrip1.TabIndex = 0;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "&File";
			// 
			// openToolStripMenuItem
			// 
			this.openToolStripMenuItem.Name = "openToolStripMenuItem";
			this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.openToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
			this.openToolStripMenuItem.Text = "&Open...";
			this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(152, 6);
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
			this.exitToolStripMenuItem.Text = "&Exit";
			// 
			// viewToolStripMenuItem
			// 
			this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.scriptCodeToolStripMenuItem,
            this.bytecodeToolStripMenuItem});
			this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
			this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
			this.viewToolStripMenuItem.Text = "&View";
			// 
			// scriptCodeToolStripMenuItem
			// 
			this.scriptCodeToolStripMenuItem.Name = "scriptCodeToolStripMenuItem";
			this.scriptCodeToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
			this.scriptCodeToolStripMenuItem.Text = "Script code";
			// 
			// bytecodeToolStripMenuItem
			// 
			this.bytecodeToolStripMenuItem.Checked = true;
			this.bytecodeToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
			this.bytecodeToolStripMenuItem.Name = "bytecodeToolStripMenuItem";
			this.bytecodeToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
			this.bytecodeToolStripMenuItem.Text = "Bytecode";
			// 
			// debugToolStripMenuItem
			// 
			this.debugToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stepOverToolStripMenuItem,
            this.stepInToolStripMenuItem,
            this.stepOutToolStripMenuItem,
            this.toolStripMenuItem2,
            this.toggleBreakpointToolStripMenuItem,
            this.toolStripMenuItem3,
            this.gOToolStripMenuItem});
			this.debugToolStripMenuItem.Name = "debugToolStripMenuItem";
			this.debugToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
			this.debugToolStripMenuItem.Text = "&Debug";
			// 
			// stepOverToolStripMenuItem
			// 
			this.stepOverToolStripMenuItem.Name = "stepOverToolStripMenuItem";
			this.stepOverToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F10;
			this.stepOverToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
			this.stepOverToolStripMenuItem.Text = "Step-Over";
			// 
			// stepInToolStripMenuItem
			// 
			this.stepInToolStripMenuItem.Name = "stepInToolStripMenuItem";
			this.stepInToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F11;
			this.stepInToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
			this.stepInToolStripMenuItem.Text = "Step-In";
			// 
			// stepOutToolStripMenuItem
			// 
			this.stepOutToolStripMenuItem.Name = "stepOutToolStripMenuItem";
			this.stepOutToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F12;
			this.stepOutToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
			this.stepOutToolStripMenuItem.Text = "Step-Out";
			// 
			// toolStripMenuItem2
			// 
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			this.toolStripMenuItem2.Size = new System.Drawing.Size(187, 6);
			// 
			// toggleBreakpointToolStripMenuItem
			// 
			this.toggleBreakpointToolStripMenuItem.Name = "toggleBreakpointToolStripMenuItem";
			this.toggleBreakpointToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F9;
			this.toggleBreakpointToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
			this.toggleBreakpointToolStripMenuItem.Text = "Toggle Breakpoint";
			// 
			// toolStripMenuItem3
			// 
			this.toolStripMenuItem3.Name = "toolStripMenuItem3";
			this.toolStripMenuItem3.Size = new System.Drawing.Size(187, 6);
			// 
			// gOToolStripMenuItem
			// 
			this.gOToolStripMenuItem.Name = "gOToolStripMenuItem";
			this.gOToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
			this.gOToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
			this.gOToolStripMenuItem.Text = "GO";
			// 
			// helpToolStripMenuItem
			// 
			this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
			this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
			this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
			this.helpToolStripMenuItem.Text = "&Help";
			// 
			// aboutToolStripMenuItem
			// 
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			this.aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
			this.aboutToolStripMenuItem.Text = "&About";
			// 
			// toolStrip1
			// 
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1});
			this.toolStrip1.Location = new System.Drawing.Point(0, 24);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(1094, 25);
			this.toolStrip1.TabIndex = 1;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// statusStrip1
			// 
			this.statusStrip1.Location = new System.Drawing.Point(0, 712);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(1094, 22);
			this.statusStrip1.TabIndex = 2;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 49);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.codeView);
			this.splitContainer1.Size = new System.Drawing.Size(1094, 663);
			this.splitContainer1.SplitterDistance = 364;
			this.splitContainer1.TabIndex = 3;
			// 
			// splitContainer2
			// 
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.Location = new System.Drawing.Point(0, 0);
			this.splitContainer2.Name = "splitContainer2";
			this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.splitContainer2.Size = new System.Drawing.Size(364, 663);
			this.splitContainer2.SplitterDistance = 307;
			this.splitContainer2.TabIndex = 0;
			// 
			// codeView
			// 
			this.codeView.ActiveLine = -1;
			this.codeView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.codeView.CursorLine = 0;
			this.codeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.codeView.Font = new System.Drawing.Font("Consolas", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.codeView.ForeColor = System.Drawing.Color.Gainsboro;
			this.codeView.Location = new System.Drawing.Point(0, 0);
			this.codeView.Margin = new System.Windows.Forms.Padding(4);
			this.codeView.Name = "codeView";
			this.codeView.Size = new System.Drawing.Size(726, 663);
			this.codeView.SourceCode = null;
			this.codeView.TabIndex = 1;
			// 
			// toolStripButton1
			// 
			this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
			this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton1.Name = "toolStripButton1";
			this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
			this.toolStripButton1.Text = "Step-In";
			this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1094, 734);
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.statusStrip1);
			this.Controls.Add(this.toolStrip1);
			this.Controls.Add(this.menuStrip1);
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "MainForm";
			this.Text = "Moon# Debugger";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer2.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.SplitContainer splitContainer2;
		private SourceCodeDebugControl codeView;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem debugToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem stepOverToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem stepInToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem stepOutToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
		private System.Windows.Forms.ToolStripMenuItem toggleBreakpointToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
		private System.Windows.Forms.ToolStripMenuItem gOToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem scriptCodeToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem bytecodeToolStripMenuItem;
		private System.Windows.Forms.ToolStripButton toolStripButton1;


	}
}

