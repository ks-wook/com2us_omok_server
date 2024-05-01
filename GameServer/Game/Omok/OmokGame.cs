﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace GameServer;

public enum StoneType
{
    None,
    Black,
    White
}

public class OmokGame
{
    StoneType[,] board;
    public int BoardSize = 19;

    public string winUserId = string.Empty; // white or black user id
    public string blackUserId = string.Empty;
    public string whiteUserId = string.Empty;

    public OmokGame()
    {
        board = new StoneType[BoardSize, BoardSize];
    }

    public void StartGame(string blackUserId, string whiteUserId)
    {
        ClearBoard();
        this.blackUserId = blackUserId;
        this.whiteUserId = whiteUserId;
    }

    // 돌을 둔 위치를 오목판에 반영
    public void PlaceStone(int row, int col, string userId)
    {
        if(userId == blackUserId)
        {
            board[row, col] = StoneType.Black;
        }
        else if(userId == whiteUserId)
        {
            board[row, col] = StoneType.White;
        }
    }

    // 승부가 났는지 체크
    public bool CheckWinner(int lastRow, int lastCol)
    {
        StoneType stone = board[lastRow, lastCol];

        // TEST -------
        setWinner(stone);
        return true;
        // ------------

        // 가로 체크
        int count = 1;
        for (int i = lastCol - 1; i >= 0 && board[lastRow, i] == stone; i--)
        {
            count++;
        }
        for (int i = lastCol + 1; i < BoardSize && board[lastRow, i] == stone; i++)
        {
            count++;
        }
        if (count >= 5)
        {
            setWinner(stone);
            return true;
        }

        // 세로 체크
        count = 1;
        for (int i = lastRow - 1; i >= 0 && board[i, lastCol] == stone; i--)
        {
            count++;
        }
        for (int i = lastRow + 1; i < BoardSize && board[i, lastCol] == stone; i++)
        {
            count++;
        }
        if (count >= 5)
        {
            setWinner(stone);
            return true;
        }

        // 대각선 체크
        count = 1;
        for (int i = 1; lastRow - i >= 0 && lastCol - i >= 0 && board[lastRow - i, lastCol - i] == stone; i++)
        {
            count++;
        }
        for (int i = 1; lastRow + i < BoardSize && lastCol + i < BoardSize && board[lastRow + i, lastCol + i] == stone; i++)
        {
            count++;
        }
        if (count >= 5)
        {
            setWinner(stone);
            return true;
        }

        // 역대각선 체크
        count = 1;
        for (int i = 1; lastRow - i >= 0 && lastCol + i < BoardSize && board[lastRow - i, lastCol + i] == stone; i++)
        {
            count++;
        }
        for (int i = 1; lastRow + i < BoardSize && lastCol - i >= 0 && board[lastRow + i, lastCol - i] == stone; i++)
        {
            count++;
        }
        if (count >= 5)
        {
            setWinner(stone);
            return true;
        }

        return false;
    }

    public void setWinner(StoneType stone)
    {
        if(stone == StoneType.Black)
        {
            winUserId = blackUserId;
        }
        else if(stone == StoneType.White)
        {
            winUserId = whiteUserId;
        }
    }

    // 오목판 초기화
    public void ClearBoard()
    {
        for (int i = 0; i < BoardSize; i++)
        {
            for (int j = 0; j < BoardSize; j++)
            {
                board[i, j] = StoneType.None;
            }
        }
    }
}
