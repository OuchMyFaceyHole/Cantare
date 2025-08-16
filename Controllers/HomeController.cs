using Cantare.Database;
using Cantare.Database.Models;
using Cantare.Database.Services;
using Cantare.Helpers;
using Cantare.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Cantare.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly SongSelectionService songSelectionService;

        private readonly BackendDatabase backendDatabase;

        private readonly HTMLGenerator htmlGenerator;

        private readonly IConfiguration configuration;

        private readonly UserManager<UserModel> userManager;

        public HomeController(SongSelectionService songSelectionService, BackendDatabase backendDatabase, HTMLGenerator htmlGenerator,
            IConfiguration configuration, UserManager<UserModel> userManager)
        {
            this.songSelectionService = songSelectionService;
            this.backendDatabase = backendDatabase;
            this.htmlGenerator = htmlGenerator;
            this.configuration = configuration;
            this.userManager = userManager;
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
            var calendarData = await backendDatabase.SongCalender.FirstOrDefaultAsync(song => song.DateUsed.Date == DateTime.Parse(date).Date);
            if (calendarData == default)
            {
                await songSelectionService.GenerateTodaysSong();
                calendarData = await backendDatabase.SongCalender.FirstOrDefaultAsync(song => song.DateUsed.Date == DateTime.Parse(date).Date);
            }
            return File((await songSelectionService.GenerateSongSnippet(calendarData.SongInfo, calendarData.StartPoint)).songData, "audio/mpeg");
        }

        [HttpGet("GetSongImageData")]
        public async Task<IActionResult> GetSongImageData(string date = "")
        {
            var calendarData = await backendDatabase.SongCalender.Include(song => song.SongInfo.SongImage).FirstAsync(song => song.DateUsed.Date == DateTime.Parse(date).Date);
            return File(calendarData.SongInfo.SongImage.ImageData, "image/jpeg");
        }

        [HttpPost("SongGuess")]
        public async Task<IActionResult> SongGuess(string songDate, string songGuess, int guessCount)
        {
            var guessStatus = GuessEnumeration.Wrong;
            var songData = songGuess.Split('/', 3);
            var songInput = await backendDatabase.AvailableSongs.FirstAsync(sng => sng.Artist == songData[0] && sng.AlbumTitle == songData[1] && sng.SongTitle == songData[2]);
            var songToCheck = await backendDatabase.SongCalender.FirstAsync(sng => sng.DateUsed.Date == DateTime.Parse(songDate).Date);

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
                    guessStatus = GuessEnumeration.WrongSongSameArtist;
                    if (songToCheck.SongInfo.AlbumTitle.Length > songData[1].Length)
                    {
                        if (songToCheck.SongInfo.AlbumTitle.Contains(songData[1], StringComparison.CurrentCultureIgnoreCase))
                        {
                            guessStatus = GuessEnumeration.WrongVersion;
                        }
                    }
                    else
                    {
                        if (songData[1].Contains(songToCheck.SongInfo.AlbumTitle, StringComparison.CurrentCultureIgnoreCase))
                        {
                            guessStatus = GuessEnumeration.WrongVersion;
                        }
                    }
                }
            }

            var returnData = JObject.FromObject(new
            {
                GuessResult = guessStatus,
                GuessData = songInput,
                CorrectSong = guessStatus == GuessEnumeration.Correct || guessCount == 6 ? songToCheck.SongInfo : null
            });

            var user = await userManager.Users.Include(usr => usr.CalendarAnswers).ThenInclude(answer => answer.Guesses).ThenInclude(guess => guess.Song)
                .Include(usr => usr.CalendarAnswers).ThenInclude(answer => answer.Song).FirstOrDefaultAsync(usr => usr.UserName == User.Identity.Name);
            var guessForDate = user.CalendarAnswers.FirstOrDefault(ca => ca.SongDate == songToCheck.DateUsed);

            if (guessForDate == default)
            {
                guessForDate = new UserDateModel(songToCheck);
                user.CalendarAnswers.Add(guessForDate);
            }

            if (guessForDate.Guesses.Exists(guess => guess.GuessStatus == GuessEnumeration.Correct) || guessForDate.Guesses.Count == 6)
            {
                return Ok(returnData.ToString(Newtonsoft.Json.Formatting.None));
            }

            guessForDate.Guesses.Add(new GuessModel(guessStatus, songInput));

            await userManager.UpdateAsync(user);

            return Ok(returnData.ToString(Newtonsoft.Json.Formatting.None));
        }

        [HttpPost("SongSkip")]
        public async Task<IActionResult> SongSkip(string songDate)
        {
            var songToCheck = await backendDatabase.SongCalender.FirstAsync(sng => sng.DateUsed.Date == DateTime.Parse(songDate).Date);
            var user = await userManager.Users.Include(usr => usr.CalendarAnswers).ThenInclude(answer => answer.Guesses).ThenInclude(guess => guess.Song)
                .Include(usr => usr.CalendarAnswers).ThenInclude(answer => answer.Song).FirstOrDefaultAsync(usr => usr.UserName == User.Identity.Name);
            var guessForDate = user.CalendarAnswers.FirstOrDefault(ca => ca.SongDate == songToCheck.DateUsed);

            if (guessForDate == default)
            {
                guessForDate = new UserDateModel(songToCheck);
                user.CalendarAnswers.Add(guessForDate);
            }

            if (guessForDate.Guesses.Count < 5)
            {
                guessForDate.Guesses.Add(new GuessModel(GuessEnumeration.Skipped, songToCheck.SongInfo));
                await userManager.UpdateAsync(user);
                return Ok();
            }
            else
            {
                return Forbid();
            }

        }

        [HttpGet("GetSongNames")]
        public IActionResult GetSongNames(string songTitleStart)
        {
            var songs = backendDatabase.AvailableSongs.Where(sng =>
                EF.Functions.Like(sng.SongTitle, $"%{songTitleStart}%")).Take(6).Select(sng =>
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

        [HttpGet("GetGuessData")]
        public async Task<IActionResult> GetGuessData(string songDate)
        {
            var user = await userManager.Users.Include(usr => usr.CalendarAnswers).ThenInclude(answer => answer.Guesses).ThenInclude(guess => guess.Song)
                .Include(usr => usr.CalendarAnswers).ThenInclude(answer => answer.Song).FirstOrDefaultAsync(usr => usr.UserName == User.Identity.Name);
            var guessForDate = user.CalendarAnswers.FirstOrDefault(ca => ca.SongDate == DateTime.Parse(songDate).Date);

            if (guessForDate == default)
            {
                return Ok();
            }
            else
            {

                var returnData = JObject.FromObject(new
                {
                    Guesses = guessForDate.Guesses,
                    CorrectSong = guessForDate.Guesses.Count == 6 || guessForDate.Guesses.Exists(guess => guess.GuessStatus == GuessEnumeration.Correct) ? guessForDate.Song.SongInfo : null
                });

                return Ok(returnData.ToString(Newtonsoft.Json.Formatting.None));
            }
        }

    }
}