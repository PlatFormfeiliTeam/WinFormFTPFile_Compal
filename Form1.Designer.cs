namespace WinFormFTPFile_Compal
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.btn_AutoRun = new System.Windows.Forms.Button();
            this.lbl_msg = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btn_AutoRun
            // 
            this.btn_AutoRun.Location = new System.Drawing.Point(79, 44);
            this.btn_AutoRun.Name = "btn_AutoRun";
            this.btn_AutoRun.Size = new System.Drawing.Size(94, 33);
            this.btn_AutoRun.TabIndex = 0;
            this.btn_AutoRun.Text = "AutoRun";
            this.btn_AutoRun.UseVisualStyleBackColor = true;
            this.btn_AutoRun.Click += new System.EventHandler(this.btn_AutoRun_Click);
            // 
            // lbl_msg
            // 
            this.lbl_msg.AutoSize = true;
            this.lbl_msg.Location = new System.Drawing.Point(96, 105);
            this.lbl_msg.Name = "lbl_msg";
            this.lbl_msg.Size = new System.Drawing.Size(41, 12);
            this.lbl_msg.TabIndex = 1;
            this.lbl_msg.Text = "label1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(550, 522);
            this.Controls.Add(this.lbl_msg);
            this.Controls.Add(this.btn_AutoRun);
            this.Name = "Form1";
            this.Text = "Compal Ftp File";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_AutoRun;
        private System.Windows.Forms.Label lbl_msg;
    }
}

