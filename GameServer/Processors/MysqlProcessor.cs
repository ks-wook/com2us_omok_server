using GameServer.Packet;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace GameServer;

public class MysqlProcessor
{

    Dictionary<int, Func<MemoryPackBinaryRequestInfo, Task>> _packetHandlerMap = new Dictionary<int, Func<MemoryPackBinaryRequestInfo, Task>>(); // 패킷의 ID와 패킷 핸들러를 같이 등록한다.

    PacketHandlerGameResult? _packetHandlerGameResult;


    System.Threading.Thread? _processThread = null; // 패킷 처리용 쓰레드 선언
    bool IsThreadRunning = false;

    BufferBlock<MemoryPackBinaryRequestInfo> _mysqlRecvBuffer = new BufferBlock<MemoryPackBinaryRequestInfo>();

    // db 프로세스마다 고유한 db연결객체
    MySqlConnection? _mysqlConnector;

    RoomManager? _roomManager;
    UserManager? _userManager;

    PacketProcessor? _packetProcessor;


    public void CreateAndStart(RoomManager roomManager, UserManager userManager, string mysqlConnectionStr, PacketProcessor? packetProcessor)
    {
        if(roomManager == null || userManager == null 
            || mysqlConnectionStr == null || packetProcessor == null)
        {
            MainServer.MainLogger.Error("mysql Processor 생성에 실패하였습니다.");
            throw new NullReferenceException();
        }

        _roomManager = roomManager;
        _userManager = userManager;

        _mysqlConnector = new MySqlConnection(mysqlConnectionStr);

        _packetProcessor = packetProcessor; 

        RegisterPakcetHandler();

        IsThreadRunning = true;
        _processThread = new System.Threading.Thread(this.MysqlProcess);
        _processThread.Start();
    }



    void RegisterPakcetHandler()
    {
        if (_roomManager == null || _userManager == null 
            || _mysqlConnector == null || _packetProcessor == null)
        {
            MainServer.MainLogger.Error("mysql Processor 패킷 등록에 실패하였습니다.");
            throw new NullReferenceException();
        }

        _packetHandlerGameResult = new PacketHandlerGameResult(_roomManager, _userManager, _mysqlConnector, _packetProcessor);
        _packetHandlerGameResult.RegisterPacketHandler(_packetHandlerMap);
    }


    public void Destory()
    {
        IsThreadRunning = false;
        _mysqlRecvBuffer.Complete();
    }


    public void Insert(MemoryPackBinaryRequestInfo data)
    {
        _mysqlRecvBuffer.Post(data);
    }

    void MysqlProcess()
    {
        while (IsThreadRunning)
        {
            try
            {
                var packet = _mysqlRecvBuffer.Receive();

                var header = new MemoryPackPacketHeadInfo();
                header.Read(packet.Data);

                if (_packetHandlerMap.ContainsKey(header.Id))
                {
                    _packetHandlerMap[header.Id](packet);
                }
                else
                {
                    MainServer.MainLogger.Error("등록되지 않은 패킷");
                }
            }
            catch (Exception ex)
            {
                MainServer.MainLogger.Error(ex.ToString());
            }


        }


    }

}
