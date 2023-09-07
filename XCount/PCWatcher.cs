using Dalamud.Game.ClientState.Objects.SubKinds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using ECommons.GameFunctions;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using ECommons.DalamudServices;

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
        public List<PlayerCharacter> enemyPlayers;
        public List<PlayerCharacter> targetPlayers;
        public Dictionary<string, PlayerCharacter> tempPlayersDict;
        private XCPlugin plugin = null!;

        public PCWatcher(XCPlugin plugin)
        {
            travelPlayers = new List<PlayerCharacter>();
            unDowPlayers = new List<PlayerCharacter>();
            GMsCharacters = new List<PlayerCharacter>();
            invPlayers = new List<PlayerCharacter>();
            excelPlayers = new List<PlayerCharacter>();
            enemyPlayers = new List<PlayerCharacter>();
            targetPlayers = new List<PlayerCharacter>();
            CountResults.isUpdate = false;
            tempPlayersDict = new Dictionary<string, PlayerCharacter>();
            this.plugin = plugin;
        }

        private IntPtr _funcCanAttack;

        public unsafe bool CanAttack(PlayerCharacter character)
        {
            if (character == null) return false;

            return ((delegate*<long, IntPtr, long>)_funcCanAttack)(142L, character.Address) == 1;
        }

        public void Enable()
        {
            _funcCanAttack =
                Svc.SigScanner.ScanText("48 89 5C 24 ?? 57 48 83 EC 20 48 8B DA 8B F9 E8 ?? ?? ?? ?? 4C 8B C3 ");
            CountResults.isUpdate = true;
            Svc.Framework.Update += OnFrameworkUpdate;
        }

        public void OnFrameworkUpdate(object _)
        {
            if (!Svc.ClientState.IsLoggedIn) return;
            if (Svc.ClientState.LocalPlayer == null) return;
            if (!Svc.ClientState.LocalPlayer.IsCharacterVisible()) return;
            // 获取玩家列表
            playerCharacters = Svc.Objects.OfType<PlayerCharacter>().Where(pc => pc.ObjectId != 3758096384);
            travelPlayers.Clear();
            unDowPlayers.Clear();
            GMsCharacters.Clear();
            invPlayers.Clear();
            excelPlayers.Clear();
            enemyPlayers.Clear();
            targetPlayers.Clear();
            if (plugin.Configuration.EnableDistanceSort)
            {
                // 排序
                playerCharacters = playerCharacters.OrderBy(StaticUtil.DistanceToPlayer);
            }

            foreach (PlayerCharacter character in playerCharacters)
            {
                // 合并搜索部分
                if (plugin.Configuration.TempStat && CountResults.CountsDict["<union>"] < 9999)
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
                if (plugin.Configuration.EnableNameSrarch)
                {
                    string name = character.Name.TextValue;
                    if (plugin.Configuration.NameListStr.Contains(name))
                    {
                        if (plugin.Configuration.EnableAlert)
                        {
                            plugin.Configuration.EnableAlert = false;
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

                // 找到PVP里的敌人
                if (Svc.ClientState.IsPvP && character.IsValid() && character.IsTargetable() &&
                    CanAttack(character))
                {
                    enemyPlayers.Add(character);
                }

                // 找到以你为目标的人
                if (character.TargetObject?.Address == Svc.ClientState.LocalPlayer.Address)
                {
                    targetPlayers.Add(character);
                }
            }


            CountResults.CountsDict["<union>"] = tempPlayersDict.Count;
            CountResults.CountsDict["<all>"] = playerCharacters.Count();
            CountResults.CountsDict["<foreign>"] = travelPlayers.Count();
            CountResults.CountsDict["<nowar>"] = unDowPlayers.Count();
            CountResults.CountsDict["<war>"] = CountResults.CountsDict["<all>"] - unDowPlayers.Count();
            CountResults.CountsDict["<inv>"] = invPlayers.Count();
            CountResults.CountsDict["<excel>"] = excelPlayers.Count();
            CountResults.CountsDict["<enemy>"] = enemyPlayers.Count();
            CountResults.CountsDict["<targetU>"] = targetPlayers.Count();
            if (plugin.Configuration.ShowInDtr)
            {
                string originStr = plugin.Configuration.dtrStr;
                // 如果开启合并统计
                if (plugin.Configuration.TempStat)
                {
                    originStr += plugin.Configuration.UnionStr;
                }

                // 设置状态栏
                plugin.dtrEntry.Text = CountResults.ResultString(originStr);
            }

            // 判断人数是否超过阈值
            if (playerCharacters.Count() >= plugin.Configuration.AlertCount)
            {
                if (plugin.Configuration.EnableCountAlert)
                {
                    plugin.chat.SendMessage(
                        $"/e 人数阈值：{plugin.Configuration.AlertCount}，当前人数：{playerCharacters.Count()}<se.1><se.2>");
                    if (playerCharacters.Count() == 2)
                    {
                        Svc.Chat.PrintError("看 看 你 身 后");
                    }

                    plugin.Configuration.EnableCountAlert = false;
                    plugin.Configuration.Save();
                    // 判断是否重复开启
                    if (plugin.Configuration.CountAlertRepeat > 0)
                    {
                        Task.Run(async () =>
                        {
                            await Task.Delay(plugin.Configuration.CountAlertRepeat * 1000);
                            plugin.Configuration.EnableCountAlert = true;
                            plugin.Configuration.Save();
                        });
                    }
                }
            }

            if (plugin.Configuration.EnableGMAlert || plugin.Configuration.EnableGMDraw)
            {
                // 如果开了警报，而且有这样的玩家
                if (plugin.Configuration.EnableGMAlert && GMsCharacters.Count() != 0)
                {
                    plugin.chat.SendMessage(plugin.Configuration.GmAlertStr);
                    plugin.Configuration.EnableGMAlert = false;
                    plugin.Configuration.Save();
                    // 判断是否重复开启
                    if (plugin.Configuration.GmAlertRepeat > 0)
                    {
                        Task.Run(async () =>
                        {
                            await Task.Delay(plugin.Configuration.GmAlertRepeat * 1000);
                            plugin.Configuration.EnableGMAlert = true;
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
            CountResults.CountsDict["<union>"] = 0;
            Svc.Framework.Update -= OnFrameworkUpdate;
        }
    }
}
