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
        

        TcpListener Listener;
       

        Cell[,] pole = new Cell[8, 8];
        Cell buf_pic;
        Cell otkuda_jum;

        int flag_for_timer = 1;

        Cell[,] buf = new Cell[8, 8]; // AI cup
        
        int flagOnStep = 1;//

        //
        //Condition:
        //1 - white
        //2 - black



        


        private void mainP()
        {
            Listener.Start();  

            while ( true)
            {
                Client(Listener.AcceptTcpClient());
            }
            

        }

        public void Client(TcpClient Client)
        {
            // INFO

            // Необходимые заголовки: ответ сервера, тип и длина содержимого. После двух пустых строк - само содержимое
            string Str = SaveToString();
            // Приведем строку к виду массива байт
            byte[] byte_buffer = Encoding.ASCII.GetBytes(Str);
            //END INFO



            string Request = "";
            // Буфер для хранения принятых от клиента данных
            byte[] Buffer = new byte[1024];
            // Переменная для хранения количества байт, принятых от клиента
            int Count;
            // Читаем из потока клиента до тех пор, пока от него поступают данные
            while ((Count = Client.GetStream().Read(Buffer, 0, Buffer.Length)) > 0)
            {
                // Преобразуем эти данные в строку и добавим ее к переменной Request
                Request += Encoding.ASCII.GetString(Buffer, 0, Count);
                // Запрос должен обрываться последовательностью \r\n\r\n
                // Либо обрываем прием данных сами, если длина строки Request превышает 4 килобайта
                
                if (Request.IndexOf("\r\n\r\n") >= 0 || Request.Length > 4096)
                {
                    break;
                }
            }



            int flagOnStep_Client = -1;

            if (Request == "black\r\n\r\n")
                flagOnStep_Client = 0;
            else if (Request == "white\r\n\r\n")
                flagOnStep_Client = 1;

            if (flagOnStep_Client == -1 || flagOnStep_Client != flagOnStep)
            {
                Str = "BAD";
                byte_buffer = Encoding.ASCII.GetBytes(Str);
                Client.GetStream().Write(byte_buffer, 0, byte_buffer.Length);
                Client.Close();
                return;
            }




            
            // Отправим его клиенту
            Client.GetStream().Write(byte_buffer, 0, byte_buffer.Length);

            Request = "";
            Buffer = new byte[1024];
            while ((Count = Client.GetStream().Read(Buffer, 0, Buffer.Length)) > 0)
            {
                Request += Encoding.ASCII.GetString(Buffer, 0, Count);
                if (Request.IndexOf("\r\n\r\n") >= 0 || Request.Length > 4096)
                {
                    break;
                }
            }

            draw(Request);





            // Закроем соединение
            Client.Close();
            flagOnStep = flagOnStep == 1 ? 0 : 1;
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

           draw("0101010110101010010101010000000000000000202020200202020220202020");
            // MessageBox.Show(SaveToString());

            //Socket working begin
            //
            // получаем адреса для запуска сокета
            //ListenerFirst
            Listener = new TcpListener(IPAddress.Any,port);
            //mainP();


        }









        private void checkToWin()
        {
             
            int white_win = 1;
            int black_win = 1;
            for(int i =0; i < 8; i ++)
            {
                for(int j = 0; j < 8; j++)
                {
                    if (pole[i, j].Condition == 1)
                        black_win = 0;

                    if (pole[i, j].Condition == 2)
                        white_win = 0;
                }
            }

            if (white_win == 1)
                Win("White");
            if (black_win == 1)
                Win("Black");
        }

        private void Win(string who)
        {
            //Clear();
            MessageBox.Show(who+" IS WIN!!!");              // Вывод сообщения о победе
            label1.Text = "NICE JOB, YOU MAY EXIT";         // И еще мы меняем лейблик
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
            //
            //очистка delItems ячеек, соответствующих ячейке в которую мы походили
            //
            /*
            int len = delItems.Count;

            for(int i = 0; i < len; i++)
            {
                if(delItems[i].greenItem.mypic.Name == trs.mypic.Name)
                {
                    Clear(delItems[i].deletedItems);
                }
            }*/
        }

        private void ClearDelItems()
        {
            // очистка для delItems
            /*
            for (int i = 0; i < delItems.Count; i++)
            {
                delItems.Remove(delItems[0]);
            }
        }

        private void Clear()
        {
            //
            //просто чистим массив move_queve
            //
            int len = move_queve.Count;

            for (int i = 0; i < len; i++)
            {

                int y = move_queve[0].y;
                int x = move_queve[0].x;

                pole[y,x].mypic.Image = black.Image;
                pole[y, x].Condition = 0;

                move_queve.Remove(move_queve[0]);
            }
            */
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
            
        }

        private void parse(String s)
        {

        }

        private void draw(String s)
        {
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

            return buf;
        }

        private void INIT()
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            mainP();
        }
    }
}