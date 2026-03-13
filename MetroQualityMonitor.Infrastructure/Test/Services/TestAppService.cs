using MetroQualityMonitor.Application.Test.Models;
using MetroQualityMonitor.Application.Test.Services;
using MetroQualityMonitor.Domain.Test.Entities;
using MetroQualityMonitor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MetroQualityMonitor.Infrastructure.Test.Services;

public class TestAppService : ITestAppService
{
    private readonly MetroQualityMonitorDbContext _dbContext;

    public TestAppService(MetroQualityMonitorDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<TestEntityDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.TestEntities
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => new TestEntityDto
            {
                Id = x.Id,
                Name = x.Name,
                CreateDateTimeUtc = x.CreateDateTimeUtc
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<TestEntityDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _dbContext.TestEntities
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new TestEntityDto
            {
                Id = x.Id,
                Name = x.Name,
                CreateDateTimeUtc = x.CreateDateTimeUtc
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TestEntityDto> CreateAsync(CreateTestEntityRequest request, CancellationToken cancellationToken)
    {
        var entity = new TestEntity
        {
            Name = request.Name,
            CreateDateTimeUtc = DateTime.UtcNow
        };

        _dbContext.TestEntities.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new TestEntityDto
        {
            Id = entity.Id,
            Name = entity.Name,
            CreateDateTimeUtc = entity.CreateDateTimeUtc
        };
    }
}