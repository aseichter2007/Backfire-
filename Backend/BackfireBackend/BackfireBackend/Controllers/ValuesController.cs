using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BackfireBackend.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BackfireBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        
        // GET: api/<ValuesController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "not", "used" };
        }

        // GET api/<ValuesController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "not used";
        }

        // POST api/<ValuesController>/routing
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PostedData file)
        {
            //If this takes off I need a semaphore or an SSD to keep the disk from write saturating.
            if (!ModelState.IsValid||file.file==null||file.file=="")
            {
                return BadRequest(ModelState);
            }

            string currentLocation = Directory.GetCurrentDirectory();
            string fileRoute ="\\"+ file.make+"\\"+file.model+"\\"+file.year+"\\"+file.fix+"\\";
            int fileNumber = 0;
            while (System.IO.File.Exists(currentLocation+fileRoute+fileNumber+".wav"))
            {
                fileNumber++;
            }

            System.IO.File.WriteAllText(currentLocation + fileRoute + fileNumber + ".wav", file.file);


            return Ok();
        }
    }
}
