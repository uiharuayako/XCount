using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Numerics;

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
        public bool EnableNameSrarch { get; set; } = false;

        // 是否画出不可见玩家
        public bool EnableDrawInvis { get; set; } = false;
        // 是否画出PVP中的敌人
        public bool EnableDrawEnemies { get; set; } = false;
        // 是否画出以你为目标的玩家
        public bool EnableDrawTargetU { get; set; } = false;

        // 开启警报（搜索id警报）
        public bool EnableAlert { get; set; } = false;

        public string NameListStr { get; set; } = "";

        // 开启警报（人数警报）
        public bool EnableCountAlert { get; set; } = false;
        public int CountAlertRepeat { get; set; } = 0;

        public int AlertCount { get; set; } = 200;

        // GM警报
        public bool EnableGMAlert { get; set; } = true;

        public bool EnableGMDraw { get; set; } = true;

        // 警报内容
        public string GmAlertStr { get; set; } = "/e 兄弟们有TMD GM在附近<se.1>!!!";

        public int GmAlertRepeat { get; set; } = 0;

        // 发送聊天
        public string ChatStr { get; set; } = "/e 附近总人数<all>，非战职人数<nowar>";

        // 临时统计（合并统计）
        public bool TempStat { get; set; } = false;

        public string UnionStr { get; set; } = " 合并统计:<union>";

        // 开启按距离排序
        public bool EnableDistanceSort { get; set; } = true;

        public bool EnableOnlineList=false;
        // 腾讯文档参数
        public string TXDocUrl { get; set; } = "https://docs.qq.com/sheet/xxx";
        public string TXLocalPadId = "300000000$xxxxx";
        public string TXCookie = "你的cookie";
        public bool DrawExcel=false;
        public string SheetName = "表格名";
        public string ExcelPath = "";
        public int NameCol = 1;
        public int ServerCol = 2;
        public bool AutoUpdateTXDoc = false;

        // 绘制功能
        public bool EnablePainter { get; set; } = false;
        public float DrawRadius { get; set; } = 0.4f;
        public float DrawWeight { get; set; } = 0.5f;
        public Vector4 DrawColor { get; set; } = new Vector4(1, 0, 0, 1);

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
