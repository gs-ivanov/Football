namespace Tournament.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Threading.Tasks;
    using Tournament.Data;
    using Tournament.Data.Models;

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
            var teams = await data.Teams.OrderByDescending(t => t.Points).ToListAsync();
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
    }
}