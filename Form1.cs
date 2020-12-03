using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Security.Permissions;
using System.Diagnostics;
using OpenCvSharp;
namespace WindowsFormsApp1
{
    public partial class form2 : Form
    {
        //창모드
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;
        private const int SW_SHOWMAXIMIZED = 3;

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        //마우스
        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("user32")]
        public static extern int SetCursorPos(int x, int y);

        private const int MouseEV_Move = 0x0001;        /* mouse move 			*/
        private const int MouseEV_LeftDown = 0x0002;    /* left button down 	*/
        private const int MouseEV_LeftUp = 0x0004;  /* left button up 		*/
        private const int MouseEV_RightDown = 0x0008; 	/* right button down 	*/

        private readonly ManualResetEvent stoppeing_event_ = new ManualResetEvent(false);
        TimeSpan interval_;
        public void MouseSetPosNclick(int x, int y)
        {
            try
            {
                SetCursorPos(x, y);
                stoppeing_event_.WaitOne(interval_);
                MouseClick_now();
            }
            catch (Exception e)
            {
                MessageBox.Show("MouseSetPosNclick\r\n" + e.Message);
            }
        }

        public void MouseClick_now()
        {
            try
            {
                mouse_event(MouseEV_LeftDown, 0, 0, 0, 0);
                mouse_event(MouseEV_LeftUp, 0, 0, 0, 0);
                stoppeing_event_.WaitOne(100);
            }
            catch (Exception e)
            {
                MessageBox.Show("MouseClick_now\r\n" + e.Message);
            }
        }

        //마우스 클릭 이벤트
        public void InClick(int x, int y)
        {
            
            Console.WriteLine("X : " + x + " Y : " + y +" 클릭");
            MouseSetPosNclick(x, y);
        }


        //키보드
        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags,UIntPtr dwExtraInfo);
        
        [System.Runtime.InteropServices.DllImport("User32", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        internal static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, IntPtr lParam);

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr Parent, IntPtr Child, string lpszClass, string lpszWindows);

        //메이플 핸들
        String AppPlayerName = "MapleStory";
        //String AppPlayerName = "사진";
        Bitmap test_img = null;


        public form2()
        {
            InitializeComponent();
            init();

        }
 
        public void init() {
            //ngm에서 게임 실행 후 종료 되면 이 스크립트 부터 시작

            Thread.Sleep(1000);
            ////////////////////////////사용 변수 셋팅////////////////////////
            //read_file(); 파일을 읽어서 어느 서버인지 확인

            //////////////////////////// 메이플 상태 확인/////////////////////
            //maple_check(); 켜진지 체크
            window_set(); //메이플 화면 이동 및 최상단
            Thread.Sleep(1000);

            ////////////////////////////정상 접속 계정 확인/////////////////////
            //invailed_check(); 레드pc? 잠긴 계정 그게 있는지 체크

            Console.WriteLine("invailed_check2 실행 전");
            invailed_check2(); //비활성화 체크
            Console.WriteLine("invailed_check2 실행 완료");
            Thread.Sleep(1000);

            ////////////////////////////서버 선택 및 채널 입장/////////////////////
            select_server("엘리시움");
            Thread.Sleep(1000);

            rand_chanal(); //랜덤 채널 선택
            Thread.Sleep(1000);

            ////////////////////////////캐릭터 선택 및 2차비번/////////////////////
            
            first_character();// 첫캐릭터 선택
            Thread.Sleep(1000);

            
            second_pw_matching();// 2차 비밀번호 선택
            Thread.Sleep(1000);
            ////////////////////////////접속 완료 점검/////////////////////
            //lie_check(); 거짓말 탐지기확인
            //done_check(); 접속 완료 체크

            ////////////////////////////접속 완료 /////////////////////
            //finish(); db 에 접속 완료

            ////////////////////////////사용 함수 /////////////////////
            //TrySearch(Bitmap screen_img, Bitmap find_img) 검색된 이미지를 클릭
            //TrySearch_mouse(Bitmap screen_img, Bitmap find_img, int x, int y) 검색된 이미지가 있을시 원하는 곳에 마우스 클릭
        }
        
