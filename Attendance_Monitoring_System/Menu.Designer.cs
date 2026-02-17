namespace Attendance_Monitoring_System
{
    partial class Menu
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
            this.BtnBarScanner = new System.Windows.Forms.Button();
            this.s = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // BtnBarScanner
            // 
            this.BtnBarScanner.Location = new System.Drawing.Point(134, 119);
            this.BtnBarScanner.Name = "BtnBarScanner";
            this.BtnBarScanner.Size = new System.Drawing.Size(108, 23);
            this.BtnBarScanner.TabIndex = 0;
            this.BtnBarScanner.Text = "Barcode Scanner";
            this.BtnBarScanner.UseVisualStyleBackColor = true;
            this.BtnBarScanner.Click += new System.EventHandler(this.button1_Click);
            // 
            // s
            // 
            this.s.Location = new System.Drawing.Point(134, 148);
            this.s.Name = "s";
            this.s.Size = new System.Drawing.Size(108, 23);
            this.s.TabIndex = 1;
            this.s.Text = "Camera Scanner";
            this.s.UseVisualStyleBackColor = true;
            this.s.Click += new System.EventHandler(this.s_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(119, 46);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(145, 55);
            this.label1.TabIndex = 2;
            this.label1.Text = "Menu";
            // 
            // Menu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(367, 221);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.s);
            this.Controls.Add(this.BtnBarScanner);
            this.Name = "Menu";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Menu";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Menu_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BtnBarScanner;
        private System.Windows.Forms.Button s;
        private System.Windows.Forms.Label label1;
    }
}