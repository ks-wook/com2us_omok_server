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

    Dictionary<int, Func<byte[], Task>> _packetHandlerMap = new Dictionary<int, Func<byte[], Task>>(); // 패킷의 ID와 패킷 핸들러를 같이 등록한다.

    PacketHandlerGameResult? _packetHandlerGameResult;


    System.Threading.Thread? _processThread = null; // 패킷 처리용 쓰레드 선언
    bool IsThreadRunning = false;

    BufferBlock<byte[]> _mysqlRecvBuffer = new BufferBlock<byte[]>();

    // db 프로세스마다 고유한 db연결객체
    MySqlConnection? _mysqlConnection;

    RoomManager? _roomManager;
    UserManager? _userManager;

    PacketProcessor? _packetProcessor;


    public void CreateAndStart(RoomManager roomManager, UserManager userManager, string mysqlConnectionStr, PacketProcessor? packetProcessor)
    {
        if(roomManager == null || userManager == null 
            || mysqlConnectionStr == null || packetProcessor == null)
        {
            Console.WriteLine("mysql Processor 생성에 실패하였습니다.");
            throw new NullReferenceException();
        }

        _roomManager = roomManager;
        _userManager = userManager;

        _mysqlConnection = new MySqlConnection(mysqlConnectionStr);

        _packetProcessor = packetProcessor; 

        RegisterPakcetHandler();

        IsThreadRunning = true;
        _processThread = new System.Threading.Thread(this.MysqlProcess);
        _processThread.Start();
    }



    void RegisterPakcetHandler()
    {
        if (_roomManager == null || _userManager == null 
            || _mysqlConnection == null || _packetProcessor == null)
        {
            Console.WriteLine("mysql Processor 패킷 등록에 실패하였습니다.");
            throw new NullReferenceException();
        }

        _packetHandlerGameResult = new PacketHandlerGameResult(_roomManager, _userManager, _mysqlConnection, _packetProcessor);
        _packetHandlerGameResult.RegisterPacketHandler(_packetHandlerMap);
    }


    public void Destory()
    {
        IsThreadRunning = false;
        _mysqlRecvBuffer.Complete();
    }


    public void Insert(byte[] data)
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
                header.Read(packet);

                if (_packetHandlerMap.ContainsKey(header.Id))
                {
                    _packetHandlerMap[header.Id](packet);
                }
                else
                {
                    Console.WriteLine("등록되지 않은 패킷");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }


        }


    }

}
