using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Interface.Colors;

namespace XCount.Windows
{
    public class PlayerListWindow : Window, IDisposable
    {
        private Configuration Configuration;
        private PCWatcher watcher;
        private XCPlugin plugin;
        private List<PlayerCharacter> searchNameList;
        private List<PlayerCharacter> searchFCList;
        private string searchName;
        private string searchFC;
        public PlayerListWindow(XCPlugin plugin) : base(
            "XCount玩家列表")
        {
            Configuration = plugin.Configuration;
            this.plugin = plugin;
            watcher = plugin.watcher;
            searchName = "";
            searchFC = "";
            searchNameList = new List<PlayerCharacter>();
            searchFCList = new List<PlayerCharacter>();
        }

        public void Dispose() { }

        public override void Draw()
        {
            try
            {
                bool enableSort = plugin.Configuration.enableDistanceSort;
                if (ImGui.Checkbox("按距离排序", ref enableSort))
                {
                    plugin.Configuration.enableDistanceSort = enableSort;
                    plugin.Configuration.Save();
                }

                ImGui.SameLine();
                string serverName = "未知";
                if (XCPlugin.ClientState.LocalPlayer.CurrentWorld.GameData != null)
                {
                    serverName = XCPlugin.ClientState.LocalPlayer.CurrentWorld.GameData.Name;
                }
                ImGui.Text($"当前服务器：{serverName}");
                if (ImGui.InputText("搜索名称", ref searchName, 100))
                {
                }
                if (ImGui.InputText("搜索部队", ref searchFC, 100))
                {

                }

                if (ImGui.BeginChild("Player List", new Vector2(0f, -1f), true))
                {
                    if (ImGui.CollapsingHeader($"所有玩家（共{watcher.playerCharacters.Count()}）"))
                    {
                        ImGui.Columns(8, "All Players");
                        ImGui.Text("昵称");
                        ImGui.NextColumn();
                        ImGui.Text("部队");
                        ImGui.NextColumn();
                        ImGui.Text("原服务器");
                        ImGui.NextColumn();
                        ImGui.Text("等级");
                        ImGui.NextColumn();
                        ImGui.Text("职业");
                        ImGui.NextColumn();
                        ImGui.Text("目标");
                        ImGui.NextColumn();
                        ImGui.Text("距离");
                        ImGui.NextColumn();
                        ImGui.Text("ID");
                        ImGui.NextColumn();
                        foreach (var player in watcher.playerCharacters)
                        {
                            Vector4 color = ImGuiColors.DalamudGrey;
                            switch (player.ClassJob.GameData.Role)
                            {
                                case 0:
                                    color = ImGuiColors.ParsedPurple;
                                    break;
                                case 1:
                                    color = ImGuiColors.TankBlue;
                                    break;
                                case 2:
                                case 3:
                                    color = ImGuiColors.DPSRed;
                                    break;
                                case 4:
                                    color = ImGuiColors.HealerGreen;
                                    break;
                            }
                            ImGui.TextColored(color, player.Name.TextValue);
                            ImGui.NextColumn();
                            string fcTag = "未知部队";
                            if (!player.CompanyTag.TextValue.Equals(""))
                            {
                                fcTag = player.CompanyTag.TextValue;
                            }

                            ImGui.TextColored(fcTag.Equals("未知部队") ? ImGuiColors.DalamudGrey : ImGuiColors.ParsedGold, fcTag);
                            ImGui.NextColumn();
                            ImGui.Text(player.HomeWorld.GameData.Name);
                            ImGui.NextColumn();
                            ImGui.Text(player.Level.ToString());
                            ImGui.NextColumn();
                            ImGui.TextColored(color, player.ClassJob.GameData.Name);
                            ImGui.NextColumn();
                            string targetName = "无";
                            if (player.TargetObject != null)
                            {
                                targetName = player.TargetObject.Name.TextValue;
                            }

                            ImGui.Text(targetName);
                            ImGui.NextColumn();
                            ImGui.Text(StaticUtil.DistanceToPlayer(player).ToString());
                            ImGui.NextColumn();
                            ImGui.Text(player.ObjectId.ToString());
                            ImGui.NextColumn();
                        }
                    }
                    if (ImGui.CollapsingHeader($"所有玩家（累计统计）（共{watcher.tempPlayers.Count}）"))
                    {
                        ImGui.Columns(8, "All Players");
                        ImGui.Text("昵称");
                        ImGui.NextColumn();
                        ImGui.Text("部队");
                        ImGui.NextColumn();
                        ImGui.Text("原服务器");
                        ImGui.NextColumn();
                        ImGui.Text("等级");
                        ImGui.NextColumn();
                        ImGui.Text("职业");
                        ImGui.NextColumn();
                        ImGui.Text("目标");
                        ImGui.NextColumn();
                        ImGui.Text("距离");
                        ImGui.NextColumn();
                        ImGui.Text("ID");
                        ImGui.NextColumn();
                        foreach (var player in watcher.tempPlayers)
                        {
                            Vector4 color = ImGuiColors.DalamudGrey;
                            switch (player.ClassJob.GameData.Role)
                            {
                                case 0:
                                    color = ImGuiColors.ParsedPurple;
                                    break;
                                case 1:
                                    color = ImGuiColors.TankBlue;
                                    break;
                                case 2:
                                case 3:
                                    color = ImGuiColors.DPSRed;
                                    break;
                                case 4:
                                    color = ImGuiColors.HealerGreen;
                                    break;
                            }
                            ImGui.TextColored(color, player.Name.TextValue);
                            ImGui.NextColumn();
                            string fcTag = "未知部队";
                            if (!player.CompanyTag.TextValue.Equals(""))
                            {
                                fcTag = player.CompanyTag.TextValue;
                            }

                            ImGui.TextColored(fcTag.Equals("未知部队") ? ImGuiColors.DalamudGrey : ImGuiColors.ParsedGold, fcTag);
                            ImGui.NextColumn();
                            ImGui.Text(player.HomeWorld.GameData.Name);
                            ImGui.NextColumn();
                            ImGui.Text(player.Level.ToString());
                            ImGui.NextColumn();
                            ImGui.TextColored(color, player.ClassJob.GameData.Name);
                            ImGui.NextColumn();
                            string targetName = "无";
                            if (player.TargetObject != null)
                            {
                                targetName = player.TargetObject.Name.TextValue;
                            }

                            ImGui.Text(targetName);
                            ImGui.NextColumn();
                            ImGui.Text(StaticUtil.DistanceToPlayer(player).ToString());
                            ImGui.NextColumn();
                            ImGui.Text(player.ObjectId.ToString());
                            ImGui.NextColumn();
                        }
                    }
                    if (!searchName.Equals(""))
                    {
                        searchNameList = watcher.playerCharacters.Where(pc => pc.Name.TextValue.Contains(searchName)).ToList();
                        if (ImGui.CollapsingHeader($"名称搜索结果（共{searchNameList.Count}）"))
                        {
                            ImGui.Columns(8, "Name Players");
                            ImGui.Text("昵称");
                            ImGui.NextColumn();
                            ImGui.Text("部队");
                            ImGui.NextColumn();
                            ImGui.Text("原服务器");
                            ImGui.NextColumn();
                            ImGui.Text("等级");
                            ImGui.NextColumn();
                            ImGui.Text("职业");
                            ImGui.NextColumn();
                            ImGui.Text("目标");
                            ImGui.NextColumn();
                            ImGui.Text("距离");
                            ImGui.NextColumn();
                            ImGui.Text("ID");
                            ImGui.NextColumn();
                            foreach (var player in searchNameList)
                            {
                                Vector4 color = ImGuiColors.DalamudGrey;
                                switch (player.ClassJob.GameData.Role)
                                {
                                    case 0:
                                        color = ImGuiColors.ParsedPurple;
                                        break;
                                    case 1:
                                        color = ImGuiColors.TankBlue;
                                        break;
                                    case 2:
                                    case 3:
                                        color = ImGuiColors.DPSRed;
                                        break;
                                    case 4:
                                        color = ImGuiColors.HealerGreen;
                                        break;
                                }
                                ImGui.TextColored(color, player.Name.TextValue);
                                ImGui.NextColumn();
                                string fcTag = "未知部队";
                                if (!player.CompanyTag.TextValue.Equals(""))
                                {
                                    fcTag = player.CompanyTag.TextValue;
                                }

                                ImGui.TextColored(fcTag.Equals("未知部队") ? ImGuiColors.DalamudGrey : ImGuiColors.ParsedGold, fcTag);
                                ImGui.NextColumn();
                                ImGui.Text(player.HomeWorld.GameData.Name);
                                ImGui.NextColumn();
                                ImGui.Text(player.Level.ToString());
                                ImGui.NextColumn();
                                ImGui.TextColored(color, player.ClassJob.GameData.Name);
                                ImGui.NextColumn();
                                string targetName = "无";
                                if (player.TargetObject != null)
                                {
                                    targetName = player.TargetObject.Name.TextValue;
                                }

                                ImGui.Text(targetName);
                                ImGui.NextColumn();
                                ImGui.Text(StaticUtil.DistanceToPlayer(player).ToString());
                                ImGui.NextColumn();
                                ImGui.Text(player.ObjectId.ToString());
                                ImGui.NextColumn();
                            }
                        }

                    }
                    if (!searchFC.Equals(""))
                    {
                        searchFCList = watcher.playerCharacters.Where(pc => pc.CompanyTag.TextValue.Contains(searchFC)).ToList();
                        if (ImGui.CollapsingHeader($"部队搜索结果（共{searchFCList.Count}）"))
                        {
                            ImGui.Columns(8, "FC Players");
                            ImGui.Text("昵称");
                            ImGui.NextColumn();
                            ImGui.Text("部队");
                            ImGui.NextColumn();
                            ImGui.Text("原服务器");
                            ImGui.NextColumn();
                            ImGui.Text("等级");
                            ImGui.NextColumn();
                            ImGui.Text("职业");
                            ImGui.NextColumn();
                            ImGui.Text("目标");
                            ImGui.NextColumn();
                            ImGui.Text("距离");
                            ImGui.NextColumn();
                            ImGui.Text("ID");
                            ImGui.NextColumn();
                            foreach (var player in searchFCList)
                            {
                                Vector4 color = ImGuiColors.DalamudGrey;
                                switch (player.ClassJob.GameData.Role)
                                {
                                    case 0:
                                        color = ImGuiColors.ParsedPurple;
                                        break;
                                    case 1:
                                        color = ImGuiColors.TankBlue;
                                        break;
                                    case 2:
                                    case 3:
                                        color = ImGuiColors.DPSRed;
                                        break;
                                    case 4:
                                        color = ImGuiColors.HealerGreen;
                                        break;
                                }
                                ImGui.TextColored(color, player.Name.TextValue);
                                ImGui.NextColumn();
                                string fcTag = "未知部队";
                                if (!player.CompanyTag.TextValue.Equals(""))
                                {
                                    fcTag = player.CompanyTag.TextValue;
                                }

                                ImGui.TextColored(fcTag.Equals("未知部队") ? ImGuiColors.DalamudGrey : ImGuiColors.ParsedGold, fcTag);
                                ImGui.NextColumn();
                                ImGui.Text(player.HomeWorld.GameData.Name);
                                ImGui.NextColumn();
                                ImGui.Text(player.Level.ToString());
                                ImGui.NextColumn();
                                ImGui.TextColored(color, player.ClassJob.GameData.Name);
                                ImGui.NextColumn();
                                string targetName = "无";
                                if (player.TargetObject != null)
                                {
                                    targetName = player.TargetObject.Name.TextValue;
                                }

                                ImGui.Text(targetName);
                                ImGui.NextColumn();
                                ImGui.Text(StaticUtil.DistanceToPlayer(player).ToString());
                                ImGui.NextColumn();
                                ImGui.Text(player.ObjectId.ToString());
                                ImGui.NextColumn();
                            }
                        }

                    }
                    ImGui.EndChild();
                }
            }
            catch (Exception e)
            {
                Dalamud.Logging.PluginLog.Log($"切换区域异常{e.Message}");
            }
        }
    }
}