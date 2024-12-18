using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QianMo.Core.Infrastructure.Data;
using QianMo.Core.Models;
using QianMo.Core.Utilities;
using QianMo.Core.Algorithms;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO.Compression;
using Microsoft.AspNetCore.Components.Forms;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SpatioTemporalCoexistence.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpatioTemporalCoexistenceController : ControllerBase
    {
        private readonly ILogger<SpatioTemporalCoexistenceController> _logger;

        public SpatioTemporalCoexistenceController(ILogger<SpatioTemporalCoexistenceController> logger)
        {
            this._logger = logger;
        }

        private List<IGrouping<string, TwoTrajectoryGroup>> Analyse(List<Trajectory> tracks, TimeSpan timeThreshold,
            double distanceThreshold)
        {
            var stc = new QianMo.Core.Algorithms.SpatioTemporalCoexistence(_logger);
            ConcurrentBag<TwoTrajectoryGroup> groups = [];
            Parallel.For(0, tracks.Count, i =>
            // for (int i = 0; i < tracks.Count; i++)
            {
                var tracki = tracks[i];
                for (int j = i + 1; j < tracks.Count; j++)
                {
                    var trackj = tracks[j];
                    (List<IGeoInfoModel>, List<IGeoInfoModel>)? v = stc.CheckForBoxIntersection(tracki, trackj, timeThreshold);
                    if (v == null) continue;
                    var now = DateTime.Now;
                    int step = Math.Max(1, (int)(Math.Min(tracki.GeoPoints.Count, trackj.GeoPoints.Count) * 0.007));
                    var timess = stc.FindCompanionSegments(v.Value.Item1, v.Value.Item2, timeThreshold, distanceThreshold, step);
                    _logger.LogDebug("find_companion_segments:{0},{1},{2}", i, j, DateTime.Now - now);
                    if (timess == null) continue;
                    _logger.LogDebug("coexistence:{0},{1},{2},{3}", i, j, tracki.Name, trackj.Name);
                    var mergedTimes = stc.MergeTimes(timess, 60 * 3);
                    var filteredList = mergedTimes.Where(tup => (tup.Item2 - tup.Item1).TotalMinutes > 3).ToList();
                    if (filteredList.Count == 0) continue;
                    for (int k = 0; k < filteredList.Count; k++)
                    {
                        var (st, et) = filteredList[k];
                        var psi = tracki.GeoPoints.Where(t => t.Time >= st && t.Time <= et).ToList();
                        if (psi.Count < 2) continue;
                        var psj = trackj.GeoPoints.Where(t => t.Time >= st && t.Time <= et).ToList();
                        if (psj.Count < 2) continue;
                        var tracki_1 = new Trajectory();
                        tracki_1.Init(psi);
                        var trackj_1 = new Trajectory();
                        trackj_1.Init(psj);

                        groups.Add(new TwoTrajectoryGroup
                        {
                            Name1 = tracki.Name,
                            Name2 = trackj.Name,
                            Start = st,
                            End = et
                        });
                    }
                }
            });
            stc.GenerateGroupName([.. groups]);
            var dict = groups.GroupBy(t => t.Group).ToList();
            foreach (var items in dict)
            {
                var names = new HashSet<string>();
                foreach (var item in items)
                {
                    names.Add(item.Name1);
                    names.Add(item.Name2);
                }
            }
            return dict;
        }
        [HttpPost("[action]")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> AnalyseByCompressPltFile(IFormFile file, int timeThreshold = 3, double distanceThreshold = 0.001, bool withTrack = false)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");
            var guid = Guid.NewGuid().ToString();
            var dir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", guid);
            Directory.CreateDirectory(dir);
            var filePath = Path.Combine(dir, file.FileName);

            _logger.LogInformation($"开始下载文件：{file.FileName}");
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            var dataReader = new DataFileReader<GeoInfoModel>();
            List<Trajectory> tracks = [];
            _logger.LogInformation($"开始解压文件：{file.FileName}");
            using (var zip = ZipFile.Open(filePath, ZipArchiveMode.Read))
            {
                //zip.ExtractToDirectory(dir, true);
                foreach (var ey in zip.Entries)
                {
                    if (ey.Name.EndsWith(".plt") == false) continue;
                    //在条目中写入内容
                    using var reader = new StreamReader(ey.Open());
                    var text = await reader.ReadToEndAsync();
                    var track = DataFileReader<GeoInfoModel>.GetTrajectory(ey.Name, text, "\r\n", 6);
                    if (track != null)
                    {
                        tracks.Add(track);
                    }
                }
            }
            //_logger.LogInformation($"开始加载轨迹：{file.FileName}");
            //var pltFiles = FileTool.GetAllFile(dir, "*.plt");
            //var tracks = dataReader.GetTrajectories(pltFiles);

            tracks = tracks
                .DistinctBy(t => t.Name)
                .ToList();
            Directory.Delete(dir, true);
            _logger.LogInformation($"开始分析轨迹：{file.FileName},数量：{tracks.Count}");
            var ret = Analyse(tracks, TimeSpan.FromSeconds(timeThreshold), distanceThreshold);
            Dictionary<string, Trajectory?> trackDict = [];
            if (withTrack)
            {
                foreach (var group in ret)
                {
                    foreach (var item in group)
                    {
                        trackDict[item.Name1] = tracks.FirstOrDefault(t => t.Name == item.Name1);
                        trackDict[item.Name2] = tracks.FirstOrDefault(t => t.Name == item.Name2);
                    }
                }
            }
            _logger.LogInformation($"完成分析轨迹,组数：{ret.Count}");
            return Ok(new { groups = ret, tracks = trackDict.Values });
        }

        [HttpPost("[action]")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> AnalyseByPltFiles(List<IFormFile> file, int timeThreshold = 3, double distanceThreshold = 0.001, bool withTrack = false)
        {
            if (file == null || file.Count == 0)
                return BadRequest("No file uploaded.");
            var guid = Guid.NewGuid().ToString();
            var dir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", guid);
            Directory.CreateDirectory(dir);
            List<Trajectory> tracks = [];
            foreach (var item in file)
            {
                _logger.LogDebug($"开始下载文件：{item.FileName}");
                //var filePath = Path.Combine(dir, item.FileName);
                //using var stream = new FileStream(filePath, FileMode.Create);
                using var reader = new StreamReader(item.OpenReadStream());
                var track = DataFileReader<GeoInfoModel>.GetTrajectory(item.FileName, await reader.ReadToEndAsync(), "\r\n", 6);
                if (track != null)
                {
                    tracks.Add(track);
                }
                //await item.CopyToAsync(stream);
            }
            //_logger.LogInformation($"开始加载轨迹");
            var dataReader = new DataFileReader<GeoInfoModel>();
            //var pltFiles = FileTool.GetAllFile(dir, "*.plt");
            //var tracks = dataReader.GetTrajectories(pltFiles);
            tracks = tracks
               .DistinctBy(t => t.Name)
               .ToList();
            Directory.Delete(dir, true);
            _logger.LogInformation($"开始分析轨迹,数量：{tracks.Count}");
            var ret = Analyse(tracks, TimeSpan.FromSeconds(timeThreshold), distanceThreshold);
            Dictionary<string, Trajectory?> trackDict = [];
            if (withTrack)
            {
                foreach (var group in ret)
                {
                    foreach (var item in group)
                    {
                        trackDict[item.Name1] = tracks.FirstOrDefault(t => t.Name == item.Name1);
                        trackDict[item.Name2] = tracks.FirstOrDefault(t => t.Name == item.Name2);
                    }
                }
            }
            _logger.LogInformation($"完成分析轨迹,组数：{ret.Count}");
            return Ok(new { groups = ret, tracks = trackDict.Values });
        }
    }
}
