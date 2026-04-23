namespace OnForkHub.Persistence.Repositories;

/// <summary>
/// Entity Framework repository implementation for User entity.
/// </summary>
public class UserRepositoryEF(IEntityFrameworkDataContext context) : IUserRepositoryEF
{
    private const string EntityName = nameof(UserEntity);

    private readonly IEntityFrameworkDataContext _context = context;

    /// <inheritdoc/>
    public async Task<RequestResult<UserEntity>> CreateAsync(UserEntity user)
    {
        ArgumentNullException.ThrowIfNull(user);
        try
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return RequestResult<UserEntity>.Success(user);
        }
        catch (DbUpdateException ex)
        {
            var persistenceException = PersistenceExceptionHandler.HandleDbException(ex, "create", EntityName);
            throw persistenceException;
        }
        catch (Exception ex) when (ex is not PersistenceException)
        {
            throw new DatabaseOperationException("create", ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<RequestResult<UserEntity>> DeleteAsync(Id id)
    {
        try
        {
            var user = await EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(_context.Users, u => u.Id == id.ToString());

            if (user == null)
            {
                return RequestResult<UserEntity>.WithError($"{EntityName} not found with ID: {id}.");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RequestResult<UserEntity>.Success(user);
        }
        catch (DbUpdateException ex)
        {
            var persistenceException = PersistenceExceptionHandler.HandleDbException(ex, "delete", EntityName);
            throw persistenceException;
        }
        catch (Exception ex) when (ex is not PersistenceException)
        {
            throw new DatabaseOperationException("delete", ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<RequestResult<IEnumerable<UserEntity>>> GetAllAsync(int page, int size)
    {
        try
        {
            var users = await EntityFrameworkQueryableExtensions.ToListAsync(
                _context.Users.AsNoTracking().OrderByDescending(u => u.CreatedAt).Skip((page - 1) * size).Take(size)
            );

            return RequestResult<IEnumerable<UserEntity>>.Success(users);
        }
        catch (Exception ex)
        {
            return RequestResult<IEnumerable<UserEntity>>.WithError($"Error retrieving {EntityName} list: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<RequestResult<UserEntity>> GetByIdAsync(Id id)
    {
        try
        {
            var user = await EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(_context.Users.AsNoTracking(), u => u.Id == id.ToString());

            return user != null
                ? RequestResult<UserEntity>.Success(user)
                : RequestResult<UserEntity>.WithError($"{EntityName} not found with ID: {id}.");
        }
        catch (Exception ex)
        {
            return RequestResult<UserEntity>.WithError($"Error retrieving {EntityName}: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<RequestResult<UserEntity>> GetByEmailAsync(string email)
    {
        try
        {
            var user = await EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(_context.Users.AsNoTracking(), u => u.Email.Value == email);

            return user != null
                ? RequestResult<UserEntity>.Success(user)
                : RequestResult<UserEntity>.WithError($"{EntityName} not found with email: {email}.");
        }
        catch (Exception ex)
        {
            return RequestResult<UserEntity>.WithError($"Error retrieving {EntityName} by email: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<RequestResult<UserEntity>> UpdateAsync(UserEntity user)
    {
        ArgumentNullException.ThrowIfNull(user);
        try
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return RequestResult<UserEntity>.Success(user);
        }
        catch (DbUpdateException ex)
        {
            var persistenceException = PersistenceExceptionHandler.HandleDbException(ex, "update", EntityName);
            throw persistenceException;
        }
        catch (Exception ex) when (ex is not PersistenceException)
        {
            throw new DatabaseOperationException("update", ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsByEmailAsync(string email)
    {
        try
        {
            return await EntityFrameworkQueryableExtensions.AnyAsync(_context.Users.AsNoTracking(), u => u.Email.Value == email);
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetTotalCountAsync()
    {
        try
        {
            return await EntityFrameworkQueryableExtensions.CountAsync(_context.Users.AsNoTracking());
        }
        catch (Exception)
        {
            return 0;
        }
    }
}
