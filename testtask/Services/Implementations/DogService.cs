using Microsoft.EntityFrameworkCore;
using System.Data;
using testtask.Data;
using testtask.DTOs;
using testtask.Models;
using testtask.Services.Interfaces;

namespace testtask.Services.Implementations
{
    public class DogService : IDogService
    {
        private readonly AppDbContext _context;

        public DogService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DogDto>> GetDogsAsync(string attribute, string order, int? pageNumber, int? pageSize)
        {
            var query = _context.Dogs.AsQueryable();
            if (!string.IsNullOrEmpty(attribute) && !string.IsNullOrEmpty(order)) //attribute and order
            {
                var normalizedAttribute = char.ToUpper(attribute[0]) + attribute.Substring(1);
                var property = typeof(DogDto).GetProperty(normalizedAttribute);
                if (property == null) { throw new ArgumentException("Invalid attribute name", nameof(attribute)); }

                if (order == "asc") { query = query.OrderBy(d => EF.Property<object>(d, property.Name)); }
                else if (order == "desc") { query = query.OrderByDescending(d => EF.Property<object>(d, property.Name)); }
                else { throw new ArgumentException("Invalid order name", nameof(order)); }
            }
            else if (!string.IsNullOrEmpty(attribute) || !string.IsNullOrEmpty(order)) { throw new ArgumentException("Invalid sorting inputs"); }

            if (pageNumber.HasValue && pageSize.HasValue) //pageNumber and pageSize
            {
                if (pageNumber <= 0 || pageSize <= 0) { throw new ArgumentOutOfRangeException(null, "Invalid pageNumber or/and pageSize"); }
                query = query.Skip(pageSize.Value * (pageNumber.Value - 1)).Take(pageSize.Value);
            }
            else if (pageNumber.HasValue || pageSize.HasValue) { throw new ArgumentException("Invalid paging inputs"); }

            var dogs = await query.ToListAsync();
            return dogs.Select(d => new DogDto
            {
                Name = d.Name,
                Color = d.Color,
                TailLength = d.TailLength,
                Weight = d.Weight
            });
        }

        public async Task CreateDogAsync(DogDto dto)
        {
            var nameTaken = await _context.Dogs.FirstOrDefaultAsync(d => d.Name == dto.Name);
            if (nameTaken != null)
            {
                throw new DuplicateNameException("Name is already taken");
            }
            var dog = new Dog
            {
                Name = dto.Name,
                Color = dto.Color,
                TailLength = dto.TailLength,
                Weight = dto.Weight
            };
            _context.Dogs.Add(dog);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new DuplicateNameException("Name is already taken", ex);
            }
        }
    }
}
