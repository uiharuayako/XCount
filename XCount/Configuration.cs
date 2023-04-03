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
        // 开启警报
        public bool enableAlert { get; set; } = false;
        public string nameListStr { get; set; }="";
        // 发送聊天
        public string chatStr { get; set; } = "/e 附近总人数<all>，非战职人数<nowar>";
        // 临时统计（合并统计）
        public bool tempStat { get; set; } = false;
        public string unionStr { get; set; } = " 合并统计:<union>";
        // 开启按距离排序
        public bool enableDistanceSort { get; set; } = true;
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
