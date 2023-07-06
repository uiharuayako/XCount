using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCount
{

    

    public struct SimplePlayer
    {
        public string name;
        public string server;

        public SimplePlayer(string name, string server)
        {
            this.name = name;
            this.server = server;
        }
        public bool IsReal()
        {
            return ExcelProcess.Servers.Contains(server);
        }

        public bool Equals(SimplePlayer other)
        {
            return name == other.name && server == other.server;
        }

        public override int GetHashCode()
        {
            return name.GetHashCode() ^ server.GetHashCode();
        }

    }
    public class ExcelProcess
    {
        public static readonly List<string> Servers = new List<string>()
        {
            "水晶塔",
            "银泪湖",
            "太阳海岸",
            "伊修加德",
            "红茶川",
            "紫水栈桥",
            "延夏",
            "静语庄园",
            "摩杜纳",
            "海猫茶屋",
            "柔风海湾",
            "琥珀原",
            "潮风亭",
            "神拳痕",
            "白银乡",
            "白金幻象",
            "旅人栈桥",
            "拂晓之间",
            "龙巢神殿",
            "梦羽宝境",
            "拉诺西亚",
            "幻影群岛",
            "神意之地",
            "萌芽池",
            "红玉海",
            "宇宙和音",
            "沃仙曦染",
            "晨曦王座"
        };

        public static HashSet<SimplePlayer> ExcelList = new HashSet<SimplePlayer>();

        public static void ReadPlayerInfoFromExcel(
            string filePath, string sheetName, int nameColumnIndex, int serverColumnIndex)
        {
            try {
                ExcelList.Clear();
                // 打开Excel文件
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    // 获取指定的工作表
                    var worksheet = package.Workbook.Worksheets[sheetName];

                    // 获取行数和列数
                    int rowCount = worksheet.Dimension.Rows;
                    int columnCount = worksheet.Dimension.Columns;

                    // 从指定列读取数据
                    for (int row = 2; row <= rowCount; row++) // 假设数据从第二行开始，第一行为标题行
                    {
                        // 读取姓名和服务器列的值
                        string name = worksheet.Cells[row, nameColumnIndex].Value?.ToString();
                        string server = worksheet.Cells[row, serverColumnIndex].Value?.ToString();

                        // 创建SimplePlayer实例并添加到HashSet中
                        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(server) && Servers.Contains(server))
                        {
                            SimplePlayer player = new SimplePlayer(name, server);
                            ExcelList.Add(player);
                        }
                    }
                }
            }catch(Exception ex) { }
        }
    }
}
