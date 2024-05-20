
namespace OmokClient
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new System.Windows.Forms.Label();
            textBoxIP = new System.Windows.Forms.TextBox();
            label2 = new System.Windows.Forms.Label();
            textBoxPort = new System.Windows.Forms.TextBox();
            checkBoxLocalHostIP = new System.Windows.Forms.CheckBox();
            btnConnect = new System.Windows.Forms.Button();
            btnDisconnect = new System.Windows.Forms.Button();
            label3 = new System.Windows.Forms.Label();
            textBoxUserID = new System.Windows.Forms.TextBox();
            label4 = new System.Windows.Forms.Label();
            textBoxPs = new System.Windows.Forms.TextBox();
            button3 = new System.Windows.Forms.Button();
            groupBox1 = new System.Windows.Forms.GroupBox();
            button7 = new System.Windows.Forms.Button();
            textBoxRoomSendMsg = new System.Windows.Forms.TextBox();
            listBoxRoomChatMsg = new System.Windows.Forms.ListBox();
            listBoxRoomUserList = new System.Windows.Forms.ListBox();
            button6 = new System.Windows.Forms.Button();
            button5 = new System.Windows.Forms.Button();
            button4 = new System.Windows.Forms.Button();
            textBoxRoomNumber = new System.Windows.Forms.TextBox();
            label5 = new System.Windows.Forms.Label();
            groupBox2 = new System.Windows.Forms.GroupBox();
            TurnTimeoutLabel = new System.Windows.Forms.Label();
            label9 = new System.Windows.Forms.Label();
            UserIDLabel = new System.Windows.Forms.Label();
            label11 = new System.Windows.Forms.Label();
            LoseCountLabel = new System.Windows.Forms.Label();
            WinCountLabel = new System.Windows.Forms.Label();
            label8 = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            MatchRequestBtn = new System.Windows.Forms.Button();
            label6 = new System.Windows.Forms.Label();
            listBoxLog = new System.Windows.Forms.ListBox();
            panel1 = new System.Windows.Forms.Panel();
            GameResultText = new System.Windows.Forms.Label();
            labelStatus = new System.Windows.Forms.Label();
            btnCreateAccount = new System.Windows.Forms.Button();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(3, 11);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(81, 15);
            label1.TabIndex = 0;
            label1.Text = "API 서버 주소";
            // 
            // textBoxIP
            // 
            textBoxIP.Location = new System.Drawing.Point(90, 8);
            textBoxIP.Name = "textBoxIP";
            textBoxIP.Size = new System.Drawing.Size(85, 23);
            textBoxIP.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(181, 11);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(59, 15);
            label2.TabIndex = 2;
            label2.Text = "포트 번호";
            // 
            // textBoxPort
            // 
            textBoxPort.Location = new System.Drawing.Point(249, 9);
            textBoxPort.Name = "textBoxPort";
            textBoxPort.Size = new System.Drawing.Size(46, 23);
            textBoxPort.TabIndex = 3;
            textBoxPort.Text = "";
            // 
            // checkBoxLocalHostIP
            // 
            checkBoxLocalHostIP.AutoSize = true;
            checkBoxLocalHostIP.Checked = true;
            checkBoxLocalHostIP.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxLocalHostIP.Location = new System.Drawing.Point(304, 12);
            checkBoxLocalHostIP.Name = "checkBoxLocalHostIP";
            checkBoxLocalHostIP.Size = new System.Drawing.Size(102, 19);
            checkBoxLocalHostIP.TabIndex = 4;
            checkBoxLocalHostIP.Text = "localhost 사용";
            checkBoxLocalHostIP.UseVisualStyleBackColor = true;
            // 
            // btnConnect
            // 
            btnConnect.Enabled = false;
            btnConnect.Location = new System.Drawing.Point(418, 9);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new System.Drawing.Size(70, 24);
            btnConnect.TabIndex = 5;
            btnConnect.Text = "접속하기";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += button1_Click;
            // 
            // btnDisconnect
            // 
            btnDisconnect.Location = new System.Drawing.Point(494, 9);
            btnDisconnect.Name = "btnDisconnect";
            btnDisconnect.Size = new System.Drawing.Size(77, 24);
            btnDisconnect.TabIndex = 6;
            btnDisconnect.Text = "접속 끊기";
            btnDisconnect.UseVisualStyleBackColor = true;
            btnDisconnect.Click += btnDisconnect_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(16, 49);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(42, 15);
            label3.TabIndex = 7;
            label3.Text = "UserID";
            // 
            // textBoxUserID
            // 
            textBoxUserID.Location = new System.Drawing.Point(68, 47);
            textBoxUserID.Name = "textBoxUserID";
            textBoxUserID.Size = new System.Drawing.Size(85, 23);
            textBoxUserID.TabIndex = 8;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(173, 50);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(57, 15);
            label4.TabIndex = 9;
            label4.Text = "Password";
            // 
            // textBoxPs
            // 
            textBoxPs.Location = new System.Drawing.Point(249, 47);
            textBoxPs.Name = "textBoxPs";
            textBoxPs.Size = new System.Drawing.Size(85, 23);
            textBoxPs.TabIndex = 10;
            // 
            // button3
            // 
            button3.Location = new System.Drawing.Point(345, 45);
            button3.Name = "button3";
            button3.Size = new System.Drawing.Size(70, 24);
            button3.TabIndex = 11;
            button3.Text = "LogIn";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(button7);
            groupBox1.Controls.Add(textBoxRoomSendMsg);
            groupBox1.Controls.Add(listBoxRoomChatMsg);
            groupBox1.Controls.Add(listBoxRoomUserList);
            groupBox1.Controls.Add(button6);
            groupBox1.Controls.Add(button5);
            groupBox1.Controls.Add(button4);
            groupBox1.Controls.Add(textBoxRoomNumber);
            groupBox1.Controls.Add(label5);
            groupBox1.Location = new System.Drawing.Point(16, 74);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(399, 246);
            groupBox1.TabIndex = 12;
            groupBox1.TabStop = false;
            groupBox1.Text = "Room";
            // 
            // button7
            // 
            button7.Location = new System.Drawing.Point(340, 201);
            button7.Name = "button7";
            button7.Size = new System.Drawing.Size(44, 23);
            button7.TabIndex = 18;
            button7.Text = "채팅";
            button7.UseVisualStyleBackColor = true;
            button7.Click += button7_Click;
            // 
            // textBoxRoomSendMsg
            // 
            textBoxRoomSendMsg.Location = new System.Drawing.Point(19, 201);
            textBoxRoomSendMsg.Name = "textBoxRoomSendMsg";
            textBoxRoomSendMsg.Size = new System.Drawing.Size(315, 23);
            textBoxRoomSendMsg.TabIndex = 17;
            // 
            // listBoxRoomChatMsg
            // 
            listBoxRoomChatMsg.FormattingEnabled = true;
            listBoxRoomChatMsg.ItemHeight = 15;
            listBoxRoomChatMsg.Location = new System.Drawing.Point(123, 57);
            listBoxRoomChatMsg.Name = "listBoxRoomChatMsg";
            listBoxRoomChatMsg.Size = new System.Drawing.Size(261, 139);
            listBoxRoomChatMsg.TabIndex = 16;
            // 
            // listBoxRoomUserList
            // 
            listBoxRoomUserList.FormattingEnabled = true;
            listBoxRoomUserList.ItemHeight = 15;
            listBoxRoomUserList.Location = new System.Drawing.Point(18, 57);
            listBoxRoomUserList.Name = "listBoxRoomUserList";
            listBoxRoomUserList.Size = new System.Drawing.Size(99, 139);
            listBoxRoomUserList.TabIndex = 15;
            // 
            // button6
            // 
            button6.Location = new System.Drawing.Point(259, 24);
            button6.Name = "button6";
            button6.Size = new System.Drawing.Size(102, 23);
            button6.TabIndex = 14;
            button6.Text = "게임 Ready";
            button6.UseVisualStyleBackColor = true;
            button6.Click += button6_Click;
            // 
            // button5
            // 
            button5.Location = new System.Drawing.Point(188, 24);
            button5.Name = "button5";
            button5.Size = new System.Drawing.Size(65, 23);
            button5.TabIndex = 13;
            button5.Text = "나가기";
            button5.UseVisualStyleBackColor = true;
            button5.Click += button5_Click;
            // 
            // button4
            // 
            button4.Location = new System.Drawing.Point(117, 23);
            button4.Name = "button4";
            button4.Size = new System.Drawing.Size(65, 23);
            button4.TabIndex = 12;
            button4.Text = "입장";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // textBoxRoomNumber
            // 
            textBoxRoomNumber.Location = new System.Drawing.Point(65, 24);
            textBoxRoomNumber.Name = "textBoxRoomNumber";
            textBoxRoomNumber.Size = new System.Drawing.Size(46, 23);
            textBoxRoomNumber.TabIndex = 9;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(13, 27);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(47, 15);
            label5.TabIndex = 8;
            label5.Text = "방 번호";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(TurnTimeoutLabel);
            groupBox2.Controls.Add(label9);
            groupBox2.Controls.Add(UserIDLabel);
            groupBox2.Controls.Add(label11);
            groupBox2.Controls.Add(LoseCountLabel);
            groupBox2.Controls.Add(WinCountLabel);
            groupBox2.Controls.Add(label8);
            groupBox2.Controls.Add(label7);
            groupBox2.Controls.Add(MatchRequestBtn);
            groupBox2.Controls.Add(label6);
            groupBox2.Location = new System.Drawing.Point(421, 74);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(150, 246);
            groupBox2.TabIndex = 9;
            groupBox2.TabStop = false;
            groupBox2.Text = "나의 정보";
            // 
            // TurnTimeoutLabel
            // 
            TurnTimeoutLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            TurnTimeoutLabel.Location = new System.Drawing.Point(47, 150);
            TurnTimeoutLabel.Name = "TurnTimeoutLabel";
            TurnTimeoutLabel.Size = new System.Drawing.Size(43, 31);
            TurnTimeoutLabel.TabIndex = 0;
            TurnTimeoutLabel.Text = "-1";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(29, 124);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(99, 15);
            label9.TabIndex = 21;
            label9.Text = "나의 턴 잔여시간";
            // 
            // UserIDLabel
            // 
            UserIDLabel.AutoSize = true;
            UserIDLabel.Location = new System.Drawing.Point(83, 32);
            UserIDLabel.Name = "UserIDLabel";
            UserIDLabel.Size = new System.Drawing.Size(29, 15);
            UserIDLabel.TabIndex = 20;
            UserIDLabel.Text = "Null";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new System.Drawing.Point(29, 32);
            label11.Name = "label11";
            label11.Size = new System.Drawing.Size(49, 15);
            label11.TabIndex = 19;
            label11.Text = "UserID: ";
            // 
            // LoseCountLabel
            // 
            LoseCountLabel.AutoSize = true;
            LoseCountLabel.Location = new System.Drawing.Point(83, 82);
            LoseCountLabel.Name = "LoseCountLabel";
            LoseCountLabel.Size = new System.Drawing.Size(19, 15);
            LoseCountLabel.TabIndex = 18;
            LoseCountLabel.Text = "-1";
            // 
            // WinCountLabel
            // 
            WinCountLabel.AutoSize = true;
            WinCountLabel.Location = new System.Drawing.Point(83, 57);
            WinCountLabel.Name = "WinCountLabel";
            WinCountLabel.Size = new System.Drawing.Size(19, 15);
            WinCountLabel.TabIndex = 17;
            WinCountLabel.Text = "-1";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(29, 82);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(38, 15);
            label8.TabIndex = 16;
            label8.Text = "Lose: ";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(29, 57);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(35, 15);
            label7.TabIndex = 15;
            label7.Text = "Win: ";
            // 
            // MatchRequestBtn
            // 
            MatchRequestBtn.Enabled = false;
            MatchRequestBtn.Location = new System.Drawing.Point(29, 200);
            MatchRequestBtn.Name = "MatchRequestBtn";
            MatchRequestBtn.Size = new System.Drawing.Size(92, 23);
            MatchRequestBtn.TabIndex = 14;
            MatchRequestBtn.Text = "매칭 시작";
            MatchRequestBtn.UseVisualStyleBackColor = true;
            MatchRequestBtn.Click += button2_Click;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(173, 37);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(47, 15);
            label6.TabIndex = 8;
            label6.Text = "방 번호";
            // 
            // listBoxLog
            // 
            listBoxLog.FormattingEnabled = true;
            listBoxLog.ItemHeight = 15;
            listBoxLog.Location = new System.Drawing.Point(12, 326);
            listBoxLog.Name = "listBoxLog";
            listBoxLog.Size = new System.Drawing.Size(559, 229);
            listBoxLog.TabIndex = 17;
            // 
            // panel1
            // 
            panel1.BackColor = System.Drawing.Color.Peru;
            panel1.Controls.Add(GameResultText);
            panel1.Location = new System.Drawing.Point(584, 45);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(603, 581);
            panel1.TabIndex = 18;
            panel1.Paint += panel1_Paint;
            panel1.MouseDown += panel1_MouseDown;
            panel1.MouseMove += panel1_MouseMove;
            // 
            // GameResultText
            // 
            GameResultText.AutoSize = true;
            GameResultText.Enabled = false;
            GameResultText.Font = new System.Drawing.Font("Microsoft Sans Serif", 100F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            GameResultText.Location = new System.Drawing.Point(156, 179);
            GameResultText.Name = "GameResultText";
            GameResultText.Size = new System.Drawing.Size(328, 153);
            GameResultText.TabIndex = 22;
            GameResultText.Text = "결과!";
            GameResultText.Visible = false;
            // 
            // labelStatus
            // 
            labelStatus.AutoSize = true;
            labelStatus.Location = new System.Drawing.Point(16, 571);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new System.Drawing.Size(62, 15);
            labelStatus.TabIndex = 19;
            labelStatus.Text = "서버 상태:";
            // 
            // btnCreateAccount
            // 
            btnCreateAccount.Location = new System.Drawing.Point(421, 46);
            btnCreateAccount.Name = "btnCreateAccount";
            btnCreateAccount.Size = new System.Drawing.Size(75, 21);
            btnCreateAccount.TabIndex = 20;
            btnCreateAccount.Text = "계정 생성";
            btnCreateAccount.UseVisualStyleBackColor = true;
            btnCreateAccount.Click += btnCreateAccount_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1199, 639);
            Controls.Add(btnCreateAccount);
            Controls.Add(labelStatus);
            Controls.Add(panel1);
            Controls.Add(listBoxLog);
            Controls.Add(groupBox1);
            Controls.Add(groupBox2);
            Controls.Add(button3);
            Controls.Add(textBoxPs);
            Controls.Add(label4);
            Controls.Add(textBoxUserID);
            Controls.Add(label3);
            Controls.Add(btnDisconnect);
            Controls.Add(btnConnect);
            Controls.Add(checkBoxLocalHostIP);
            Controls.Add(textBoxPort);
            Controls.Add(label2);
            Controls.Add(textBoxIP);
            Controls.Add(label1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            Name = "MainForm";
            Text = "Form1";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxIP;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxPort;
        private System.Windows.Forms.CheckBox checkBoxLocalHostIP;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnDisconnect;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxUserID;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxPs;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.TextBox textBoxRoomSendMsg;
        private System.Windows.Forms.ListBox listBoxRoomChatMsg;
        private System.Windows.Forms.ListBox listBoxRoomUserList;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.TextBox textBoxRoomNumber;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ListBox listBoxLog;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Button btnCreateAccount;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button MatchRequestBtn;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label LoseCountLabel;
        private System.Windows.Forms.Label WinCountLabel;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label UserIDLabel;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label TurnTimeoutLabel;
        private System.Windows.Forms.Label GameResultText;
    }
}

