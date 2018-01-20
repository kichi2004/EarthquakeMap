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
            this.detailTextBox = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.mainPicbox = new System.Windows.Forms.PictureBox();
            this.kyoshinMonitor = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.mainPicbox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.kyoshinMonitor)).BeginInit();
            this.SuspendLayout();
            // 
            // nowtime
            // 
            this.nowtime.Font = new System.Drawing.Font("Yu Gothic UI", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.nowtime.Location = new System.Drawing.Point(2, -13);
            this.nowtime.Name = "nowtime";
            this.nowtime.Size = new System.Drawing.Size(218, 68);
            this.nowtime.TabIndex = 0;
            this.nowtime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // infoType
            // 
            this.infoType.BackColor = System.Drawing.Color.Transparent;
            this.infoType.Font = new System.Drawing.Font("Yu Gothic UI", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.infoType.Location = new System.Drawing.Point(2, 41);
            this.infoType.Name = "infoType";
            this.infoType.Size = new System.Drawing.Size(218, 49);
            this.infoType.TabIndex = 1;
            this.infoType.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // detailTextBox
            // 
            this.detailTextBox.BackColor = System.Drawing.Color.LightGray;
            this.detailTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.detailTextBox.Font = new System.Drawing.Font("Yu Gothic UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.detailTextBox.Location = new System.Drawing.Point(0, 90);
            this.detailTextBox.Multiline = true;
            this.detailTextBox.Name = "detailTextBox";
            this.detailTextBox.ReadOnly = true;
            this.detailTextBox.Size = new System.Drawing.Size(220, 98);
            this.detailTextBox.TabIndex = 4;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.White;
            this.pictureBox1.Image = global::AllInformationViewer2.Properties.Resources.logo_anied;
            this.pictureBox1.Location = new System.Drawing.Point(96, 394);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(118, 37);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 5;
            this.pictureBox1.TabStop = false;
            // 
            // mainPicbox
            // 
            this.mainPicbox.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.mainPicbox.Location = new System.Drawing.Point(220, 0);
            this.mainPicbox.Name = "mainPicbox";
            this.mainPicbox.Size = new System.Drawing.Size(773, 435);
            this.mainPicbox.TabIndex = 3;
            this.mainPicbox.TabStop = false;
            // 
            // kyoshinMonitor
            // 
            this.kyoshinMonitor.BackgroundImage = global::AllInformationViewer2.Properties.Resources.basemap_new_black;
            this.kyoshinMonitor.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.kyoshinMonitor.Location = new System.Drawing.Point(0, 185);
            this.kyoshinMonitor.Name = "kyoshinMonitor";
            this.kyoshinMonitor.Size = new System.Drawing.Size(220, 250);
            this.kyoshinMonitor.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.kyoshinMonitor.TabIndex = 2;
            this.kyoshinMonitor.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(993, 435);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.detailTextBox);
            this.Controls.Add(this.mainPicbox);
            this.Controls.Add(this.kyoshinMonitor);
            this.Controls.Add(this.infoType);
            this.Controls.Add(this.nowtime);
            this.Font = new System.Drawing.Font("Yu Gothic UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "Form1";
            this.Text = "All Information Viewer 2";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.mainPicbox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.kyoshinMonitor)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label nowtime;
        private System.Windows.Forms.Label infoType;
        private System.Windows.Forms.PictureBox kyoshinMonitor;
        private System.Windows.Forms.PictureBox mainPicbox;
        private System.Windows.Forms.TextBox detailTextBox;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}

