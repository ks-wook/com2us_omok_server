using GameServer.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace GameServer;

public class PacketProcessor
{
    Dictionary<int, Action<MemoryPackBinaryRequestInfo>>_packetHandlerMap = new Dictionary<int, Action<MemoryPackBinaryRequestInfo>>(); // 패킷의 ID와 패킷 핸들러를 같이 등록한다.
    
    PacketHandlerUser? _packetHandlerAuth;
    PacketHandlerRoom? _packetHandlerRoom;
    PacketHandlerGame? _packetHandlerGame;
    
    System.Threading.Thread? _processThread = null; // 패킷 처리용 쓰레드 선언
    bool IsThreadRunning = false;

    // 비동기 접근 가능한 bufferBlock, 여러 스레드가 동시에 접근하여도 블럭킹 되지 않는다.
    BufferBlock<MemoryPackBinaryRequestInfo> _recvBuffer = new BufferBlock<MemoryPackBinaryRequestInfo>();


    MysqlProcessor? _mysqlProcessor;
    RedisProcessor? _redisProcessor;

    RoomManager? _roomManager;
    UserManager? _userManager;



    public void CreateAndStart(RoomManager roomManager, UserManager userManager, MysqlProcessor mysqlProcessor, RedisProcessor redisProcessor)
    {
        if(roomManager == null || userManager == null || mysqlProcessor == null)
        {
            MainServer.MainLogger.Error("[CreateAndStart] Packet Processor 생성 실패");
            throw new NullReferenceException();
        }

        _roomManager = roomManager;
        _userManager = userManager;

        _mysqlProcessor = mysqlProcessor;
        _redisProcessor = redisProcessor;

        // 패킷 처리용 쓰레드를 생성하고, 패킷 처리를 도맡아한다.    

        RegisterPakcetHandler();



        IsThreadRunning = true;
        _processThread = new System.Threading.Thread(this.Process);
        _processThread.Start();
    }

    void RegisterPakcetHandler()
    {
        if (_roomManager == null || _userManager == null || _mysqlProcessor == null)
        {
            MainServer.MainLogger.Error("[RegisterPakcetHandler] Managers Null");
            throw new NullReferenceException();
        }

        // 여러 종류의 패킷 핸들러에 선언된 핸들러들을 패킷 프로세서의 핸들러에 최종 등록
        _packetHandlerAuth = new PacketHandlerUser(_userManager, _redisProcessor);
        _packetHandlerAuth.RegisterPacketHandler(_packetHandlerMap);

        _packetHandlerRoom = new PacketHandlerRoom(_roomManager, _userManager);
        _packetHandlerRoom.RegisterPacketHandler(_packetHandlerMap);

        _packetHandlerGame= new PacketHandlerGame(_roomManager, _userManager, _mysqlProcessor);
        _packetHandlerGame.RegisterPacketHandler(_packetHandlerMap);
    }


    public void Destory()
    {
        IsThreadRunning = false;
        _recvBuffer.Complete();
    }


    public void Insert(MemoryPackBinaryRequestInfo packet)
    {
        _recvBuffer.Post(packet);
    }



    void Process()
    {
        while (IsThreadRunning)
        {
            try
            {
                var packet = _recvBuffer.Receive();

                var header = new MemoryPackPacketHeadInfo();
                header.Read(packet.Data);

                if (_packetHandlerMap.ContainsKey(header.Id))
                {
                    _packetHandlerMap[header.Id](packet);
                }
                else
                {
                    Console.WriteLine("세션 번호 {0}, PacketID {1}, 받은 데이터 크기: {2}", packet.SessionID, header.Id, packet.Body.Length);
                }
            }
            catch (Exception ex)
            {
                MainServer.MainLogger.Error(ex.ToString());
            }
        }
    }
}
