using Dalamud.Game.ClientState.Objects.SubKinds;
using System;
using System.Collections.Generic;
using System.Linq;

namespace XCount
{
    public class PCWatcher : IDisposable
    {
        private IEnumerable<PlayerCharacter> playerCharacters;
        private IEnumerable<PlayerCharacter> travelPlayers;
        private IEnumerable<PlayerCharacter> unDowPlayers;
        private HashSet<string> tempPlayers;
        private XCPlugin plugin = null!;
        public PCWatcher(XCPlugin plugin)
        {
            CountResults.isUpdate = false;
            tempPlayers = new HashSet<string>();
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
            if (plugin.Configuration.tempStat && CountResults.UnionPlayer < 9999)
            {
                // 如果合并搜索开启，则执行：
                foreach (PlayerCharacter character in playerCharacters)
                {
                    if (tempPlayers.Add(character.Name.ToString() + "@" + character.HomeWorld.GameData.Name.ToString()))
                    {
                        Dalamud.Logging.PluginLog.Log(character.Name.ToString());
                    }                    
                }
            }
            CountResults.UnionPlayer = tempPlayers.Count();
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
                    if (plugin.Configuration.nameList.Contains(name))
                    {
                        CountResults.resultListStr.AppendLine(name);
                    }
                }
            }
        }
        public void clearTemp()
        {
            Dalamud.Logging.PluginLog.Log($"人数{tempPlayers.Count}");
            if (tempPlayers.Count > 0)
            {
                Dalamud.Logging.PluginLog.Log("清空人数");
                tempPlayers.Clear();
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
