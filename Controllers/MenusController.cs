using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plants.info.API.Data.Models;
using Plants.info.API.Data.Services;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Plants.info.API.Controllers
{
    [Route("api/menus/")] // Base url for all menus. 
    [Authorize] //we have no way to pass down id values to this policy, therefore a custom attribute needs to be created but it is not recommended.
    [ApiController]
    public class MenusController : Controller
    {
        private readonly IMenusRepository _menusRepository;

        public MenusController(IMenusRepository menusRepository)
        {
            _menusRepository = menusRepository;
        }
        [HttpGet("genus")]
        // GET: /<controller>/
        public async Task<ActionResult<IEnumerable<Genus>>> getGenusList()
        {
           var genusList =  await _menusRepository.getGenusList();

            if (genusList == null) return NotFound();

            return Ok(genusList); 
        }
        [HttpGet("genus/{genusId}")]
        // GET: /<controller>/
        public async Task<ActionResult<string>> getGenusList(int genusId)
        {
            var genus = await _menusRepository.getGenusById(genusId); 

            if (genus == null) return NotFound();

            return Ok(genus.Name);
        }
    } 
}

