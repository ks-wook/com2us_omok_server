using MemoryPack;
using OmokClient;
using OmokClient.Game;
using OmokClient.Hive;
using PacketData;
using System;
using System.DirectoryServices;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OmokClient
{
    public partial class MainForm : Form
    {
        string _hiveCreateAccountAPI;

        string _hiveLoginAPI;
        string _gameLoginAPI;

        string _gameReqMatchAPI;
        string _gameCheckMatchAPI;

        System.Threading.Timer matchCmplTimer;

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

            
            button4.Enabled = false; // 방 입장 버튼 비활성화
            button6.Enabled = false; // 게임 준비는 방에 있을 때에만 활성화

            Omok_Init();
            DevLog.Write("프로그램 시작 !!!", LOG_LEVEL.INFO);
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
            // 대전 서버 접속은 매칭 완료 시 자동으로 이루어진다.
            DevLog.Write("대전 서버에 접속합니다.");

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

        // 로그인 요청 -> 하이브 -> 게임 api 순으로 진행
        private async void button3_Click(object sender, EventArgs e)
        {
            DevLog.Write("로그인 시도");

            _hiveLoginAPI = "http://" + textBoxIP.Text + ":5014/Login";
            _gameLoginAPI = "http://" + textBoxIP.Text + ":5015/Login";

            if (checkBoxLocalHostIP.Checked)
            {
                _hiveLoginAPI = "http://localhost" + ":5014/Login";
                _gameLoginAPI = "http://localhost" + ":5015/Login";
            }

            try
            {
                // 하이브 서버 로그인 요청
                HttpClient client = new HttpClient();
                OmokClient.Hive.LoginReq hiveLoginReq = new OmokClient.Hive.LoginReq()
                {
                    Id = textBoxUserID.Text,
                    Password = textBoxPs.Text,
                };

                DevLog.Write($"Hive 서버로 로그인 중...");

                HttpResponseMessage httpHiveLoginRes = await client.PostAsJsonAsync(_hiveLoginAPI, hiveLoginReq);

                if (httpHiveLoginRes == null || httpHiveLoginRes.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    DevLog.Write($"HttpError: {httpHiveLoginRes.StatusCode}");
                    return;
                }

                OmokClient.Hive.LoginRes hiveLoginRes = await httpHiveLoginRes.Content.ReadFromJsonAsync<OmokClient.Hive.LoginRes>();

                // 게임 api 서버 로그인 요청
                OmokClient.Game.LoginReq gameLoginReq = new OmokClient.Game.LoginReq()
                {
                    Uid = hiveLoginRes.UserId,
                    Token = hiveLoginRes.Token,
                };

                DevLog.Write($"GameAPI 서버로 로그인 중...");

                HttpResponseMessage httpGameLoginRes = await client.PostAsJsonAsync(_gameLoginAPI, gameLoginReq);

                if (httpGameLoginRes == null || httpGameLoginRes.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    DevLog.Write($"HttpError: {httpGameLoginRes.StatusCode}");
                    return;
                }

                OmokClient.Game.LoginRes gameLoginRes = await httpGameLoginRes.Content.ReadFromJsonAsync<OmokClient.Game.LoginRes>();

                // 얻어온 정보로 나의 정보 업데이트
                UserGameData userGameData = gameLoginRes.UserGameData;
                UserIDLabel.Text = userGameData.uid.ToString();
                WinCountLabel.Text = userGameData.total_win_cnt.ToString();
                LoseCountLabel.Text = userGameData.total_lose_cnt.ToString();

                // 아이디 및 토큰 저장
                ClientUserId = userGameData.uid.ToString();
                ClientLoginToken = hiveLoginRes.Token;

                // 매칭 요청 버튼 활성화
                MatchRequestBtn.Enabled = true;

                DevLog.Write($"GameAPI 서버 로그인 성공");
            }
            catch (Exception ex)
            {
                DevLog.Write(ex.ToString());
            }
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

            PostSendPacket((int)PACKETID.PKTReqRoomLeave, packet);
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



        // 계정생성
        private async void btnCreateAccount_Click(object sender, EventArgs e)
        {
            DevLog.Write("계정 생성");

            _hiveCreateAccountAPI = "http://" + textBoxIP.Text + ":5014/CreateAccount";

            if (checkBoxLocalHostIP.Checked)
            {
                _hiveCreateAccountAPI = "http://localhost" + ":5014/CreateAccount";
            }


            try
            {
                // 하이브 서버 계정생성 요청
                HttpClient client = new HttpClient();
                OmokClient.Hive.CreateAccountReq hiveCreateAccountReq = new OmokClient.Hive.CreateAccountReq()
                {
                    Id = textBoxUserID.Text,
                    Password = textBoxPs.Text,
                };

                HttpResponseMessage httpHiveCreateAccountRes = await client.PostAsJsonAsync(_hiveCreateAccountAPI, hiveCreateAccountReq);

                if (httpHiveCreateAccountRes == null || httpHiveCreateAccountRes.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    DevLog.Write($"HttpError: {httpHiveCreateAccountRes.StatusCode}");
                    return;
                }

                OmokClient.Hive.CreateAccountRes hiveCreateAccountRes = await httpHiveCreateAccountRes.Content
                    .ReadFromJsonAsync<OmokClient.Hive.CreateAccountRes>();

                DevLog.Write($"계정 생성 결과, ErrorCode : {hiveCreateAccountRes.result}");

            }
            catch (Exception ex)
            {
                DevLog.Write($"{ex.ToString()}");
            }
        }

        // 매칭 요청 버튼
        private async void button2_Click(object sender, EventArgs e)
        {
            // 로그인이 되어있는지 확인
            if(ClientUserId == string.Empty)
            {
                DevLog.Write("먼저 로그인을 해주세요");
                return;
            }

            // 매칭 요청 버튼 비활성화
            MatchRequestBtn.Enabled = false;

            _gameReqMatchAPI = "http://" + textBoxIP.Text + ":5015/RequestMatching";

            if (checkBoxLocalHostIP.Checked)
            {
                _gameReqMatchAPI = "http://localhost" + ":5015/RequestMatching";
            }


            // 매칭 요청 전송
            try
            {
                HttpClient client = new HttpClient();

                // 요청 헤더 설정
                client.DefaultRequestHeaders.Add("uid", ClientUserId);
                client.DefaultRequestHeaders.Add("token", ClientLoginToken);
                
                OmokClient.Game.MatchingRequest matchingRequest = new OmokClient.Game.MatchingRequest()
                {
                    UserID = ClientUserId
                };

                HttpResponseMessage httpGamematchingRequestRes = await client.PostAsJsonAsync(_gameReqMatchAPI, matchingRequest);

                if (httpGamematchingRequestRes == null || httpGamematchingRequestRes.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    DevLog.Write($"HttpError: {httpGamematchingRequestRes.StatusCode}");
                    return;
                }

                OmokClient.Game.MatchResponse gameMatchingRequestRes = await httpGamematchingRequestRes.Content
                    .ReadFromJsonAsync<OmokClient.Game.MatchResponse>();

                DevLog.Write($"매칭 요청 결과, ErrorCode : {gameMatchingRequestRes.Result}");


                // 2초에 한번씩 매칭이 완료되었는지 http 요청 전송
                matchCmplTimer = new System.Threading.Timer(CheckMatchComplete, null, 0, 2000);

            }
            catch (Exception ex)
            {
                DevLog.Write($"{ex.ToString()}");
            }

        }

        private async void CheckMatchComplete(object obj)
        {
            _gameCheckMatchAPI = "http://" + textBoxIP.Text + ":5015/CheckMatching";

            if (checkBoxLocalHostIP.Checked)
            {
                _gameCheckMatchAPI = "http://localhost" + ":5015/CheckMatching";
            }

            // 2초에 한번씩 매칭이 완료되었는지 http 요청 전송
            HttpClient client = new HttpClient();

            // 요청 헤더 설정
            client.DefaultRequestHeaders.Add("uid", ClientUserId);
            client.DefaultRequestHeaders.Add("token", ClientLoginToken);

            OmokClient.Game.CheckMatchingReq checkMatchingReq = new OmokClient.Game.CheckMatchingReq()
            {
                UserID = ClientUserId
            };

            HttpResponseMessage httpGameCheckMatchingReqRes = await client.PostAsJsonAsync(_gameCheckMatchAPI, checkMatchingReq);

            if (httpGameCheckMatchingReqRes == null || httpGameCheckMatchingReqRes.StatusCode != System.Net.HttpStatusCode.OK)
            {
                DevLog.Write($"HttpError: {httpGameCheckMatchingReqRes.StatusCode}");
                return;
            }

            OmokClient.Game.MatchingCompleteData gameMatchingRequestRes = await httpGameCheckMatchingReqRes.Content
                .ReadFromJsonAsync<OmokClient.Game.MatchingCompleteData>();

            if(gameMatchingRequestRes.IsMatched == false) // 매칭 미완료
            {
                DevLog.Write($"매칭중 입니다...");
            }
            else // 게임 매칭 완료
            {
                matchCmplTimer.Dispose();

                DevLog.Write($"매칭 완료! 대전 서버로 연결 됩니다.");

                // 배정된 방 번호 저장
                textBoxRoomNumber.Text = gameMatchingRequestRes.RoomNumber.ToString();

                // IP 및 포트 저장
                // textBoxIP.Text = gameMatchingRequestRes.ServerAddress;
                textBoxPort.Text = gameMatchingRequestRes.Port.ToString();

                // 대전 서버 접속
                button1_Click(null, null);
                DevLog.Write($"대전 서버로 접속 중...");

                // 서버 로그인 요청
                var loginReq = new PKTReqLogin();
                loginReq.UserId = ClientUserId;
                loginReq.AuthToken = ClientLoginToken;
                var packet = MemoryPackSerializer.Serialize(loginReq);

                PostSendPacket((int)PACKETID.PKTReqLogin, packet);
            }

        }
    }
}
