using MetroQualityMonitor.Application.Test.Models;

namespace MetroQualityMonitor.Application.Test.Services;

public interface ITestAppService
{
    Task<IReadOnlyCollection<TestEntityDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<TestEntityDto?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<TestEntityDto> CreateAsync(CreateTestEntityRequest request, CancellationToken cancellationToken);
}