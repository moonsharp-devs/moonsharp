namespace NugetTests_net35
{
	partial class Form1
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
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.lblVersion = new System.Windows.Forms.Label();
			this.lblTestResult = new System.Windows.Forms.Label();
			this.button1 = new System.Windows.Forms.Button();
			this.label5 = new System.Windows.Forms.Label();
			this.lblPlatform = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(34, 33);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(96, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "Version check :";
			this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(48, 146);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(81, 16);
			this.label2.TabIndex = 0;
			this.label2.Text = "Simple test :";
			this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// lblVersion
			// 
			this.lblVersion.AutoSize = true;
			this.lblVersion.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblVersion.Location = new System.Drawing.Point(136, 33);
			this.lblVersion.Name = "lblVersion";
			this.lblVersion.Size = new System.Drawing.Size(71, 16);
			this.lblVersion.TabIndex = 1;
			this.lblVersion.Text = "lblVersion";
			// 
			// lblTestResult
			// 
			this.lblTestResult.AutoSize = true;
			this.lblTestResult.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblTestResult.Location = new System.Drawing.Point(136, 146);
			this.lblTestResult.Name = "lblTestResult";
			this.lblTestResult.Size = new System.Drawing.Size(91, 16);
			this.lblTestResult.TabIndex = 2;
			this.lblTestResult.Text = "lblTestResult";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(52, 234);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(205, 59);
			this.button1.TabIndex = 3;
			this.button1.Text = "TryDebugger";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(32, 90);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(97, 16);
			this.label5.TabIndex = 0;
			this.label5.Text = "Platform check:";
			this.label5.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// lblPlatform
			// 
			this.lblPlatform.AutoSize = true;
			this.lblPlatform.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblPlatform.Location = new System.Drawing.Point(136, 90);
			this.lblPlatform.Name = "lblPlatform";
			this.lblPlatform.Size = new System.Drawing.Size(71, 16);
			this.lblPlatform.TabIndex = 1;
			this.lblPlatform.Text = "lblVersion";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(311, 330);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.lblTestResult);
			this.Controls.Add(this.lblPlatform);
			this.Controls.Add(this.lblVersion);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.Name = "Form1";
			this.Text = "Nuget on .NET 4.5";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label lblVersion;
		private System.Windows.Forms.Label lblTestResult;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label lblPlatform;
	}
}

