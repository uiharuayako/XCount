using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace XCount
{
    public class TXDocDownloader
    {
        public static bool IsFileExists => !"未找到文件".Equals(FileName);

        public static string FileName = "未找到文件";
        private string documentUrl;
        private string localPadId;
        private HttpClient client;

        public TXDocDownloader(string documentUrl, string localPadId, string cookieValue)
        {
            this.documentUrl = documentUrl;
            this.localPadId = localPadId;
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("Cookie", cookieValue);
        }

        public async Task<string> GetNowUserIndex()
        {
            HttpResponseMessage response = await client.GetAsync(this.documentUrl);
            var parser = await response.Content.ReadAsStringAsync();
            var globalMultiUserList = new Regex(@"window.global_multi_user=(.*?);").Matches(parser);
            if (globalMultiUserList.Count > 0)
            {
                dynamic userDict = JsonConvert.DeserializeObject(globalMultiUserList[0].Groups[1].Value);
                return userDict.nowUserIndex;
            }

            return "cookie过期,请重新输入";
        }

        public async Task<string> ExportExcelTask(string exportExcelUrl)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("docId", this.localPadId),
                new KeyValuePair<string, string>("version", "2")
            });

            var res = await client.PostAsync(exportExcelUrl, content);
            dynamic json = JsonConvert.DeserializeObject(await res.Content.ReadAsStringAsync());
            return json.operationId;
        }
        public async Task DownloadExcel(string checkProgressUrl, string fileName)
        {
            try
            {
                string fileUrl = "";
                var startTime = DateTime.Now;
                while (true)
                {
                    var res = await client.GetAsync(checkProgressUrl);
                    dynamic json = JsonConvert.DeserializeObject(await res.Content.ReadAsStringAsync());
                    int? progress = json.progress;
                    if (progress == 100)
                    {
                        fileUrl = json.file_url;
                        break;
                    }
                    else if ((DateTime.Now - startTime).TotalSeconds > 30)
                    {
                        XCPlugin.ChatGui.PrintError("下载文件超时");
                        return; // 退出方法，不抛出异常
                    }
                }

                if (!string.IsNullOrEmpty(fileUrl))
                {
                    var res = await client.GetAsync(fileUrl);
                    if (res.IsSuccessStatusCode)
                    {
                        using (var ms = await res.Content.ReadAsStreamAsync())
                        {
                            using (var fs = File.Create(Path.Combine(XCPlugin.PluginPath, fileName)))
                            {
                                ms.Seek(0, SeekOrigin.Begin);
                                await ms.CopyToAsync(fs);
                            }
                        }
                        XCPlugin.ChatGui.Print("下载成功，文件名: " + fileName);
                        FileName = StaticUtil.IsFileExists(XCPlugin.PluginPath, "TXList", "xlsx");
                    }
                    else
                    {
                        XCPlugin.ChatGui.PrintError("下载文件时出错，下载excel文件不成功");
                    }
                }
                else
                {
                    XCPlugin.ChatGui.PrintError("下载文件地址获取失败，下载excel文件不成功");
                }
            }
            catch (Exception e)
            {
                // 处理异常情况，如记录日志、通知用户等
                XCPlugin.ChatGui.PrintError("下载文件时出现异常（这个问题出现的最常见原因是Cookies过期）：" + e.Message);
            }
        }

    }
}
