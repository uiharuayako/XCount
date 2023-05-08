using Dalamud.Game.Gui.Dtr;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Numerics;

namespace XCount.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;
    private PCWatcher watcher;
    private XCPlugin plugin;

    public ConfigWindow(XCPlugin plugin) : base(
        "XCount设置",
        ImGuiWindowFlags.NoCollapse)
    {
        Size = new Vector2(320, ImGui.GetTextLineHeightWithSpacing() + ImGui.GetTextLineHeight() * 18);
        SizeCondition = ImGuiCond.Once;
        watcher = plugin.watcher;
        Configuration = plugin.Configuration;
        this.plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        var isDisplay = plugin.MainWindow.IsOpen;
        if (ImGui.Checkbox("显示窗口", ref isDisplay))
        {
            plugin.MainWindow.IsOpen = isDisplay;
        }

        var isDisplayList = plugin.PlayerListWindow.IsOpen;
        if (ImGui.Checkbox("显示玩家列表", ref isDisplayList))
        {
            plugin.PlayerListWindow.IsOpen = isDisplayList;
        }

        var enableDrawInv=Configuration.enableDrawInv;
        if (ImGui.Checkbox("绘制不可见玩家", ref enableDrawInv))
        {
            Configuration.enableDrawInv = enableDrawInv;
            Configuration.Save();
        }
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("当玩家在过场动画/切换区域加载时/其他情况下，\n其模型会不可见，勾选此项会把这样的玩家画出来");

        var isEnable = Configuration.EnablePlugin;
        if (ImGui.Checkbox("开启计数", ref isEnable))
        {
            Configuration.EnablePlugin = isEnable;
            // 如果 开启计数 选项关闭，而且监听器已开启，则卸载
            if (!isEnable && CountResults.isUpdate)
            {
                watcher.Dispose();
            }

            // 如果 开启计数 选项开启，而且监听器已关闭，则加载
            if (isEnable && !CountResults.isUpdate)
            {
                watcher.Enable();
            }

            Configuration.Save();
        }

        if (ImGui.CollapsingHeader("人数警报"))
        {
            var enableCountAlert = Configuration.enableCountAlert;
            if (ImGui.Checkbox("开启人数警报", ref enableCountAlert))
            {
                Configuration.enableCountAlert = enableCountAlert;
                Configuration.Save();
            }
            var alertCount = Configuration.alertCount;
            if (ImGui.InputInt("人数警报阈值", ref alertCount))
            {
                Configuration.alertCount = alertCount;
                Configuration.Save();
            }
            var countAlertRepeat = Configuration.countAlertRepeat;
            if (ImGui.InputInt("人数警报重复间隔（秒）", ref countAlertRepeat))
            {
                Configuration.countAlertRepeat = countAlertRepeat;
                Configuration.Save();
            }
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("当执行人数警报之后，会自动关闭人数警报，并在这\n一栏设置的秒数之后自动重新开启，设置0则手动开启。");
        }
        if (ImGui.CollapsingHeader("冒险者（GM）警报"))
        {
            var enableAdventurerDraw = Configuration.enableAdventurerDraw;
            if (ImGui.Checkbox("开启冒险者绘制", ref enableAdventurerDraw))
            {
                Configuration.enableAdventurerDraw = enableAdventurerDraw;
                Configuration.Save();
            }
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("此设置独立于下面一条设置");
            var enableAdventurerAlert = Configuration.enableAdventurerAlert;
            if (ImGui.Checkbox("开启冒险者警报", ref enableAdventurerAlert))
            {
                Configuration.enableAdventurerAlert = enableAdventurerAlert;
                Configuration.Save();
            }
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("当周围出现职业为“冒险者”的玩家时，会触发警报\n该玩家有一定概率为隐身的GM\n详见插件主页的介绍");
            var advAlertStr =Configuration.advAlertStr;
            if (ImGui.InputText("冒险者警报内容", ref advAlertStr, 500))
            {
                Configuration.advAlertStr = advAlertStr;
                Configuration.Save();
            }
            var advAlertRepeat = Configuration.advAlertRepeat;
            if (ImGui.InputInt("冒险者警报重复间隔（秒）", ref advAlertRepeat))
            {
                Configuration.advAlertRepeat = advAlertRepeat;
                Configuration.Save();
            }
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("当执行冒险者警报之后，会自动关闭冒险者警报，并在这\n一栏设置的秒数之后自动重新开启，设置0则手动开启。");
        }

        var enableTempStat = Configuration.tempStat;
        if (ImGui.Checkbox("合并统计", ref enableTempStat))
        {
            Configuration.tempStat = enableTempStat;
            // 如果合并统计关闭，则清空计数
            if (!enableTempStat)
            {
                watcher.clearTemp();
            }

            Configuration.Save();
        }

        if (ImGui.Button("清除累计计数"))
        {
            watcher.clearTemp();
        }

        var enableNameSrarch = Configuration.enableNameSrarch;
        var enableAlert = Configuration.enableAlert;
        if (ImGui.Checkbox("搜索id", ref enableNameSrarch))
        {
            Configuration.enableNameSrarch = enableNameSrarch;
            Configuration.Save();
        }

        if (Configuration.enableNameSrarch)
        {
            var nameListStr = Configuration.nameListStr;
            if (ImGui.InputText("查找名单", ref nameListStr, 200))
            {
                Configuration.nameListStr = nameListStr;
                Configuration.Save();
            }

            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("输入玩家完整名称（不需要服务器），可输入多个，不支持模糊查找");
            if (ImGui.Checkbox("开启警报", ref enableAlert))
            {
                Configuration.enableAlert = enableAlert;
                StaticUtil.EnableAlertChat = enableAlert;
                Configuration.Save();
            }
        }

        var showInDtr = Configuration.ShowInDtr;
        if (ImGui.Checkbox("状态栏显示", ref showInDtr))
        {
            Configuration.ShowInDtr = showInDtr;
            // 如果关闭，则卸载
            if (!showInDtr)
            {
                plugin.disposeDtr();
            }
            else
            {
                plugin.loadDtr();
            }

            Configuration.Save();
        }

        if (Configuration.ShowInDtr)
        {
            var dtrStr = Configuration.dtrStr;
            if (ImGui.InputText("状态栏字符串", ref dtrStr, 200))
            {
                Configuration.dtrStr = dtrStr;
                Configuration.Save();
            }

            if (ImGui.IsItemHovered())
                ImGui.SetTooltip(CountResults.HelpMsg());
            var unionStr = Configuration.unionStr;
            if (ImGui.InputText("合并统计字符串", ref unionStr, 200))
            {
                Configuration.unionStr = unionStr;
                Configuration.Save();
            }

            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("只有当合并统计被开启时，才会显示在状态栏中的字符串\n占位符规则和状态栏字符串一样");
        }

        var chatStr = Configuration.chatStr;
        if (ImGui.InputText("发送命令", ref chatStr, 200))
        {
            Configuration.chatStr = chatStr;
            Configuration.Save();
        }

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(CountResults.HelpMsg());
        if (ImGui.Button("立即发送"))
        {
            plugin.sendChatMsg();
        }

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("使用命令/xcchat也可以发送哦");
    }
}
