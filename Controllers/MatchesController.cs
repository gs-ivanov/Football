namespace Tournament.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Tournament.Data;
    using Tournament.Data.Models;
    using Tournament.Models.Matches;

    using static Tournament.WebConstants;

    public class MatchesController : Controller
    {
        private readonly TurnirDbContext data;

        public MatchesController(TurnirDbContext context)
        {
            data = context;
        }

        public IActionResult GenerateSchedule()
        {
            var matchesExists = data.Matches.ToList();

            if (matchesExists.Count()>0)
            {
                TempData[GlobalMessageKey] = "График вече е създаден. Ако искаш нов график, избери 'Нулиране на графика'.";
                return RedirectToAction("Index", "Teams");
            }

            var teams = data.Teams.ToList();
            if (teams.Count != 4)
            {
                TempData[GlobalMessageKey] = "Трябва да има точно 4 отбора, за да се генерира график.";
                return RedirectToAction("Index", "Teams");
            }

            List<Match> matches = new List<Match>();
            DateTime startDate = DateTime.Now.AddDays(7); // Започваме след една седмица

            for (int i = 0; i < teams.Count; i++)
            {
                for (int j = i + 1; j < teams.Count; j++)
                {
                    matches.Add(new Match { HomeTeamId = teams[i].Id, AwayTeamId = teams[j].Id, MatchDate = startDate });
                    matches.Add(new Match { HomeTeamId = teams[j].Id, AwayTeamId = teams[i].Id, MatchDate = startDate.AddDays(7) });
                    startDate = startDate.AddDays(7);
                }
            }

            data.Matches.AddRange(matches);
            data.SaveChanges();

            TempData[GlobalMessageKey] = "Графикът е успешно генериран!";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Index()
        {
            var matches = data.Matches
                .OrderBy(m => m.MatchDate)
                .Select(m => new MatchViewModel
                {
                    HomeTeam = m.HomeTeam.Name,
                    AwayTeam = m.AwayTeam.Name,
                    HomeTeamGoals = m.HomeTeamGoals,
                    AwayTeamGoals = m.AwayTeamGoals,
                    MatchDate = m.MatchDate
                })
                .ToList();

            return View(matches);
        }


        [Authorize(Roles = "Administrator")]
        public IActionResult Reset()
        {
            var matchesExists = data.Matches.ToList();

            if (matchesExists.Count() == 0)
            {
                TempData[GlobalMessageKey] = "Графика вече е нулиран. Ако искаш нов график, избери 'Generate Schedule'.";
                return RedirectToAction("Index", "Teams");
            }
            var match = this.data.Matches
                .Where(m => m.Id > 0)
                .Select(m=>m.HomeTeam.Name)
                .FirstOrDefault();

            if (match == null) return NotFound();

            string mesage = "Ready to RESET schedule!";
            ViewBag.Msg = mesage;
            return View();
        }

        [HttpPost, ActionName("Reset")]
        [Authorize(Roles = "Administrator")]
        public IActionResult ResetConfirmed()
        {
            List<Match> itemsToDelete = this.data.Matches.ToList();
            this.data.Matches.RemoveRange(itemsToDelete);
            this.data.SaveChanges();

            // Нулиране на статистиките на отборите
            var teams = this.data.Teams.ToList();
            foreach (var team in teams)
            {
                team.Wins = 0;
                team.Losts = 0;
                team.Draws = 0;  // Нулиране на равните мачове
                team.GoalsScored = 0;
                team.GoalsConceded = 0;
            }

            this.data.SaveChanges();

            TempData[GlobalMessageKey] = "Графикът и статистиките са нулирани. За нов график - избери 'Generate Schedule'.";

            //return RedirectToAction(nameof(AllTeams));
            return RedirectToAction("Index", "Teams");

        }

    }
}
