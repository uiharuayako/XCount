using Dalamud.Game.ClientState.Objects.SubKinds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using ECommons.GameFunctions;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace XCount
{
    public class PCWatcher : IDisposable
    {
        public IEnumerable<PlayerCharacter> playerCharacters;
        public List<PlayerCharacter> travelPlayers;
        public List<PlayerCharacter> unDowPlayers;
        public List<PlayerCharacter> GMsCharacters;
        public List<PlayerCharacter> invPlayers;
        public List<PlayerCharacter> excelPlayers;
        public Dictionary<string, PlayerCharacter> tempPlayersDict;
        private XCPlugin plugin = null!;

        public PCWatcher(XCPlugin plugin)
        {
            travelPlayers = new List<PlayerCharacter>();
            unDowPlayers = new List<PlayerCharacter>();
            GMsCharacters = new List<PlayerCharacter>();
            invPlayers = new List<PlayerCharacter>();
            excelPlayers = new List<PlayerCharacter>();
            CountResults.isUpdate = false;
            tempPlayersDict = new Dictionary<string, PlayerCharacter>();
            this.plugin = plugin;
        }

        public void Enable()
        {
            CountResults.isUpdate = true;
            XCPlugin.Framework.Update += OnFrameworkUpdate;
        }

        public void OnFrameworkUpdate(object _)
        {
            if (!XCPlugin.ClientState.IsLoggedIn) return;
            if (XCPlugin.ClientState.LocalPlayer == null) return;
            if (!XCPlugin.ClientState.LocalPlayer.IsCharacterVisible()) return;
            if (XCPlugin.ClientState.IsPvP) return;
            // 获取玩家列表
            playerCharacters = XCPlugin.ObjectTable.OfType<PlayerCharacter>().Where(pc => pc.ObjectId != 3758096384);
            travelPlayers = new List<PlayerCharacter>();
            unDowPlayers = new List<PlayerCharacter>();
            GMsCharacters = new List<PlayerCharacter>();
            invPlayers = new List<PlayerCharacter>();
            excelPlayers = new List<PlayerCharacter>();
            if (plugin.Configuration.enableDistanceSort)
            {
                // 排序
                playerCharacters = playerCharacters.OrderBy(StaticUtil.DistanceToPlayer);
            }

            foreach (PlayerCharacter character in playerCharacters)
            {
                // 合并搜索部分
                if (plugin.Configuration.tempStat && CountResults.UnionPlayer < 9999)
                {
                    // 如果合并搜索开启，则执行：
                    tempPlayersDict[$"{character.Name.TextValue}@{character.HomeWorld.GameData.Name}"] = character;
                }

                // 跨服玩家
                if (character.CurrentWorld.GameData.Name != character.HomeWorld.GameData.Name)
                {
                    travelPlayers.Add(character);
                }

                // 非战职玩家
                if (character.ClassJob.GameData.DohDolJobIndex != -1)
                {
                    unDowPlayers.Add(character);
                }

                // 不可见玩家
                if (!character.IsCharacterVisible())
                {
                    invPlayers.Add(character);
                }

                // 基于姓名搜索
                if (plugin.Configuration.enableNameSrarch)
                {
                    string name = character.Name.TextValue;
                    if (plugin.Configuration.nameListStr.Contains(name))
                    {
                        if (plugin.Configuration.enableAlert)
                        {
                            plugin.Configuration.enableAlert = false;
                            plugin.Configuration.Save();
                            plugin.chat.SendMessage($"/e 已查找到{name}!!自动关闭警报功能<se.1>");
                        }

                        CountResults.resultListStr.AppendLine(name);
                    }
                }

                // 谁是GM
                unsafe
                {
                    if (((Character*)(character.Address))->OnlineStatus <= 3 &&
                        ((Character*)(character.Address))->OnlineStatus > 0)
                    {
                        GMsCharacters.Add(character);
                    }
                }

                // 找到在线列表中的玩家
                if (plugin.Configuration.EnableOnlineList)
                {
                    if (ExcelProcess.ExcelList.Contains(
                            new SimplePlayer(character.Name.ToString(), character.HomeWorld.GameData.Name.ToString())))
                    {
                        excelPlayers.Add(character);
                    }
                }
            }


            CountResults.UnionPlayer = tempPlayersDict.Count;
            CountResults.CountAll = playerCharacters.Count();
            CountResults.TravelPlayer = travelPlayers.Count();
            CountResults.CountNoWar = unDowPlayers.Count();
            CountResults.CountWar = CountResults.CountAll - unDowPlayers.Count();
            CountResults.CountInv = invPlayers.Count();
            CountResults.DrawInvCharacters = invPlayers;
            CountResults.CountExcel = excelPlayers.Count();
            CountResults.DrawExcelCharacters = excelPlayers;
            if (plugin.Configuration.ShowInDtr)
            {
                string originStr = plugin.Configuration.dtrStr;
                // 如果开启合并统计
                if (plugin.Configuration.tempStat)
                {
                    originStr += plugin.Configuration.unionStr;
                }

                // 设置状态栏
                plugin.dtrEntry.Text = CountResults.ResultString(originStr);
            }

            // 判断人数是否超过阈值
            if (playerCharacters.Count() >= plugin.Configuration.alertCount)
            {
                if (plugin.Configuration.enableCountAlert)
                {
                    plugin.chat.SendMessage(
                        $"/e 人数阈值：{plugin.Configuration.alertCount}，当前人数：{playerCharacters.Count()}<se.1><se.2>");
                    if (playerCharacters.Count() == 2)
                    {
                        XCPlugin.ChatGui.PrintError("看 看 你 身 后");
                    }

                    plugin.Configuration.enableCountAlert = false;
                    plugin.Configuration.Save();
                    // 判断是否重复开启
                    if (plugin.Configuration.countAlertRepeat > 0)
                    {
                        Task.Run(async () =>
                        {
                            await Task.Delay(plugin.Configuration.countAlertRepeat * 1000);
                            plugin.Configuration.enableCountAlert = true;
                            plugin.Configuration.Save();
                        });
                    }
                }
            }

            if (plugin.Configuration.enableGMAlert || plugin.Configuration.enableGMDraw)
            {
                // 如果开了绘制
                if (plugin.Configuration.enableGMDraw)
                {
                    CountResults.DrawAdvCharacters = GMsCharacters;
                }

                // 如果开了警报，而且有这样的玩家
                if (plugin.Configuration.enableGMAlert && GMsCharacters.Count() != 0)
                {
                    plugin.chat.SendMessage(plugin.Configuration.gmAlertStr);
                    plugin.Configuration.enableGMAlert = false;
                    plugin.Configuration.Save();
                    // 判断是否重复开启
                    if (plugin.Configuration.gmAlertRepeat > 0)
                    {
                        Task.Run(async () =>
                        {
                            await Task.Delay(plugin.Configuration.gmAlertRepeat * 1000);
                            plugin.Configuration.enableGMAlert = true;
                            plugin.Configuration.Save();
                        });
                    }
                }
            }
        }

        public void clearTemp()
        {
            Dalamud.Logging.PluginLog.Log($"人数{tempPlayersDict.Count}");
            if (tempPlayersDict.Count > 0)
            {
                Dalamud.Logging.PluginLog.Log("清空人数");
                tempPlayersDict.Clear();
            }
        }

        // 卸载监听器
        public void Dispose()
        {
            CountResults.isUpdate = false;
            CountResults.UnionPlayer = 0;
            XCPlugin.Framework.Update -= OnFrameworkUpdate;
        }
    }
}
