using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Cantare.Database;
using Cantare.Database.Models;
using Cantare.Database.Services;
using Cantare.Models;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Cantare.Helpers;

namespace Cantare.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly SongSelectionService songSelectionService;

        private readonly BackendDatabase backendDatabase;

        private readonly HTMLGenerator htmlGenerator;

        public HomeController(SongSelectionService songSelectionService, BackendDatabase backendDatabase, HTMLGenerator htmlGenerator)
        {
           this.songSelectionService = songSelectionService;
           this.backendDatabase = backendDatabase;
           this.htmlGenerator = htmlGenerator;
        }

        
        public IActionResult Index()
        {
            return View();
        }
        

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet("GetSongAudioData")]
        public async Task<IActionResult> GetSongAudioData(string date = "")
        {
            if (date == "")
            {
                return File(songSelectionService.TodaysSongData.First(), "audio/mpeg");
            }
            else
            {
                var calendarData = await backendDatabase.SongCalender.FirstAsync(song => song.DateUsed.Date == DateTime.Parse(date).Date);
                return File((await songSelectionService.GenerateSongSnippet(calendarData.SongInfo, calendarData.StartPoint)).songData, "audio/mpeg");
            }
            
        }

        [HttpGet("GetSongImageData")]
        public async Task<IActionResult> GetSongImageData(string date = "")
        {
            if (date == "")
            {
                return File(songSelectionService.TodaysSongData.First(), "image/jpeg");
            }
            else
            {
                var calendarData = await backendDatabase.SongCalender.Include(song => song.SongInfo.SongImage).FirstAsync(song => song.DateUsed.Date == DateTime.Parse(date).Date);
                return File(calendarData.SongInfo.SongImage.ImageData, "image/jpeg");
            }
        }

        [HttpPost("SongGuess")]
        public async Task<IActionResult> SongGuess(string songGuess)
        {
            var guessStatus = GuessEnumeration.Wrong;
            var songData = songGuess.Split('/');
            var todaysSong = await backendDatabase.SongCalender.FirstAsync(sng => sng.DateUsed == DateTime.Now.Date);

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

        [HttpGet("GetCalendarData")]
        public async Task<IActionResult> GetCalendarData(int page)
        {
            return Ok(await htmlGenerator.GenerateHTML(HttpContext, "/Views/CalendarPage.cshtml", page));
        }

        [HttpGet("GetPlayingArea")]
        public async Task<IActionResult> GetPlayingArea(string date)
        {
            return Ok(await htmlGenerator.GenerateHTML(HttpContext, "/Views/Home/PlayingAreaPartial.cshtml", DateTime.Parse(date)));
        }
    }
}