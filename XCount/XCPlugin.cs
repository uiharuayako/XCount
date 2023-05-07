using System;
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
using System.Numerics;
using Dalamud.Game.Text;
using XCount.Windows;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using ImGuiNET;
using Dalamud.Interface.Colors;

namespace XCount
{
    public sealed class XCPlugin : IDalamudPlugin
    {
        public string Name => "XCount";

        // 命令列表
        private const string CommandName = "/xc";
        private const string XConfigCMD = "/xcset";
        private const string XCList = "/xclist";
        private const string CountPlayerCMD = "/xcpc";
        private const string CountNoWarCMD = "/xcnwpc";
        private const string SendChat = "/xcchat";

        private const string ClrTemp = "/xcclear";

        // 监听器
        public PCWatcher watcher;

        // UI注册
        private DalamudPluginInterface PluginInterface { get; init; }
        private CommandManager CommandManager { get; init; }
        public Configuration Configuration { get; init; }
        public WindowSystem WindowSystem = new("XCount");

        private ConfigWindow ConfigWindow { get; init; }
        public MainWindow MainWindow { get; init; }
        public PlayerListWindow PlayerListWindow { get; init; }

        public DtrBarEntry dtrEntry { get; init; }

        // Service
        [PluginService]
        [RequiredVersion("1.0")]
        public static Framework Framework { get; private set; } = null!;

        [PluginService]
        [RequiredVersion("1.0")]
        public static ObjectTable ObjectTable { get; private set; } = null!;

        [PluginService]
        [RequiredVersion("1.0")]
        public static ChatGui ChatGui { get; private set; } = null!; 
        [PluginService]
        [RequiredVersion("1.0")]
        public static GameGui GameGui { get; private set; } = null!;

        [PluginService]
        [RequiredVersion("1.0")]
        public static DtrBar DtrBar { get; private set; } = null!;

        [PluginService]
        [RequiredVersion("1.0")]
        public static ClientState ClientState { get; private set; } = null!;

        public Chat chat { get; private set; } = null!;

        

        // 插件初始化
        public XCPlugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager)
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);
            ECommons.ECommonsMain.Init(pluginInterface, this);
            chat = new Chat();
            // you might normally want to embed resources and load them from the manifest stream
            var imagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "XC.png");
            var image = this.PluginInterface.UiBuilder.LoadImage(imagePath);
            watcher = new PCWatcher(this);
            watcher.Enable();
            ConfigWindow = new ConfigWindow(this);
            MainWindow = new MainWindow(this, image);
            PlayerListWindow = new PlayerListWindow(this);
            WindowSystem.AddWindow(ConfigWindow);
            WindowSystem.AddWindow(MainWindow);
            WindowSystem.AddWindow(PlayerListWindow);
            // 初始化警报器
            if (Configuration.enableAlert)
            {
                StaticUtil.EnableAlertChat = true;
            }

            // 初始化dtr
            dtrEntry = DtrBar.Get(Name);
            dtrEntry.Shown = false;
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
            this.PluginInterface.UiBuilder.Draw += DrawTest;
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
            dtrEntry.Dispose();
            this.CommandManager.RemoveHandler(CommandName);
            this.CommandManager.RemoveHandler(XCList);
            this.CommandManager.RemoveHandler(XConfigCMD);
            this.CommandManager.RemoveHandler(CountPlayerCMD);
            this.CommandManager.RemoveHandler(CountNoWarCMD);
            this.CommandManager.RemoveHandler(SendChat);
            CommandManager.RemoveHandler(ClrTemp);
            this.PluginInterface.UiBuilder.Draw -= DrawTest;
            ECommons.ECommonsMain.Dispose();
        }

        public void sendChatMsg()
        {
            chat.SendMessage(CountResults.ResultString(Configuration.chatStr));
        }

        private void OnCommand(string command, string args)
        {
            // 处理聊天栏命令
            Dalamud.Logging.PluginLog.Log($"cmd:{command}||args:{args}");
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
                ChatGui.Print($"周围玩家数量：{CountResults.CountAll}");
            }
            else if (command == CountNoWarCMD)
            {
                ChatGui.Print($"周围非战职玩家数量：{CountResults.CountNoWar}");
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
        // 从pp那偷的代码。
        // ff人的事，怎么能叫偷呢！
        private void DrawRingWorld(GameObject actor, float radius, int numSegments, float thicc, uint colour)
        {
            var seg = numSegments / 2;
            for (var i = 0; i <= numSegments; i++)
            {
                GameGui.WorldToScreen(new Vector3(
                                          actor.Position.X  + (radius * (float)Math.Sin((Math.PI / seg) * i)),
                                          actor.Position.Y,
                                          actor.Position.Z  + (radius * (float)Math.Cos((Math.PI / seg) * i))
                                      ),
                                      out Vector2 pos);
                ImGui.GetWindowDrawList().PathLineTo(new Vector2(pos.X, pos.Y));
            }
            ImGui.GetWindowDrawList().PathStroke(colour, ImDrawFlags.None, thicc);
        }

        private void DrawTest()
        {
            if (CountResults.DrawAdvCharacters.Count == 0||!Configuration.enableAdventurerDraw)
            {
                return;
            }
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(new Vector2(0, 0));
            ImGui.Begin("Canvas",
                        ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoTitleBar |
                        ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoBackground);
            ImGui.SetWindowSize(ImGui.GetIO().DisplaySize);
            foreach (PlayerCharacter advPlayer in CountResults.DrawAdvCharacters)
            {
                DrawRingWorld(advPlayer, 0.8f, 100, 10f, ImGui.GetColorU32(ImGuiColors.DPSRed));
            }
            ImGui.End();
            ImGui.PopStyleVar();
        }
    }
}
