namespace Launcher
{
	partial class LoggingForm
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
			this.CopyButton = new System.Windows.Forms.Button();
			this.Status = new System.Windows.Forms.Label();
			this.StatusLabel = new System.Windows.Forms.Label();
			this.TextBox = new System.Windows.Forms.RichTextBox();
			this.SuspendLayout();
			// 
			// CopyButton
			// 
			this.CopyButton.Location = new System.Drawing.Point(683, 524);
			this.CopyButton.Name = "CopyButton";
			this.CopyButton.Size = new System.Drawing.Size(89, 25);
			this.CopyButton.TabIndex = 1;
			this.CopyButton.Text = "Copy";
			this.CopyButton.UseVisualStyleBackColor = true;
			this.CopyButton.Click += new System.EventHandler(this.CopyButton_Click);
			// 
			// Status
			// 
			this.Status.AutoSize = true;
			this.Status.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.Status.Location = new System.Drawing.Point(70, 9);
			this.Status.Name = "Status";
			this.Status.Size = new System.Drawing.Size(30, 17);
			this.Status.TabIndex = 2;
			this.Status.Text = "Idle";
			// 
			// StatusLabel
			// 
			this.StatusLabel.AutoSize = true;
			this.StatusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.StatusLabel.Location = new System.Drawing.Point(12, 9);
			this.StatusLabel.Name = "StatusLabel";
			this.StatusLabel.Size = new System.Drawing.Size(52, 17);
			this.StatusLabel.TabIndex = 3;
			this.StatusLabel.Text = "Status:";
			// 
			// TextBox
			// 
			this.TextBox.Location = new System.Drawing.Point(12, 29);
			this.TextBox.Name = "TextBox";
			this.TextBox.ReadOnly = true;
			this.TextBox.Size = new System.Drawing.Size(665, 520);
			this.TextBox.TabIndex = 4;
			this.TextBox.Text = "";
			this.TextBox.WordWrap = false;
			// 
			// LoggingForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(784, 561);
			this.Controls.Add(this.TextBox);
			this.Controls.Add(this.StatusLabel);
			this.Controls.Add(this.Status);
			this.Controls.Add(this.CopyButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.Name = "LoggingForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Build Log";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Button CopyButton;
		private System.Windows.Forms.Label Status;
		private System.Windows.Forms.Label StatusLabel;
		private System.Windows.Forms.RichTextBox TextBox;
	}
}