/// <summary>
/// 사용 함수
/// </summary>


        //////////////////////////// 메이플 상태 확인/////////////////////
        public void window_set() {
            IntPtr findwindow = FindWindow(null, AppPlayerName);
            MoveWindow(findwindow, 0, 0, 0, 0, true);
            ShowWindowAsync(findwindow, SW_SHOWNORMAL);
            SetForegroundWindow(findwindow); // 메이플 맨 앞으로 작동 .
        }




        public void invailed_check2()
        {
            Console.WriteLine("invailed_check2 실행");
            check_etc("비활성화", 400, 350);
        }


        /// <summary>
        /// 서버 부분
        /// </summary>
        /// <param name="server"></param>

        public void select_server(string server)
        {
            InClick( 5, 35); // 마우스 초기화
            Thread.Sleep(100);

            Bitmap server_img = null;
            server_img = new Bitmap(System.Windows.Forms.Application.StartupPath + @"\img\server\"+server+".PNG");

            IntPtr findwindow = FindWindow(null, AppPlayerName);
            if (findwindow != IntPtr.Zero)
            {
                //플레이어를 찾았을 경우
                Console.WriteLine("앱플레이어 찾았습니다.");

                //찾은 플레이어를 바탕으로 Graphics 정보를 가져옵니다.
                Graphics Graphicsdata = Graphics.FromHwnd(findwindow);

                //찾은 플레이어 창 크기 및 위치를 가져옵니다. 
                Rectangle rect = Rectangle.Round(Graphicsdata.VisibleClipBounds);

                //플레이어 창 크기 만큼의 비트맵을 선언해줍니다.
                Thread.Sleep(100);
                Bitmap bmp = new Bitmap(rect.Width, rect.Height);

                //비트맵을 바탕으로 그래픽스 함수로 선언해줍니다.
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    //찾은 플레이어의 크기만큼 화면을 캡쳐합니다.
                    IntPtr hdc = g.GetHdc();
                    PrintWindow(findwindow, hdc, 0x2);
                    g.ReleaseHdc(hdc);
                }
                // pictureBox1 이미지를 표시해줍니다.
                pictureBox1.Image = bmp;
                TrySearch(bmp, server_img); // 서버 선택

                //
            }
            else
            {
                Console.WriteLine("앱플레이어 찾을 수 없습니다.");
            }

        }

        public void rand_chanal()
        {
            Random x = new Random();
            Random y = new Random();
            int locx = x.Next(260, 550);
            int locy = y.Next(310, 430);
            InClick(locx, locy);
            InClick(locx, locy);
        }

        /// <summary>
        /// 접속 부분
        /// </summary>
        /// 

        public void first_character()
        {
            InClick(140, 230);
            InClick(140, 230);
        }

        public void second_pw_matching()
        {
            sencond_pwd("q");
            InClick(5, 35); // 마우스 초기화
            Thread.Sleep(1000);

            sencond_pwd("q");
            InClick(5, 35); // 마우스 초기화
            Thread.Sleep(1000);

            sencond_pwd("w");
            InClick(5, 35); // 마우스 초기화
            Thread.Sleep(1000);

            sencond_pwd("w");
            InClick(5, 35); // 마우스 초기화
            Thread.Sleep(1000);

            sencond_pwd("1");
            InClick(5, 35); // 마우스 초기화
            Thread.Sleep(1000);

            sencond_pwd("1");
            InClick(5, 35); // 마우스 초기화
            Thread.Sleep(1000);

            sencond_pwd("확인");
            InClick(5, 35); // 마우스 초기화
            Thread.Sleep(1000);
        }

        public void sencond_pwd(string pwd)
        {
            InClick( 5, 35); // 마우스 초기화
            Thread.Sleep(100);
            Bitmap server_img = null;
            server_img = new Bitmap(System.Windows.Forms.Application.StartupPath + @"\img\2pw\" + pwd + ".PNG");

            IntPtr findwindow = FindWindow(null, AppPlayerName);
            if (findwindow != IntPtr.Zero)
            {
                //플레이어를 찾았을 경우
                Console.WriteLine("앱플레이어 찾았습니다.");

                //찾은 플레이어를 바탕으로 Graphics 정보를 가져옵니다.
                Graphics Graphicsdata = Graphics.FromHwnd(findwindow);

                //찾은 플레이어 창 크기 및 위치를 가져옵니다. 
                Rectangle rect = Rectangle.Round(Graphicsdata.VisibleClipBounds);

                //플레이어 창 크기 만큼의 비트맵을 선언해줍니다.
                Thread.Sleep(100);
                Bitmap bmp = new Bitmap(rect.Width, rect.Height);

                //비트맵을 바탕으로 그래픽스 함수로 선언해줍니다.
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    //찾은 플레이어의 크기만큼 화면을 캡쳐합니다.
                    IntPtr hdc = g.GetHdc();
                    PrintWindow(findwindow, hdc, 0x2);
                    g.ReleaseHdc(hdc);
                }
                // pictureBox1 이미지를 표시해줍니다.
                pictureBox1.Image = bmp;
                TrySearch(bmp, server_img); // 서버 선택
            }
            else
            {
                Console.WriteLine("앱플레이어 찾을 수 없습니다.");
            }

        }


        public void check_etc(string etc,int x, int y)   
        {
            Console.WriteLine(etc + " 처리중.");
            test_img = new Bitmap(System.Windows.Forms.Application.StartupPath + @"\img\check\"+ etc + ".PNG");
            IntPtr findwindow = FindWindow(null, AppPlayerName);
            if (findwindow != IntPtr.Zero)
            {
                //플레이어를 찾았을 경우

                //찾은 플레이어를 바탕으로 Graphics 정보를 가져옵니다.
                Graphics Graphicsdata = Graphics.FromHwnd(findwindow);

                //찾은 플레이어 창 크기 및 위치를 가져옵니다. 
                Rectangle rect = Rectangle.Round(Graphicsdata.VisibleClipBounds);

                //플레이어 창 크기 만큼의 비트맵을 선언해줍니다.
                
                Bitmap bmp = new Bitmap(rect.Width, rect.Height);

                //비트맵을 바탕으로 그래픽스 함수로 선언해줍니다.
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    //찾은 플레이어의 크기만큼 화면을 캡쳐합니다.
                    IntPtr hdc = g.GetHdc();
                    PrintWindow(findwindow, hdc, 0x2);
                    g.ReleaseHdc(hdc);
                }

                // pictureBox1 이미지를 표시해줍니다.
                pictureBox1.Image = bmp;
                TrySearch_mouse(bmp, test_img, x, y);
            }
            else {
                Console.WriteLine("앱플레이어 찾을 수 없습니다.");
            }
        }





        /// <summary>
        /// 이미지 매치 기능
        /// </summary>
        /// <param name="screen_img"></param>
        /// <param name="find_img"></param>
        // 이미지 매치
        public void TrySearch(Bitmap screen_img, Bitmap find_img)
        {
            Console.WriteLine("이미지 매치.");
            //Mat screen = null, find = null, res = null;

            try
            {
                //스크린 이미지 선언
                using (Mat ScreenMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(screen_img))
                //찾을 이미지 선언
                using (Mat FindMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(find_img))
                //스크린 이미지에서 FindMat 이미지를 찾아라
                using (Mat res = ScreenMat.MatchTemplate(FindMat, TemplateMatchModes.CCoeffNormed))
                {
                    //찾은 이미지의 유사도를 담을 더블형 최대 최소 값을 선언합니다.
                    double minval, maxval = 0;
                    //찾은 이미지의 위치를 담을 포인트형을 선업합니다.
                    OpenCvSharp.Point minloc, maxloc;
                    //찾은 이미지의 유사도 및 위치 값을 받습니다. 
                    Cv2.MinMaxLoc(res, out minval, out maxval, out minloc, out maxloc);
                    Console.WriteLine("찾은 이미지의 유사도 : " + maxval);
                    Console.WriteLine(maxloc.X + " " + maxloc.Y);

                    if (maxval >= 0.6)
                    {
                        InClick(maxloc.X + 10, maxloc.Y + 20);
                        
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("오류");
                Console.WriteLine(e.Message, ToString());
            }
            finally
            {

            }
        }

        public void TrySearch_mouse(Bitmap screen_img, Bitmap find_img, int x, int y)
        {
            Console.WriteLine("이미지 매치.");
            //Mat screen = null, find = null, res = null;

            try
            {
                //스크린 이미지 선언
                using (Mat ScreenMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(screen_img))
                //찾을 이미지 선언
                using (Mat FindMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(find_img))
                //스크린 이미지에서 FindMat 이미지를 찾아라
                using (Mat res = ScreenMat.MatchTemplate(FindMat, TemplateMatchModes.CCoeffNormed))
                {
                    //찾은 이미지의 유사도를 담을 더블형 최대 최소 값을 선언합니다.
                    double minval, maxval = 0;
                    //찾은 이미지의 위치를 담을 포인트형을 선업합니다.
                    OpenCvSharp.Point minloc, maxloc;
                    //찾은 이미지의 유사도 및 위치 값을 받습니다. 
                    Cv2.MinMaxLoc(res, out minval, out maxval, out minloc, out maxloc);
                    Console.WriteLine("찾은 이미지의 유사도 : " + maxval);
                    Console.WriteLine(maxloc.X + " " + maxloc.Y);

                    if (maxval >= 0.6)
                    {
                        //InClick(maxloc.X + 10, maxloc.Y + 20);
                        InClick(x, y);
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("오류");
                Console.WriteLine(e.Message, ToString());
            }
            finally
            {

            }
        }
    }
}
