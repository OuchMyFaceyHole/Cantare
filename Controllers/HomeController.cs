using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftTrueRandom.Database;
using SwiftTrueRandom.Database.Models;
using SwiftTrueRandom.Database.Services;
using SwiftTrueRandom.Models;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace SwiftTrueRandom.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly SongSelectionService songSelectionService;

        private readonly BackendDatabase backendDatabase;

        public HomeController(SongSelectionService songSelectionService, BackendDatabase backendDatabase)
        {
           this.songSelectionService = songSelectionService;
           this.backendDatabase = backendDatabase;
        }

        
        public IActionResult Index()
        {
            return View();
        }
        
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet("GetSongData")]
        public IActionResult GetSongData()
        {
            var todaysSongData = songSelectionService.TodaysSongData.First();

            return File(todaysSongData, "audio/mpeg");
        }

        [HttpPost("SongGuess")]
        public IActionResult SongGuess(string songGuess)
        {
            var guessStatus = GuessEnumeration.Wrong;
            var songData = songGuess.Split('/');
            var todaysSong = backendDatabase.SongCalender.First(sng => sng.DateUsed == DateTime.Now.Date);

            if (todaysSong.SongInfo.Artist == songData[0])
            {
                if (todaysSong.SongInfo.AlbumTitle == songData[1])
                {
                    if (todaysSong.SongInfo.SongTitle == songData[2])
                    {
                        guessStatus = GuessEnumeration.Correct;
                    }
                    else
                    {
                        guessStatus = GuessEnumeration.WrongSongSameAlbum;
                    }
                }
                else
                {
                    if (todaysSong.SongInfo.AlbumTitle.Length > songData[1].Length)
                    {
                        if (todaysSong.SongInfo.AlbumTitle.Contains(songData[1]))
                        {
                            guessStatus = GuessEnumeration.WrongVersion;
                        }
                    }
                    else 
                    {
                        if (songData[1].Contains(todaysSong.SongInfo.AlbumTitle))
                        {
                            guessStatus = GuessEnumeration.WrongVersion;
                        }
                    }
                    guessStatus = GuessEnumeration.WrongSongSameArtist;
                }
            }
            return Ok(guessStatus);
        }

        [HttpGet("GetSongNames")]
        public IActionResult GetSongNames(string songTitleStart)
        {
            var songs = backendDatabase.AvailableSongs.Where(sng => 
                EF.Functions.Like(sng.SongTitle, $"{songTitleStart}%")).Take(6).Select(sng => 
                    $"{sng.Artist}/{sng.AlbumTitle}/{sng.SongTitle}");

           return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(songs));
        }
            
    }
}