using testtask.DTOs;

namespace testtask.Services.Interfaces
{
    public interface IDogService
    {
        Task<IEnumerable<DogDto>> GetDogsAsync(string attribute, string order, int? pageNumber, int? pageSize);
        Task CreateDogAsync(DogDto dto);
    }
}
