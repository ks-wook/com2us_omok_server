using GameServer.Packet;
using GameServer.PacketHandler;
using MySqlConnector;
using SqlKata.Execution;
using SuperSocket.SocketBase.Logging;
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
    ILog _logger;

    Dictionary<int, Action<MemoryPackBinaryRequestInfo, QueryFactory>> _packetHandlerMap = new Dictionary<int, Action<MemoryPackBinaryRequestInfo, QueryFactory>>(); // 패킷의 ID와 패킷 핸들러를 같이 등록한다.

    PacketHandlerGameResult? _packetHandlerGameResult;

    List<Thread> _processThreads = new List<Thread>(); // 패킷 처리용 쓰레드 리스트

    string? _mysqlConnecitonStr;
    bool IsThreadRunning = false;

    BufferBlock<MemoryPackBinaryRequestInfo> _mysqlRecvBuffer = new BufferBlock<MemoryPackBinaryRequestInfo>();

    Action<MemoryPackBinaryRequestInfo>? _mainPacketInsert;

    public void CreateAndStart(ILog logger, string mysqlConnectionStr, int DBThreadCount, Action<MemoryPackBinaryRequestInfo> mainPacketInsert)
    {
        _logger = logger;

        if(mysqlConnectionStr == null || mainPacketInsert == null)
        {
            logger.Error("mysql Processor 생성에 실패하였습니다.");
            throw new NullReferenceException();
        }

        _mainPacketInsert = mainPacketInsert; 

        _mysqlConnecitonStr = mysqlConnectionStr;

        RegisterPakcetHandler();

        IsThreadRunning = true;
        for (int i = 0; i < DBThreadCount; i++)
        {
            System.Threading.Thread processThread = new System.Threading.Thread(this.MysqlProcess);
            _processThreads.Add(processThread);
            processThread.Start();
        }
    }

    void RegisterPakcetHandler()
    {
        if (_mainPacketInsert == null)
        {
            _logger.Error("mysql Processor 패킷 등록에 실패하였습니다.");
            throw new NullReferenceException();
        }

        _packetHandlerGameResult = new PacketHandlerGameResult(_logger, _mainPacketInsert);
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
        // 쓰레드 마다 고유한 DB 연결객체 생성, queryfactory 객체만 넘겨서 쿼리 실행
        SqlKata.Compilers.MySqlCompiler dbCompiler;
        QueryFactory queryFactory;

        MySqlConnection _dbConnector = new MySqlConnection(_mysqlConnecitonStr);

        dbCompiler = new SqlKata.Compilers.MySqlCompiler();
        queryFactory = new QueryFactory(_dbConnector, dbCompiler);

        while (IsThreadRunning)
        {
            try
            {
                var packet = _mysqlRecvBuffer.Receive();

                var header = new MemoryPackPacketHeadInfo();
                header.Read(packet.Data);

                if (_packetHandlerMap.ContainsKey(header.Id))
                {
                    _packetHandlerMap[header.Id](packet, queryFactory);
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
