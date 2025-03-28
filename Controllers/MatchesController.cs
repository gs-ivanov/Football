namespace Tournament.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

            if (matchesExists.Count() > 0)
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
                    matches.Add(new Match { HomeTeamId = teams[j].Id, AwayTeamId = teams[i].Id, MatchDate = startDate });
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
                    Id = m.Id,
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
                .Select(m => m.HomeTeam.Name)
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
                team.Points = 0;
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

        [Authorize(Roles = "Administrator")]
        public IActionResult Edit(int id)
        {
            var match = data.Matches
                .Where(m => m.Id == id)
                .Select(m => new MatchFormModel
                {
                    Id = m.Id,
                    HomeTeam = m.HomeTeam.Name,
                    AwayTeam = m.AwayTeam.Name,
                    MatchDate = m.MatchDate,
                    HomeTeamGoals = m.HomeTeamGoals,
                    AwayTeamGoals = m.AwayTeamGoals
                })
                .FirstOrDefault();

            if (match == null)
            {
                return NotFound();
            }

            return View(match);
        }


        [HttpPost, ActionName("Edit")]
        [Authorize(Roles = "Administrator")]
        public IActionResult EditMatches(int id, MatchFormModel match)
        {
            if (!ModelState.IsValid)
            {
                return View(match);
            }

            var matchData = this.data.Matches.FirstOrDefault(m => m.Id == id);
            if (matchData == null)
            {
                return NotFound();
            }

            // Взимаме отборите по Id
            var homeTeam = this.data.Teams.FirstOrDefault(t => t.Id == matchData.HomeTeamId);
            var awayTeam = this.data.Teams.FirstOrDefault(t => t.Id == matchData.AwayTeamId);

            if (homeTeam == null || awayTeam == null)
            {
                return NotFound();
            }

            // 🔹 Премахване на старите резултати от статистиките
            RemoveOldMatchStats(matchData, homeTeam, awayTeam);

            // 🔹 Запазване на новите резултати
            matchData.HomeTeamGoals = match.HomeTeamGoals;
            matchData.AwayTeamGoals = match.AwayTeamGoals;
            matchData.MatchDate = match.MatchDate;

            // 🔹 Актуализиране на статистиката за новия резултат
            UpdateNewMatchStats(matchData, homeTeam, awayTeam);

            this.data.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        private void RemoveOldMatchStats(Match match, Team homeTeam, Team awayTeam)
        {
            if (match.HomeTeamGoals > match.AwayTeamGoals)
            {
                if (homeTeam.Wins > 0) homeTeam.Wins--;
                if (awayTeam.Losts > 0) awayTeam.Losts--;
                if (homeTeam.Points >= 2) homeTeam.Points -= 2;
            }
            else if (match.HomeTeamGoals < match.AwayTeamGoals)
            {
                if (awayTeam.Wins > 0) awayTeam.Wins--;
                if (homeTeam.Losts > 0) homeTeam.Losts--;
                if (awayTeam.Points >= 2) awayTeam.Points -= 2;
            }
            else
            {
                if (homeTeam.Draws > 0) homeTeam.Draws--;
                if (awayTeam.Draws > 0) awayTeam.Draws--;
                if (homeTeam.Points >= 1) homeTeam.Points -= 1;
                if (awayTeam.Points >= 1) awayTeam.Points -= 1;
            }

            homeTeam.GoalsScored -= match.HomeTeamGoals;
            homeTeam.GoalsConceded -= match.AwayTeamGoals;
            awayTeam.GoalsScored -= match.AwayTeamGoals;
            awayTeam.GoalsConceded -= match.HomeTeamGoals;
        }

        private void UpdateNewMatchStats(Match match, Team homeTeam, Team awayTeam)
        {
            if (match.HomeTeamGoals > match.AwayTeamGoals)
            {
                homeTeam.Wins++;
                awayTeam.Losts++;
                homeTeam.Points += 2;
            }
            else if (match.HomeTeamGoals < match.AwayTeamGoals)
            {
                awayTeam.Wins++;
                homeTeam.Losts++;
                awayTeam.Points += 2;
            }
            else
            {
                homeTeam.Draws++;
                awayTeam.Draws++;
                homeTeam.Points += 1;
                awayTeam.Points += 1;
            }

            homeTeam.GoalsScored += match.HomeTeamGoals;
            homeTeam.GoalsConceded += match.AwayTeamGoals;
            awayTeam.GoalsScored += match.AwayTeamGoals;
            awayTeam.GoalsConceded += match.HomeTeamGoals;
        }

    }
}
