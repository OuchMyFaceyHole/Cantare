
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using SwiftTrueRandom.Models;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace SwiftTrueRandom.Database.Services
{
    public class SongSelectionService
    {
        private readonly IServiceScopeFactory scopeFactory;

        private readonly string SongsBasePath;

        private readonly char SlashToUse;

        private const uint byteRate = 176400;

        private const uint bytesToCopy = 1058400;

        public ConcurrentBag<byte[]> TodaysSongData { get; private set; } = new ConcurrentBag<byte[]>();

        public SongSelectionService(IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            this.scopeFactory = scopeFactory;
            SongsBasePath = configuration["SongsBasePath"];

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                SlashToUse = '/';
            }
            else
            {
                SlashToUse = '\\';
            }

            using var scope = scopeFactory.CreateScope();
            var backendDatabase = scope.ServiceProvider.GetRequiredService<BackendDatabase>();
            if (!backendDatabase.AvailableSongs.Any())
            {
                CheckDirectory();
            }
            var todaysSong = backendDatabase.SongCalender.FirstOrDefault(song => song.DateUsed.Date == DateTime.Now.Date);
            if (todaysSong != default)
            {
                TodaysSongData.Clear();
                TodaysSongData.Add(GenerateSongSnippet(todaysSong.SongInfo.SongPath, todaysSong.StartPoint).songData);
            }
            else
            {
                TodaysSongData.Clear();
                GenerateTodaysSong();
            }
        }

        public (byte[] songData, int startPoint) GenerateSongSnippet(string path, int currentStartPoint = 0)
        {
            using var musicFile = System.IO.File.OpenRead(path);
            var headerBuffer = new byte[44];
            musicFile.Read(headerBuffer);
            var waveStructure = new WAVHeaderModel();
            unsafe
            {
                fixed (byte* ptr = headerBuffer)
                {
                    Buffer.MemoryCopy(ptr, &waveStructure, 44, 44);
                }
            }
            var lengthOfSong = waveStructure.SubChunk2Size / byteRate;
            var startPoint = 0;
            if (currentStartPoint == 0)
            {
                var rand = new Random();
                startPoint = rand.Next(20, (int)(lengthOfSong - 20));
            }
            else
            {
                startPoint = currentStartPoint;
            }
            var dataBuffer = new byte[bytesToCopy];
            musicFile.Position = startPoint * byteRate;
            musicFile.Read(dataBuffer);
            waveStructure.SubChunk2Size = bytesToCopy;
            waveStructure.ChunkSize = 36 + waveStructure.SubChunk2Size;
            var convertedHeader = new byte[44];
            unsafe
            {
                byte* headerPointer = (byte*)&waveStructure;
                for (int i = 0; i < 44; i++)
                {
                    convertedHeader[i] = headerPointer[i];
                }
            }
            var sixSecondSnippet = new MemoryStream();
            sixSecondSnippet.Write(convertedHeader, 0, convertedHeader.Length);
            sixSecondSnippet.Write(dataBuffer, 0, (int)bytesToCopy);
            var dataToSend = sixSecondSnippet.ToArray();
            return (dataToSend, startPoint);
        }

        private void GenerateTodaysSong()
        {
            using var scope = scopeFactory.CreateScope();
            var backendDatabase = scope.ServiceProvider.GetRequiredService<BackendDatabase>();
            var rand = new Random();
            var songToUse = backendDatabase.AvailableSongs.Skip(rand.Next(0, backendDatabase.AvailableSongs.Count() - 1)).Take(1).ToArray()[0];

            var songData = GenerateSongSnippet(songToUse.SongPath);
            
            backendDatabase.SongCalender.Add(new Models.CalendarSongModel(songData.startPoint, songToUse));
            backendDatabase.SaveChanges();
            TodaysSongData.Add(songData.songData);
        }

        private void CheckDirectory()
        {
            using var scope = scopeFactory.CreateScope();
            var backendDatabase = scope.ServiceProvider.GetRequiredService<BackendDatabase>();

            var songPaths = Directory.EnumerateFiles(SongsBasePath, "*.wav", SearchOption.AllDirectories);
            foreach (var song in songPaths)
            {
                try
                {
                    var songSplit = song.Split(SongsBasePath + SlashToUse)[1].Split(SlashToUse);
                    var songMatch = backendDatabase.AvailableSongs.FirstOrDefault(sng => sng.Artist == songSplit[0] && 
                        sng.AlbumTitle == songSplit[1] && sng.SongTitle == songSplit[2]);
                    if (songMatch == default)
                    {
                        backendDatabase.AvailableSongs.Add(new Models.SongModel(songSplit[0], songSplit[1], songSplit[2].Split(".")[0], song));
                    }
                    else if (songMatch.SongPath != song)
                    {
                        backendDatabase.AvailableSongs.Remove(songMatch);
                        backendDatabase.AvailableSongs.Add(new Models.SongModel(songSplit[0], songSplit[1], songSplit[2].Split(".")[0], song));
                    }
                }
                catch { }
            }

            backendDatabase.SaveChanges();
        }
    }
}
