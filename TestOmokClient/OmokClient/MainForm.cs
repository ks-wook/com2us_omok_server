using MemoryPack;
using OmokClient;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OmokClient
{
    public partial class MainForm : Form
    {        
        public MainForm()
        {
            InitializeComponent();
        }


        

        private void MainForm_Load(object sender, EventArgs e)
        {
            PacketBuffer.Init((8096 * 10), MemoryPackPacketHeadInfo.HeadSize, 2048);

            IsNetworkThreadRunning = true;
            NetworkReadThread = new System.Threading.Thread(this.NetworkReadProcess);
            NetworkReadThread.Start();
            NetworkSendThread = new System.Threading.Thread(this.NetworkSendProcess);
            NetworkSendThread.Start();

            IsBackGroundProcessRunning = true;
            dispatcherUITimer.Tick += new EventHandler(BackGroundProcess);
            dispatcherUITimer.Interval = 100;
            dispatcherUITimer.Start();

            btnDisconnect.Enabled = false;

            SetPacketHandler();


            Omok_Init();
            DevLog.Write("프로그램 시작 !!!", LOG_LEVEL.INFO);

            timer.Elapsed += TimerElapsed;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            IsNetworkThreadRunning = false;
            IsBackGroundProcessRunning = false;

            Network.Close();
        }


        // 접속하기
        private void button1_Click(object sender, EventArgs e)
        {
            string address = textBoxIP.Text;

            if (checkBoxLocalHostIP.Checked)
            {
                address = "127.0.0.1";
            }

            int port = Convert.ToInt32(textBoxPort.Text);

            if (Network.Connect(address, port))
            {
                labelStatus.Text = string.Format("{0}. 서버에 접속 중", DateTime.Now);
                btnConnect.Enabled = false;
                btnDisconnect.Enabled = true;

                DevLog.Write($"서버에 접속 중", LOG_LEVEL.INFO);
            }
            else
            {
                labelStatus.Text = string.Format("{0}. 서버에 접속 실패", DateTime.Now);
            }
        }

        // 접속 끊기
        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            SetDisconnectd();
            Network.Close();
        }

        // 로그인 요청
        private void button3_Click(object sender, EventArgs e)
        {
            var loginReq = new PKTReqLogin();
            loginReq.UserId = textBoxUserID.Text;
            loginReq.AuthToken = textBoxAT.Text;
            var packet = MemoryPackSerializer.Serialize(loginReq);

            PostSendPacket((int)PACKETID.PKTReqLogin, packet);
            DevLog.Write($"로그인 요청:  {textBoxUserID.Text}, {textBoxAT.Text}");
        }

        // 방 입장
        private void button4_Click(object sender, EventArgs e)
        {
            var enterRoomReq = new PKTReqRoomEnter();
            enterRoomReq.RoomNumber = Int32.Parse(textBoxRoomNumber.Text);
            var packet = MemoryPackSerializer.Serialize(enterRoomReq);

            PostSendPacket((int)PACKETID.PKTReqRoomEnter, packet);
            DevLog.Write($"방 입장 요청:  {textBoxRoomNumber.Text} 번");
        }

        // 방 나가기
        private void button5_Click(object sender, EventArgs e)
        {
            PKTReqRoomLeave leaveRoomReq = new PKTReqRoomLeave();
            var packet = MemoryPackSerializer.Serialize(leaveRoomReq);

            PostSendPacket((int)PACKETID.PKTReqRoomLeave,  packet);
        }

        // 게임 준비 완료
        private void button6_Click(object sender, EventArgs e)
        {
            PKTReqReadyOmok readyOmok = new PKTReqReadyOmok();
            var packet = MemoryPackSerializer.Serialize(readyOmok);
            PostSendPacket((int)PACKETID.PKTReqReadyOmok, packet);
            
            //PostSendPacket(CSCommon.PacketID.ReqReadyOmok, null);

            DevLog.Write($"게임 준비 완료 요청");            
        }

        // 채팅
        private void button7_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxRoomSendMsg.Text))
            {
                MessageBox.Show("채팅 메시지를 입력하세요");
                return;
            }

            var requestPkt = new PKTReqRoomChat();
            requestPkt.ChatMsg = textBoxRoomSendMsg.Text;
            var packet = MemoryPackSerializer.Serialize(requestPkt);

            PostSendPacket((int)PACKETID.PKTReqRoomChat, packet);
            DevLog.Write($"방 채팅 요청");
        }



        // 연결 확인용 테스트 패킷 전송
        private void btnTest_Click(object sender, EventArgs e)
        {
            //// StartGame(true, "My", "Other");
            //// 테스트 패킷 전송


            //var testPacket = new C_Test();
            //testPacket.Msg = "Hello Server";
            
            //var sendPacketData = MemoryPackSerializer.Serialize(testPacket);

            //PostSendPacket((int)PACKETID.C_Test, sendPacketData);

        }
    }
}
