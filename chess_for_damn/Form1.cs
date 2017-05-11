using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;

namespace chess_for_damn
{
    
    public partial class Form1 : Form
    {

        static int port = 8005;

        public static string defaultBoard = "000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000";
        TcpListener Listener;
       

        Cell[,] pole = new Cell[8, 8];
        Cell buf_pic;
        Cell otkuda_jum;

        int flag_for_timer = 1;

        Cell[,] buf = new Cell[8, 8]; // AI cup

        List<string> history = new List<string>();
        int history_idx = 0;
        
        int flagOnStep = 1;//

        int flagOnStopServer = 0;

        string white_player_name = "";
        string black_player_name = "";



        //
        //Condition:
        //1 - white
        //2 - black






        private void mainP()
        {
            Listener.Start();  
            Client( Listener.AcceptTcpClient());
            Listener.Stop();
            

        }

        public void Client(TcpClient Client)
        {
            if (Client == null)
                return;
            // INFO
            try
            {
                string Str = SaveToString();
                
                Str = SaveToString();
   
                // Необходимые заголовки: ответ сервера, тип и длина содержимого. После двух пустых строк - само содержимое
                
                // Приведем строку к виду массива байт
                byte[] byte_buffer = Encoding.ASCII.GetBytes(Str);
                //END INFO



                string player_color = "";
                // Буфер для хранения принятых от клиента данных
                byte[] Buffer = new byte[1024];
                // Переменная для хранения  байт
                int Count;
                // Читаем из потока 
                while ((Count = Client.GetStream().Read(Buffer, 0, Buffer.Length)) > 0)
                {
                    // Преобразуем в строку
                    player_color += Encoding.ASCII.GetString(Buffer, 0, Count);
                    // Запрос должен обрываться последовательностью \r\n\r\n
                    // Либо обрываем прием данных сами, если длина строки Request превышает 1 килобайт
                
                    if (player_color.IndexOf("\r\n\r\n") >= 0 || player_color.Length > 1024)
                    {
                        break;
                    }
                }



                int flagOnStep_Client = -1;

                if (player_color.Substring(0,5) == "black")
                {
                    flagOnStep_Client = 0;
                    try { 
                    black_player_name = player_color.Substring(7, player_color.Length - 11);
                    }
                    catch(Exception e) { }
                }
                    
                else if (player_color.Substring(0, 5) == "white")
                {
                    flagOnStep_Client = 1;
                    try { 
                    white_player_name = player_color.Substring(6,player_color.Length-11);
                    }
                    catch (Exception e) { }
                }

                if (flagOnStep_Client == -1 || flagOnStep_Client != flagOnStep)
                {
                    Str = "BAD\r\n\r\n";
                    byte_buffer = Encoding.ASCII.GetBytes(Str);
                    Client.GetStream().Write(byte_buffer, 0, byte_buffer.Length);
                    //Client.GetStream().Flush();
                    //Client.GetStream().Close();
                    Client.Close();
                    mainP();
                    return;
                }




                Client.GetStream().Flush();
                // Отправим его клиенту
                Client.GetStream().Write(byte_buffer, 0, byte_buffer.Length);
                Client.GetStream().Flush();

                String Request = ""; // читаем заново
                Buffer = new byte[1024];
                while ((Count = Client.GetStream().Read(Buffer, 0, Buffer.Length)) > 0)
                {
                    Request += Encoding.ASCII.GetString(Buffer, 0, Count);
                    if (Request.IndexOf("\r\n\r\n") >= 0 || Request.Length > 4096)
                    {
                        break;
                    }
                }

                if(Request.Length < 64)
                {
                    richTextBox1.Text += "Плохие данные получены от клиента : " + player_color;
                    return;
                }

                history.Add(Request); // добавим в историю ходов
                history_idx++;

                draw_with_check(Request, (flagOnStep == 1 ? 1 : 2 ) ); // (flagOnStep+1)%2+1 ?

                // Закроем соединение
                Client.Close();
                flagOnStep = flagOnStep == 1 ? 0 : 1;
            }
            catch (Exception e)
            {
                mainP();
            }
        }










