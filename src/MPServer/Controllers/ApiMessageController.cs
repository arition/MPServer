using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MPServer.Data;
using MPServer.Models;
using Newtonsoft.Json;
using System.Reflection;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace MPServer.Controllers
{
    [Authorize]
    [Route("api/heartBeat")]
    public class ApiMessageController : Controller
    {
        private readonly AppDbContext _database;

        public ApiMessageController(AppDbContext db)
        {
            _database = db;
        }

        [HttpGet("type")]
        [Authorize(Roles = "ViewMessage")]
        public IActionResult GetTypeDef()
        {
            return Ok(typeof(Message).GetProperties()
                .Where(t => t.GetCustomAttribute<JsonIgnoreAttribute>() == null &&
                            t.GetCustomAttribute<DisplayAttribute>() != null &&
                            !t.Name.Contains("Id"))
                .Select(x => new
                {
                    name = char.ToLowerInvariant(x.Name[0]) + x.Name.Substring(1),
                    display = x.GetCustomAttribute<DisplayAttribute>().Name,
                    required = x.GetCustomAttribute<RequiredAttribute>() != null,
                    type =
                    x.PropertyType.IsGenericParameter && x.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)
                        ? Nullable.GetUnderlyingType(x.PropertyType).Name
                        : x.PropertyType.Name
                }));
        }

        [HttpGet]
        [Authorize(Roles = "ViewMessage")]
        public async Task<IActionResult> Get(int? page)
        {
            var result = _database.Message.AsQueryable();
            result = result.OrderByDescending(t => t.MessageTime);
            var count = await result.CountAsync();
            if (count == 0) return Ok(new ApiListModel<Message>(1, 1, new List<Message>()));
            var maxPage = count/Variables.ItemPerPage + 1;
            var currentPage = Utils.ProcessInvalidPages(page, maxPage);
            result = result.Skip((currentPage - 1)*Variables.ItemPerPage).Take(Variables.ItemPerPage);
            return Ok(new ApiListModel<Message>(maxPage, currentPage, result));
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "ViewMessage")]
        public async Task<IActionResult> Get(int id)
        {
            var result = _database.Message.Where(t => t.MessageId == id);
            var count = await result.CountAsync();
            if (count == 0) return NotFound();
            return Ok(await result.FirstAsync());
        }

        [HttpPost]
        [Authorize(Roles = "SendMessage")]
        public async Task<IActionResult> Post([FromBody] Message messageModel)
        {
            if (ModelState.IsValid)
            {
                
                _database.Entry(messageModel).State = EntityState.Added;
                await _database.SaveChangesAsync();
                return Ok();
            }
            return BadRequest(ModelState);
        }
    }
}
