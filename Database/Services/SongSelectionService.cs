
using FFMpegCore;
using FFMpegCore.Pipes;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using SwiftTrueRandom.Database.Models;
using SwiftTrueRandom.Models;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace SwiftTrueRandom.Database.Services
{
    public class SongSelectionService
    {
        private readonly IServiceScopeFactory scopeFactory;

        private readonly List<string> SongPaths = [];

        private readonly string SongConfigPath = "";

        private const uint byteRate = 176400;

        private const uint bytesToCopy = 1058400;

        public ConcurrentBag<byte[]> TodaysSongData { get; private set; } = new ConcurrentBag<byte[]>();

        public SongSelectionService(IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            this.scopeFactory = scopeFactory;
            SongConfigPath = configuration["SongsConfig"];

            var jsonData = JObject.Parse(File.ReadAllText(SongConfigPath));
            SongPaths = ((JArray)jsonData["Songs"]).ToObject<List<string>>();

            using var scope = scopeFactory.CreateScope();
            var backendDatabase = scope.ServiceProvider.GetRequiredService<BackendDatabase>();
            RefreshSongs(backendDatabase).Wait();
            var todaysSong = backendDatabase.SongCalender.FirstOrDefault(song => song.DateUsed.Date == DateTime.Now.Date);
            if (todaysSong != default)
            {
                TodaysSongData.Clear();
                TodaysSongData.Add(GenerateSongSnippet(todaysSong.SongInfo, todaysSong.StartPoint).Result.songData);
            }
            else
            {
                TodaysSongData.Clear();
                GenerateTodaysSong().Wait();
            }
        }

        public async Task<(byte[] songData, int startPoint)> GenerateSongSnippet(SongModel song, int currentStartPoint = 0)
        {
            var startPoint = 0;
            if (currentStartPoint == 0)
            {
                var rand = new Random();
                startPoint = rand.Next(20, (song.SongLength - 20));
            }
            else
            {
                startPoint = currentStartPoint;
            }

            var audioData = new MemoryStream();
            await FFMpegArguments.FromFileInput(song.SongPath).OutputToPipe(new StreamPipeSink(audioData), options =>
            {
                options.ForceFormat("mp3");
                options.SelectStream(song.AudioStreamIndex);
                options.Seek(TimeSpan.FromSeconds(startPoint));
                options.WithDuration(TimeSpan.FromSeconds(6));
                options.WithAudioSamplingRate(44100);
            }).ProcessAsynchronously();
            return (audioData.ToArray(), startPoint);
        }

        public async Task RefreshSongs(BackendDatabase backendDatabase)
        {
            foreach (var songPath in SongPaths)
            {
                if (File.Exists(songPath))
                {
                    if (!await backendDatabase.AvailableSongs.AnyAsync(song => song.SongPath == songPath))
                    {
                        var metaData = FFProbe.Analyse(songPath);
                        var songTags = metaData.Format.Tags;
                        try
                        {
                            var artist = songTags.ContainsKey("album_artist") ? songTags["album_artist"] : songTags["artist"];
                            await backendDatabase.AvailableSongs.AddAsync(new Models.SongModel(
                                artist.Trim(), songTags["album"].Trim(), songTags["title"].Trim(), songPath, 
                                (int)metaData.Duration.TotalSeconds, metaData.PrimaryAudioStream.Index));;
                        }
                        catch (KeyNotFoundException)
                        {
                            //Need to log missing required metadata
                        }
                    }
                }
                else
                {
                    var possibleMatch = await backendDatabase.AvailableSongs.FirstOrDefaultAsync(song => song.SongPath == songPath);
                    if (possibleMatch != default)
                    {
                        possibleMatch.IsEnabled = false;
                        //Probably need to alert this somehow
                    }
                    else
                    {
                        //Needs error log
                    }
                }
            }

            await backendDatabase.SaveChangesAsync();
        }

        private async Task GenerateTodaysSong()
        {
            using var scope = scopeFactory.CreateScope();
            var backendDatabase = scope.ServiceProvider.GetRequiredService<BackendDatabase>();
            var rand = new Random();
            var songToUse = backendDatabase.AvailableSongs.Skip(rand.Next(0, backendDatabase.AvailableSongs.Count())).Take(1).ElementAt(0);

            var songData = await GenerateSongSnippet(songToUse);
            
            await backendDatabase.SongCalender.AddAsync(new Models.CalendarSongModel(songData.startPoint, songToUse));
            await backendDatabase.SaveChangesAsync();
            TodaysSongData.Add(songData.songData);
        }
    }
}
