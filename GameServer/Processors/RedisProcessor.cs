using CloudStructures;
using GameServer.Packet;
using GameServer.PacketHandler;
using SuperSocket.SocketBase.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace GameServer;

public class RedisProcessor
{
    ILog _logger;

    Dictionary<int, Func<MemoryPackBinaryRequestInfo, Task>> _packetHandlerMap = new Dictionary<int, Func<MemoryPackBinaryRequestInfo, Task>>(); // 패킷의 ID와 패킷 핸들러를 같이 등록한다.

    PacketHandlerTokenVerify? _packetHandlerTokenVeriry;


    System.Threading.Thread? _processThread = null; // 패킷 처리용 쓰레드 선언
    bool IsThreadRunning = false;

    BufferBlock<MemoryPackBinaryRequestInfo> _redisRecvBuffer = new BufferBlock<MemoryPackBinaryRequestInfo>();

    // db 프로세스마다 고유한 db연결객체
    RedisConnection? _redisConnector;

    Action<MemoryPackBinaryRequestInfo>? _mainPacketInsert;

    public void CreateAndStart(ILog logger, string redisConnectionStr, Action<MemoryPackBinaryRequestInfo> mainPacketInsert)
    {
        _logger = logger;

        if (redisConnectionStr == null || mainPacketInsert == null)
        {
            _logger.Error("redis Processor 생성에 실패하였습니다.");
            throw new NullReferenceException();
        }

        _redisConnector = new RedisConnection(new RedisConfig("defalut", redisConnectionStr));

        _mainPacketInsert = mainPacketInsert;

        RegisterPakcetHandler();

        IsThreadRunning = true;
        _processThread = new System.Threading.Thread(this.RedisProcess);
        _processThread.Start();
    }



    void RegisterPakcetHandler()
    {
        if (_redisConnector == null || _mainPacketInsert == null)
        {
            _logger.Error("redis Processor 패킷 등록에 실패하였습니다.");
            throw new NullReferenceException();
        }

        _packetHandlerTokenVeriry = new PacketHandlerTokenVerify(_logger, _redisConnector, _mainPacketInsert);
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
                    _logger.Error("등록되지 않은 패킷");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
            }


        }


    }





}
