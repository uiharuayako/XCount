using Dalamud.Game.ClientState.Objects.SubKinds;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ECommons.DalamudServices;

namespace XCount
{
    static class StaticUtil
    {
        public static bool EnableAlertChat=false;

        public static float DistanceToPlayer(IPlayerCharacter obj)
        {
            if (obj == null) return 0;
            Vector3 objPosition = new(obj.Position.X, obj.Position.Y, obj.Position.Z);
            Vector3 selfPosition = new(Svc.ClientState.LocalPlayer.Position.X, Svc.ClientState.LocalPlayer.Position.Y, Svc.ClientState.LocalPlayer.Position.Z);
            return Math.Max(0, Vector3.Distance(objPosition, selfPosition) - obj.HitboxRadius - Svc.ClientState.LocalPlayer.HitboxRadius);
        }
        public static string IsFileExists(string directoryPath, string prefix, string extension,bool isDelete=false)
        {
            string result = "未找到文件";
            // 检查文件夹是否存在
            if (!Directory.Exists(directoryPath))
            {
                return result;
            }

            // 获取所有文件的全名（包含路径）
            string[] files = Directory.GetFiles(directoryPath);

            foreach (var file in files)
            {
                // 获取文件名（不包含路径）
                string fileName = Path.GetFileName(file);
                // 检查文件名是否以特定前缀开头，且扩展名匹配
                if (fileName.StartsWith(prefix) && Path.GetExtension(fileName) == "." + extension)
                {
                    // 如果匹配，删除文件
                    if(isDelete) File.Delete(file);
                    result = file;
                }
            }
            return result;
        }

        public static async void DownLoadTXDoc(Configuration config)
        {
            TXDocDownloader tx= new TXDocDownloader(config.TXDocUrl, config.TXLocalPadId, config.TXCookie);
            string nowUserIndex = await tx.GetNowUserIndex();
            // 导出文件任务url
            string exportExcelUrl = $"https://docs.qq.com/v1/export/export_office?u={nowUserIndex}";
            // 获取导出任务的操作id
            string operationId = await tx.ExportExcelTask(exportExcelUrl);
            string checkProgressUrl =
                $"https://docs.qq.com/v1/export/query_progress?u={nowUserIndex}&operationId={operationId}";
            string fileName = $"TXList{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.xlsx";
            await tx.DownloadExcel(checkProgressUrl, fileName);
            config.ExcelPath = Path.Combine(XCPlugin.PluginPath, fileName);
            config.Save();
            ExcelProcess.ReadPlayerInfoFromExcel(config.ExcelPath, config.SheetName,
                                                 config.NameCol, config.ServerCol);
        }
    }
}
