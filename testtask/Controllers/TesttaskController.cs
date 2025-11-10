using Microsoft.AspNetCore.Mvc;
using testtask.DTOs;
using testtask.Services.Interfaces;

namespace testtask.Controllers
{
    [ApiController]
    public class TesttaskController : ControllerBase
    {
        private readonly IDogService _dogService;

        public TesttaskController(IDogService dogService)
        {
            _dogService = dogService;
        }

        [HttpGet("ping")]
        public string Ping()
        {
            return "Dogshouseservice.Version1.0.1";
        }

        [HttpGet("dogs")]
        public async Task<IActionResult> GetDogs(string attribute = "", string order = "", int? pageNumber = null, int? pageSize = null)
        {
            var dogs = await _dogService.GetDogsAsync(attribute, order, pageNumber, pageSize);
            return Ok(dogs);
        }

        [HttpPost("dog")]
        public async Task<IActionResult> CreateDog(DogDto dto)
        {
            await _dogService.CreateDogAsync(dto);
            return Created();
        }
    }
}
