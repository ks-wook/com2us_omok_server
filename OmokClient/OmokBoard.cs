﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace OmokClient
{
    public partial class MainForm : Form
    {
        private System.Threading.Timer turnTimeoutTimer;
        private int remainingTurnTime = 0; // 초 단위

        CSCommon.OmokRule OmokLogic = new CSCommon.OmokRule();

        #region 오목 게임 상수
        private int 시작위치 = 30;
        private int 눈금크기 = 30;
        private int 돌크기 = 20;
        private int 화점크기 = 10;
        private int 바둑판크기 = 19;

        private Pen 검은펜 = new Pen(Color.Black);
        private SolidBrush 빨간색 = new SolidBrush(Color.Red);
        private SolidBrush 검은색 = new SolidBrush(Color.Black);
        private SolidBrush 흰색 = new SolidBrush(Color.White);

        private SoundPlayer 시작효과음;
        private SoundPlayer 종료효과음;
        private SoundPlayer 승리효과음;
        private SoundPlayer 바둑돌소리;
        private SoundPlayer 무르기요청;
        private SoundPlayer 오류효과음;
        #endregion

        private int 전x좌표 = -1, 전y좌표 = -1;

        bool IsMyTurn = false;

        private bool AI모드 = false;
        private CSCommon.OmokRule.돌종류 컴퓨터돌 = CSCommon.OmokRule.돌종류.없음;

        string MyPlayerName = "";
        string 흑돌플레이어Name = "";
        string 백돌플레이어Name = "";
        
        AIPlayer OmokAI = new();


        void Omok_Init()
        {
            DoubleBuffered = true;

            var curDir = System.Windows.Forms.Application.StartupPath;
            시작효과음 = new SoundPlayer($"{curDir}\\sound\\대국시작.wav");
            승리효과음 = new SoundPlayer($"{curDir}\\sound\\대국승리.wav");
            바둑돌소리 = new SoundPlayer($"{curDir}\\sound\\바둑돌소리.wav");
            무르기요청 = new SoundPlayer($"{curDir}\\sound\\무르기.wav");
            오류효과음 = new SoundPlayer($"{curDir}\\sound\\오류.wav");
            종료효과음 = new SoundPlayer($"{curDir}\\sound\\대국종료.wav");

            //ai = new AI(바둑판);
            //컴퓨터돌 = 돌종류.백돌;
        }

        //오목 게임 시작
        void StartGame(bool isMyTurn, string myPlayerName, string otherPlayerName)
        {
            turnTimeoutTimer = new System.Threading.Timer(UpdateTimer, null, 0, 1000);
            remainingTurnTime = 10;

            MyPlayerName = myPlayerName;

            if (isMyTurn)
            {
                흑돌플레이어Name = myPlayerName;
                백돌플레이어Name = otherPlayerName;
            }
            else
            {
                흑돌플레이어Name = otherPlayerName;
                백돌플레이어Name = myPlayerName;
            }

            IsMyTurn = isMyTurn;

            전x좌표 = 전y좌표 = -1;
            시작효과음.Play();

            OmokLogic.StartGame();

            if (AI모드 == true && 컴퓨터돌 == CSCommon.OmokRule.돌종류.흑돌)
            {
                컴퓨터두기();
            }

            panel1.Invalidate();
        }

        void EndGame()
        {
            OmokLogic.EndGame();

            종료효과음.Play();

            MyPlayerName = "";
            백돌플레이어Name = "";
            흑돌플레이어Name = "";

            turnTimeoutTimer.Dispose();
        }
                      

        void DisableAIMode()
        {
            if (AI모드 == true)
            {
                AI모드 = false;
            }
        }



        #region omok UI
        void panel1_Paint(object sender, PaintEventArgs e)
        {
            for (int i = 0; i < 바둑판크기; i++)                     // 바둑판 선 그리기
            {
                e.Graphics.DrawLine(검은펜, 시작위치, 시작위치 + i * 눈금크기, 시작위치 + 18 * 눈금크기, 시작위치 + i * 눈금크기);
                e.Graphics.DrawLine(검은펜, 시작위치 + i * 눈금크기, 시작위치, 시작위치 + i * 눈금크기, 시작위치 + 18 * 눈금크기);
            }

            for (int i = 0; i < 3; i++)                              // 화점 그리기
            {
                for (int j = 0; j < 3; j++)
                {
                    Rectangle r = new Rectangle(시작위치 + 눈금크기 * 3 + 눈금크기 * i * 6 - 화점크기 / 2,
                        시작위치 + 눈금크기 * 3 + 눈금크기 * j * 6 - 화점크기 / 2, 화점크기, 화점크기);

                    e.Graphics.FillEllipse(검은색, r);
                }
            }

            if (OmokLogic.게임종료 == false)
            {
                for (int i = 0; i < 바둑판크기; i++)
                {
                    for (int j = 0; j < 바둑판크기; j++)
                    {
                        돌그리기(i, j);
                    }
                }

                if (OmokLogic.현재돌x좌표 >= 0 && OmokLogic.현재돌y좌표 >= 0)
                {
                    현재돌표시();
                }

                현재턴_플레이어_정보();
            }
        }

        void 돌그리기(int x, int y)
        {
            Graphics g = panel1.CreateGraphics();

            Rectangle r = new Rectangle(시작위치 + 눈금크기 * x - 돌크기 / 2,
                시작위치 + 눈금크기 * y - 돌크기 / 2, 돌크기, 돌크기);

            if (OmokLogic.바둑판알(x, y) == (int)CSCommon.OmokRule.돌종류.흑돌)                              // 검은 돌
            {
                g.FillEllipse(검은색, r);
            }
            else if (OmokLogic.바둑판알(x, y) == (int)CSCommon.OmokRule.돌종류.백돌)                         // 흰 돌
            {
                g.FillEllipse(흰색, r);
            }
        }

        void 현재돌표시()
        {
            // 가장 최근에 놓은 돌에 화점 크기만한 빨간 점으로 표시하기
            Graphics g = panel1.CreateGraphics();

            Rectangle 앞에찍은돌을다시찍기위한구역 = new Rectangle(시작위치 + 눈금크기 * OmokLogic.전돌x좌표 - 돌크기 / 2,
                시작위치 + 눈금크기 * OmokLogic.전돌y좌표 - 돌크기 / 2, 돌크기, 돌크기);

            Rectangle r = new Rectangle(시작위치 + 눈금크기 * OmokLogic.현재돌x좌표 - 화점크기 / 2,
                    시작위치 + 눈금크기 * OmokLogic.현재돌y좌표 - 화점크기 / 2, 화점크기, 화점크기);

            // 초기화값이 -1이므로 -1보다 큰 값이 존재하면 찍은 값이 존재함
            if (OmokLogic.전돌x좌표 >= 0 && OmokLogic.전돌y좌표 >= 0)
            {
                // 전돌 다시 찍어서 빨간 점 없애기
                if (OmokLogic.바둑판알(OmokLogic.전돌x좌표, OmokLogic.전돌y좌표) == (int)CSCommon.OmokRule.돌종류.흑돌)
                {
                    g.FillEllipse(검은색, 앞에찍은돌을다시찍기위한구역);
                }
                else if (OmokLogic.바둑판알(OmokLogic.전돌x좌표, OmokLogic.전돌y좌표) == (int)CSCommon.OmokRule.돌종류.백돌)
                {
                    g.FillEllipse(흰색, 앞에찍은돌을다시찍기위한구역);
                }
            }

            // 화점 크기만큼 빨간 점 찍기
            g.FillEllipse(빨간색, r);
        }

        void 현재턴_플레이어_정보()        // 화면 하단에 다음에 둘 돌의 색을 표시
        {
            Graphics g = panel1.CreateGraphics();
            string str;
            Font 글꼴 = new Font("HY견고딕", 15);

            if (IsMyTurn)
            {
                if(IsBlackUser)
                {
                    str = "현재 턴 돌";
                    g.FillEllipse(검은색, 시작위치 + 115, 599, 돌크기, 돌크기);
                    g.DrawString(str, 글꼴, 검은색, 시작위치, 600);

                    g.DrawString($"PlayerName: {흑돌플레이어Name}", 글꼴, 검은색, (시작위치 + 120 + 돌크기), 600);
                }
                else
                {
                    str = "현재 턴 돌";
                    g.FillEllipse(흰색, 시작위치 + 115, 599, 돌크기, 돌크기);
                    g.DrawString(str, 글꼴, 검은색, 시작위치, 600);

                    g.DrawString($"PlayerName: {백돌플레이어Name}", 글꼴, 검은색, (시작위치 + 120 + 돌크기), 600);
                } 
            }
            else // 다음 돌 표시
            {
                if (IsBlackUser == false)
                {
                    str = "현재 턴 돌";
                    g.FillEllipse(검은색, 시작위치 + 115, 599, 돌크기, 돌크기);
                    g.DrawString(str, 글꼴, 검은색, 시작위치, 600);

                    g.DrawString($"PlayerName: {흑돌플레이어Name}", 글꼴, 검은색, (시작위치 + 120 + 돌크기), 600);
                }
                else
                {
                    str = "현재 턴 돌";
                    g.FillEllipse(흰색, 시작위치 + 115, 599, 돌크기, 돌크기);
                    g.DrawString(str, 글꼴, 검은색, 시작위치, 600);

                    g.DrawString($"PlayerName: {백돌플레이어Name}", 글꼴, 검은색, (시작위치 + 120 + 돌크기), 600);
                }
            }


        }


        // 보드에 마우스 클릭시 실행되는 메서드
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (OmokLogic.게임종료 || IsMyTurn == false)
            {
                return;
            }

            DevLog.Write("돌 두기 요청");

            int x, y;

            // 왼쪽클릭만 허용
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            x = (e.X - 시작위치 + 10) / 눈금크기;
            y = (e.Y - 시작위치 + 10) / 눈금크기;

            // 바둑판 크기를 벗어나는지 확인
            if (x < 0 || x >= 바둑판크기 || y < 0 || y >= 바둑판크기)
            {
                return;
            }

            if(IsBlackUser) // 내가 흑돌
            {
                플레이어_돌두기(true, x, y, true);
            }
            else // 내가 백돌
            {
                플레이어_돌두기(true, x, y, false);
            }
        }

        // isNotify가 false인 경우 상대의 돌, true인 경우 나의 돌로 간주
        void 플레이어_돌두기(bool isNotify, int x, int y, bool isBlack)
        {
            if(isBlack)
            {
                DevLog.Write("흑돌");
            }
            else
            {
                DevLog.Write("백돌");
            }

            var ret = OmokLogic.돌두기(x, y, isBlack);

            if(ret != CSCommon.돌두기_결과.Success)
            {
                return;
            }

            돌그리기(x, y);
            현재돌표시();
            OmokLogic.오목확인(x, y);

            // 내가 돌을 두는 경우가 Notify = true인 경우다.
            if (isNotify == true)
            {
                SendPacketOmokPut(x, y);
            }
            
            Rectangle r = new Rectangle(시작위치, 590, 시작위치 + 돌크기 + 350, 돌크기 + 10);
            panel1.Invalidate(r);
        }

        void 플레이어_차례바꾸기()
        {
            // OmokLogic.차례바꾸기();
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)     // 현재 차례의 돌 잔상 구현 (마우스 움직일때)
        {
            // 내 턴인 경우에만 잔상을 표시한다.
            if (OmokLogic.게임종료 || IsMyTurn == false)
            {
                return;
            }

            int x, y;

            Color 검은색투명화 = Color.FromArgb(70, Color.Black);
            Color 흰색투명화 = Color.FromArgb(70, Color.White);
            SolidBrush 투명한검은색 = new SolidBrush(검은색투명화);
            SolidBrush 투명한흰색 = new SolidBrush(흰색투명화);

            x = (e.X - 시작위치 + 10) / 눈금크기;
            y = (e.Y - 시작위치 + 10) / 눈금크기;

            // 바둑판 크기를 벗어나는지 확인
            if (x < 0 || x >= 바둑판크기 || y < 0 || y >= 바둑판크기)
            {
                return;
            }
            else if (OmokLogic.바둑판알(x, y) == (int)CSCommon.OmokRule.돌종류.없음 &&
                        !OmokLogic.게임종료 &&
                        (전x좌표 != x || 전y좌표 != y)
                        )
            {
                // 바둑판 해당 좌표에 아무것도 없고, 좌표가 변경되면
                Graphics g = panel1.CreateGraphics();

                Rectangle 앞에찍은돌을지우기위한구역 = new Rectangle(시작위치 + 눈금크기 * 전x좌표 - 돌크기 / 2,
                                        시작위치 + 눈금크기 * 전y좌표 - 돌크기 / 2, 돌크기, 돌크기);

                Rectangle r = new Rectangle(시작위치 + 눈금크기 * x - 돌크기 / 2,
                                        시작위치 + 눈금크기 * y - 돌크기 / 2, 돌크기, 돌크기);

                // 먼저 그린 잔상을 지우고 새로운 잔상을 그린다.
                panel1.Invalidate(앞에찍은돌을지우기위한구역);

                if (IsBlackUser)
                    g.FillEllipse(투명한검은색, r);
                else
                    g.FillEllipse(투명한흰색, r);

                전x좌표 = x;
                전y좌표 = y;
            }
        }
        #endregion



        void 컴퓨터두기()
        {
            //int x = 0, y = 0;
            //CSCommon.돌두기_결과 ret;

            //do
            //{
            //    OmokAI.AI_PutAIPlayer(ref x, ref y, false, 2);
            //    ret = OmokLogic.돌두기(x, y);
            //} while (ret != CSCommon.돌두기_결과.Success);

            //돌그리기(x, y);
            //현재돌표시();
            //OmokLogic.오목확인(x, y);
        }


        private void UpdateTimer(object state)
        {
            if(remainingTurnTime <= 0)
            {
                return;
            }

            if(IsMyTurn == false || CurSceen != ClientSceen.GAME_PLAYING) // 내 턴에만 동작
            {
                remainingTurnTime = 10;
                return;
            }

            // 남은 시간 감소
            remainingTurnTime--;

            // Label 컨트롤 텍스트 업데이트
            TurnTimeoutLabel.Text = remainingTurnTime.ToString();
        }
    }
}
