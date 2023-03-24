using Dalamud.Game.Gui.Dtr;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Linq;
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
        Size = new Vector2(320, ImGui.GetTextLineHeightWithSpacing() + ImGui.GetTextLineHeight() * 13);
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

        var enableTempStat = Configuration.tempStat;
        if (ImGui.Checkbox("合并统计", ref enableTempStat))
        {
            Configuration.tempStat = enableTempStat;

            Configuration.Save();
        }

        if (ImGui.Button("清除累计计数"))
        {
            watcher.clearTemp();
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

        var chatStr = Configuration.chatStr;
        if (ImGui.InputText("发送命令", ref chatStr, 200))
        {
            Configuration.chatStr = chatStr;
            Configuration.Save();
        }

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(CountResults.HelpMsg());
        var isHardWatch = Configuration.enableHwStat;
        if (ImGui.Checkbox("开启硬件监控", ref isHardWatch))
        {
            Configuration.enableHwStat = isHardWatch;
            // 如果 开启硬件监控 选项关闭，而且监控器已开启，则卸载
            if (!isHardWatch)
            {
                plugin.hwWatcher.Dispose();
            }

            // 如果 开启硬件监控 选项开启，而且监听器已关闭，则加载
            if (isHardWatch)
            {
                plugin.hwWatcher.Enable();
            }

            Configuration.Save();
        }

        // 如果开启硬件监控，则加载硬件监控的相关设置
        var mUrl = Configuration.msiUrl;
        var mUser = Configuration.msiUser;
        var mPass = Configuration.msiPass;
        var mInterval = Configuration.msiInterval;
        if (ImGui.CollapsingHeader("硬件监控菜单"))
        {
            if (ImGui.InputText("url", ref mUrl, 200))
            {
                Configuration.msiUrl = mUrl;
                Configuration.Save();
            }

            if (ImGui.InputText("用户名", ref mUser, 200))
            {
                Configuration.msiUser = mUser;
                Configuration.Save();
            }

            if (ImGui.InputText("密码", ref mPass, 200))
            {
                Configuration.msiPass = mPass;
                Configuration.Save();
            }

            if (ImGui.InputInt("获取频率(ms)", ref mInterval, 500))
            {
                Configuration.msiInterval = mInterval;
                Configuration.Save();
            }

            if (ImGui.CollapsingHeader("硬件监控项目"))
            {
                if (ImGui.Button("新建硬件监控项目"))
                {
                    Configuration.hardWareItems.Add(new HardWareItem("", "<info>"));
                    Configuration.Save();
                }

                if (Configuration.hardWareItems.Any())
                {
                    for (int i = 0; i < Configuration.hardWareItems.Count(); i++)
                    {
                        if (ImGui.CollapsingHeader($"硬件项目{i + 1}"))
                        {
                            var itemName = Configuration.hardWareItems[i].name;
                            if (ImGui.InputText($"项目名称{i + 1}", ref itemName, 200))
                            {
                                Configuration.hardWareItems[i].name = itemName;
                                Configuration.Save();
                            }
                            if (ImGui.IsItemHovered())
                                ImGui.SetTooltip("输入节点srcName的信息，详细设置见Github上的Readme");

                            var itemInfoStr = Configuration.hardWareItems[i].infoStr;
                            if (ImGui.InputText($"信息字符串{i + 1}", ref itemInfoStr, 200))
                            {
                                Configuration.hardWareItems[i].infoStr = itemInfoStr;
                                Configuration.Save();
                            }
                            if (ImGui.IsItemHovered())
                                ImGui.SetTooltip("使用<info>指代数据，试试输入：RAM:<info> MB");
                            if (ImGui.Button($"删除##Remove{i}"))
                            {
                                Configuration.hardWareItems.RemoveAt(i);
                                Configuration.Save();
                            }

                            ImGui.SameLine();
                            // 往上移，不能是第一个元素
                            if (ImGui.Button($"↑##ItemUp{i}") && i != 0)
                            {
                                HardWareItem temp = Configuration.hardWareItems[i];
                                Configuration.hardWareItems[i] = Configuration.hardWareItems[i - 1];
                                Configuration.hardWareItems[i - 1] = temp;
                                Configuration.Save();
                            }

                            ImGui.SameLine();
                            // 往下移，不能是最后一个元素
                            if (ImGui.Button($"↓##ItemDown{i}") && i != Configuration.hardWareItems.Count() - 1)
                            {
                                HardWareItem temp = Configuration.hardWareItems[i];
                                Configuration.hardWareItems[i] = Configuration.hardWareItems[i + 1];
                                Configuration.hardWareItems[i + 1] = temp;
                                Configuration.Save();
                            }
                        }
                    }
                }
            }
        }
    }
}
