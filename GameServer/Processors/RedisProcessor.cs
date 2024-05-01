using CloudStructures;
using GameServer.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace GameServer;

public class RedisProcessor
{
    Dictionary<int, Func<MemoryPackBinaryRequestInfo, Task>> _packetHandlerMap = new Dictionary<int, Func<MemoryPackBinaryRequestInfo, Task>>(); // 패킷의 ID와 패킷 핸들러를 같이 등록한다.

    PacketHandlerTokenVerify? _packetHandlerTokenVeriry;


    System.Threading.Thread? _processThread = null; // 패킷 처리용 쓰레드 선언
    bool IsThreadRunning = false;

    BufferBlock<MemoryPackBinaryRequestInfo> _redisRecvBuffer = new BufferBlock<MemoryPackBinaryRequestInfo>();

    // db 프로세스마다 고유한 db연결객체
    RedisConnection? _redisConnector;

    RoomManager? _roomManager;
    UserManager? _userManager;

    PacketProcessor? _packetProcessor;


    public void CreateAndStart(RoomManager roomManager, UserManager userManager, string redisConnectionStr, PacketProcessor? packetProcessor)
    {
        if (roomManager == null || userManager == null
            || redisConnectionStr == null || packetProcessor == null)
        {
            Console.WriteLine("redis Processor 생성에 실패하였습니다.");
            throw new NullReferenceException();
        }

        _roomManager = roomManager;
        _userManager = userManager;


        _redisConnector = new RedisConnection(new RedisConfig("defalut", redisConnectionStr));

        _packetProcessor = packetProcessor;

        RegisterPakcetHandler();

        IsThreadRunning = true;
        _processThread = new System.Threading.Thread(this.RedisProcess);
        _processThread.Start();
    }



    void RegisterPakcetHandler()
    {
        if (_roomManager == null || _userManager == null
            || _redisConnector == null || _packetProcessor == null)
        {
            Console.WriteLine("redis Processor 패킷 등록에 실패하였습니다.");
            throw new NullReferenceException();
        }

        _packetHandlerTokenVeriry = new PacketHandlerTokenVerify(_roomManager, _userManager, _redisConnector, _packetProcessor);
        _packetHandlerTokenVeriry.RegisterPacketHandler(_packetHandlerMap);
    }


    public void Destory()
    {
        IsThreadRunning = false;
        _redisRecvBuffer.Complete();
    }


    public void Insert(MemoryPackBinaryRequestInfo data)
    {
        _redisRecvBuffer.Post(data);
    }

    void RedisProcess()
    {
        while (IsThreadRunning)
        {
            try
            {
                var packet = _redisRecvBuffer.Receive();

                var header = new MemoryPackPacketHeadInfo();
                header.Read(packet.Data);

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
