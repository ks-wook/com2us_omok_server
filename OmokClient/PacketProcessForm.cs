using CSCommon;
using GameServer;
using MemoryPack;
using OmokClient;
using PacketData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

#pragma warning disable CA1416

namespace OmokClient
{
    public partial class MainForm : Form
    {
        static public string ClientUserId { get; set; } = string.Empty;
        static public string ClientLoginToken { get; set; } = string.Empty;
        static public bool IsBlackUser { get; set; } = false;

        Dictionary<UInt16, Action<byte[]>> _packetHandlerMap = new Dictionary<UInt16, Action<byte[]>>();

        void SetPacketHandler()
        {
            //PacketFuncDic.Add(PACKET_ID.PACKET_ID_ERROR_NTF, PacketProcess_ErrorNotify);
            //PacketFuncDic.Add(PacketID.ResLogin, PacketProcess_Login);
            //PacketFuncDic.Add(PacketID.ResRoomEnter, PacketProcess_RoomEnter);
            //PacketFuncDic.Add(PacketID.NtfRoomUserList, PacketProcess_RoomUserList);
            //PacketFuncDic.Add(PacketID.NtfRoomNewUser, PacketProcess_RoomNewUser);
            //PacketFuncDic.Add(PacketID.ResRoomLeave, PacketProcess_RoomLeave);
            //PacketFuncDic.Add(PacketID.NtfRoomLeaveUser, PacketProcess_RoomLeaveUser);
            //PacketFuncDic.Add(PacketID.ResRoomChat, PacketProcess_RoomChatResponse);
            //PacketFuncDic.Add(PacketID.NtfRoomChat, PacketProcess_RoomChatNotify);
            //PacketFuncDic.Add(PacketID.ResReadyOmok, PacketProcess_ReadyOmokResponse);
            //PacketFuncDic.Add(PacketID.NtfReadyOmok, PacketProcess_ReadyOmokNotify);
            //PacketFuncDic.Add(PacketID.NtfStartOmok, PacketProcess_StartOmokNotify);
            //PacketFuncDic.Add(PacketID.ResPutMok, PacketProcess_PutMokResponse);
            //PacketFuncDic.Add(PacketID.NTFPutMok, PacketProcess_PutMokNotify);
            //PacketFuncDic.Add(PacketID.NTFEndOmok, PacketProcess_EndOmokNotify);


            _packetHandlerMap.Add((int)PACKETID.PKTResLogin, PKTResLoginHandler);
            _packetHandlerMap.Add((int)PACKETID.PKTResRoomEnter, PKTResRoomEnterHandler);
            _packetHandlerMap.Add((int)PACKETID.PKTResRoomLeave, PKTResRoomLeaveHandler);
            _packetHandlerMap.Add((int)PACKETID.PKTResRoomChat, PKTResRoomChatHandler);
            _packetHandlerMap.Add((int)PACKETID.PKTResReadyOmok, PKTResReadyOmokHandler);
            _packetHandlerMap.Add((int)PACKETID.PKTNtfStartOmok, PKTNtfStartOmokHandler);
            _packetHandlerMap.Add((int)PACKETID.PKTResPutMok, PKTResPutMokHandler);
            _packetHandlerMap.Add((int)PACKETID.PKTNtfEndOmok, PKTNtfEndOmokHandler);
            _packetHandlerMap.Add((int)PACKETID.PKTReqPing, PKTReqPingHandler);
        }

        void PacketProcess(byte[] packet)
        {
            var header = new MemoryPackPacketHeadInfo();
            header.Read(packet);

            var packetID = header.Id;

            if (_packetHandlerMap.ContainsKey(packetID))
            {
                _packetHandlerMap[packetID](packet);
            }
            else
            {
                DevLog.Write("Unknown Packet Id: " + packetID);
            }
        }

        void PacketProcess_ErrorNotify(byte[] packetData)
        {
            /*var notifyPkt = new ErrorNtfPacket();
            notifyPkt.FromBytes(bodyData);

            DevLog.Write($"에러 통보 받음:  {notifyPkt.Error}");*/
        }


        // 대전 서버 로그인 패킷
        void PKTResLoginHandler(byte[] packetData)
        {
            var responsePkt = MemoryPackSerializer.Deserialize<PKTResLogin>(packetData);

            if (responsePkt.Result == ErrorCode.None)
            {
                CurSceen = ClientSceen.LOGIN;
                ClientUserId = responsePkt.UserId;
                DevLog.Write($"UserId: {ClientUserId}, 대전 서버 로그인 성공");

                // 배정된 방으로 접속
                var enterRoomReq = new PKTReqRoomEnter();
                enterRoomReq.RoomNumber = Int32.Parse(textBoxRoomNumber.Text);
                var packet = MemoryPackSerializer.Serialize(enterRoomReq);

                PostSendPacket((int)PACKETID.PKTReqRoomEnter, packet);
                DevLog.Write($"방 입장 요청:  {textBoxRoomNumber.Text} 번");
            }
        }

