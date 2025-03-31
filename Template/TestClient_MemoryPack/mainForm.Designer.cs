namespace csharp_test_client
{
    partial class mainForm
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            btnDisconnect = new System.Windows.Forms.Button();
            btnConnect = new System.Windows.Forms.Button();
            groupBox5 = new System.Windows.Forms.GroupBox();
            textBoxPort = new System.Windows.Forms.TextBox();
            label10 = new System.Windows.Forms.Label();
            checkBoxLocalHostIP = new System.Windows.Forms.CheckBox();
            textBoxIP = new System.Windows.Forms.TextBox();
            label9 = new System.Windows.Forms.Label();
            labelStatus = new System.Windows.Forms.Label();
            listBoxLog = new System.Windows.Forms.ListBox();
            button3 = new System.Windows.Forms.Button();
            textBox3 = new System.Windows.Forms.TextBox();
            groupBox5.SuspendLayout();
            SuspendLayout();
            // 
            // btnDisconnect
            // 
            btnDisconnect.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 129);
            btnDisconnect.Location = new System.Drawing.Point(421, 55);
            btnDisconnect.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            btnDisconnect.Name = "btnDisconnect";
            btnDisconnect.Size = new System.Drawing.Size(88, 32);
            btnDisconnect.TabIndex = 29;
            btnDisconnect.Text = "접속 끊기";
            btnDisconnect.UseVisualStyleBackColor = true;
            btnDisconnect.Click += btnDisconnect_Click;
            // 
            // btnConnect
            // 
            btnConnect.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 129);
            btnConnect.Location = new System.Drawing.Point(420, 20);
            btnConnect.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new System.Drawing.Size(88, 32);
            btnConnect.TabIndex = 28;
            btnConnect.Text = "접속하기";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            // 
            // groupBox5
            // 
            groupBox5.Controls.Add(textBoxPort);
            groupBox5.Controls.Add(label10);
            groupBox5.Controls.Add(checkBoxLocalHostIP);
            groupBox5.Controls.Add(textBoxIP);
            groupBox5.Controls.Add(label9);
            groupBox5.Location = new System.Drawing.Point(12, 15);
            groupBox5.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            groupBox5.Name = "groupBox5";
            groupBox5.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            groupBox5.Size = new System.Drawing.Size(403, 65);
            groupBox5.TabIndex = 27;
            groupBox5.TabStop = false;
            groupBox5.Text = "Socket 더미 클라이언트 설정";
            // 
            // textBoxPort
            // 
            textBoxPort.Location = new System.Drawing.Point(225, 25);
            textBoxPort.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            textBoxPort.MaxLength = 6;
            textBoxPort.Name = "textBoxPort";
            textBoxPort.Size = new System.Drawing.Size(51, 23);
            textBoxPort.TabIndex = 18;
            textBoxPort.Text = "32452";
            textBoxPort.WordWrap = false;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new System.Drawing.Point(163, 30);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(62, 15);
            label10.TabIndex = 17;
            label10.Text = "포트 번호:";
            // 
            // checkBoxLocalHostIP
            // 
            checkBoxLocalHostIP.AutoSize = true;
            checkBoxLocalHostIP.Checked = true;
            checkBoxLocalHostIP.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxLocalHostIP.Location = new System.Drawing.Point(285, 30);
            checkBoxLocalHostIP.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            checkBoxLocalHostIP.Name = "checkBoxLocalHostIP";
            checkBoxLocalHostIP.Size = new System.Drawing.Size(102, 19);
            checkBoxLocalHostIP.TabIndex = 15;
            checkBoxLocalHostIP.Text = "localhost 사용";
            checkBoxLocalHostIP.UseVisualStyleBackColor = true;
            // 
            // textBoxIP
            // 
            textBoxIP.Location = new System.Drawing.Point(68, 24);
            textBoxIP.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            textBoxIP.MaxLength = 6;
            textBoxIP.Name = "textBoxIP";
            textBoxIP.Size = new System.Drawing.Size(87, 23);
            textBoxIP.TabIndex = 11;
            textBoxIP.Text = "0.0.0.0";
            textBoxIP.WordWrap = false;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(6, 29);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(62, 15);
            label9.TabIndex = 10;
            label9.Text = "서버 주소:";
            // 
            // labelStatus
            // 
            labelStatus.AutoSize = true;
            labelStatus.Location = new System.Drawing.Point(10, 571);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new System.Drawing.Size(112, 15);
            labelStatus.TabIndex = 40;
            labelStatus.Text = "서버 접속 상태: ???";
            // 
            // listBoxLog
            // 
            listBoxLog.FormattingEnabled = true;
            listBoxLog.HorizontalScrollbar = true;
            listBoxLog.ItemHeight = 15;
            listBoxLog.Location = new System.Drawing.Point(10, 384);
            listBoxLog.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            listBoxLog.Name = "listBoxLog";
            listBoxLog.Size = new System.Drawing.Size(490, 169);
            listBoxLog.TabIndex = 41;
            // 
            // button3
            // 
            button3.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 129);
            button3.Location = new System.Drawing.Point(414, 95);
            button3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            button3.Name = "button3";
            button3.Size = new System.Drawing.Size(92, 32);
            button3.TabIndex = 41;
            button3.Text = "echo 보내기";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // textBox3
            // 
            textBox3.Location = new System.Drawing.Point(16, 99);
            textBox3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            textBox3.MaxLength = 32;
            textBox3.Name = "textBox3";
            textBox3.Size = new System.Drawing.Size(392, 23);
            textBox3.TabIndex = 40;
            textBox3.Text = "test1";
            textBox3.WordWrap = false;
            // 
            // mainForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(524, 590);
            Controls.Add(button3);
            Controls.Add(textBox3);
            Controls.Add(labelStatus);
            Controls.Add(listBoxLog);
            Controls.Add(btnDisconnect);
            Controls.Add(btnConnect);
            Controls.Add(groupBox5);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            Name = "mainForm";
            Text = "네트워크 테스트 클라이언트";
            FormClosing += mainForm_FormClosing;
            Load += mainForm_Load;
            groupBox5.ResumeLayout(false);
            groupBox5.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btnDisconnect;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.TextBox textBoxPort;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.CheckBox checkBoxLocalHostIP;
        private System.Windows.Forms.TextBox textBoxIP;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.ListBox listBoxLog;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TextBox textBox3;
    }
}

