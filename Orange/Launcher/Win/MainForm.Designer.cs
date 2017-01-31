namespace Launcher
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
			if (disposing && (components != null)) {
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
			this.Logo = new System.Windows.Forms.PictureBox();
			this.Label = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.Logo)).BeginInit();
			this.SuspendLayout();
			// 
			// Logo
			// 
			this.Logo.Image = global::Launcher.Properties.Resources.Logo;
			this.Logo.InitialImage = null;
			this.Logo.Location = new System.Drawing.Point(0, 0);
			this.Logo.Name = "Logo";
			this.Logo.Size = new System.Drawing.Size(256, 256);
			this.Logo.TabIndex = 0;
			this.Logo.TabStop = false;
			this.Logo.UseWaitCursor = true;
			// 
			// Label
			// 
			this.Label.AutoSize = true;
			this.Label.Font = new System.Drawing.Font("Microsoft Sans Serif", 30F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.Label.Location = new System.Drawing.Point(47, 252);
			this.Label.Name = "Label";
			this.Label.Size = new System.Drawing.Size(162, 46);
			this.Label.TabIndex = 2;
			this.Label.Text = "Loading";
			this.Label.UseWaitCursor = true;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(256, 300);
			this.Controls.Add(this.Label);
			this.Controls.Add(this.Logo);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "MainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "MainForm";
			this.UseWaitCursor = true;
			((System.ComponentModel.ISupportInitialize)(this.Logo)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox Logo;
		private System.Windows.Forms.Label Label;
	}
}