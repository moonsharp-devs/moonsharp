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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.connectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.scriptCodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.bytecodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.stepOverToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.stepInToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
			this.toggleBreakpointToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
			this.gOToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.btnOpenFile = new System.Windows.Forms.ToolStripButton();
			this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.toolGO = new System.Windows.Forms.ToolStripButton();
			this.toolStepOver = new System.Windows.Forms.ToolStripButton();
			this.toolStepIN = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButton5 = new System.Windows.Forms.ToolStripButton();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			this.tabControl2 = new System.Windows.Forms.TabControl();
			this.tabPage3 = new System.Windows.Forms.TabPage();
			this.lvWatches = new MoonSharp.Debugger.DoubleBufferedListView();
			this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.toolStrip2 = new System.Windows.Forms.ToolStrip();
			this.btnAddWatch = new System.Windows.Forms.ToolStripButton();
			this.btnRemoveWatch = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.btnViewWatch = new System.Windows.Forms.ToolStripButton();
			this.toolGoToCodeWatches = new System.Windows.Forms.ToolStripButton();
			this.label3 = new System.Windows.Forms.Label();
			this.tabPage4 = new System.Windows.Forms.TabPage();
			this.lvVStack = new MoonSharp.Debugger.DoubleBufferedListView();
			this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.toolStrip3 = new System.Windows.Forms.ToolStrip();
			this.btnViewVStk = new System.Windows.Forms.ToolStripButton();
			this.toolGoToCodeVStack = new System.Windows.Forms.ToolStripButton();
			this.label2 = new System.Windows.Forms.Label();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.lvCallStack = new MoonSharp.Debugger.DoubleBufferedListView();
			this.colAddress = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colReturn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colBP = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.toolStrip4 = new System.Windows.Forms.ToolStrip();
			this.toolGoToCodeXStack = new System.Windows.Forms.ToolStripButton();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.label1 = new System.Windows.Forms.Label();
			this.splitContainer3 = new System.Windows.Forms.SplitContainer();
			this.codeView = new MoonSharp.Debugger.SourceCodeDebugControl();
			this.txtOutput = new System.Windows.Forms.TextBox();
			this.menuStrip1.SuspendLayout();
			this.toolStrip1.SuspendLayout();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			this.tabControl2.SuspendLayout();
			this.tabPage3.SuspendLayout();
			this.toolStrip2.SuspendLayout();
			this.tabPage4.SuspendLayout();
			this.toolStrip3.SuspendLayout();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.toolStrip4.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.splitContainer3.Panel1.SuspendLayout();
			this.splitContainer3.Panel2.SuspendLayout();
			this.splitContainer3.SuspendLayout();
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
            this.connectToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "&File";
			// 
			// openToolStripMenuItem
			// 
			this.openToolStripMenuItem.Image = global::MoonSharp.Debugger.Properties.Resources.Open_6529;
			this.openToolStripMenuItem.Name = "openToolStripMenuItem";
			this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.openToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
			this.openToolStripMenuItem.Text = "&Open...";
			this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
			// 
			// connectToolStripMenuItem
			// 
			this.connectToolStripMenuItem.Image = global::MoonSharp.Debugger.Properties.Resources.AddConnection_477;
			this.connectToolStripMenuItem.Name = "connectToolStripMenuItem";
			this.connectToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
			this.connectToolStripMenuItem.Text = "Connect...";
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
			this.stepOverToolStripMenuItem.Image = global::MoonSharp.Debugger.Properties.Resources.StepOver_6328;
			this.stepOverToolStripMenuItem.Name = "stepOverToolStripMenuItem";
			this.stepOverToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F10;
			this.stepOverToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
			this.stepOverToolStripMenuItem.Text = "Step-Over";
			this.stepOverToolStripMenuItem.Click += new System.EventHandler(this.stepOverToolStripMenuItem_Click);
			// 
			// stepInToolStripMenuItem
			// 
			this.stepInToolStripMenuItem.Image = global::MoonSharp.Debugger.Properties.Resources.StepIn_6326;
			this.stepInToolStripMenuItem.Name = "stepInToolStripMenuItem";
			this.stepInToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F11;
			this.stepInToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
			this.stepInToolStripMenuItem.Text = "Step-In";
			this.stepInToolStripMenuItem.Click += new System.EventHandler(this.stepInToolStripMenuItem_Click);
			// 
			// toolStripMenuItem2
			// 
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			this.toolStripMenuItem2.Size = new System.Drawing.Size(187, 6);
			// 
			// toggleBreakpointToolStripMenuItem
			// 
			this.toggleBreakpointToolStripMenuItem.Image = global::MoonSharp.Debugger.Properties.Resources.BreakpointEnabled_6584_16x;
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
			this.gOToolStripMenuItem.Image = global::MoonSharp.Debugger.Properties.Resources.startwithoutdebugging_6556;
			this.gOToolStripMenuItem.Name = "gOToolStripMenuItem";
			this.gOToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
			this.gOToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
			this.gOToolStripMenuItem.Text = "GO";
			this.gOToolStripMenuItem.Click += new System.EventHandler(this.gOToolStripMenuItem_Click);
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
            this.btnOpenFile,
            this.toolStripButton2,
            this.toolStripSeparator1,
            this.toolStripButton3,
            this.toolStripSeparator2,
            this.toolGO,
            this.toolStepOver,
            this.toolStepIN,
            this.toolStripSeparator3,
            this.toolStripButton5});
			this.toolStrip1.Location = new System.Drawing.Point(0, 24);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(1094, 25);
			this.toolStrip1.TabIndex = 1;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// btnOpenFile
			// 
			this.btnOpenFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.btnOpenFile.Image = global::MoonSharp.Debugger.Properties.Resources.Open_6529;
			this.btnOpenFile.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.btnOpenFile.Name = "btnOpenFile";
			this.btnOpenFile.Size = new System.Drawing.Size(23, 22);
			this.btnOpenFile.Text = "Open File";
			this.btnOpenFile.Click += new System.EventHandler(this.btnOpenFile_Click);
			// 
			// toolStripButton2
			// 
			this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButton2.Image = global::MoonSharp.Debugger.Properties.Resources.AddConnection_477;
			this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton2.Name = "toolStripButton2";
			this.toolStripButton2.Size = new System.Drawing.Size(23, 22);
			this.toolStripButton2.Text = "toolStripButton2";
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// toolStripButton3
			// 
			this.toolStripButton3.Checked = true;
			this.toolStripButton3.CheckState = System.Windows.Forms.CheckState.Checked;
			this.toolStripButton3.Image = global::MoonSharp.Debugger.Properties.Resources.DisassemblyWindow_6536;
			this.toolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton3.Name = "toolStripButton3";
			this.toolStripButton3.Size = new System.Drawing.Size(76, 22);
			this.toolStripButton3.Text = "Bytecode";
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
			// 
			// toolGO
			// 
			this.toolGO.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolGO.Image = global::MoonSharp.Debugger.Properties.Resources.startwithoutdebugging_6556;
			this.toolGO.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolGO.Name = "toolGO";
			this.toolGO.Size = new System.Drawing.Size(23, 22);
			this.toolGO.Text = "GO";
			this.toolGO.Click += new System.EventHandler(this.toolGO_Click);
			// 
			// toolStepOver
			// 
			this.toolStepOver.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStepOver.Image = global::MoonSharp.Debugger.Properties.Resources.StepOver_6328;
			this.toolStepOver.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStepOver.Name = "toolStepOver";
			this.toolStepOver.Size = new System.Drawing.Size(23, 22);
			this.toolStepOver.Text = "Step-Over";
			this.toolStepOver.Click += new System.EventHandler(this.toolStepOver_Click);
			// 
			// toolStepIN
			// 
			this.toolStepIN.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStepIN.Image = global::MoonSharp.Debugger.Properties.Resources.StepIn_6326;
			this.toolStepIN.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStepIN.Name = "toolStepIN";
			this.toolStepIN.Size = new System.Drawing.Size(23, 22);
			this.toolStepIN.Text = "Step-In";
			this.toolStepIN.Click += new System.EventHandler(this.toolStripButton1_Click);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
			// 
			// toolStripButton5
			// 
			this.toolStripButton5.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButton5.Image = global::MoonSharp.Debugger.Properties.Resources.BreakpointEnabled_6584_16x;
			this.toolStripButton5.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton5.Name = "toolStripButton5";
			this.toolStripButton5.Size = new System.Drawing.Size(23, 22);
			this.toolStripButton5.Text = "toolStripButton5";
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
			this.splitContainer1.Panel2.Controls.Add(this.splitContainer3);
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
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.tabControl2);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.tabControl1);
			this.splitContainer2.Size = new System.Drawing.Size(364, 663);
			this.splitContainer2.SplitterDistance = 307;
			this.splitContainer2.TabIndex = 0;
			// 
			// tabControl2
			// 
			this.tabControl2.Controls.Add(this.tabPage3);
			this.tabControl2.Controls.Add(this.tabPage4);
			this.tabControl2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl2.ImageList = this.imageList1;
			this.tabControl2.Location = new System.Drawing.Point(0, 0);
			this.tabControl2.Name = "tabControl2";
			this.tabControl2.SelectedIndex = 0;
			this.tabControl2.Size = new System.Drawing.Size(364, 307);
			this.tabControl2.TabIndex = 1;
			// 
			// tabPage3
			// 
			this.tabPage3.Controls.Add(this.lvWatches);
			this.tabPage3.Controls.Add(this.toolStrip2);
			this.tabPage3.Controls.Add(this.label3);
			this.tabPage3.ImageIndex = 3;
			this.tabPage3.Location = new System.Drawing.Point(4, 23);
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage3.Size = new System.Drawing.Size(356, 280);
			this.tabPage3.TabIndex = 0;
			this.tabPage3.Text = "Watches";
			this.tabPage3.UseVisualStyleBackColor = true;
			// 
			// lvWatches
			// 
			this.lvWatches.BackColor = System.Drawing.SystemColors.Window;
			this.lvWatches.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
			this.lvWatches.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lvWatches.FullRowSelect = true;
			this.lvWatches.GridLines = true;
			this.lvWatches.Location = new System.Drawing.Point(3, 28);
			this.lvWatches.Name = "lvWatches";
			this.lvWatches.Size = new System.Drawing.Size(350, 249);
			this.lvWatches.TabIndex = 4;
			this.lvWatches.UseCompatibleStateImageBehavior = false;
			this.lvWatches.View = System.Windows.Forms.View.Details;
			this.lvWatches.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lvWatches_MouseDoubleClick);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Name";
			this.columnHeader1.Width = 72;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Type";
			this.columnHeader2.Width = 57;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Value";
			this.columnHeader3.Width = 111;
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "Symbol loc.";
			this.columnHeader4.Width = 72;
			// 
			// toolStrip2
			// 
			this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnAddWatch,
            this.btnRemoveWatch,
            this.toolStripSeparator4,
            this.btnViewWatch,
            this.toolGoToCodeWatches});
			this.toolStrip2.Location = new System.Drawing.Point(3, 3);
			this.toolStrip2.Name = "toolStrip2";
			this.toolStrip2.Size = new System.Drawing.Size(350, 25);
			this.toolStrip2.TabIndex = 3;
			this.toolStrip2.Text = "toolStrip2";
			// 
			// btnAddWatch
			// 
			this.btnAddWatch.Image = global::MoonSharp.Debugger.Properties.Resources.AddMark_10580;
			this.btnAddWatch.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.btnAddWatch.Name = "btnAddWatch";
			this.btnAddWatch.Size = new System.Drawing.Size(49, 22);
			this.btnAddWatch.Text = "Add";
			this.btnAddWatch.Click += new System.EventHandler(this.btnAddWatch_Click);
			// 
			// btnRemoveWatch
			// 
			this.btnRemoveWatch.Image = global::MoonSharp.Debugger.Properties.Resources.Clearallrequests_8816;
			this.btnRemoveWatch.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.btnRemoveWatch.Name = "btnRemoveWatch";
			this.btnRemoveWatch.Size = new System.Drawing.Size(70, 22);
			this.btnRemoveWatch.Text = "Remove";
			this.btnRemoveWatch.Click += new System.EventHandler(this.btnRemoveWatch_Click);
			// 
			// toolStripSeparator4
			// 
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
			// 
			// btnViewWatch
			// 
			this.btnViewWatch.Image = global::MoonSharp.Debugger.Properties.Resources.FindSymbol_6263;
			this.btnViewWatch.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.btnViewWatch.Name = "btnViewWatch";
			this.btnViewWatch.Size = new System.Drawing.Size(52, 22);
			this.btnViewWatch.Text = "View";
			this.btnViewWatch.Click += new System.EventHandler(this.btnViewWatch_Click);
			// 
			// toolGoToCodeWatches
			// 
			this.toolGoToCodeWatches.Image = global::MoonSharp.Debugger.Properties.Resources.GoToDeclaration_5576;
			this.toolGoToCodeWatches.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolGoToCodeWatches.Name = "toolGoToCodeWatches";
			this.toolGoToCodeWatches.Size = new System.Drawing.Size(87, 22);
			this.toolGoToCodeWatches.Text = "Go to Code";
			this.toolGoToCodeWatches.ToolTipText = "Go to Code";
			this.toolGoToCodeWatches.Click += new System.EventHandler(this.toolGoToCodeWatches_Click);
			// 
			// label3
			// 
			this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label3.Location = new System.Drawing.Point(3, 3);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(350, 274);
			this.label3.TabIndex = 2;
			this.label3.Text = "Not Implemented Yet";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// tabPage4
			// 
			this.tabPage4.Controls.Add(this.lvVStack);
			this.tabPage4.Controls.Add(this.toolStrip3);
			this.tabPage4.Controls.Add(this.label2);
			this.tabPage4.ImageIndex = 1;
			this.tabPage4.Location = new System.Drawing.Point(4, 23);
			this.tabPage4.Name = "tabPage4";
			this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage4.Size = new System.Drawing.Size(356, 280);
			this.tabPage4.TabIndex = 1;
			this.tabPage4.Text = "V-Stack";
			this.tabPage4.UseVisualStyleBackColor = true;
			// 
			// lvVStack
			// 
			this.lvVStack.BackColor = System.Drawing.SystemColors.Window;
			this.lvVStack.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7});
			this.lvVStack.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lvVStack.FullRowSelect = true;
			this.lvVStack.GridLines = true;
			this.lvVStack.Location = new System.Drawing.Point(3, 28);
			this.lvVStack.Name = "lvVStack";
			this.lvVStack.Size = new System.Drawing.Size(350, 249);
			this.lvVStack.TabIndex = 7;
			this.lvVStack.UseCompatibleStateImageBehavior = false;
			this.lvVStack.View = System.Windows.Forms.View.Details;
			this.lvVStack.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lvVStack_MouseDoubleClick);
			// 
			// columnHeader5
			// 
			this.columnHeader5.Text = "Stack ofs";
			this.columnHeader5.Width = 72;
			// 
			// columnHeader6
			// 
			this.columnHeader6.Text = "Type";
			this.columnHeader6.Width = 94;
			// 
			// columnHeader7
			// 
			this.columnHeader7.Text = "Value";
			this.columnHeader7.Width = 157;
			// 
			// toolStrip3
			// 
			this.toolStrip3.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnViewVStk,
            this.toolGoToCodeVStack});
			this.toolStrip3.Location = new System.Drawing.Point(3, 3);
			this.toolStrip3.Name = "toolStrip3";
			this.toolStrip3.Size = new System.Drawing.Size(350, 25);
			this.toolStrip3.TabIndex = 6;
			this.toolStrip3.Text = "toolStrip3";
			// 
			// btnViewVStk
			// 
			this.btnViewVStk.Image = global::MoonSharp.Debugger.Properties.Resources.FindSymbol_6263;
			this.btnViewVStk.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.btnViewVStk.Name = "btnViewVStk";
			this.btnViewVStk.Size = new System.Drawing.Size(52, 22);
			this.btnViewVStk.Text = "View";
			this.btnViewVStk.Click += new System.EventHandler(this.btnViewVStk_Click);
			// 
			// toolGoToCodeVStack
			// 
			this.toolGoToCodeVStack.Image = global::MoonSharp.Debugger.Properties.Resources.GoToDeclaration_5576;
			this.toolGoToCodeVStack.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolGoToCodeVStack.Name = "toolGoToCodeVStack";
			this.toolGoToCodeVStack.Size = new System.Drawing.Size(87, 22);
			this.toolGoToCodeVStack.Text = "Go to Code";
			this.toolGoToCodeVStack.ToolTipText = "Go to Code";
			this.toolGoToCodeVStack.Click += new System.EventHandler(this.toolGoToCodeVStack_Click);
			// 
			// label2
			// 
			this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label2.Location = new System.Drawing.Point(3, 3);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(350, 274);
			this.label2.TabIndex = 1;
			this.label2.Text = "Not Implemented Yet";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// imageList1
			// 
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList1.Images.SetKeyName(0, "CallStackWindow_6561.png");
			this.imageList1.Images.SetKeyName(1, "Centered_11691.png");
			this.imageList1.Images.SetKeyName(2, "CodeCoverageResults_8592.png");
			this.imageList1.Images.SetKeyName(3, "LocalsWindow_6562.png");
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.ImageList = this.imageList1;
			this.tabControl1.Location = new System.Drawing.Point(0, 0);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(364, 352);
			this.tabControl1.TabIndex = 0;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.lvCallStack);
			this.tabPage1.Controls.Add(this.toolStrip4);
			this.tabPage1.ImageIndex = 0;
			this.tabPage1.Location = new System.Drawing.Point(4, 23);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(356, 325);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Call Stack";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// lvCallStack
			// 
			this.lvCallStack.BackColor = System.Drawing.SystemColors.Window;
			this.lvCallStack.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colAddress,
            this.colName,
            this.colReturn,
            this.colBP});
			this.lvCallStack.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lvCallStack.FullRowSelect = true;
			this.lvCallStack.GridLines = true;
			this.lvCallStack.Location = new System.Drawing.Point(3, 28);
			this.lvCallStack.Name = "lvCallStack";
			this.lvCallStack.Size = new System.Drawing.Size(350, 294);
			this.lvCallStack.TabIndex = 8;
			this.lvCallStack.UseCompatibleStateImageBehavior = false;
			this.lvCallStack.View = System.Windows.Forms.View.Details;
			// 
			// colAddress
			// 
			this.colAddress.Text = "Address";
			this.colAddress.Width = 72;
			// 
			// colName
			// 
			this.colName.Text = "Name";
			this.colName.Width = 106;
			// 
			// colReturn
			// 
			this.colReturn.Text = "Return";
			this.colReturn.Width = 72;
			// 
			// colBP
			// 
			this.colBP.Text = "Base Ptr";
			this.colBP.Width = 72;
			// 
			// toolStrip4
			// 
			this.toolStrip4.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolGoToCodeXStack});
			this.toolStrip4.Location = new System.Drawing.Point(3, 3);
			this.toolStrip4.Name = "toolStrip4";
			this.toolStrip4.Size = new System.Drawing.Size(350, 25);
			this.toolStrip4.TabIndex = 7;
			this.toolStrip4.Text = "toolStrip4";
			// 
			// toolGoToCodeXStack
			// 
			this.toolGoToCodeXStack.Image = global::MoonSharp.Debugger.Properties.Resources.GoToDeclaration_5576;
			this.toolGoToCodeXStack.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolGoToCodeXStack.Name = "toolGoToCodeXStack";
			this.toolGoToCodeXStack.Size = new System.Drawing.Size(87, 22);
			this.toolGoToCodeXStack.Text = "Go to Code";
			this.toolGoToCodeXStack.ToolTipText = "Go to Code";
			this.toolGoToCodeXStack.Click += new System.EventHandler(this.toolGoToCodeXStack_Click);
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.label1);
			this.tabPage2.ImageIndex = 2;
			this.tabPage2.Location = new System.Drawing.Point(4, 23);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = new System.Drawing.Size(356, 325);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Coroutines";
			this.tabPage2.UseVisualStyleBackColor = true;
			// 
			// label1
			// 
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.Location = new System.Drawing.Point(3, 3);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(350, 319);
			this.label1.TabIndex = 1;
			this.label1.Text = "Not Implemented Yet";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// splitContainer3
			// 
			this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer3.Location = new System.Drawing.Point(0, 0);
			this.splitContainer3.Name = "splitContainer3";
			this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer3.Panel1
			// 
			this.splitContainer3.Panel1.Controls.Add(this.codeView);
			// 
			// splitContainer3.Panel2
			// 
			this.splitContainer3.Panel2.Controls.Add(this.txtOutput);
			this.splitContainer3.Size = new System.Drawing.Size(726, 663);
			this.splitContainer3.SplitterDistance = 446;
			this.splitContainer3.TabIndex = 0;
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
			this.codeView.Size = new System.Drawing.Size(726, 446);
			this.codeView.SourceCode = null;
			this.codeView.TabIndex = 1;
			// 
			// txtOutput
			// 
			this.txtOutput.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.txtOutput.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtOutput.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
			this.txtOutput.Location = new System.Drawing.Point(0, 0);
			this.txtOutput.Multiline = true;
			this.txtOutput.Name = "txtOutput";
			this.txtOutput.ReadOnly = true;
			this.txtOutput.Size = new System.Drawing.Size(726, 213);
			this.txtOutput.TabIndex = 0;
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
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			this.splitContainer2.ResumeLayout(false);
			this.tabControl2.ResumeLayout(false);
			this.tabPage3.ResumeLayout(false);
			this.tabPage3.PerformLayout();
			this.toolStrip2.ResumeLayout(false);
			this.toolStrip2.PerformLayout();
			this.tabPage4.ResumeLayout(false);
			this.tabPage4.PerformLayout();
			this.toolStrip3.ResumeLayout(false);
			this.toolStrip3.PerformLayout();
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage1.PerformLayout();
			this.toolStrip4.ResumeLayout(false);
			this.toolStrip4.PerformLayout();
			this.tabPage2.ResumeLayout(false);
			this.splitContainer3.Panel1.ResumeLayout(false);
			this.splitContainer3.Panel2.ResumeLayout(false);
			this.splitContainer3.Panel2.PerformLayout();
			this.splitContainer3.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.SplitContainer splitContainer2;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem debugToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem stepOverToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem stepInToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
		private System.Windows.Forms.ToolStripMenuItem toggleBreakpointToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
		private System.Windows.Forms.ToolStripMenuItem gOToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem scriptCodeToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem bytecodeToolStripMenuItem;
		private System.Windows.Forms.ToolStripButton toolStepIN;
		private System.Windows.Forms.TabControl tabControl2;
		private System.Windows.Forms.TabPage tabPage3;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TabPage tabPage4;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.ToolStripButton btnOpenFile;
		private System.Windows.Forms.ToolStripButton toolStripButton2;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton toolStripButton3;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripButton toolGO;
		private System.Windows.Forms.ToolStripButton toolStepOver;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripButton toolStripButton5;
		private System.Windows.Forms.ToolStripMenuItem connectToolStripMenuItem;
		private DoubleBufferedListView lvWatches;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.ToolStrip toolStrip2;
		private System.Windows.Forms.ToolStripButton btnAddWatch;
		private System.Windows.Forms.ToolStripButton btnRemoveWatch;
		private System.Windows.Forms.ToolStripButton btnViewWatch;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
		private System.Windows.Forms.SplitContainer splitContainer3;
		private SourceCodeDebugControl codeView;
		private System.Windows.Forms.TextBox txtOutput;
		private DoubleBufferedListView lvVStack;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		private System.Windows.Forms.ColumnHeader columnHeader6;
		private System.Windows.Forms.ColumnHeader columnHeader7;
		private System.Windows.Forms.ToolStrip toolStrip3;
		private System.Windows.Forms.ToolStripButton btnViewVStk;
		private System.Windows.Forms.ToolStripButton toolGoToCodeWatches;
		private System.Windows.Forms.ToolStripButton toolGoToCodeVStack;
		private DoubleBufferedListView lvCallStack;
		private System.Windows.Forms.ColumnHeader colAddress;
		private System.Windows.Forms.ColumnHeader colName;
		private System.Windows.Forms.ColumnHeader colReturn;
		private System.Windows.Forms.ColumnHeader colBP;
		private System.Windows.Forms.ToolStrip toolStrip4;
		private System.Windows.Forms.ToolStripButton toolGoToCodeXStack;


	}
}

