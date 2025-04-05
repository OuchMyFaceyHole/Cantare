using Microsoft.AspNetCore.Mvc;
using Cantare.Database;
using Cantare.Database.Models;
using Cantare.Database.Services;
using Cantare.Models;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Cantare.Helpers;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace Cantare.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly SongSelectionService songSelectionService;

        private readonly BackendDatabase backendDatabase;

        private readonly HTMLGenerator htmlGenerator;

        private readonly IConfiguration configuration;

        public HomeController(SongSelectionService songSelectionService, BackendDatabase backendDatabase, HTMLGenerator htmlGenerator , IConfiguration configuration)
        {
            this.songSelectionService = songSelectionService;
            this.backendDatabase = backendDatabase;
            this.htmlGenerator = htmlGenerator;
            this.configuration = configuration;
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
                var calendarData = await backendDatabase.SongCalender.FirstOrDefaultAsync(song => song.DateUsed.Date == DateTime.Parse(date).Date);
                if (calendarData == default)
                {
                    await songSelectionService.GenerateTodaysSong();
                    calendarData = await backendDatabase.SongCalender.FirstOrDefaultAsync(song => song.DateUsed.Date == DateTime.Parse(date).Date);
                }
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
        public async Task<IActionResult> SongGuess(string songDate, string songGuess, int guessCount)
        {
            var guessStatus = GuessEnumeration.Wrong;
            var songData = songGuess.Split('/');
            var songToCheck = await backendDatabase.SongCalender.FirstAsync(sng => sng.DateUsed == DateTime.Parse(songDate));

            if (songToCheck.SongInfo.Artist == songData[0])
            {
                if (songToCheck.SongInfo.AlbumTitle == songData[1])
                {
                    if (songToCheck.SongInfo.SongTitle == songData[2])
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
                    if (songToCheck.SongInfo.AlbumTitle.Length > songData[1].Length)
                    {
                        if (songToCheck.SongInfo.AlbumTitle.Contains(songData[1]))
                        {
                            guessStatus = GuessEnumeration.WrongVersion;
                        }
                    }
                    else 
                    {
                        if (songData[1].Contains(songToCheck.SongInfo.AlbumTitle))
                        {
                            guessStatus = GuessEnumeration.WrongVersion;
                        }
                    }
                    guessStatus = GuessEnumeration.WrongSongSameArtist;
                }
            }
            var returnData = JObject.FromObject(new
            {
                GuessResult = guessStatus,
                GuessCount = guessCount,
                SongDate = songDate,
                GuessData = songToCheck.SongInfo
            });
            return Ok(returnData.ToString());
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