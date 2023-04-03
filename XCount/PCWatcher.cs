using Dalamud.Game.ClientState.Objects.SubKinds;
using System;
using System.Collections.Generic;
using System.Linq;

namespace XCount
{
    public class PCWatcher : IDisposable
    {
        public IEnumerable<PlayerCharacter> playerCharacters;
        public IEnumerable<PlayerCharacter> travelPlayers;
        public IEnumerable<PlayerCharacter> unDowPlayers;
        public Dictionary<string, PlayerCharacter> tempPlayersDict;
        private XCPlugin plugin = null!;

        public PCWatcher(XCPlugin plugin)
        {
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
            // 获取玩家列表
            playerCharacters = XCPlugin.ObjectTable.OfType<PlayerCharacter>();
            if (plugin.Configuration.enableDistanceSort)
            {
                // 排序
                playerCharacters = playerCharacters.OrderBy(StaticUtil.DistanceToPlayer);
            }

            if (plugin.Configuration.tempStat && CountResults.UnionPlayer < 9999)
            {
                // 如果合并搜索开启，则执行：
                foreach (PlayerCharacter character in playerCharacters)
                {
                    tempPlayersDict[$"{character.Name.TextValue}@{character.HomeWorld.GameData.Name}"] = character;
                }
            }

            CountResults.UnionPlayer = tempPlayersDict.Count;
            CountResults.CountAll = playerCharacters.Count();
            // 不在本服的玩家
            travelPlayers = playerCharacters.Where(pc => pc.CurrentWorld.GameData.Name != pc.HomeWorld.GameData.Name);
            CountResults.TravelPlayer = travelPlayers.Count();
            // 非战职玩家
            unDowPlayers = playerCharacters.Where(pc => pc.ClassJob.GameData.DohDolJobIndex != -1);
            CountResults.CountNoWar = unDowPlayers.Count();
            // 计算战职玩家数量
            CountResults.CountWar = CountResults.CountAll - unDowPlayers.Count();

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

            // 搜索指定玩家
            if (plugin.Configuration.enableNameSrarch)
            {
                CountResults.resultListStr.Clear();
                // 遍历玩家列表
                foreach (PlayerCharacter playerCharacter in playerCharacters)
                {
                    string name = playerCharacter.Name.TextValue;
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
                        plugin.Reapeat().Start();
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
