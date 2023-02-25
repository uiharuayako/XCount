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
        private XCPlugin plugin = null!;
        public PCWatcher(XCPlugin plugin)
        {
            CountResults.isUpdate = false;
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
                // 设置状态栏
                plugin.dtrEntry.Text = CountResults.ResultString(plugin.Configuration.dtrStr);
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
        // 卸载监听器
        public void Dispose()
        {
            CountResults.isUpdate = false;
            XCPlugin.Framework.Update -= OnFrameworkUpdate;
        }
    }
}
