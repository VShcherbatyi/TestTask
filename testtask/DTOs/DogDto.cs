using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace testtask.DTOs
{
    public class DogDto
    {
        [Required]
        public required string Name { get; set; }
        [Required]
        public required string Color { get; set; }
        [JsonPropertyName("tail_length")]
        [Range(1, int.MaxValue, ErrorMessage = "Tail length must be greater than 0")]
        public int TailLength { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Weight must be greater than 0")]
        public int Weight { get; set; }
    }
}
