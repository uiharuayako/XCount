using System.Collections.Generic;
using System.Text;

namespace XCount
{
    public class CountResults
    {
        // 指示监听器是否开启
        public static bool isUpdate { get; set; }
        public static int CountAll { get; set; }
        public static int CountWar { get; set; }
        public static int CountNoWar { get; set; }
        public static int TravelPlayer { get; set; }
        public static int UnionPlayer { get; set; }
        public static StringBuilder resultListStr { get; set; }
        // 静态构造函数
        static CountResults()
        {
            CountAll = 0;
            CountWar = 0;
            CountNoWar = 0;
            TravelPlayer = 0;
            UnionPlayer = 0;
            isUpdate = false;
            resultListStr = new StringBuilder("");
        }
        // 替换文字
        // <all>：全部玩家数量
        // <war>：战职玩家数量
        // <nowar>：非战职玩家数量
        public static string ResultString(string inputStr)
        {
            return inputStr.Replace("<all>", CountAll.ToString()).Replace("<war>", CountWar.ToString()).Replace("<nowar>", CountNoWar.ToString()).Replace("<foreign>",TravelPlayer.ToString());
        }
        public static string HelpMsg()
        {
            return "<all>：全部玩家数量\n<war>：战职玩家数量\n<nowar>：非战职玩家数量\n<foreign>：放浪神数量";
        }
    }
}
