namespace MoonSharp.Debugger
{
	partial class SourceCodeDebugControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.vertScroll = new System.Windows.Forms.VScrollBar();
			this.horizScroll = new System.Windows.Forms.HScrollBar();
			this.SuspendLayout();
			// 
			// vertScroll
			// 
			this.vertScroll.Dock = System.Windows.Forms.DockStyle.Right;
			this.vertScroll.Location = new System.Drawing.Point(939, 0);
			this.vertScroll.Name = "vertScroll";
			this.vertScroll.Size = new System.Drawing.Size(17, 656);
			this.vertScroll.TabIndex = 0;
			this.vertScroll.Scroll += new System.Windows.Forms.ScrollEventHandler(this.vertScroll_Scroll);
			this.vertScroll.ValueChanged += new System.EventHandler(this.vertScroll_ValueChanged);
			// 
			// horizScroll
			// 
			this.horizScroll.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.horizScroll.Location = new System.Drawing.Point(0, 639);
			this.horizScroll.Name = "horizScroll";
			this.horizScroll.Size = new System.Drawing.Size(939, 17);
			this.horizScroll.TabIndex = 1;
			this.horizScroll.Scroll += new System.Windows.Forms.ScrollEventHandler(this.horizScroll_Scroll);
			this.horizScroll.ValueChanged += new System.EventHandler(this.horizScroll_ValueChanged);
			// 
			// SourceCodeDebugControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 18F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.Controls.Add(this.horizScroll);
			this.Controls.Add(this.vertScroll);
			this.DoubleBuffered = true;
			this.Font = new System.Drawing.Font("Consolas", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ForeColor = System.Drawing.Color.Gainsboro;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.Name = "SourceCodeDebugControl";
			this.Size = new System.Drawing.Size(956, 656);
			this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.SourceCodeDebugControl_MouseClick);
			this.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.SourceCodeDebugControl_PreviewKeyDown);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.VScrollBar vertScroll;
		private System.Windows.Forms.HScrollBar horizScroll;
	}
}
