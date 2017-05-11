using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.IO;
namespace chess_for_damn
{
    class C_File
    {

        
        public static string filename = "score.txt";
        
        public static string read_current_score(string first_player, string second_player)
        {
            float first_player_value = 0;
            float second_player_value = 0;

            List<string> C_value_list = new List<string>();
            string[] C_value = File.ReadAllLines(filename);

            foreach(string el in C_value)
            {
                C_value_list.Add(el);
            }

            for(int i =0;i<C_value_list.Count;i++)
            {
                string[] buf = C_value_list[i].Split('-');
                if (i % 2 == 0)                //если четная -> имя
                {
                    if (buf[0] == first_player && buf[1] == second_player)
                    {
                        string[] buf_score = C_value_list[i + 1].Split('-');
                        first_player_value += float.Parse(buf_score[0]);
                        second_player_value += float.Parse(buf_score[1]);
                    }
                    else if (buf[1] == second_player && buf[0] == second_player)
                    {
                        string[] buf_score = C_value_list[i + 1].Split('-');
                        first_player_value += float.Parse(buf_score[1]);
                        second_player_value += float.Parse(buf_score[0]);
                    }
                }
            }
            return (first_player_value/2.0).ToString() + "-" + (second_player_value / 2.0).ToString();
            
        }


        public static void add_new_score(string first_player, string second_player, int F_val, int S_val)
        {
            string C_value = File.ReadAllText(filename);
            C_value += "\r\n"+first_player+"-"+second_player+"\r\n";
            C_value += F_val + "-" + S_val;
            File.WriteAllText(filename, C_value);
        }





    }
}
