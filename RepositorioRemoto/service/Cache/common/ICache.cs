using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace RepositorioRemoto.Cache.Common;

public interface ICache<T>
{
    Task<Result<T, DomainError>> GetAsync(string key);

    Task<Result<bool, DomainError>> SetAsync(
        string key,
        T value,
        TimeSpan? expiration = null);

    Task<Result<bool, DomainError>> RemoveAsync(string key);

    Task<Result<bool, DomainError>> ClearAsync();
}