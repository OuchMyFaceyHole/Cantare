using Microsoft.AspNetCore.Mvc;
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

        public HomeController(SongSelectionService songSelectionService)
        {
           this.songSelectionService = songSelectionService;
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

            return File(todaysSongData, "audio/wav");
        }

        [HttpPost("SongGuess")]
        public IActionResult SongGuess()
        {
            return View();
        }
    }
}