using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;

namespace XCount
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public bool EnablePlugin { get; set; } = true;
        public bool ShowInDtr { get; set; } = false;
        public string dtrStr { get; set; } = "人数:<all>";
        // 名单检测
        public bool enableNameSrarch { get; set; } = false;
        public string nameListStr { get; set; }="";
        public List<string> nameList { get; set; } = new List<string>();
        // 发送聊天
        public string chatStr { get; set; } = "/e 附近总人数<all>，非战职人数<nowar>";
        // 临时统计（合并统计）
        public bool tempStat { get; set; } = false;
        // 硬件监控
        public bool enableHwStat { get; set; } = false;

        // MSI Remote url，账号，密码
        public string msiUrl { get; set; } = "http://127.0.0.1:82/mahm";
        public string msiUser { get; set; } = "MSIAfterburner";
        public string msiPass { get; set; } = "17cc95b4017d496f82";
        // 设定获取硬件信息的频率，单位：ms
        public int msiInterval { get; set; } = 1000;
        // 获取硬件信息的项
        public List<HardWareItem> hardWareItems { get; set; } = new List<HardWareItem>();
        // the below exist just to make saving less cumbersome
        [NonSerialized]
        private DalamudPluginInterface? PluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.PluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.PluginInterface!.SavePluginConfig(this);
        }
    }
}
