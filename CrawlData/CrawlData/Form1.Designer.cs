namespace CrawlData
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
            this.BtnExport = new System.Windows.Forms.Button();
            this.CbbFormatter = new System.Windows.Forms.ComboBox();
            this.CbbWebsite = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // BtnExport
            // 
            this.BtnExport.Location = new System.Drawing.Point(321, 36);
            this.BtnExport.Name = "BtnExport";
            this.BtnExport.Size = new System.Drawing.Size(75, 23);
            this.BtnExport.TabIndex = 2;
            this.BtnExport.Text = "Export";
            this.BtnExport.UseVisualStyleBackColor = true;
            this.BtnExport.Click += new System.EventHandler(this.BtnExport_Click);
            // 
            // CbbFormatter
            // 
            this.CbbFormatter.FormattingEnabled = true;
            this.CbbFormatter.Location = new System.Drawing.Point(117, 88);
            this.CbbFormatter.Name = "CbbFormatter";
            this.CbbFormatter.Size = new System.Drawing.Size(176, 21);
            this.CbbFormatter.TabIndex = 3;
            this.CbbFormatter.TabStop = false;
            // 
            // CbbWebsite
            // 
            this.CbbWebsite.FormattingEnabled = true;
            this.CbbWebsite.Location = new System.Drawing.Point(117, 36);
            this.CbbWebsite.Name = "CbbWebsite";
            this.CbbWebsite.Size = new System.Drawing.Size(176, 21);
            this.CbbWebsite.TabIndex = 4;
            this.CbbWebsite.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(43, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Website";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(43, 88);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Formartter";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(437, 161);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.CbbWebsite);
            this.Controls.Add(this.CbbFormatter);
            this.Controls.Add(this.BtnExport);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Crawl Data";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BtnExport;
        private System.Windows.Forms.ComboBox CbbFormatter;
        private System.Windows.Forms.ComboBox CbbWebsite;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}

