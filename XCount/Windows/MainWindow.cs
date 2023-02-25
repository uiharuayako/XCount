using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using ImGuiScene;

namespace XCount.Windows;

public class MainWindow : Window, IDisposable
{
    private TextureWrap Image;
    private XCPlugin Plugin;
    public MainWindow(XCPlugin plugin, TextureWrap image) : base(
        "XCount", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(200, 200),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.Image = image;
        this.Plugin = plugin;
    }

    public void Dispose()
    {
        this.Image.Dispose();
    }

    public override void Draw()
    {
        ImGui.Image(this.Image.ImGuiHandle, new Vector2(16, 16));
        ImGui.TextColored(ImGuiColors.DalamudRed, $"周边玩家总数 {CountResults.CountAll}");
        ImGui.Text($"战职玩家总数 {CountResults.CountWar}");
        ImGui.Text($"生产采集总数 {CountResults.CountNoWar}");
        ImGui.Text($"跨服玩家总数 {CountResults.TravelPlayer}");
        ImGui.TextColored(ImGuiColors.DalamudRed, $"玩家搜索结果");
        ImGui.TextColored(ImGuiColors.DalamudYellow,CountResults.resultListStr.ToString());
#if DEBUG
        ImGui.Text($"周边物体总数 {XCPlugin.ObjectTable.Count()}");
        PlayerCharacter me = XCPlugin.ClientState.LocalPlayer;
        ImGui.Text($"初始世界：{me.CurrentWorld.GameData.Name}");
        ImGui.Text($"职业：{me.ClassJob.GameData.Name}，{me.ClassJob.GameData.DohDolJobIndex}");
#endif
        if (ImGui.Button("打开设置"))
        {
            this.Plugin.DrawConfigUI();
        }

        ImGui.Spacing();
    }
}