        public Form1()
        {
            InitializeComponent();
        }


        
        private void Form1_Load(object sender, EventArgs e)
        {
            label1.Text = "Ход Белых";

            System.Windows.Forms.PictureBox[,] mass = new System.Windows.Forms.PictureBox[8, 8]; // инициализируем массив пикчербокса

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    
                    int f = 0;                                              //
                    string nam = "";                                        //
                    nam = "PBox" + i + j;                                   // создаем новый пикчербокс
                    mass[i, j] = new System.Windows.Forms.PictureBox();     // и задаем ему нужные Св-Ва
                    
                    if ((i + 1) % 2 == 1)
                    {
                        if ((j + 1) % 2 == 1)
                        {
                            mass[i, j].Image = white.Image;
                        }
                        else
                        {
                            if (i < 3)
                            {
                                f = 1;
                                mass[i, j].Image = white_chess.Image;
                            }
                            else if (i > 4)
                            {
                                f = 2;
                                mass[i, j].Image = black_chess.Image;
                            }
                            else
                            {
                                if (i < 3)
                                {
                                    f = 1;
                                    mass[i, j].Image = white_chess.Image;
                                }
                                else if (i > 4)
                                {
                                    f = 2;
                                    mass[i, j].Image = black_chess.Image;
                                }
                                else
                                    mass[i, j].Image = black.Image;
                            }
                        }
                    }
                    else
                    {
                        if ((j + 2) % 2 == 0)
                        {
                            if (i < 3)
                            {
                                f = 1;
                                mass[i, j].Image = white_chess.Image;
                            }
                            else if (i > 4)
                            {
                                f = 2;
                                mass[i, j].Image = black_chess.Image;
                            }
                            else
                                mass[i, j].Image = black.Image;
                        }
                        else
                        {

                            mass[i, j].Image = white.Image;
                        }
                    }
                    mass[i, j].Name = nam;
                    mass[i, j].Location = new System.Drawing.Point(j * 70, i * 70);
                    mass[i, j].Size = new System.Drawing.Size(70, 70);
                    mass[i, j].TabIndex = 0;
                    mass[i, j].TabStop = false;
                    //mass[i, j].Click += new System.EventHandler(this.Any_Click);
                    this.Controls.Add(mass[i, j]);
                    
                    pole[i, j] = new Cell(mass[i, j], f);       // f - Condition
                                                                // Condition:
                                                                // 1 - White
                                                                // 2 - Black          
                }
            }
            defaultBoard = SaveToString();
            //draw("0101010110101010010101010000000000020000200020200202020220202020");
            // MessageBox.Show(SaveToString());
            history.Add(SaveToString());
            //Socket working begin
            //
            // получаем адреса для запуска сокета
            //ListenerFirst
            Listener = new TcpListener(IPAddress.Any,port);
            //mainP();

