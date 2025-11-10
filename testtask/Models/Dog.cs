namespace testtask.Models
{
    public class Dog
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Color { get; set; }
        public int TailLength { get; set; }
        public int Weight { get; set; }
    }
}
