using System;
using System.Collections.Generic;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using ECommons.Automation;
using ECommons.DalamudServices;
using System.IO;
using System.Linq;
using System.Numerics;
using Dalamud.Game.Text;
using XCount.Windows;
using System.Threading.Tasks;
using System.Timers;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using ImGuiNET;
using Dalamud.Interface.Colors;
using Dalamud.Plugin.Services;
using ECommons;
using ECommons.GameFunctions;
using Microsoft.Extensions.Configuration;
using static Lumina.Data.Parsing.Layer.LayerCommon;

namespace XCount
{
    public sealed class XCPlugin : IDalamudPlugin
    {
        public string Name => "XCount";

        // 插件路径
        public static string PluginPath = "";

        // 命令列表
        private const string CommandName = "/xc";
        private const string XConfigCMD = "/xcset";
        private const string XCList = "/xclist";
        private const string CountPlayerCMD = "/xcpc";
        private const string CountNoWarCMD = "/xcnwpc";
        private const string SendChat = "/xcchat";

        private const string ClrTemp = "/xcclear";

        // 监听器
        public static PCWatcher watcher;

        // UI注册
        private IDalamudPluginInterface PluginInterface { get; init; }
        private ICommandManager CommandManager { get; init; }
        public Configuration Configuration { get; init; }
        public WindowSystem WindowSystem = new("XCount");

        private ConfigWindow ConfigWindow { get; init; }
        public MainWindow MainWindow { get; init; }
        public PlayerListWindow PlayerListWindow { get; init; }

        public IDtrBarEntry dtrEntry { get; init; }

        [PluginService]
        public static IDtrBar DtrBar { get; private set; } = null!;
        public Chat chat { get; private set; } = null!;
        
        // 插件初始化
        public XCPlugin(
             IDalamudPluginInterface pluginInterface,
             ICommandManager commandManager)
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;
            
            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);
            ECommons.ECommonsMain.Init(pluginInterface, this, Module.All);
            chat = new Chat();
            // you might normally want to embed resources and load them from the manifest stream
            var imagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "XC.png");
            PluginPath = PluginInterface.AssemblyLocation.DirectoryName;
            watcher = new PCWatcher(this);
            watcher.Enable();

            ConfigWindow = new ConfigWindow(this);
            MainWindow = new MainWindow(this, imagePath);
            PlayerListWindow = new PlayerListWindow(this);
            WindowSystem.AddWindow(ConfigWindow);
            WindowSystem.AddWindow(MainWindow);
            WindowSystem.AddWindow(PlayerListWindow);
            // 初始化警报器
            if (Configuration.EnableAlert)
            {
                StaticUtil.EnableAlertChat = true;
            }

            // 初始化dtr
            dtrEntry = Svc.DtrBar.Get(Name);
            dtrEntry.Shown = false;
            dtrEntry.OnClick += MainWindow.Toggle;
            if (Configuration.ShowInDtr)
            {
                loadDtr();
            }

            this.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "显示XCount Gui"
            });
            this.CommandManager.AddHandler(XCList, new CommandInfo(OnCommand)
            {
                HelpMessage = "显示玩家列表"
            });
            this.CommandManager.AddHandler(XConfigCMD, new CommandInfo(OnCommand)
            {
                HelpMessage = "打开XConfig配置"
            });
            this.CommandManager.AddHandler(CountPlayerCMD, new CommandInfo(OnCommand)
            {
                HelpMessage = "返回周围玩家总数量到聊天栏"
            });
            this.CommandManager.AddHandler(CountNoWarCMD, new CommandInfo(OnCommand)
            {
                HelpMessage = "返回周围不是战职的玩家总数量到聊天栏"
            });
            this.CommandManager.AddHandler(SendChat, new CommandInfo(OnCommand)
            {
                HelpMessage = "发送消息到聊天（在设置菜单自定义消息）"
            });
            this.CommandManager.AddHandler(ClrTemp, new CommandInfo(OnCommand)
            {
                HelpMessage = "临时统计人数归零归零归归零"
            });

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
            if (Configuration.EnableOnlineList)
            {
                StaticUtil.IsFileExists(PluginPath, "TXList", "xlsx");
                ExcelProcess.ReadPlayerInfoFromExcel(Configuration.ExcelPath, Configuration.SheetName,
                                                     Configuration.NameCol, Configuration.ServerCol);
                if (Configuration.AutoUpdateTXDoc)
                {
                    StaticUtil.DownLoadTXDoc(Configuration);
                }
            }
        }

        public void loadDtr()
        {
            dtrEntry.Shown = true;
            dtrEntry.Text = "XCount";
        }

        public void disposeDtr()
        {
            dtrEntry.Shown = false;
        }

        public void Dispose()
        {
            this.WindowSystem.RemoveAllWindows();
            ConfigWindow.Dispose();
            MainWindow.Dispose();
            PlayerListWindow.Dispose();
            watcher.Dispose();
            dtrEntry.Remove();
            this.CommandManager.RemoveHandler(CommandName);
            this.CommandManager.RemoveHandler(XCList);
            this.CommandManager.RemoveHandler(XConfigCMD);
            this.CommandManager.RemoveHandler(CountPlayerCMD);
            this.CommandManager.RemoveHandler(CountNoWarCMD);
            this.CommandManager.RemoveHandler(SendChat);
            CommandManager.RemoveHandler(ClrTemp);
            ECommons.ECommonsMain.Dispose();
        }

        public void sendChatMsg()
        {
            chat.SendMessage(CountResults.ResultString(Configuration.ChatStr));
        }

        private void OnCommand(string command, string args)
        {
            // 处理聊天栏命令
           Svc.Log.Info($"cmd:{command}||args:{args}");
            if (command == CommandName)
            {
                MainWindow.Toggle();
            }

            if (command == XCList)
            {
                PlayerListWindow.Toggle();
            }
            else if (command == XConfigCMD)
            {
                ConfigWindow.Toggle();
            }
            else if (command == CountPlayerCMD)
            {
                Svc.Chat.Print($"周围玩家数量：{CountResults.CountsDict["<all>"]}");
            }
            else if (command == CountNoWarCMD)
            {
                Svc.Chat.Print($"周围非战职玩家数量：{CountResults.CountsDict["<nowar>"]}");
            }
            else if (command == SendChat)
            {
                sendChatMsg();
            }
            else if (command == ClrTemp)
            {
                watcher.clearTemp();
            }
        }

        private void DrawUI()
        {
            this.WindowSystem.Draw();
        }

        public void DrawConfigUI()
        {
            ConfigWindow.Toggle();
        }
    }
}
