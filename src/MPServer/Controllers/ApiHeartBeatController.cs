using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MPServer.Data;
using MPServer.Models;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace MPServer.Controllers
{
    [Authorize]
    [Route("api/HeartBeat")]
    public class ApiHeartBeatController : Controller
    {
        private readonly AppDbContext _database;

        public ApiHeartBeatController(AppDbContext db)
        {
            _database = db;
        }

        [HttpGet]
        [Authorize(Roles = "ViewHeartBeat")]
        public async Task<IActionResult> Get(int? page)
        {
            var result = _database.HeartBeat.AsQueryable();
            result = result.OrderByDescending(t => t.HeartBeatTime);
            var count = await result.CountAsync();
            if (count == 0) return Ok(new ApiListModel<HeartBeat>(1, 1, new List<HeartBeat>()));
            var maxPage = count/Variables.ItemPerPage + 1;
            var currentPage = Utils.ProcessInvalidPages(page, maxPage);
            return Ok(new ApiListModel<HeartBeat>(maxPage, currentPage, result));
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "ViewHeartBeat")]
        public async Task<IActionResult> Get(int id)
        {
            var result = _database.HeartBeat.Where(t => t.HeartBeatId == id);
            var count = await result.CountAsync();
            if (count == 0) return NotFound();
            return Ok(await result.FirstAsync());
        }

        [HttpPost]
        [Authorize(Roles = "SendHeartBeat")]
        public async Task<IActionResult> Post([FromBody] HeartBeatModel heartBeatModel)
        {
            if (ModelState.IsValid)
            {
                var heartBeat = new HeartBeat
                {
                    Device = heartBeatModel.Device,
                    HeartBeatTime = DateTime.UtcNow,
                    IPAddress = HttpContext.Connection.RemoteIpAddress.ToString()
                };
                _database.Entry(heartBeat).State = EntityState.Added;
                await _database.SaveChangesAsync();
            }
            return BadRequest(ModelState);
        }
    }
}
