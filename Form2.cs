using System;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using OpenCvSharp;
using MySql.Data.MySqlClient;
using System.Net;
using System.Diagnostics; // 프로세스 킬
using System.ComponentModel;



namespace WindowsFormsApp1
{
    public partial class Form2 : Form
    {
        string id;
        string pw;
        string secondpw;
        string login_type;
        string server;
        string name;
        string pc_num;

        Bitmap check_img = null;

  


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
        private const int MouseEV_RightDown = 0x0008;   /* right button down 	*/
        
        private readonly ManualResetEvent stoppeing_event_ = new ManualResetEvent(false);
        TimeSpan interval_;


        public void MouseSetPosNclick(int x, int y)
        {
            try
            {
                MoveTo(x, y);
                stoppeing_event_.WaitOne(interval_);
                MouseClick_now();
            }
            catch (Exception e)
            {
                MessageBox.Show("MouseSetPosNclick\r\n" + e.Message);
            }
        }


        public void MouseSetPosNclick2(int x, int y)
        {
            try
            {
                SetCursorPos(x, y);
                Thread.Sleep(100);
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

        public void InClick(int x, int y)
        {
            Console.WriteLine("X : " + x + " Y : " + y + " 클릭");
            MouseSetPosNclick(x, y);
        }




        // 마우스 자연스러운 이동
        private const float MOUSE_SMOOTH = 200f;

        public static void MoveTo(int targetX, int targetY)
        {
            var targetPosition = new System.Drawing.Point(targetX, targetY);
            var curPos = Cursor.Position;

            var diffX = targetPosition.X - curPos.X;
            var diffY = targetPosition.Y - curPos.Y;

            for (int i = 0; i <= MOUSE_SMOOTH; i++)
            {
                float x = curPos.X + (diffX / MOUSE_SMOOTH * i);
                float y = curPos.Y + (diffY / MOUSE_SMOOTH * i);
                Cursor.Position = new System.Drawing.Point((int)x, (int)y);
                Thread.Sleep(1);
            }

            if (Cursor.Position != targetPosition)
            {
                MoveTo(targetPosition.X, targetPosition.Y);
            }
        }


        //키보드
        [DllImport("user32.dll")]
public static extern void keybd_event(byte vk, byte scan, int flags, ref int extrainfo);


        [System.Runtime.InteropServices.DllImport("User32", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        internal static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, IntPtr lParam);

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr Parent, IntPtr Child, string lpszClass, string lpszWindows);

        [DllImport("user32.dll")]
        public static extern int BringWindowToTop(IntPtr hwnd);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        
        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;

        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);





        

        //메이플 핸들
        String AppPlayerName = "MapleStory";
        //String AppPlayerName = "사진";
   

        System.Threading.Thread TrackWorkthread; // 선언
        public Form2()
        {
            InitializeComponent();

                //ngm 모두 종료
                Process[] processList = Process.GetProcessesByName("NGM.Client.Editor");
            if (processList.Length > 0) {
                processList[0].Kill();
            }
            Process[] processList2 = Process.GetProcessesByName("NGM.Client.Player");
            if (processList2.Length > 0)
            {
                processList2[0].Kill();
            }
            

        }
        public void stop()
        {
        }
            public void init() { /////////// 매크로 시작 //////////////////////
                                 //ngm에서 게임 실행 후 종료 되면 이 스크립트 부터 시작

            ////////////////////////////사용 변수 셋팅////////////////////////
            

            get_server_db();
            //server = "엘리시움";
            
            Console.WriteLine(server);
            Console.WriteLine(id);
            Console.WriteLine(name);
            Console.WriteLine(pw);
            Console.WriteLine(login_type);
            Console.WriteLine(secondpw);
            send_msg_db("작업 시작", 'B');

            

        Start: // 접속 시작
            if (maple_check() == 0)//메이플 상태확인 켜지지 않았으면 대기
            {  
                window_set(); //메이플 화면 이동 및 최상단
                //invailed_check(); 레드pc? 잠긴 계정 있는지 체크
                //lie_check(); 거짓말 탐지기확인

                Thread.Sleep(1000);
                if(done_macro() == 0) { // 접속 완료 체크
                Second_pw:
                    second_pw_matching();// 2차 비밀번호 입력
                    
                    Thread.Sleep(1000);
                Character:
                    first_character();// 첫캐릭터 선택
                    
                    Thread.Sleep(1000);
                Channel:
                    rand_channel(); //랜덤 채널 선택
                    Thread.Sleep(1000);
                Server:
                    select_server(server); // 서버선택
                    Thread.Sleep(1000);
                Invailed:
                    invailed_check2(); //비활성화 체크
                    Thread.Sleep(1000);
                    input_id_pw(); //로그인 타입 확인, id,pw 입력
                    Thread.Sleep(1000);
                    select_id(); // 계정 선택
                    Thread.Sleep(1000);
                    error_check();
                    goto Start;
                }
            ////////////////////////////접속 완료 /////////////////////
            Done:
                Console.WriteLine("메이플 접속 완료");
                //finish(); db 에 접속 완료 msg 전송
            }
            else {
                Console.WriteLine("메이플 확인 불가 게임런처 실행");
                // 런처 실행
                game_start(); 
                goto Start;
            }


            ////////////////////////////사용 함수 /////////////////////
            //TrySearch(Bitmap screen_img, Bitmap find_img) 검색된 이미지를 클릭
            //TrySearch_mouse(Bitmap screen_img, Bitmap find_img, int x, int y) 검색된 이미지가 있을시 원하는 곳에 마우스 클릭
        }

        /// <summary>
        /// 사용 함수
        /// </summary>
                //마우스 클릭 이벤트

        public void input_id_pw() {
            if (check_image("로그인창") == 1)
            {
                if (login_type == "메이플") {
                    InClick(440, 320);
                }
                else {
                    InClick(580, 320);
                }

                Thread.Sleep(1000);
                InClick(380, 363);
                int Info = 0;

                Thread.Sleep(1000);

                keybd_event(36, 0, 0, ref Info);
                Thread.Sleep(1);
                keybd_event(36, 0, 0x0002, ref Info);

                Thread.Sleep(1000);

                keybd_event(16, 0, 0, ref Info); // shift
                Thread.Sleep(1);
                keybd_event(35, 0, 0, ref Info);
                Thread.Sleep(1);
                keybd_event(35, 0, 0x0002, ref Info);
                Thread.Sleep(1000);
                keybd_event(16, 0, 0x0002, ref Info);

                Thread.Sleep(1000);

                keybd_event(46, 0, 0, ref Info);
                Thread.Sleep(1);
                keybd_event(46, 0, 0x0002, ref Info);
                Thread.Sleep(1000);
                //16 쉬프트
                //36 홈
                //46 delete
                //35 end

                SendKeys.SendWait(id);
                Thread.Sleep(1000);
                
                /*
                keybd_event(9, 0, 0, ref Info);   //탭
                Thread.Sleep(1);
                keybd_event(9, 0, 0x0002, ref Info);  //탭
                Thread.Sleep(1000);
                */
                InClick(380, 410);

                Thread.Sleep(1000);

                keybd_event(36, 0, 0, ref Info);
                Thread.Sleep(1);
                keybd_event(36, 0, 0x0002, ref Info);

                Thread.Sleep(1000);

                keybd_event(16, 0, 0, ref Info); // shift
                Thread.Sleep(1);
                keybd_event(35, 0, 0, ref Info);
                Thread.Sleep(1);
                keybd_event(35, 0, 0x0002, ref Info);
                Thread.Sleep(1000);
                keybd_event(16, 0, 0x0002, ref Info);

                Thread.Sleep(1000);

                keybd_event(46, 0, 0, ref Info);
                Thread.Sleep(1);
                keybd_event(46, 0, 0x0002, ref Info);
                Thread.Sleep(1000);
                //16 쉬프트
                //36 홈
                //46 delete
                //35 end
                Thread.Sleep(1000);


                SendKeys.SendWait(pw);

                Thread.Sleep(1000);
                keybd_event(13, 0, 0, ref Info);   //엔터
                Thread.Sleep(100);
                keybd_event(13, 0, 0x0002, ref Info);  //엔터
                Thread.Sleep(1000);
            }
            else
            {
                Console.WriteLine("로그인창 확인 불가");
            }
        }

        public void select_id()
        {
            if (check_image("계정선택") == 1)
            {

                Thread.Sleep(1000);
                int Info = 0;
                keybd_event(40, 0, 0, ref Info);   //아래
                Thread.Sleep(1);
                keybd_event(40, 0, 0x0002, ref Info);  //아래
                Thread.Sleep(1000);
                keybd_event(13, 0, 0, ref Info);   //엔터
                Thread.Sleep(1);
                keybd_event(13, 0, 0x0002, ref Info);  //엔터
                Thread.Sleep(1000);
            }
            else
            {
                Console.WriteLine("계정선택 화면 확인 불가");
            }
        }
        //컴퓨터 번호
        public string com_num()
        {
            IPHostEntry IPHost = Dns.GetHostByName(Dns.GetHostName());
            string myip = IPHost.AddressList[0].ToString();
            Console.WriteLine(myip);
            //문자열 치환
            if (myip.Contains("114.202.76."))
            {
                myip = myip.Replace("114.202.76.", "");
                myip = "1-" + myip;
                pc_num = "2-" + myip;
            }
            else if (myip.Contains("123.212.128."))
            {
                myip = myip.Replace("123.212.128.", "");
                myip = "2-" + myip;
                pc_num = "2-" + myip;
            }
            else
            {
                myip = "1-300"; // test 용
                pc_num = "1-300";
            }
            return myip;
        }

        public void game_start()
        {
            //Process.Start("D:/Nexon/Maple/GameLauncher.exe");  //PC방 좌표
            Process.Start("C:/Nexon/Maple/GameLauncher.exe");
            Thread.Sleep(2000);
            int Info = 0;
            keybd_event(40, 0, 0, ref Info);   // ALT key 다운
            Thread.Sleep(1);
            keybd_event(40, 0, 0x0002, ref Info);  // ALT key 업
            Thread.Sleep(2000);

            keybd_event(13, 0, 0, ref Info);   //엔터
            Thread.Sleep(1);
            keybd_event(13, 0, 0x0002, ref Info);  //엔터

            Thread.Sleep(15000);
        }


        /* public int done_check() {
            // return ;
         }
         */
        // 데이터베이스
        public void get_server_db()
        {
            try
            {
                MySqlConnection conn;
                string strconn = "Server = 27.124.211.103; database = maruplaydb; uid = maru; pwd = 1234;";
                string cum_num = com_num();
                conn = new MySqlConnection(strconn);
                conn.Open();
                Console.WriteLine("데이터베이스에 연결하였습니다.", "Information");
                string select_query = "select game_server,id,pw,login_type,2pw,cus_code from start_user_table where pc_num = '" + cum_num + "'";
                MySqlCommand command = new MySqlCommand(select_query, conn); // db 작동
                MySqlDataReader table = command.ExecuteReader(); // 데이터 처리
                while (table.Read())
                {
                    server = (String)table["game_server"];
                    name = (String)table["cus_code"];
                    id = (String)table["id"];
                    pw = (String)table["pw"];
                    login_type = (String)table["login_type"];
                    secondpw = (String)table["2pw"];
                }
                table.Close();
                //DB닫기
                conn.Close();
                //return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, "Information");
                //return "1";
            }
        }
        public void send_msg_db(string msg, char state)
        {
            try
            {
                // msg 삽입
                MySqlConnection conn;
                string strconn = "Server = 27.124.211.103; database = maruplaydb; uid = maru; pwd = 1234;";
                string cum_num = com_num();
                conn = new MySqlConnection(strconn);
                conn.Open();
                Console.WriteLine("데이터베이스에 연결하였습니다.", "Information");
                string select_query = "UPDATE msg_new SET stamp = now(), cus_code='"+ name + "', id='"+ id + "',state='"+ state + "',msg='"+msg+"' WHERE pc_num = '"+pc_num+"';";

                MySqlCommand command = new MySqlCommand(select_query, conn); // db 작동

                MySqlDataReader table = command.ExecuteReader(); // 데이터 처리

                //DB닫기
                conn.Close();
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, "Information");
               
            }
        }
        /// <summary>
        /// 사용함수
        /// </summary>
        /// 
        //////////////////////////// 메이플 상태 확인/////////////////////
        public void window_set() {
            IntPtr findwindow = FindWindow(null, AppPlayerName);
            if (!findwindow.Equals(IntPtr.Zero))
            {
                //플레이어 창 크기 만큼의 비트맵을 선언해줍니다.

                BringWindowToTop(findwindow);
                SetWindowPos(findwindow, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE);
                ShowWindowAsync(findwindow, SW_SHOWNORMAL);
                /*
                MoveWindow(findwindow, 0, 0, 0, 0, true);
                
                SetForegroundWindow(findwindow); // 메이플 맨 앞으로 작동 .
                */
            }
        }

