using Dalamud.Game.Gui.Dtr;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using Dalamud.Game.Gui;
using System.IO;
using Dalamud.Interface.Colors;
using Dalamud.Utility;

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
        watcher = XCPlugin.watcher;
        Configuration = plugin.Configuration;
        this.plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        if (ImGui.BeginTabBar("XCountConfig"))
        {
            if (ImGui.BeginTabItem("常规"))
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

                var enableTempStat = Configuration.TempStat;
                if (ImGui.Checkbox("合并统计", ref enableTempStat))
                {
                    Configuration.TempStat = enableTempStat;
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

                var enableNameSrarch = Configuration.EnableNameSrarch;
                var enableAlert = Configuration.EnableAlert;
                if (ImGui.Checkbox("搜索id", ref enableNameSrarch))
                {
                    Configuration.EnableNameSrarch = enableNameSrarch;
                    Configuration.Save();
                }

                if (Configuration.EnableNameSrarch)
                {
                    var nameListStr = Configuration.NameListStr;
                    if (ImGui.InputText("查找名单", ref nameListStr, 200))
                    {
                        Configuration.NameListStr = nameListStr;
                        Configuration.Save();
                    }

                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip("输入玩家完整名称（不需要服务器），可输入多个，不支持模糊查找");
                    if (ImGui.Checkbox("开启警报", ref enableAlert))
                    {
                        Configuration.EnableAlert = enableAlert;
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
                    var unionStr = Configuration.UnionStr;
                    if (ImGui.InputText("合并统计字符串", ref unionStr, 200))
                    {
                        Configuration.UnionStr = unionStr;
                        Configuration.Save();
                    }

                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip("只有当合并统计被开启时，才会显示在状态栏中的字符串\n占位符规则和状态栏字符串一样");
                }

                var chatStr = Configuration.ChatStr;
                if (ImGui.InputText("发送命令", ref chatStr, 200))
                {
                    Configuration.ChatStr = chatStr;
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
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("绘制设置"))
            {
                var enablePainter = plugin.Configuration.EnablePainter;
                if (ImGui.Checkbox("启用绘制功能", ref enablePainter))
                {
                    plugin.Configuration.EnablePainter = enablePainter;
                    XCPlugin.Painter.Enable = enablePainter;
                    plugin.Configuration.Save();
                }

                var enableDrawInv = Configuration.EnableDrawInvis;
                if (ImGui.Checkbox("绘制不可见玩家", ref enableDrawInv))
                {
                    Configuration.EnableDrawInvis = enableDrawInv;
                    Configuration.Save();
                }
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip("当玩家在过场动画/切换区域加载时/其他情况下，\n其模型会不可见，勾选此项会把这样的玩家画出来");

                var enableDrawEnemy = Configuration.EnableDrawEnemies;
                if (ImGui.Checkbox("绘制敌人", ref enableDrawEnemy))
                {
                    Configuration.EnableDrawEnemies = enableDrawEnemy;
                    Configuration.Save();
                }

                var enableDrawTargetU = Configuration.EnableDrawTargetU;
                if (ImGui.Checkbox("绘制以你为目标的人", ref enableDrawTargetU))
                {
                    Configuration.EnableDrawTargetU = enableDrawTargetU;
                    Configuration.Save();
                }
                var drawRadius = Configuration.DrawRadius;
                if (ImGui.InputFloat("绘制半径", ref drawRadius))
                {
                    Configuration.DrawRadius = drawRadius;
                    Configuration.Save();
                }

                var drawWeight = Configuration.DrawWeight;
                if (ImGui.InputFloat("绘制厚度", ref drawWeight))
                {
                    Configuration.DrawWeight = drawWeight;
                    Configuration.Save();
                }
                
                // 颜色选择器
                var drawColor = Configuration.DrawColor;
                if (ImGui.ColorEdit4("绘制颜色", ref drawColor))
                {
                    Configuration.DrawColor = drawColor;
                    Configuration.Save();
                }


                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("人数警报"))
            {
                var enableCountAlert = Configuration.EnableCountAlert;
                if (ImGui.Checkbox("开启人数警报", ref enableCountAlert))
                {
                    Configuration.EnableCountAlert = enableCountAlert;
                    Configuration.Save();
                }

                var alertCount = Configuration.AlertCount;
                if (ImGui.InputInt("人数警报阈值", ref alertCount))
                {
                    Configuration.AlertCount = alertCount;
                    Configuration.Save();
                }

                var countAlertRepeat = Configuration.CountAlertRepeat;
                if (ImGui.InputInt("人数警报重复间隔（秒）", ref countAlertRepeat))
                {
                    Configuration.CountAlertRepeat = countAlertRepeat;
                    Configuration.Save();
                }

                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip("当执行人数警报之后，会自动关闭人数警报，并在这\n一栏设置的秒数之后自动重新开启，设置0则手动开启。");
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("GM警报"))
            {
                var enableGmDraw = Configuration.EnableGMDraw;
                if (ImGui.Checkbox("开启GM绘制", ref enableGmDraw))
                {
                    Configuration.EnableGMDraw = enableGmDraw;
                    Configuration.Save();
                }

                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip("此设置独立于下面一条设置");
                var enableGmAlert = Configuration.EnableGMAlert;
                if (ImGui.Checkbox("开启GM警报", ref enableGmAlert))
                {
                    Configuration.EnableGMAlert = enableGmAlert;
                    Configuration.Save();
                }

                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip("通过OnlineStatus判断GM");
                var gmAlertStr = Configuration.GmAlertStr;
                if (ImGui.InputText("GM警报内容", ref gmAlertStr, 500))
                {
                    Configuration.GmAlertStr = gmAlertStr;
                    Configuration.Save();
                }

                var gmAlertRepeat = Configuration.GmAlertRepeat;
                if (ImGui.InputInt("GM警报重复间隔（秒）", ref gmAlertRepeat))
                {
                    Configuration.GmAlertRepeat = gmAlertRepeat;
                    Configuration.Save();
                }

                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip("当执行GM警报之后，会自动关闭GM警报，并在这\n一栏设置的秒数之后自动重新开启，设置0则手动开启。");
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("在线列表绘制"))
            {
                bool enableOnlineList = Configuration.EnableOnlineList;
                if (ImGui.Checkbox("启用在线表格相关功能", ref enableOnlineList))
                {
                    Configuration.EnableOnlineList = enableOnlineList;
                    Configuration.Save();
                }

                string excelPath = Configuration.ExcelPath;
                if (ImGui.InputText("表格路径", ref excelPath, 2000))
                {
                    Configuration.ExcelPath = excelPath;
                    Configuration.Save();
                }

                string sheetName = Configuration.SheetName;
                if (ImGui.InputText("工作表名", ref sheetName, 200))
                {
                    Configuration.SheetName = sheetName;
                    Configuration.Save();
                }

                int nameCol = Configuration.NameCol;
                // 输入姓名所在列
                if (ImGui.InputInt("姓名列", ref nameCol))
                {
                    Configuration.NameCol = nameCol;
                    Configuration.Save();
                }

                int serverCol = Configuration.ServerCol;
                if (ImGui.InputInt("服务器列", ref serverCol))
                {
                    Configuration.ServerCol = serverCol;
                    Configuration.Save();
                }

                if (ImGui.Button("读取文件内容"))
                {
                    ExcelProcess.ReadPlayerInfoFromExcel(Configuration.ExcelPath, Configuration.SheetName,
                                                         Configuration.NameCol, Configuration.ServerCol);
                }

                bool drawExcel=Configuration.DrawExcel;
                if (ImGui.Checkbox("绘制列表中玩家", ref drawExcel))
                {
                    Configuration.DrawExcel = drawExcel;
                    Configuration.Save();
                }

                if (ImGui.CollapsingHeader("列表预览"))
                {
                    foreach (var player in ExcelProcess.ExcelList)
                    {
                        ImGui.Text($"{player.name}@{player.server}");
                    }
                }

                if (ImGui.CollapsingHeader("腾讯文档下载"))
                {
                    ImGui.TextColored(TXDocDownloader.IsFileExists ? ImGuiColors.HealerGreen : ImGuiColors.DPSRed,
                                      $"当前文件状态：{TXDocDownloader.FileName}");
                    ImGui.SameLine();
                    if (ImGui.Button("刷新文件状态"))
                    {
                        TXDocDownloader.FileName =
                            StaticUtil.IsFileExists(XCPlugin.PluginPath, "TXList", "xlsx");
                        Configuration.ExcelPath=TXDocDownloader.FileName;
                        Configuration.Save();
                    }

                    string txDocUrl = Configuration.TXDocUrl;
                    if (ImGui.InputText("腾讯文档链接", ref txDocUrl, 200))
                    {
                        Configuration.TXDocUrl = txDocUrl;
                        Configuration.Save();
                    }

                    string txLocalPadId = Configuration.TXLocalPadId;
                    if (ImGui.InputText("LocalPayId", ref txLocalPadId, 200))
                    {
                        Configuration.TXLocalPadId = txLocalPadId;
                        Configuration.Save();
                    }

                    string txCookie = Configuration.TXCookie;
                    if (ImGui.InputText("Cookies", ref txCookie, 3000))
                    {
                        Configuration.TXCookie = txCookie;
                        Configuration.Save();
                    }
                    bool autoUpdate=Configuration.AutoUpdateTXDoc;
                    if (ImGui.Checkbox("启动时自动更新", ref autoUpdate))
                    {
                        Configuration.AutoUpdateTXDoc = autoUpdate;
                        Configuration.Save();
                    }

                    if (ImGui.Button("填写指南"))
                    {
                        Util.OpenLink("https://blog.csdn.net/qq_45731111/article/details/124994682");
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("下载最新列表"))
                    {
                        StaticUtil.IsFileExists(XCPlugin.PluginPath, "TXList", "xlsx", true);
                        StaticUtil.DownLoadTXDoc(Configuration);
                    }
                }
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }

        ImGui.End();
    }
}
