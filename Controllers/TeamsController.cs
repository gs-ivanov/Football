namespace Tournament.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Tournament.Data;
    using Tournament.Data.Models;
    using Tournament.Models.Teams;
    using System.Text;
    using System;

    [Authorize]
    public class TeamsController : Controller
    {
        private readonly TurnirDbContext data;

        public TeamsController(TurnirDbContext data)
        {
            this.data = data;
        }

        public async Task<IActionResult> Index()
        {
            var teams = await data.Teams
                .OrderByDescending(t => t.Points)
                .ThenByDescending(t=>t.GoalsScored)
                .ToListAsync();
            return View(teams);
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create(Team team)
        {
            if (ModelState.IsValid)
            {
                data.Add(team);
                await data.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(team);
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult CreateMultiple()
        {

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> CreateMultiple(TeamFormModel team)
        {
            List<Team> teams = new()
            {
                new Team { Name = "Team " + "A", City = "City " + "A", Trener = "Trener " + "A" },
                new Team { Name = "Team " + "B", City = "City " + "B", Trener = "Trener " + "B" },
                new Team { Name = "Team " + "C", City = "City " + "C", Trener = "Trener " + "C" },
                new Team { Name = "Team " + "D", City = "City " + "D", Trener = "Trener " + "D" },
                new Team { Name = "Team " + "E", City = "City " + "E", Trener = "Trener " + "E" },
                new Team { Name = "Team " + "F", City = "City " + "F", Trener = "Trener " + "F" },
                new Team { Name = "Team " + "H", City = "City " + "H", Trener = "Trener " + "H" },
                new Team { Name = "Team " + "G", City = "City " + "G", Trener = "Trener " + "G" },
            };

            var x = teams
                .Take(team.TeamCount)
                .ToList();

            if (this.data.Teams.Count()==0 && team.TeamCount==teams.Count())
            {
               await data.AddRangeAsync(teams);
                await data.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
                return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var team = await data.Teams.FindAsync(id);
            if (team == null) return NotFound();

            return View(team);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, Team team)
        {
            if (id != team.Id) return NotFound();

            if (ModelState.IsValid)
            {
                data.Update(team);
                await data.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(team);
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var team = await data.Teams.FindAsync(id);
            if (team == null) return NotFound();

            return View(team);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var team = await data.Teams.FindAsync(id);
            if (team != null)
            {
                data.Teams.Remove(team);
                await data.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(int id)
        {
            var team = this.data.Teams
                .Where(t => t.Id == id)
                .Select(t => new TeamDetailsViewModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    City = t.City,
                    Trener = t.Trener,
                    Points=t.Points,
                    Wins = t.Wins,
                    Losts = t.Losts,
                    GoalsScored = t.GoalsScored,
                    GoalsConceded = t.GoalsConceded,
                    Draws = t.Draws
                })
                .FirstOrDefault();

            if (team == null)
            {
                return NotFound();
            }

            return View(team);
        }

    }
}