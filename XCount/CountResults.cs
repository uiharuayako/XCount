using System.Collections.Generic;
using System.Text;
using Dalamud.Game.ClientState.Objects.SubKinds;

namespace XCount
{
    public class CountResults
    {
        // 指示监听器是否开启
        public static bool isUpdate { get; set; }
        public static Dictionary<string,int> CountsDict=new Dictionary<string, int>();
        public static StringBuilder resultListStr { get; set; }
        // 静态构造函数
        static CountResults()
        {
            foreach (var str in CountStrings)
            {
                CountsDict.Add(str, 0);
            }
            isUpdate = false;
            resultListStr = new StringBuilder("");
        }
        public static string[] CountStrings = { "<all>", "<nowar>", "<war>", "<foreign>","<inv>","<excel>", "<union>","<enemy>","<targetU>" };

        // 替换文字
        // <all>：全部玩家数量
        // <war>：战职玩家数量
        // <nowar>：非战职玩家数量
        // <foreign>：放浪神数量
        // <union>：合并统计
        public static string ResultString(string inputStr)
        {
            foreach(var item in CountsDict)
            {
                inputStr = inputStr.Replace(item.Key, item.Value.ToString());
            }
            return inputStr;
        }

        public static string HelpMsg()
        {
            return "<all>：全部玩家数量\n<war>：战职玩家数量\n<nowar>：非战职玩家数量\n<foreign>：放浪神数量\n<inv>：不可见玩家数量\n<union>：合并统计\n<enemy>：敌对玩家\n<targetU>：以你为目标的玩家";
        }
    }
}
