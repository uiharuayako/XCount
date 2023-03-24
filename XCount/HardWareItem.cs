using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCount
{
    // 用于存放需要监视的硬件信息的类，包含2个String对象：
    // name：xml里面node名称
    // infoStr：内容字符串
    public class HardWareItem
    {
        public string name;
        public string infoStr;
        public HardWareItem(string name, string infoStr)
        {
            this.name = name;
            this.infoStr = infoStr;
        }
        // 替换字符串中<info>为传入的str
        public string ReplaceInfo(string str)
        {
            return infoStr.Replace("<info>", str);
        }
    }
}
