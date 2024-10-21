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
using ECommons.DalamudServices;
using ECommons.GameFunctions;

namespace XCount.Windows
{
    public class PlayerListWindow : Window, IDisposable
    {
        private Configuration Configuration;
        private PCWatcher watcher;
        private XCPlugin plugin;
        private List<IPlayerCharacter> searchNameList;
        private List<IPlayerCharacter> searchFCList;
        private string searchName;

        private string searchFC;

        // 决定显示的元素
        private bool displayAll;
        private bool displayHistory;

        public PlayerListWindow(XCPlugin plugin) : base(
            "XCount玩家列表")
        {
            Configuration = plugin.Configuration;
            this.plugin = plugin;
            watcher = XCPlugin.watcher;
            searchName = "";
            searchFC = "";
            searchNameList = new List<IPlayerCharacter>();
            searchFCList = new List<IPlayerCharacter>();
            displayAll = true;
            displayHistory = false;
        }

        public void Dispose() { }

        public override void Draw()
        {
            try
            {
                bool enableSort = plugin.Configuration.EnableDistanceSort;
                if (ImGui.Checkbox("按距离排序", ref enableSort))
                {
                    plugin.Configuration.EnableDistanceSort = enableSort;
                    plugin.Configuration.Save();
                }

                ImGui.SameLine();
                string serverName = "未知";
                if (Svc.ClientState.LocalPlayer.CurrentWorld.GameData != null)
                {
                    serverName = Svc.ClientState.LocalPlayer.CurrentWorld.GameData.Name;
                }

                ImGui.Text($"当前服务器：{serverName}");
                if (ImGui.InputText("搜索名称", ref searchName, 100)) { }

                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip("如果人多，可能需要关闭以下的两个选项才能看见搜索结果");
                if (ImGui.InputText("搜索部队", ref searchFC, 100)) { }

                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip("如果人多，可能需要关闭以下的两个选项才能看见搜索结果");
                if (ImGui.BeginChild("Player List", new Vector2(0f, -1f), true))
                {
                    ImGui.Checkbox("显示当前玩家", ref displayAll);
                    ImGui.SameLine();
                    ImGui.Checkbox("显示累计统计", ref displayHistory);
                    ImGui.SameLine();
                    if (ImGui.Button("清空累计统计"))
                    {
                        watcher.clearTemp();
                    }

                    if (displayAll)
                    {
                        ImGui.Text($"所有玩家（共{watcher.playerCharacters.Count()}）");
                        ImGui.Columns(12, "All Players");
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
                        ImGui.Text("X");
                        ImGui.NextColumn();
                        ImGui.Text("Y");
                        ImGui.NextColumn();
                        ImGui.Text("Z");
                        ImGui.NextColumn();
                        ImGui.Text("ID");
                        ImGui.NextColumn();
                        ImGui.Text("可见性");
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

                            ImGui.TextColored(fcTag.Equals("未知部队") ? ImGuiColors.DalamudGrey : ImGuiColors.ParsedGold,
                                              fcTag);
                            ImGui.NextColumn();
                            ImGui.Text(player.HomeWorld.GameData.Name);
                            ImGui.NextColumn();
                            ImGui.Text(player.Level.ToString());
                            ImGui.NextColumn();
                            ImGui.TextColored(
                                color, $"{player.ClassJob.GameData.Abbreviation}:{player.ClassJob.GameData.Name}");
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
                            ImGui.Text(Math.Round(player.Position.X, 3).ToString());
                            ImGui.NextColumn();
                            ImGui.Text(Math.Round(player.Position.Y, 3).ToString());
                            ImGui.NextColumn();
                            ImGui.Text(Math.Round(player.Position.Z, 3).ToString());
                            ImGui.NextColumn();
                            ImGui.Text(player.DataId.ToString());
                            ImGui.NextColumn();
                            bool visible = player.IsCharacterVisible();
                            ImGui.TextColored(visible ? ImGuiColors.HealerGreen : ImGuiColors.DPSRed,
                                              visible ? "是" : "否");
                            ImGui.NextColumn();
                        }

                        ImGui.Columns(1);
                    }

                    if (displayHistory)
                    {
                        ImGui.Text($"所有玩家（累计统计）（共{watcher.tempPlayersDict.Count}）");
                        ImGui.Columns(9, "All Players");
                        ImGui.Text("添加记录");
                        ImGui.NextColumn();
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
                        foreach (var playerDict in watcher.tempPlayersDict)
                        {
                            ImGui.TextColored(ImGuiColors.DalamudViolet, playerDict.Key);
                            ImGui.NextColumn();
                            var player = playerDict.Value;
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

                            ImGui.TextColored(fcTag.Equals("未知部队") ? ImGuiColors.DalamudGrey : ImGuiColors.ParsedGold,
                                              fcTag);
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
                            ImGui.Text(player.DataId.ToString());
                            ImGui.NextColumn();
                        }

                        ImGui.Columns(1);
                    }

                    if (!searchName.Equals(""))
                    {
                        searchNameList = watcher.playerCharacters.Where(pc => pc.Name.TextValue.Contains(searchName))
                                                .ToList();
                        if (ImGui.CollapsingHeader($"名称搜索结果（共{searchNameList.Count}）"))
                        {
                            ImGui.BeginChild("Name Players List");
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

                                ImGui.TextColored(
                                    fcTag.Equals("未知部队") ? ImGuiColors.DalamudGrey : ImGuiColors.ParsedGold, fcTag);
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
                                ImGui.Text(player.DataId.ToString());
                                ImGui.NextColumn();
                            }

                            ImGui.Columns(1);
                            ImGui.EndChild();
                        }
                    }

                    if (!searchFC.Equals(""))
                    {
                        searchFCList = watcher.playerCharacters.Where(pc => pc.CompanyTag.TextValue.Contains(searchFC))
                                              .ToList();
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

                                ImGui.TextColored(
                                    fcTag.Equals("未知部队") ? ImGuiColors.DalamudGrey : ImGuiColors.ParsedGold, fcTag);
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
                                ImGui.Text(player.DataId.ToString());
                                ImGui.NextColumn();
                            }

                            ImGui.Columns(1);
                        }
                    }

                    ImGui.EndChild();
                }
            }
            catch (Exception e)
            {
                Svc.Log.Info($"切换区域异常{e.Message}");
            }
        }
    }
}