        void PKTResRoomEnterHandler(byte[] packetData)
        {
            var responsePkt = MemoryPackSerializer.Deserialize<PKTResRoomEnter>(packetData);
            // DevLog.Write($"방 입장 결과:  {(ErrorCode)responsePkt.Result}");

            if (responsePkt.UserId == ClientUserId) // 자신의 방 입장
            {
                CurSceen = ClientSceen.ROOM;
                AddRoomChatMessageList("Note", $"{responsePkt.RoomNumber}번 방에 입장하였습니다.");
                button6.Enabled = true; // 준비 버튼

                // 기존의 모든 방 유저들을 방 유저 리스트에 추가한다.
                foreach(string userId in responsePkt.RoomUserList)
                {
                    AddRoomUserList("User " + userId);
                }
            }
            else // 다른 사람의 방 입장
            {
                AddRoomChatMessageList("Note", $"유저 '{responsePkt.UserId}' 가 입장하였습니다.");

                //  상대의 정보만 방 유저 리스트에 추가한다.
                AddRoomUserList("User " + responsePkt.UserId);
            }

        }

        void PacketProcess_RoomUserList(byte[] packetData)
        {
            //var notifyPkt = MessagePackSerializer.Deserialize<PKTNtfRoomUserList>(packetData);

            //for (int i = 0; i < notifyPkt.UserIDList.Count; ++i)
            //{
            //    AddRoomUserList(notifyPkt.UserIDList[i]);
            //}

            //DevLog.Write($"방의 기존 유저 리스트 받음");
        }

        void PacketProcess_RoomNewUser(byte[] packetData)
        {
            //var notifyPkt = MessagePackSerializer.Deserialize<PKTNtfRoomNewUser>(packetData);

            //AddRoomUserList(notifyPkt.UserID);

            //DevLog.Write($"방에 새로 들어온 유저 받음");
        }


        void PKTResRoomLeaveHandler(byte[] packetData)
        {
            var responsePkt = MemoryPackSerializer.Deserialize<PKTResRoomLeave>(packetData);
            DevLog.Write(responsePkt.UserId);
            if (responsePkt.UserId == ClientUserId) // 자신 퇴장
            {
                ClearRoomUserList();
                AddRoomChatMessageList("Note", "방에서 퇴장하였습니다.");
                button6.Enabled = false; // 준비 버튼
            }
            else // 상대 퇴장
            {
                RemoveRoomUserList("User " + responsePkt.UserId);
                AddRoomChatMessageList("Note", $"유저 '{responsePkt.UserId}'가 퇴장하였습니다.");
            }
        }

        void PacketProcess_RoomLeaveUser(byte[] packetData)
        {
            //var notifyPkt = MessagePackSerializer.Deserialize<PKTNtfRoomLeaveUser>(packetData);

            //RemoveRoomUserList(notifyPkt.UserID);

            //DevLog.Write($"방에서 나간 유저 받음");
        }


        void PKTResRoomChatHandler(byte[] packetData)
        {
            var responsePkt = MemoryPackSerializer.Deserialize<PKTResRoomChat>(packetData);

            AddRoomChatMessageList(responsePkt.UserId, responsePkt.ChatMsg);
        }


        void PacketProcess_RoomChatNotify(byte[] packetData)
        {
            //var notifyPkt = MessagePackSerializer.Deserialize<PKTNtfRoomChat>(packetData);

            //AddRoomChatMessageList(notifyPkt.UserID, notifyPkt.ChatMessage);
        }

        void AddRoomChatMessageList(string userID, string message)
        {
            if (listBoxRoomChatMsg.Items.Count > 512)
            {
                listBoxRoomChatMsg.Items.Clear();
            }

            listBoxRoomChatMsg.Items.Add($"[{userID}]: {message}");
            listBoxRoomChatMsg.SelectedIndex = listBoxRoomChatMsg.Items.Count - 1;
        }

        void PacketProcess_ReadyOmokResponse(byte[] packetData)
        {
            //var responsePkt = MessagePackSerializer.Deserialize<PKTResReadyOmok>(packetData);

            //DevLog.Write($"게임 준비 완료 요청 결과:  {(ErrorCode)responsePkt.Result}");

            //if ((ErrorCode)responsePkt.Result == ErrorCode.None)
            //{
            //    CurSceen = ClientSceen.GAME_READY;
            //}
        }