        public int maple_check() {
            IntPtr findwindow = FindWindow(null, AppPlayerName);
            if (findwindow != IntPtr.Zero)
            {
                // 메이플 핸들을 찾았을때
                Console.WriteLine("메이플 스토리발견 동작 실행");
                send_msg_db("런처 실행 중", 'C');
                return 0;
            }
            else {
                // 메이플 핸들을 찾지 못했을때
                Console.WriteLine("메이플 스토리를 찾지 못했습니다... 150초 대기중");
                return 1;
            }
        }

        public void invailed_check2()
        {
            check_etc("비활성화", 512, 438);
        }

        /// <summary>
        /// 서버 부분
        /// </summary>
        /// <param name="server"></param>

        public void select_server(string server)
        {
            MoveTo(5, 35);
            //InClick( 5, 35); // 마우스 초기화
            Thread.Sleep(100);
            
            //검증
            if (check_image("서버선택") == 1)
            {
                Bitmap server_img = null;
                server_img = new Bitmap(System.Windows.Forms.Application.StartupPath + @"\img\server\" + server + ".PNG");
                Console.WriteLine(System.Windows.Forms.Application.StartupPath + @"\img\server\" + server + ".PNG");
                IntPtr findwindow = FindWindow(null, AppPlayerName);
                if (findwindow != IntPtr.Zero)
                {

                    //찾은 플레이어를 바탕으로 Graphics 정보를 가져옵니다.
                    Graphics Graphicsdata = Graphics.FromHwnd(findwindow);

                    //찾은 플레이어 창 크기 및 위치를 가져옵니다. 
                    Rectangle rect = Rectangle.Round(Graphicsdata.VisibleClipBounds);

                    //플레이어 창 크기 만큼의 비트맵을 선언해줍니다.
                    
                    Bitmap bmp = new Bitmap(rect.Width, rect.Height);
                    //Bitmap bmp = new Bitmap(1024, 768);
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
                    bmp.Dispose();
                    server_img.Dispose();

                    //
                }
                else
                {
                    Console.WriteLine("메이플스토리를 찾을 수 없습니다.");
                }
            }
            else {
                Console.WriteLine("서버선택창 확인 불가");
            }
        }
        public void rand_channel()
        {

            if (check_image("채널선택") == 1)
            {
                Random x = new Random();
                Random y = new Random();
                int locx = x.Next(360, 680);
                int locy = y.Next(360, 550);
                InClick(locx, locy);
                InClick(locx, locy);
                send_msg_db("채널선택 중", 'C');
            }
            else
            {
                Console.WriteLine("채널선택창 확인 불가");
            }
        }
        public void error_check() {
            if(check_image("2차비밀번호틀림") == 1)
            {
                send_msg_db("2차 비밀번호 틀림", 'A');
                MessageBox.Show("2차 비밀번호 틀림");
            }
            if (check_image("error1") == 1)
            {
                send_msg_db("접속 오류", 'B');
                MessageBox.Show("접속 오류");
            }
            if (check_image("OTP") == 1)
            {
                send_msg_db("OTP 발생", 'B');
                MessageBox.Show("OTP 발생");
            }
            if (check_image("넥슨아이디 전환") == 1)
            {
                send_msg_db("접속 오류", 'B');
                MessageBox.Show("접속 오류");
            }
            if (check_image("등록되지않은") == 1)
            {
                send_msg_db("접속 오류", 'B');
                MessageBox.Show("접속 오류");
            }
            if (check_image("메이플ID") == 1)
            {
                send_msg_db("접속 오류", 'B');
                MessageBox.Show("접속 오류");
            }
            if (check_image("서버연결끊김") == 1)
            {
                send_msg_db("클라이언트 종료", 'A');
                MessageBox.Show("접속 오류");
            }
            if (check_image("첫번째자리캐릭터없음") == 1)
            {
                send_msg_db("캐릭터 없음", 'B');
                MessageBox.Show("캐릭터 없음");
            }
            /*
            if (check_image("거짓말탐지기") == 1)
            {

            }*/

        }

        public int done_macro()
        {
            if (check_image("접속완료") == 1)
            {
                send_msg_db("접속 완료", 'D');
                return 1;
            }
            else
            {
                Console.WriteLine("접속완료 확인 불가");
                return 0;
            }

        }

        /// <summary>
        /// 접속 부분
        /// </summary>
        /// 

        public void first_character()
        {
            MoveTo(5, 35);
            Thread.Sleep(1000);
            if (check_image("캐릭터선택") == 1)
            {
                if (check_image("2차비밀번호") != 1) {
                    MouseSetPosNclick2(131, 361);
                    MouseSetPosNclick2(131, 361);
                    
                    send_msg_db("캐릭터 선택", 'C');
                }
            }
            else {
                Console.WriteLine("캐릭터선택창 확인 불가");
            }
        }

        public void second_pw_matching()
        {
            MoveTo(5, 35);
            
            if (check_image("2차비밀번호") == 1)
            {
                send_msg_db("2차 비밀번호 입력중", 'C');
                //검증
                sencond_pwd("q");
                MoveTo(5, 35);
                Thread.Sleep(1000);

                sencond_pwd("q");
                MoveTo(5, 35);
                Thread.Sleep(1000);

                sencond_pwd("w");
                MoveTo(5, 35);
                Thread.Sleep(1000);

                sencond_pwd("w");
                MoveTo(5, 35);
                Thread.Sleep(1000);

                sencond_pwd("1");
                MoveTo(5, 35);
                Thread.Sleep(1000);

                sencond_pwd("1");
                MoveTo(5, 35);
                Thread.Sleep(1000);

                sencond_pwd("확인");
                MoveTo(5, 35);
                Thread.Sleep(1000);
            }
            else {
                Console.WriteLine("2차비밀번호 창 확인 불가");
               
            }
        }

        public void sencond_pwd(string pwd)
        {
            MoveTo(5, 35);
            
            Thread.Sleep(1000);
            Bitmap server_img = null;
            server_img = new Bitmap(System.Windows.Forms.Application.StartupPath + @"\img\2pw\" + pwd + ".PNG");

            IntPtr findwindow = FindWindow(null, AppPlayerName);
            if (findwindow != IntPtr.Zero)
            {
                try
                {
                    //플레이어를 찾았을 경우
                    Console.WriteLine("앱플레이어 찾았습니다.");

                //찾은 플레이어를 바탕으로 Graphics 정보를 가져옵니다.
                Graphics Graphicsdata = Graphics.FromHwnd(findwindow);

                //찾은 플레이어 창 크기 및 위치를 가져옵니다. 
                Rectangle rect = Rectangle.Round(Graphicsdata.VisibleClipBounds);

                //플레이어 창 크기 만큼의 비트맵을 선언해줍니다.

                //Bitmap bmp = new Bitmap(1024, 768);
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
                bmp.Dispose();
                server_img.Dispose();
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
            else
            {
                Console.WriteLine("앱플레이어 찾을 수 없습니다.");
            }

        }

        public void check_etc(string etc,int x, int y)   
        {
            Thread.Sleep(1000);
            Console.WriteLine(etc + " 처리중.");
            Bitmap etc_img = null;
            etc_img = new Bitmap(System.Windows.Forms.Application.StartupPath + @"\img\check\"+ etc + ".PNG");
            Console.WriteLine(System.Windows.Forms.Application.StartupPath + @"\img\check\" + etc + ".PNG");
            IntPtr findwindow = FindWindow(null, AppPlayerName);
            if (findwindow != IntPtr.Zero)
            {
                try
                {
                    //플레이어를 찾았을 경우

                    //찾은 플레이어를 바탕으로 Graphics 정보를 가져옵니다.
                    Graphics Graphicsdata = Graphics.FromHwnd(findwindow);

                //찾은 플레이어 창 크기 및 위치를 가져옵니다. 
                Rectangle rect = Rectangle.Round(Graphicsdata.VisibleClipBounds);

                //플레이어 창 크기 만큼의 비트맵을 선언해줍니다.

                //Bitmap bmp = new Bitmap(1030, 797);
                Bitmap bmp = new Bitmap(rect.Width, rect.Height+50);
                   

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
                TrySearch_mouse(bmp, etc_img, x, y);
                bmp.Dispose();
                etc_img.Dispose();
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
            else {
                Console.WriteLine("메이플스토리를 찾을 수 없습니다.");
            }
        }
        public int check_image(string state)
        {
            Thread.Sleep(1000);
            Console.WriteLine(state + " 확인중.");
            
            check_img = new Bitmap(System.Windows.Forms.Application.StartupPath + @"\img\check\" + state + ".PNG");
            Console.WriteLine(System.Windows.Forms.Application.StartupPath + @"\img\check\" + state + ".PNG");

            IntPtr findwindow = FindWindow(null, AppPlayerName);
            if (findwindow != IntPtr.Zero)
            {
                try
                {
                    //플레이어를 찾았을 경우
                    Console.WriteLine(findwindow);
                //찾은 플레이어를 바탕으로 Graphics 정보를 가져옵니다.
                Graphics Graphicsdata = Graphics.FromHwnd(findwindow);
                Console.WriteLine(Graphicsdata);
               //찾은 플레이어 창 크기 및 위치를 가져옵니다. 
               Rectangle rect = Rectangle.Round(Graphicsdata.VisibleClipBounds);
                Console.WriteLine(rect);
                //플레이어 창 크기 만큼의 비트맵을 선언해줍니다.

                TypeConverter tc = TypeDescriptor.GetConverter(typeof(Bitmap));
                
                //Bitmap bmp = new Bitmap(1024, 768);

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
                if (pictureBox1.Image != null)
                    pictureBox1.Image.Dispose();

                pictureBox1.Image = bmp;
                
                int result = TrySearch_image(bmp, check_img);
                bmp.Dispose();
                check_img.Dispose();
                

                return result;
                }
                catch (Exception e)
                {
                    Console.WriteLine("오류");
                    Console.WriteLine(e.Message, ToString());
                    return 0;
                }
                finally
                {
                    
                }
            }
            else
            {
                Console.WriteLine("메이플스토리를 찾을 수 없습니다.");
                return 0;
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

                    if (maxval >= 0.8)
                    {
                        InClick(maxloc.X + 10, maxloc.Y + 20);
                        
                    }
                    screen_img.Dispose();
                    find_img.Dispose();

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

                    if (maxval >= 0.8)
                    {
                        //InClick(maxloc.X + 10, maxloc.Y + 20);
                        InClick(x, y);
                    }
                    screen_img.Dispose();
                    find_img.Dispose();
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
        public int TrySearch_image(Bitmap screen_img, Bitmap find_img)
        {
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
                    if (maxval >= 0.8)
                    {
                        return 1;
                    }
                    else {
                        return 0;
                    }
                    screen_img.Dispose();
                    find_img.Dispose();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("오류");
                Console.WriteLine(e.Message, ToString());
                return 0;
            }
        }

        private void form2_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            TrackWorkthread = new Thread(new ThreadStart(init)); //DrawTrack은 쓰레드로 구동시킬 함수
            TrackWorkthread.Start();
        }

    private void button2_Click(object sender, EventArgs e)
        {
            //TrackWorkthread.Interrupt();
            TrackWorkthread.Abort(); //종료
        }
    }
}
