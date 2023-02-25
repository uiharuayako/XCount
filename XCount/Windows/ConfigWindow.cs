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
        ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoScrollWithMouse)
    {
        Size = new Vector2(320, ImGui.GetTextLineHeightWithSpacing() + ImGui.GetTextLineHeight() * 10);
        SizeCondition = ImGuiCond.Always;
        watcher = plugin.watcher;
        Configuration = plugin.Configuration;
        this.plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
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
        var enableNameSrarch = Configuration.enableNameSrarch;
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
                Configuration.nameList.Clear();
                Configuration.nameList.AddRange(nameListStr.Split('|'));
                Configuration.Save();
            }
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("输入玩家id，用|分隔");
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
        }
    }
}
