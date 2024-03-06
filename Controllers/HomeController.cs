using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SwiftTrueRandom.Database;
using SwiftTrueRandom.Database.Models;
using SwiftTrueRandom.Database.Services;
using SwiftTrueRandom.Models;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;

namespace SwiftTrueRandom.Controllers
{
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
            // Load some crap for testing
            var todaysSong = backendDatabase.SongCalender.FirstOrDefault(song => song.DateUsed.Date == DateTime.Now.Date);


            // Find the user
            var user = this.backendDatabase.Users.FirstOrDefault(); // TODO: real filter

            if (user == default)
            {
                // Make a new user
                user = new UserModel();
                this.backendDatabase.Users.Add(user);
                this.backendDatabase.SaveChanges();
            }
            else
            {
                // Load the daily attempts
                this.backendDatabase.Entry(user).Collection(u => u.CalendarAnswers).Load();
            }


            // Find today's attempts, or make new
            var todaysAttempts = user.CalendarAnswers.FirstOrDefault(a => a.SongDate.Date == DateTime.Today);
            if (todaysAttempts == default)
            {
                // Make new
                todaysAttempts = new UserDateModel(todaysSong);

                user.CalendarAnswers.Add(todaysAttempts);
                this.backendDatabase.SaveChanges();
            }
            else
            {
                // Load previous guesses
                this.backendDatabase.Entry(todaysAttempts).Collection(u => u.Guesses).Load();
            }

            // Log some guesses 
            todaysAttempts.Guesses.Add(new GuessModel(GuessEnumeration.Wrong, 1, "Hunter Robinson", "MYAD, The Musical", "AssBlasting 2.0 On Da Floor"));
            todaysAttempts.Guesses.Add(new GuessModel(GuessEnumeration.Wrong, 2, "Taylor Shits", "MID-night", "Anti-Hero"));
            todaysAttempts.Guesses.Add(new GuessModel(GuessEnumeration.Correct, 3, "Taylor Swift", "Speak Now", "Mine"));

            this.backendDatabase.SaveChanges();

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

            return File(todaysSongData, "audio/wav");
        }

        [HttpPost("SongGuess")]
        public IActionResult SongGuess()
        {
            return View();
        }
    }
}