            string s1= C_File.read_current_score("first","second");
            C_File.add_new_score("first","second",2,2);
        }









        private void checkToWin()
        {
             
            int white_win = 1;
            int black_win = 1;
            for(int i =0; i < 8; i ++)
            {
                for(int j = 0; j < 8; j++)
                {
                    if (pole[i, j].mypic.Image == white_chess.Image || pole[i, j].mypic.Image == white_queen.Image)
                        black_win = 0;

                    if (pole[i, j].mypic.Image == black_chess.Image || pole[i, j].mypic.Image == black_queen.Image )
                        white_win = 0;
                }
            }

            if (white_win == 1)
            {
                flagOnStopServer = 1;
                Win("White");
                return;
            }
            if (black_win == 1)
            {
                flagOnStopServer = 1;
                Win("Black");
                return;
            }

            List<string> pos_moves = new Board(SaveToString()).getPossibleBoards_s(flagOnStep == 1? 1 : 2);
            if (pos_moves.Count == 0)
            {
                flagOnStopServer = 1;
                MessageBox.Show("Draw");
                Back_button.Enabled = true;
                Forvard_button.Enabled = true;

                try { 
                    C_File.add_new_score(white_player_name, black_player_name, 1, 1);
                }catch(Exception e)
                {
                    MessageBox.Show("Ошибка работы с файлом!");
                }

                return;
            }
            
        }

        private void Win(string who)
        {
            //Clear();
            MessageBox.Show(who+" WON!!!");              // Вывод сообщения о победе
            label1.Text = "NICE JOB, YOU MAY EXIT";         // И еще мы меняем лейблик

            try { 
            if(who == "White")
            {
                C_File.add_new_score(white_player_name, black_player_name, 2, 0);
            }
            else
            {
                C_File.add_new_score(white_player_name, black_player_name, 0, 2);
            }
            }
            catch (Exception e)
            {
                MessageBox.Show("Ошибка работы с файлом!");
            }

            Back_button.Enabled = true;
            Forvard_button.Enabled = true;
        }


        

        private void MoveIt(Cell trs)
        {
            

        }

        private string getBUKVA(int n)
        {
            //switch для букв по номеру
            string Ans = "";
            switch (n)
            {
                case 0:
                    Ans += "a";
                    break;
                case 1:
                    Ans += "b";
                    break;
                case 2:
                    Ans += "c";
                    break;
                case 3:
                    Ans += "d";
                    break;
                case 4:
                    Ans += "e";
                    break;
                case 5:
                    Ans += "f";
                    break;
                case 6:
                    Ans += "g";
                    break;
                case 7:
                    Ans += "h";
                    break;
            }
            return Ans;

        }

        private void parseTo(int y0, int x0, int y1, int x1)
        {
            // парсинг ходов
            string additiveString = "";
            additiveString += getBUKVA(x0);
            additiveString += " " + (y0+1).ToString() + " --> ";
            additiveString += getBUKVA(x1);
            additiveString += " " + (y1 + 1).ToString() + "\n";

            richTextBox1.Text += additiveString;                
        }
        

        private void ClearLongItems(Cell trs)
        {
        }

        private void ClearDelItems()
        {
 
        }

        private void Clear(List<Cell> trs)
        {
            //
            //перегрузка чтобы "чистить" все возможные List-ы
            //
            int len = trs.Count;

            for (int i = 0; i < len; i++)
            {

                int y = trs[0].y;
                int x = trs[0].x;

                pole[y, x].mypic.Image = black.Image;
                pole[y, x].Condition = 0;
                //
                trs.Remove(trs[0]);
            }
        }
        

        private void pictureBox1_Click(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            Form1 FORMA = new Form1();
            FORMA.Show();
            this.Hide();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (flagOnStopServer == 0)
            {
                mainP();
                checkToWin();
                Board tmp = new Board(SaveToString());
                label1.Text = "Белые - " + (12 - tmp.black_checkers - tmp.black_queens) + " : " + (12 - tmp.white_checkers - tmp.white_queens) + " - Черные"; 
            }
            else
            {
                
            }
            
        }

        private void parse(String s)
        {

        }

        private void draw_with_check(string board, int player)
        {
            string cur_board = this.SaveToString();
            Board brd = new Board(cur_board);
            List<string> pos_boards = brd.getPossibleBoards_s(player);
            pos_boards.Add(cur_board);
            
            for(int i =0; i < pos_boards.Count; i++)
            {
                if (pos_boards[i].Substring(0,64) == board.Substring(0,64) )
                {
                    draw(pos_boards[i]);
                    return;
                }
            }
            MessageBox.Show("неправильный ход"+(player == 1 ? " белых" : " черных"));

        }

        private void draw(String s)
        {

            try
            {
                team_names.Text = white_player_name + " - " + black_player_name;
                score_label.Text = C_File.read_current_score(white_player_name, black_player_name);
            }catch(Exception e)
            {
                MessageBox.Show("ошибка работы с файлом!");
            }

            int k = 0;
             for (int i =0; i < 8; i++)
            {
                for (int j =0; j< 8; j++)
                {
                    if (s[k] == '1')
                    {
                        pole[i, j].mypic.Image = white_chess.Image;
                    }
                    else if (s[k] == '2')
                    {
                        pole[i, j].mypic.Image = black_chess.Image;
                    }
                    else if (s[k] == '3')
                    {
                        pole[i, j].mypic.Image = white_queen.Image;
                    }
                    else if (s[k] == '4')
                    {
                        pole[i, j].mypic.Image = black_queen.Image;
                    }
                    else
                    {
                        if ((i + 1) % 2 == 1)
                        {
                            if ((j + 1) % 2 == 1)
                            {
                                pole[i, j].mypic.Image = white.Image;
                            }
                            else
                                pole[i, j].mypic.Image = black.Image;
                        }
                        else
                        {
                            if ((j + 1) % 2 == 1)
                            {
                                pole[i, j].mypic.Image = black.Image;
                            }
                            else
                                pole[i, j].mypic.Image = white.Image;
                        }
                    }
                    k++;
                }
                
            }

            richTextBox1.Text += s.Substring(k);


        }

        private string SaveToString()
        {
            string buf = "";

            for (int i = 0; i < 8; i++)
            {
                for(int j = 0; j < 8; j++)
                {
                    if(pole[i,j].mypic.Image == white.Image || pole[i,j].mypic.Image == black.Image)
                    {
                        buf = buf + "0";
                    }
                    else if(pole[i,j].mypic.Image == white_chess.Image)
                    {
                        buf = buf + "1";
                    }
                    else if( pole[i,j].mypic.Image == white_queen.Image)
                    {
                        buf = buf + "3";
                    }
                    else if( pole[i,j].mypic.Image == black_chess.Image)
                    {
                        buf = buf + "2";
                    }
                    else if( pole[i,j].mypic.Image == black_queen.Image)
                    {
                        buf = buf + "4";
                    }
                }
            }

            return buf+"\r\n\r\n";
        }

        private void INIT()
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            draw(defaultBoard);
            timer1.Enabled = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if(history_idx < 0 || history.Count < history_idx)
            {
                Back_button.Enabled = false;
            }
            else
            {
                Forvard_button.Enabled = true;
                draw(history[--history_idx]);
                if(history_idx == 0)
                {
                    Back_button.Enabled = false;
                }

            }
        }

        private void Forvard_button_Click(object sender, EventArgs e)
        {
            if (history_idx < 0 || history.Count-1 <= history_idx)
            {
                Forvard_button.Enabled = false;
            }
            else
            {
                Back_button.Enabled = true;
                draw(history[++history_idx]);
                if (history.Count-1 == history_idx)
                {
                    Forvard_button.Enabled = false;
                }
            }
        }

        

        private void team_names_Click(object sender, EventArgs e)
        {

        }
    }
}

/*
0101010010101010010101010000000000000000002000200202020220200020


0101010110101010010101010000000000020000200020200202020220202020


0101010110101010000101011000000000000000200020200202020220202020


 */
