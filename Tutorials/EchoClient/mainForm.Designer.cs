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
            button1 = new System.Windows.Forms.Button();
            textSendText = new System.Windows.Forms.TextBox();
            labelStatus = new System.Windows.Forms.Label();
            listBoxLog = new System.Windows.Forms.ListBox();
            label1 = new System.Windows.Forms.Label();
            textBoxUserID = new System.Windows.Forms.TextBox();
            textBoxUserPW = new System.Windows.Forms.TextBox();
            label2 = new System.Windows.Forms.Label();
            button2 = new System.Windows.Forms.Button();
            Room = new System.Windows.Forms.GroupBox();
            textBoxRelay = new System.Windows.Forms.TextBox();
            btnRoomRelay = new System.Windows.Forms.Button();
            btnRoomChat = new System.Windows.Forms.Button();
            textBoxRoomSendMsg = new System.Windows.Forms.TextBox();
            listBoxRoomChatMsg = new System.Windows.Forms.ListBox();
            label4 = new System.Windows.Forms.Label();
            listBoxRoomUserList = new System.Windows.Forms.ListBox();
            btn_RoomLeave = new System.Windows.Forms.Button();
            btn_RoomEnter = new System.Windows.Forms.Button();
            textBoxRoomNumber = new System.Windows.Forms.TextBox();
            label3 = new System.Windows.Forms.Label();
            groupBox5.SuspendLayout();
            Room.SuspendLayout();
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
            // button1
            // 
            button1.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 129);
            button1.Location = new System.Drawing.Point(319, 88);
            button1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(100, 32);
            button1.TabIndex = 39;
            button1.Text = "echo 보내기";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // textSendText
            // 
            textSendText.Location = new System.Drawing.Point(12, 92);
            textSendText.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            textSendText.MaxLength = 32;
            textSendText.Name = "textSendText";
            textSendText.Size = new System.Drawing.Size(301, 23);
            textSendText.TabIndex = 38;
            textSendText.Text = "test1";
            textSendText.WordWrap = false;
            // 
            // labelStatus
            // 
            labelStatus.AutoSize = true;
            labelStatus.Location = new System.Drawing.Point(10, 719);
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
            listBoxLog.Location = new System.Drawing.Point(10, 532);
            listBoxLog.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            listBoxLog.Name = "listBoxLog";
            listBoxLog.Size = new System.Drawing.Size(490, 169);
            listBoxLog.TabIndex = 41;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(10, 153);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(45, 15);
            label1.TabIndex = 42;
            label1.Text = "UserID:";
            // 
            // textBoxUserID
            // 
            textBoxUserID.Location = new System.Drawing.Point(62, 149);
            textBoxUserID.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            textBoxUserID.MaxLength = 6;
            textBoxUserID.Name = "textBoxUserID";
            textBoxUserID.Size = new System.Drawing.Size(87, 23);
            textBoxUserID.TabIndex = 43;
            textBoxUserID.Text = "jacking75";
            textBoxUserID.WordWrap = false;
            // 
            // textBoxUserPW
            // 
            textBoxUserPW.Location = new System.Drawing.Point(220, 150);
            textBoxUserPW.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            textBoxUserPW.MaxLength = 6;
            textBoxUserPW.Name = "textBoxUserPW";
            textBoxUserPW.Size = new System.Drawing.Size(87, 23);
            textBoxUserPW.TabIndex = 45;
            textBoxUserPW.Text = "jacking75";
            textBoxUserPW.WordWrap = false;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(158, 154);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(53, 15);
            label2.TabIndex = 44;
            label2.Text = "PassWD:";
            // 
            // button2
            // 
            button2.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 129);
            button2.Location = new System.Drawing.Point(316, 144);
            button2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(100, 32);
            button2.TabIndex = 46;
            button2.Text = "Login";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // Room
            // 
            Room.Controls.Add(textBoxRelay);
            Room.Controls.Add(btnRoomRelay);
            Room.Controls.Add(btnRoomChat);
            Room.Controls.Add(textBoxRoomSendMsg);
            Room.Controls.Add(listBoxRoomChatMsg);
            Room.Controls.Add(label4);
            Room.Controls.Add(listBoxRoomUserList);
            Room.Controls.Add(btn_RoomLeave);
            Room.Controls.Add(btn_RoomEnter);
            Room.Controls.Add(textBoxRoomNumber);
            Room.Controls.Add(label3);
            Room.Location = new System.Drawing.Point(13, 213);
            Room.Name = "Room";
            Room.Size = new System.Drawing.Size(495, 312);
            Room.TabIndex = 47;
            Room.TabStop = false;
            Room.Text = "Room";
            // 
            // textBoxRelay
            // 
            textBoxRelay.Location = new System.Drawing.Point(306, 30);
            textBoxRelay.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            textBoxRelay.MaxLength = 6;
            textBoxRelay.Name = "textBoxRelay";
            textBoxRelay.Size = new System.Drawing.Size(109, 23);
            textBoxRelay.TabIndex = 55;
            textBoxRelay.Text = "test";
            textBoxRelay.WordWrap = false;
            // 
            // btnRoomRelay
            // 
            btnRoomRelay.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 129);
            btnRoomRelay.Location = new System.Drawing.Point(420, 25);
            btnRoomRelay.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            btnRoomRelay.Name = "btnRoomRelay";
            btnRoomRelay.Size = new System.Drawing.Size(66, 32);
            btnRoomRelay.TabIndex = 54;
            btnRoomRelay.Text = "Relay";
            btnRoomRelay.UseVisualStyleBackColor = true;
            btnRoomRelay.Click += btnRoomRelay_Click;
            // 
            // btnRoomChat
            // 
            btnRoomChat.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 129);
            btnRoomChat.Location = new System.Drawing.Point(437, 265);
            btnRoomChat.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            btnRoomChat.Name = "btnRoomChat";
            btnRoomChat.Size = new System.Drawing.Size(50, 32);
            btnRoomChat.TabIndex = 53;
            btnRoomChat.Text = "chat";
            btnRoomChat.UseVisualStyleBackColor = true;
            btnRoomChat.Click += btnRoomChat_Click;
            // 
            // textBoxRoomSendMsg
            // 
            textBoxRoomSendMsg.Location = new System.Drawing.Point(13, 269);
            textBoxRoomSendMsg.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            textBoxRoomSendMsg.MaxLength = 32;
            textBoxRoomSendMsg.Name = "textBoxRoomSendMsg";
            textBoxRoomSendMsg.Size = new System.Drawing.Size(419, 23);
            textBoxRoomSendMsg.TabIndex = 52;
            textBoxRoomSendMsg.Text = "test1";
            textBoxRoomSendMsg.WordWrap = false;
            // 
            // listBoxRoomChatMsg
            // 
            listBoxRoomChatMsg.FormattingEnabled = true;
            listBoxRoomChatMsg.ItemHeight = 15;
            listBoxRoomChatMsg.Location = new System.Drawing.Point(144, 81);
            listBoxRoomChatMsg.Name = "listBoxRoomChatMsg";
            listBoxRoomChatMsg.Size = new System.Drawing.Size(343, 169);
            listBoxRoomChatMsg.TabIndex = 51;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(10, 64);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(55, 15);
            label4.TabIndex = 50;
            label4.Text = "User List:";
            // 
            // listBoxRoomUserList
            // 
            listBoxRoomUserList.FormattingEnabled = true;
            listBoxRoomUserList.ItemHeight = 15;
            listBoxRoomUserList.Location = new System.Drawing.Point(13, 82);
            listBoxRoomUserList.Name = "listBoxRoomUserList";
            listBoxRoomUserList.Size = new System.Drawing.Size(123, 169);
            listBoxRoomUserList.TabIndex = 49;
            // 
            // btn_RoomLeave
            // 
            btn_RoomLeave.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 129);
            btn_RoomLeave.Location = new System.Drawing.Point(216, 24);
            btn_RoomLeave.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            btn_RoomLeave.Name = "btn_RoomLeave";
            btn_RoomLeave.Size = new System.Drawing.Size(66, 32);
            btn_RoomLeave.TabIndex = 48;
            btn_RoomLeave.Text = "Leave";
            btn_RoomLeave.UseVisualStyleBackColor = true;
            btn_RoomLeave.Click += btn_RoomLeave_Click;
            // 
            // btn_RoomEnter
            // 
            btn_RoomEnter.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 129);
            btn_RoomEnter.Location = new System.Drawing.Point(144, 23);
            btn_RoomEnter.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            btn_RoomEnter.Name = "btn_RoomEnter";
            btn_RoomEnter.Size = new System.Drawing.Size(66, 32);
            btn_RoomEnter.TabIndex = 47;
            btn_RoomEnter.Text = "Enter";
            btn_RoomEnter.UseVisualStyleBackColor = true;
            btn_RoomEnter.Click += btn_RoomEnter_Click;
            // 
            // textBoxRoomNumber
            // 
            textBoxRoomNumber.Location = new System.Drawing.Point(98, 25);
            textBoxRoomNumber.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            textBoxRoomNumber.MaxLength = 6;
            textBoxRoomNumber.Name = "textBoxRoomNumber";
            textBoxRoomNumber.Size = new System.Drawing.Size(38, 23);
            textBoxRoomNumber.TabIndex = 44;
            textBoxRoomNumber.Text = "0";
            textBoxRoomNumber.WordWrap = false;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(5, 31);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(90, 15);
            label3.TabIndex = 43;
            label3.Text = "Room Number:";
            // 
            // mainForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(524, 751);
            Controls.Add(Room);
            Controls.Add(button2);
            Controls.Add(textBoxUserPW);
            Controls.Add(label2);
            Controls.Add(textBoxUserID);
            Controls.Add(label1);
            Controls.Add(labelStatus);
            Controls.Add(listBoxLog);
            Controls.Add(button1);
            Controls.Add(textSendText);
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
            Room.ResumeLayout(false);
            Room.PerformLayout();
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
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textSendText;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.ListBox listBoxLog;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxUserID;
        private System.Windows.Forms.TextBox textBoxUserPW;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.GroupBox Room;
        private System.Windows.Forms.Button btn_RoomLeave;
        private System.Windows.Forms.Button btn_RoomEnter;
        private System.Windows.Forms.TextBox textBoxRoomNumber;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnRoomChat;
        private System.Windows.Forms.TextBox textBoxRoomSendMsg;
        private System.Windows.Forms.ListBox listBoxRoomChatMsg;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListBox listBoxRoomUserList;
        private System.Windows.Forms.Button btnRoomRelay;
        private System.Windows.Forms.TextBox textBoxRelay;
    }
}

