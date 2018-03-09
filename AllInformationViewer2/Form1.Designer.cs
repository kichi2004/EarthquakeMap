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
            this.mainPicbox = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.myPointCompoBox = new System.Windows.Forms.ComboBox();
            this.cityToArea = new System.Windows.Forms.CheckBox();
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
            this.detailTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.detailTextBox.Size = new System.Drawing.Size(220, 98);
            this.detailTextBox.TabIndex = 4;
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
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 379);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(112, 17);
            this.label1.TabIndex = 5;
            this.label1.Text = "予測震度表示地点";
            // 
            // myPointCompoBox
            // 
            this.myPointCompoBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.myPointCompoBox.FormattingEnabled = true;
            this.myPointCompoBox.Location = new System.Drawing.Point(52, 399);
            this.myPointCompoBox.Name = "myPointCompoBox";
            this.myPointCompoBox.Size = new System.Drawing.Size(148, 25);
            this.myPointCompoBox.TabIndex = 6;
            // 
            // cityToArea
            // 
            this.cityToArea.AutoSize = true;
            this.cityToArea.Location = new System.Drawing.Point(10, 355);
            this.cityToArea.Name = "cityToArea";
            this.cityToArea.Size = new System.Drawing.Size(107, 21);
            this.cityToArea.TabIndex = 7;
            this.cityToArea.Text = "地域ごとに描画";
            this.cityToArea.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(993, 435);
            this.Controls.Add(this.cityToArea);
            this.Controls.Add(this.myPointCompoBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.detailTextBox);
            this.Controls.Add(this.mainPicbox);
            this.Controls.Add(this.infoType);
            this.Controls.Add(this.nowtime);
            this.Font = new System.Drawing.Font("Yu Gothic UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "Form1";
            this.Text = "All Information Viewer 2";
            ((System.ComponentModel.ISupportInitialize)(this.mainPicbox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label nowtime;
        private System.Windows.Forms.Label infoType;
        private System.Windows.Forms.PictureBox mainPicbox;
        private System.Windows.Forms.TextBox detailTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox myPointCompoBox;
        private System.Windows.Forms.CheckBox cityToArea;
    }
}

