namespace MoonSharp.Debugger
{
	partial class ValueBrowser
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
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.toolBack = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.toolDigData = new System.Windows.Forms.ToolStripButton();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.lvProps = new System.Windows.Forms.ListView();
			this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			this.lvTableData = new System.Windows.Forms.ListView();
			this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.txtString = new System.Windows.Forms.TextBox();
			this.lblData = new System.Windows.Forms.Label();
			this.lvMetaTable = new System.Windows.Forms.ListView();
			this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.label4 = new System.Windows.Forms.Label();
			this.toolStrip1.SuspendLayout();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStrip1
			// 
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolBack,
            this.toolStripSeparator1,
            this.toolDigData});
			this.toolStrip1.Location = new System.Drawing.Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(963, 25);
			this.toolStrip1.TabIndex = 0;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// toolBack
			// 
			this.toolBack.Enabled = false;
			this.toolBack.Image = global::MoonSharp.Debugger.Properties.Resources.NavigateBackwards_6270;
			this.toolBack.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolBack.Name = "toolBack";
			this.toolBack.Size = new System.Drawing.Size(52, 22);
			this.toolBack.Text = "Back";
			this.toolBack.Click += new System.EventHandler(this.toolBack_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// toolDigData
			// 
			this.toolDigData.Image = global::MoonSharp.Debugger.Properties.Resources.FindSymbol_6263;
			this.toolDigData.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolDigData.Name = "toolDigData";
			this.toolDigData.Size = new System.Drawing.Size(79, 22);
			this.toolDigData.Text = "View Data";
			this.toolDigData.Click += new System.EventHandler(this.toolDigData_Click);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 25);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.lvProps);
			this.splitContainer1.Panel1.Controls.Add(this.label2);
			this.splitContainer1.Panel1.Controls.Add(this.label1);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
			this.splitContainer1.Size = new System.Drawing.Size(963, 647);
			this.splitContainer1.SplitterDistance = 321;
			this.splitContainer1.TabIndex = 1;
			// 
			// lvProps
			// 
			this.lvProps.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
			this.lvProps.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lvProps.FullRowSelect = true;
			this.lvProps.GridLines = true;
			this.lvProps.Location = new System.Drawing.Point(0, 17);
			this.lvProps.MultiSelect = false;
			this.lvProps.Name = "lvProps";
			this.lvProps.Size = new System.Drawing.Size(321, 630);
			this.lvProps.TabIndex = 3;
			this.lvProps.UseCompatibleStateImageBehavior = false;
			this.lvProps.View = System.Windows.Forms.View.Details;
			this.lvProps.SelectedIndexChanged += new System.EventHandler(this.lvProps_SelectedIndexChanged);
			this.lvProps.DoubleClick += new System.EventHandler(this.lvAnyTable_DoubleClick);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Key";
			this.columnHeader1.Width = 122;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Value";
			this.columnHeader2.Width = 175;
			// 
			// label2
			// 
			this.label2.BackColor = System.Drawing.SystemColors.ActiveCaption;
			this.label2.Dock = System.Windows.Forms.DockStyle.Top;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.label2.Location = new System.Drawing.Point(0, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(321, 17);
			this.label2.TabIndex = 1;
			this.label2.Text = "PROPERTIES";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(48, 90);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(0, 13);
			this.label1.TabIndex = 0;
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
			this.splitContainer2.Panel1.Controls.Add(this.lvTableData);
			this.splitContainer2.Panel1.Controls.Add(this.txtString);
			this.splitContainer2.Panel1.Controls.Add(this.lblData);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.lvMetaTable);
			this.splitContainer2.Panel2.Controls.Add(this.label4);
			this.splitContainer2.Size = new System.Drawing.Size(638, 647);
			this.splitContainer2.SplitterDistance = 290;
			this.splitContainer2.TabIndex = 0;
			// 
			// lvTableData
			// 
			this.lvTableData.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3,
            this.columnHeader4});
			this.lvTableData.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lvTableData.FullRowSelect = true;
			this.lvTableData.GridLines = true;
			this.lvTableData.Location = new System.Drawing.Point(0, 17);
			this.lvTableData.MultiSelect = false;
			this.lvTableData.Name = "lvTableData";
			this.lvTableData.Size = new System.Drawing.Size(638, 273);
			this.lvTableData.TabIndex = 6;
			this.lvTableData.UseCompatibleStateImageBehavior = false;
			this.lvTableData.View = System.Windows.Forms.View.Details;
			this.lvTableData.SelectedIndexChanged += new System.EventHandler(this.lvTableData_SelectedIndexChanged);
			this.lvTableData.DoubleClick += new System.EventHandler(this.lvAnyTable_DoubleClick);
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Key";
			this.columnHeader3.Width = 260;
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "Value";
			this.columnHeader4.Width = 260;
			// 
			// txtString
			// 
			this.txtString.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtString.Location = new System.Drawing.Point(0, 17);
			this.txtString.Multiline = true;
			this.txtString.Name = "txtString";
			this.txtString.ReadOnly = true;
			this.txtString.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtString.Size = new System.Drawing.Size(638, 273);
			this.txtString.TabIndex = 5;
			// 
			// lblData
			// 
			this.lblData.BackColor = System.Drawing.SystemColors.ActiveCaption;
			this.lblData.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblData.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblData.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.lblData.Location = new System.Drawing.Point(0, 0);
			this.lblData.Name = "lblData";
			this.lblData.Size = new System.Drawing.Size(638, 17);
			this.lblData.TabIndex = 2;
			this.lblData.Text = "DATA";
			this.lblData.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// lvMetaTable
			// 
			this.lvMetaTable.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader5,
            this.columnHeader6});
			this.lvMetaTable.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lvMetaTable.FullRowSelect = true;
			this.lvMetaTable.GridLines = true;
			this.lvMetaTable.Location = new System.Drawing.Point(0, 17);
			this.lvMetaTable.MultiSelect = false;
			this.lvMetaTable.Name = "lvMetaTable";
			this.lvMetaTable.Size = new System.Drawing.Size(638, 336);
			this.lvMetaTable.TabIndex = 4;
			this.lvMetaTable.UseCompatibleStateImageBehavior = false;
			this.lvMetaTable.View = System.Windows.Forms.View.Details;
			this.lvMetaTable.SelectedIndexChanged += new System.EventHandler(this.lvMetaTable_SelectedIndexChanged);
			this.lvMetaTable.DoubleClick += new System.EventHandler(this.lvAnyTable_DoubleClick);
			// 
			// columnHeader5
			// 
			this.columnHeader5.Text = "Key";
			this.columnHeader5.Width = 260;
			// 
			// columnHeader6
			// 
			this.columnHeader6.Text = "Value";
			this.columnHeader6.Width = 260;
			// 
			// label4
			// 
			this.label4.BackColor = System.Drawing.SystemColors.ActiveCaption;
			this.label4.Dock = System.Windows.Forms.DockStyle.Top;
			this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label4.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.label4.Location = new System.Drawing.Point(0, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(638, 17);
			this.label4.TabIndex = 2;
			this.label4.Text = "METATABLE";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ValueBrowser
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(963, 672);
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.toolStrip1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ValueBrowser";
			this.Text = "Moon# Value Browser";
			this.Load += new System.EventHandler(this.ValueBrowser_Load);
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel1.PerformLayout();
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel1.PerformLayout();
			this.splitContainer2.Panel2.ResumeLayout(false);
			this.splitContainer2.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripButton toolBack;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton toolDigData;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.ListView lvProps;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.SplitContainer splitContainer2;
		private System.Windows.Forms.Label lblData;
		private System.Windows.Forms.ListView lvMetaTable;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		private System.Windows.Forms.ColumnHeader columnHeader6;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox txtString;
		private System.Windows.Forms.ListView lvTableData;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ColumnHeader columnHeader4;
	}
}