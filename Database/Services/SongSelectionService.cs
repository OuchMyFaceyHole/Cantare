
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

        private const uint byteRate = 176400;

        private const uint bytesToCopy = 1058400;

        public ConcurrentBag<byte[]> TodaysSongData { get; private set; } = new ConcurrentBag<byte[]>();

        public SongSelectionService(IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            this.scopeFactory = scopeFactory;
            SongsBasePath = configuration["SongsBasePath"];

            using var scope = scopeFactory.CreateScope();
            var backendDatabase = scope.ServiceProvider.GetRequiredService<BackendDatabase>();
            var todaysSong = backendDatabase.SongCalender.FirstOrDefault(song => song.DateUsed.Date == DateTime.Now.Date);
            if (todaysSong != default)
            {
                LoadTodaysSong(todaysSong.Artist, todaysSong.AlbumTitle, todaysSong.SongTitle, todaysSong.StartPoint);
            }
            else
            {
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
            char slashToUse;
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                slashToUse = '/';
            }
            else
            {
                slashToUse = '\\';
            }

            var baseDirectory = Directory.GetDirectories(SongsBasePath);
            var rand = new Random();

            var artistFullPath = baseDirectory[rand.Next(baseDirectory.Length)];
            var artist = artistFullPath.Split(SongsBasePath + slashToUse)[1];

            var artistDirectory = Directory.GetDirectories(artistFullPath);
            var albumFullPath = artistDirectory[rand.Next(artistDirectory.Length)];
            var album = albumFullPath.Split(artistFullPath + slashToUse)[1];

            var albumDirectory = Directory.GetFiles(albumFullPath);
            var songFullPath = albumDirectory[rand.Next(albumDirectory.Length)];
            var song = songFullPath.Split(albumFullPath + slashToUse)[1].Split('.')[0];

            var songData = GenerateSongSnippet(songFullPath);
            
            var backendDatabase = scope.ServiceProvider.GetRequiredService<BackendDatabase>();
            backendDatabase.SongCalender.Add(new Models.SongInfoModel(artist, album, song, songData.startPoint));
            backendDatabase.SaveChanges();
            TodaysSongData.Add(songData.songData);
        }

        private void LoadTodaysSong(string artist, string album, string song, int startPoint)
        {
            var path = "";
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                path = $"{SongsBasePath}/{artist}/{album}/{song}.wav";
            }
            else
            {
                path = $"{SongsBasePath}\\{artist}\\{album}\\{song}.wav";
            }
            var newSongData = GenerateSongSnippet(path, startPoint);
            TodaysSongData.Add(newSongData.songData);
        }
    }
}
