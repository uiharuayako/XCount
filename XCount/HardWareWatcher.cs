using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Dalamud.Game.Gui;

namespace XCount
{
    class HarderWare
    {
        public string srcName;
        public string unitName;
        public string data;

        HarderWare(string src, string unit)
        {
            srcName = src;
            unitName = unit;
            data = "";
        }
    }

    public class HardWareWatcher : IDisposable
    {
        public bool enableWatcher;
        private List<string> resultsList;
        public HttpClient client;
        XCPlugin plugin;

        public HardWareWatcher(XCPlugin plugin)
        {
            this.plugin = plugin;
            client = new HttpClient();
            enableWatcher = false;
            resultsList = new List<string>();
            client.Timeout=TimeSpan.FromSeconds(10);
        }

        public async void Enable()
        {
            try
            {
                enableWatcher = true;
                plugin.HwBarEntry.Text = "无信息";
                plugin.HwBarEntry.Shown = true;
                // 填写需要账号密码验证的url
                string url = plugin.Configuration.msiUrl;
                string Username = plugin.Configuration.msiUser;
                string Password = plugin.Configuration.msiPass;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Username}:{Password}")));
                Task watcherTask = new Task(() =>
                {
                    HttpResponseMessage response = GetServerResponse(client, url).Result;
                    if (response is null)
                    {
                        throw new Exception("获取链接失败");
                    }

                    XmlDocument xmlDoc = new XmlDocument();
                    while (enableWatcher)
                    {
                        response = GetServerResponse(client, url).Result;
                        if (response is null)
                        {
                            throw new Exception("获取链接失败");
                        }

                        xmlDoc.LoadXml(response.Content.ReadAsStringAsync().Result);
                        XmlNodeList xnl = xmlDoc.SelectSingleNode("HardwareMonitor/HardwareMonitorEntries").ChildNodes;
                        resultsList.Clear();
                        // 从获取到的xml里获取需要的信息
                        if (plugin.Configuration.hardWareItems.Any())
                        {
                            foreach (var hardWareItem in plugin.Configuration.hardWareItems)
                            {
                                if (xnl.Count > 0)
                                {
                                    foreach (XmlNode xNode in xnl)
                                    {
                                        // 判断，相等则添加信息
                                        if (hardWareItem.name.Equals(xNode.SelectSingleNode("srcName").InnerText))
                                        {
                                            resultsList.Add(
                                                hardWareItem.ReplaceInfo(xNode.SelectSingleNode("data").InnerText));
                                        }
                                    }
                                }
                            }
                        }

                        // 信息获取完毕，设置状态栏
                        if (resultsList.Any())
                        {
                            plugin.HwBarEntry.Text = string.Join(" | ", resultsList);
                        }

                        Thread.Sleep(plugin.Configuration.msiInterval);
                    }
                });
                watcherTask.Start();
                /*
                Thread watcherThread = new Thread(async () =>
                {
                    HttpResponseMessage response = client.GetAsync(url).Result;
                    XmlDocument xmlDoc = new XmlDocument();
                    while (enableWatcher)
                    {
                        response = client.GetAsync(url).Result;
                        xmlDoc.LoadXml(response.Content.ReadAsStringAsync().Result);
                        XmlNodeList xnl = xmlDoc.SelectSingleNode("HardwareMonitor/HardwareMonitorEntries").ChildNodes;
                        resultsList.Clear();
                        // 从获取到的xml里获取需要的信息
                        if (plugin.Configuration.hardWareItems.Any())
                        {
                            foreach (var hardWareItem in plugin.Configuration.hardWareItems)
                            {
                                if (xnl.Count > 0)
                                {
                                    foreach (XmlNode xNode in xnl)
                                    {
                                        // 判断，相等则添加信息
                                        if (hardWareItem.name.Equals(xNode.SelectSingleNode("srcName").InnerText))
                                        {
                                            resultsList.Add(
                                                hardWareItem.ReplaceInfo(xNode.SelectSingleNode("data").InnerText));
                                        }
                                    }
                                }
                            }
                        }

                        // 信息获取完毕，设置状态栏
                        if (resultsList.Any())
                        {
                            plugin.HwBarEntry.Text = string.Join(" | ", resultsList);
                        }

                        Thread.Sleep(plugin.Configuration.msiInterval);
                    }
                }); // 开始运行线程并将其设置为后台线程
                watcherThread.IsBackground = true;
                watcherThread.Start();
                */
            }
            catch (Exception ex)
            {
                OnExceptionOccur(ex);
            }
        }
        // 错误处理函数
        private void OnExceptionOccur(Exception ex)
        {
            // 出错时关掉自己
            Dispose();
            Dalamud.Logging.PluginLog.Error("XCount硬件监控出错，message:" + ex.Message);
            plugin.Configuration.enableHwStat = false;
            XCPlugin.ChatGui.Print(
                "从MSI Afterburner Api获取数据失败，请确定后台开了Remote Server并且正确设置密码，硬件监控功能已自动关闭，请稍后在设置中开启");
            if (!IfExistsMSIRemoteServer())
            {
                XCPlugin.ChatGui.Print(
                    "检测到后台似乎不存在MSIAfterburnerRemoteServer.exe进程，请问是否已经开启？建议使用自动开启功能");

            }
            plugin.Configuration.Save();
        }

        private async Task<HttpResponseMessage> GetServerResponse(HttpClient httpClient, string url)
        {
            try
            {
                var response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return response;
                }
                else
                {
                    OnExceptionOccur(new Exception("获取url失败"));
                    Dalamud.Logging.PluginLog.Error($"HTTP 请求失败，状态码：{response.StatusCode}");
                    return null;
                }
            }
            catch (HttpRequestException ex)
            {
                OnExceptionOccur(ex);
                Dalamud.Logging.PluginLog.Error($"HTTP 请求失败：{ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                OnExceptionOccur(ex);
                Dalamud.Logging.PluginLog.Error($"发生异常：{ex.Message}");
                return null;
            }
        }
        //判断进程是否存在
        bool IfExistsMSIRemoteServer()
        {
            return System.Diagnostics.Process.GetProcessesByName("MSIAfterburnerRemoteServer.exe").ToList().Count > 0;
        }
        public void Dispose()
        {
            enableWatcher = false;
            plugin.HwBarEntry.Shown = false;
        }
    }
}
