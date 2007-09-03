namespace Banshee.MediaEngine.Quicktime{
	partial class ActiveXControl
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
System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ActiveXControl));
this.axQTControl1 = new AxQTOControlLib.AxQTControl();
((System.ComponentModel.ISupportInitialize)(this.axQTControl1)).BeginInit();
this.SuspendLayout();
// 
// axQTControl1
// 
this.axQTControl1.Enabled = true;
this.axQTControl1.Location = new System.Drawing.Point(37, 39);
this.axQTControl1.Name = "axQTControl1";
this.axQTControl1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axQTControl1.OcxState")));
this.axQTControl1.Size = new System.Drawing.Size(228, 152);
this.axQTControl1.TabIndex = 0;
// 
// ActiveXControl
// 
this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
this.ClientSize = new System.Drawing.Size(292, 273);
this.Controls.Add(this.axQTControl1);
this.Name = "ActiveXControl";
this.Text = "ActiveXControl";
((System.ComponentModel.ISupportInitialize)(this.axQTControl1)).EndInit();
this.ResumeLayout(false);

		}

		#endregion

        private AxQTOControlLib.AxQTControl axQTControl1;
	}
}