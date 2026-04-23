namespace OnForkHub.Persistence.Repositories;

using OnForkHub.Core.Entities;
using OnForkHub.Core.Interfaces.Repositories;
using OnForkHub.Persistence.Contexts.Base;
using OnForkHub.Persistence.Exceptions;

/// <summary>
/// Entity Framework repository implementation for VideoUpload entity.
/// </summary>
public class VideoUploadRepositoryEF(IEntityFrameworkDataContext context) : IVideoUploadRepository
{
    private const string EntityName = nameof(VideoUpload);

    private readonly IEntityFrameworkDataContext _context = context;

    /// <inheritdoc/>
    public async Task<RequestResult<VideoUpload>> AddAsync(VideoUpload upload)
    {
        ArgumentNullException.ThrowIfNull(upload);
        try
        {
            _context.VideoUploads.Add(upload);
            await _context.SaveChangesAsync();
            return RequestResult<VideoUpload>.Success(upload);
        }
        catch (DbUpdateException ex)
        {
            var persistenceException = PersistenceExceptionHandler.HandleDbException(ex, "create", EntityName);
            throw persistenceException;
        }
    }

    /// <inheritdoc/>
    public async Task<RequestResult<VideoUpload>> GetByIdAsync(Guid id)
    {
        try
        {
            var upload = await EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
                _context.VideoUploads.AsNoTracking(),
                x => x.Id == id.ToString()
            );

            return upload != null
                ? RequestResult<VideoUpload>.Success(upload)
                : RequestResult<VideoUpload>.WithError($"{EntityName} not found with ID: {id}.");
        }
        catch (Exception ex)
        {
            return RequestResult<VideoUpload>.WithError($"Error retrieving {EntityName}: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<RequestResult<VideoUpload>> UpdateAsync(VideoUpload upload)
    {
        ArgumentNullException.ThrowIfNull(upload);
        try
        {
            _context.VideoUploads.Update(upload);
            await _context.SaveChangesAsync();
            return RequestResult<VideoUpload>.Success(upload);
        }
        catch (DbUpdateException ex)
        {
            var persistenceException = PersistenceExceptionHandler.HandleDbException(ex, "update", EntityName);
            throw persistenceException;
        }
    }

    /// <inheritdoc/>
    public async Task<RequestResult<IEnumerable<VideoUpload>>> GetByUserIdAsync(string userId, int page, int pageSize)
    {
        try
        {
            var uploads = await EntityFrameworkQueryableExtensions.ToListAsync(
                _context
                    .VideoUploads.AsNoTracking()
                    .Where(x => x.UserId == userId)
                    .OrderByDescending(x => x.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
            );

            return RequestResult<IEnumerable<VideoUpload>>.Success(uploads);
        }
        catch (Exception ex)
        {
            return RequestResult<IEnumerable<VideoUpload>>.WithError($"Error retrieving {EntityName} list: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetCountByUserIdAsync(string userId)
    {
        try
        {
            return await EntityFrameworkQueryableExtensions.CountAsync(_context.VideoUploads.AsNoTracking().Where(x => x.UserId == userId));
        }
        catch (Exception)
        {
            return 0;
        }
    }
}
