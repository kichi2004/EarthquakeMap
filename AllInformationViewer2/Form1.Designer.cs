namespace AllInformationViewer2
{
    partial class Form1
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.nowtime = new System.Windows.Forms.Label();
            this.infoType = new System.Windows.Forms.Label();
            this.kyoshinMonitor = new System.Windows.Forms.PictureBox();
            this.mainPicbox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.kyoshinMonitor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.mainPicbox)).BeginInit();
            this.SuspendLayout();
            // 
            // nowtime
            // 
            this.nowtime.Font = new System.Drawing.Font("Yu Gothic UI", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.nowtime.Location = new System.Drawing.Point(2, -13);
            this.nowtime.Name = "nowtime";
            this.nowtime.Size = new System.Drawing.Size(218, 68);
            this.nowtime.TabIndex = 0;
            this.nowtime.Text = "07:00:00";
            this.nowtime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // infoType
            // 
            this.infoType.BackColor = System.Drawing.Color.Transparent;
            this.infoType.Font = new System.Drawing.Font("Yu Gothic UI", 27.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.infoType.Location = new System.Drawing.Point(2, 41);
            this.infoType.Name = "infoType";
            this.infoType.Size = new System.Drawing.Size(218, 51);
            this.infoType.TabIndex = 1;
            this.infoType.Text = "各地の震度";
            this.infoType.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // kyoshinMonitor
            // 
            this.kyoshinMonitor.Location = new System.Drawing.Point(0, 90);
            this.kyoshinMonitor.Name = "kyoshinMonitor";
            this.kyoshinMonitor.Size = new System.Drawing.Size(220, 250);
            this.kyoshinMonitor.TabIndex = 2;
            this.kyoshinMonitor.TabStop = false;
            // 
            // mainPicbox
            // 
            this.mainPicbox.Location = new System.Drawing.Point(219, 0);
            this.mainPicbox.Name = "mainPicbox";
            this.mainPicbox.Size = new System.Drawing.Size(523, 340);
            this.mainPicbox.TabIndex = 3;
            this.mainPicbox.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(742, 340);
            this.Controls.Add(this.mainPicbox);
            this.Controls.Add(this.kyoshinMonitor);
            this.Controls.Add(this.infoType);
            this.Controls.Add(this.nowtime);
            this.Font = new System.Drawing.Font("Yu Gothic UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.kyoshinMonitor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.mainPicbox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label nowtime;
        private System.Windows.Forms.Label infoType;
        private System.Windows.Forms.PictureBox kyoshinMonitor;
        private System.Windows.Forms.PictureBox mainPicbox;
    }
}