        void PKTResReadyOmokHandler(byte[] packetData)
        {
            var notifyPkt = MemoryPackSerializer.Deserialize<PKTResReadyOmok>(packetData);

            if (notifyPkt.IsReady)
            {
                DevLog.Write($"[{notifyPkt.UserId}]님은 게임 준비 완료");
            }
            else
            {
                DevLog.Write($"[{notifyPkt.UserId}]님이 게임 준비 완료 취소");
            }

        }

        void PKTNtfStartOmokHandler(byte[] packetData)
        {
            IsMyTurn = false;
            IsBlackUser = false;

            var notifyPkt = MemoryPackSerializer.Deserialize<PKTNtfStartOmok>(packetData);

            DevLog.Write($"게임 시작. 흑돌 플레이어: {notifyPkt.BlackUserId}");
            
            GameResultText.Enabled = false;
            GameResultText.Visible = false;

            if (notifyPkt.BlackUserId == ClientUserId) // 내가 선공
            {
                IsMyTurn = true;
                IsBlackUser = true;

                DevLog.Write($"나의 턴!!!");
            }

            CurSceen = ClientSceen.GAME_PLAYING;
            StartGame(IsMyTurn, notifyPkt.BlackUserId, notifyPkt.WhiteUserId);

        }

        void PacketProcess_PutMokResponse(byte[] packetData)
        {
            //var responsePkt = MessagePackSerializer.Deserialize<PKTResPutMok>(packetData);

            //if (responsePkt.Result != (Int16)ErrorCode.None)
            //{
            //    DevLog.Write($"오목 놓기 성공");
            //}
            //else
            //{
            //    DevLog.Write($"오목 놓기 실패: {(ErrorCode)responsePkt.Result}");
            //    OmokLogic.한수무르기();
            //}            
        }

        void PKTResPutMokHandler(byte[] packetData)
        {
            var notifyPkt = MemoryPackSerializer.Deserialize<PKTResPutMok>(packetData);

            if(notifyPkt.Result != ErrorCode.None)
            {
                DevLog.Write("부적절한 요청");
                return;
            }

            // 내 돌은 클릭하는 순간에 그려졌다.

            if (notifyPkt.UserId != ClientUserId && notifyPkt.IsTimeout == false) // 상대가 시간안에 돌을 둔 경우
            {
                플레이어_돌두기(false, notifyPkt.PosX, notifyPkt.PosY, notifyPkt.IsBlack);
                DevLog.Write($"오목 정보: X: {notifyPkt.PosX},  Y: {notifyPkt.PosY},   알:{notifyPkt.UserId}");
            }
            else if (notifyPkt.UserId != ClientUserId && notifyPkt.IsTimeout == true)
            {
                DevLog.Write("상대가 돌을 두지 않았습니다.");
            }

            if (notifyPkt.UserId != ClientUserId) // 상대의 턴이었던 경우
            {
                remainingTurnTime = 10;
                TurnTimeoutLabel.Text = remainingTurnTime.ToString();
                DevLog.Write($"나의 턴!!!");
            }
            else
            {
                DevLog.Write($"상대의 턴!!!");
            }

            // 상대가 시간안에 돌을 두지않은경우 돌을 그리지않고 바로 나의턴을 시작
            IsMyTurn = !IsMyTurn;
        }

        void PKTNtfEndOmokHandler(byte[] packetData)
        {
            var notifyPkt = MemoryPackSerializer.Deserialize<PKTNtfEndOmok>(packetData);

            EndGame();

            CurSceen = ClientSceen.ROOM;
            DevLog.Write($"오목 GameOver: Win: {notifyPkt.WinUserId}");

            if(ClientUserId == notifyPkt.WinUserId)
            {
                GameResultText.Text = "승리!";
                int tmp = Int32.Parse(WinCountLabel.Text);
                WinCountLabel.Text = (tmp + 1).ToString();
            }
            else
            {
                GameResultText.Text = "패배!";
                int tmp = Int32.Parse(LoseCountLabel.Text);
                LoseCountLabel.Text = (tmp + 1).ToString();
            }

            GameResultText.Enabled = true;
            GameResultText.Visible = true;

            turnTimeoutTimer.Dispose();
        }

        void PKTReqPingHandler(byte[] packetData)
        {
            var pingRes = new PKTResPing();
            var packet = MemoryPackSerializer.Serialize(pingRes);

            PostSendPacket((int)PACKETID.PKTResPing, packet);
        }
    }
}
