using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using ImGuiNET;
using ImGuiScene;

namespace XCount.Windows;

public class MainWindow : Window, IDisposable
{
    private string Image;
    private XCPlugin Plugin;
    public MainWindow(XCPlugin plugin, string image) : base(
        "XCount", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse|ImGuiWindowFlags.NoTitleBar)
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
        
    }

    public override void Draw()
    {
        var imageTex=Svc.Texture.GetFromFile(Image).GetWrapOrDefault();
        if (imageTex != null)
        {
            ImGui.Image(imageTex.ImGuiHandle, new Vector2(30, 30));
        }
        
        ImGui.SameLine();
        if (ImGui.Button("设置"))
        {
            this.Plugin.DrawConfigUI();
        }
        ImGui.SameLine();
        if (ImGui.Button("隐藏"))
        {
            this.Plugin.MainWindow.Toggle();
        }
        ImGui.SameLine();
        if (ImGui.Button("发送命令"))
        {
            Plugin.sendChatMsg();
        }
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("使用命令/xcchat也可以发送哦");
        ImGui.TextColored(ImGuiColors.DalamudRed, $"周边玩家总数 {CountResults.CountsDict["<all>"]}");
        ImGui.Text($"战职玩家总数 {CountResults.CountsDict["<war>"]}");
        ImGui.Text($"生产采集总数 {CountResults.CountsDict["<nowar>"]}");
        ImGui.Text($"跨服玩家总数 {CountResults.CountsDict["<foreign>"]}");
        if (Plugin.Configuration.TempStat)
        {
            ImGui.Text($"累计玩家计数 {CountResults.CountsDict["<union>"]}");
        }

        if (Plugin.Configuration.EnableNameSrarch)
        {
            ImGui.TextColored(ImGuiColors.DalamudRed, $"玩家搜索结果");
            ImGui.TextColored(ImGuiColors.DalamudYellow, CountResults.resultListStr.ToString());
        }
#if DEBUG
        ImGui.Text($"周边物体总数 {Svc.Objects.Count()}");
        PlayerCharacter me = Svc.ClientState.LocalPlayer;
        ImGui.Text($"初始世界：{me.CurrentWorld.GameData.Name}");
        ImGui.Text($"职业：{me.ClassJob.GameData.Name}，{me.ClassJob.GameData.DohDolJobIndex}");
#endif

        ImGui.Spacing();
    }
}
