namespace EarthquakeMap
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
            this.components = new System.ComponentModel.Container();
            this.nowtime = new System.Windows.Forms.Label();
            this.infoType = new System.Windows.Forms.Label();
            this.detailTextBox = new System.Windows.Forms.TextBox();
            this.mainPicbox = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.myPointComboBox = new System.Windows.Forms.ComboBox();
            this.cityToArea = new System.Windows.Forms.CheckBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.redrawButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.keepSetting = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
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
            this.infoType.Size = new System.Drawing.Size(218, 51);
            this.infoType.TabIndex = 1;
            this.infoType.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // detailTextBox
            // 
            this.detailTextBox.BackColor = System.Drawing.Color.LightGray;
            this.detailTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.detailTextBox.Font = new System.Drawing.Font("Yu Gothic UI", 10F);
            this.detailTextBox.Location = new System.Drawing.Point(0, 93);
            this.detailTextBox.Multiline = true;
            this.detailTextBox.Name = "detailTextBox";
            this.detailTextBox.ReadOnly = true;
            this.detailTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.detailTextBox.Size = new System.Drawing.Size(220, 190);
            this.detailTextBox.TabIndex = 4;
            // 
            // mainPicbox
            // 
            this.mainPicbox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.mainPicbox.BackgroundImage = global::EarthquakeMap.Properties.Resources.loading;
            this.mainPicbox.Location = new System.Drawing.Point(220, 0);
            this.mainPicbox.Name = "mainPicbox";
            this.mainPicbox.Size = new System.Drawing.Size(773, 435);
            this.mainPicbox.TabIndex = 3;
            this.mainPicbox.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 408);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 17);
            this.label1.TabIndex = 5;
            this.label1.Text = "予測地点";
            // 
            // myPointComboBox
            // 
            this.myPointComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.myPointComboBox.FormattingEnabled = true;
            this.myPointComboBox.Location = new System.Drawing.Point(66, 405);
            this.myPointComboBox.Name = "myPointComboBox";
            this.myPointComboBox.Size = new System.Drawing.Size(148, 25);
            this.myPointComboBox.TabIndex = 6;
            // 
            // cityToArea
            // 
            this.cityToArea.AutoSize = true;
            this.cityToArea.Location = new System.Drawing.Point(113, 326);
            this.cityToArea.Name = "cityToArea";
            this.cityToArea.Size = new System.Drawing.Size(107, 21);
            this.cityToArea.TabIndex = 7;
            this.cityToArea.Text = "描画を地域ごと";
            this.cityToArea.UseVisualStyleBackColor = true;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(5, 305);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(79, 21);
            this.checkBox1.TabIndex = 8;
            this.checkBox1.Text = "地震情報";
            this.toolTip1.SetToolTip(this.checkBox1, "地震情報の地図で、一定の震度以上の範囲で切り取ります。(readme参照)");
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(90, 305);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(105, 21);
            this.checkBox2.TabIndex = 9;
            this.checkBox2.Text = "緊急地震速報";
            this.toolTip1.SetToolTip(this.checkBox2, "緊急地震速報の予測震度マップで、一定の震度以上の範囲で切り取ります。(readme参照)");
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // redrawButton
            // 
            this.redrawButton.Location = new System.Drawing.Point(5, 374);
            this.redrawButton.Name = "redrawButton";
            this.redrawButton.Size = new System.Drawing.Size(209, 25);
            this.redrawButton.TabIndex = 10;
            this.redrawButton.Text = "地震情報再取得・描画";
            this.redrawButton.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(2, 286);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(163, 17);
            this.label2.TabIndex = 11;
            this.label2.Text = "一定の震度の範囲で切り取る";
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.Location = new System.Drawing.Point(5, 326);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(107, 21);
            this.checkBox3.TabIndex = 12;
            this.checkBox3.Text = "予測を地方ごと";
            this.toolTip1.SetToolTip(this.checkBox3, "緊急地震速報の府県ごとの予測震度を地方ごとに表示します。");
            this.checkBox3.UseVisualStyleBackColor = true;
            // 
            // keepSetting
            // 
            this.keepSetting.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.keepSetting.FormattingEnabled = true;
            this.keepSetting.Items.AddRange(new object[] {
            "維持しない",
            "震度3以上",
            "震度4以上",
            "震度5弱以上",
            "震度5強以上",
            "震度6弱以上",
            "震度6強以上",
            "震度7",
            "切り替えない"});
            this.keepSetting.Location = new System.Drawing.Point(74, 347);
            this.keepSetting.Name = "keepSetting";
            this.keepSetting.Size = new System.Drawing.Size(140, 25);
            this.keepSetting.TabIndex = 14;
            this.toolTip1.SetToolTip(this.keepSetting, "現在表示されている地震情報の表示を維持するか設定します。\r\n設定した震度以上の情報（緊急地震速報or地震情報）が発表された場合、この設定は解除されます。\r\n「切り" +
        "替えない」を選択した場合は、手動で解除するまでこの情報が維持されます。");
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(2, 350);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(70, 17);
            this.label3.TabIndex = 13;
            this.label3.Text = "情報を維持";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(993, 435);
            this.Controls.Add(this.keepSetting);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.checkBox3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.redrawButton);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.cityToArea);
            this.Controls.Add(this.myPointComboBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.detailTextBox);
            this.Controls.Add(this.mainPicbox);
            this.Controls.Add(this.infoType);
            this.Controls.Add(this.nowtime);
            this.Font = new System.Drawing.Font("Yu Gothic UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "EarthquakeMap";
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
        private System.Windows.Forms.ComboBox myPointComboBox;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox checkBox2;
        internal System.Windows.Forms.CheckBox cityToArea;
        private System.Windows.Forms.Button redrawButton;
        private System.Windows.Forms.Label label2;
        internal System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label label3;
        internal System.Windows.Forms.ComboBox keepSetting;
    }
}

