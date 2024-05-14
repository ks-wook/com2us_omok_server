using GameServer.Packet;
using SuperSocket.SocketBase.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace GameServer;

public class PacketProcessor
{
    ILog _logger;

    Dictionary<int, Action<MemoryPackBinaryRequestInfo>>_packetHandlerMap = new Dictionary<int, Action<MemoryPackBinaryRequestInfo>>(); // 패킷의 ID와 패킷 핸들러를 같이 등록한다.
    
    PacketHandlerUser? _packetHandlerUser;
    PacketHandlerRoom? _packetHandlerRoom;
    PacketHandlerGame? _packetHandlerGame;
    
    System.Threading.Thread? _processThread = null; // 패킷 처리용 쓰레드 선언
    bool IsThreadRunning = false;

    // 비동기 접근 가능한 bufferBlock, 여러 스레드가 동시에 접근하여도 블럭킹 되지 않는다.
    BufferBlock<MemoryPackBinaryRequestInfo> _recvBuffer = new BufferBlock<MemoryPackBinaryRequestInfo>();


    Action<MemoryPackBinaryRequestInfo> _mysqlInsert;
    Action<MemoryPackBinaryRequestInfo> _redisInsert;

    RoomManager? _roomManager;
    UserManager? _userManager;

    

    public void CreateAndStart(ILog Logger, RoomManager roomManager, UserManager userManager, 
        Action<MemoryPackBinaryRequestInfo> MysqlInsert, Action<MemoryPackBinaryRequestInfo> RedisInsert)
    {
        if(roomManager == null || userManager == null || MysqlInsert == null || RedisInsert == null)
        {
            _logger.Error("[CreateAndStart] Packet Processor 생성 실패");
            throw new NullReferenceException();
        }

        _logger = Logger;

        _roomManager = roomManager;
        _userManager = userManager;

        _mysqlInsert = MysqlInsert;
        _redisInsert = RedisInsert;

        // 패킷 처리용 쓰레드를 생성하고, 패킷 처리를 도맡아한다.    

        RegisterPakcetHandler();

        IsThreadRunning = true;
        _processThread = new System.Threading.Thread(this.Process);
        _processThread.Start();
    }

    void RegisterPakcetHandler()
    {
        if (_roomManager == null || _userManager == null || _mysqlInsert == null || _redisInsert == null)
        {
            _logger.Error("[RegisterPakcetHandler] Managers Null");
            throw new NullReferenceException();
        }

        // 여러 종류의 패킷 핸들러에 선언된 핸들러들을 패킷 프로세서의 핸들러에 최종 등록
        _packetHandlerUser = new PacketHandlerUser(_logger, _userManager, _redisInsert);
        _packetHandlerUser.RegisterPacketHandler(_packetHandlerMap);

        _packetHandlerRoom = new PacketHandlerRoom(_logger, _roomManager, _userManager);
        _packetHandlerRoom.RegisterPacketHandler(_packetHandlerMap);

        _packetHandlerGame= new PacketHandlerGame(_logger, _roomManager, _userManager, _mysqlInsert);
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
                _logger.Error(ex.ToString());
            }
        }
    }
}
