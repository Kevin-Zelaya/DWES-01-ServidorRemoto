using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace RepositorioRemoto.Cache.Common;

public interface ICache<T>
{
    Task<T?> GetAsync(string key);

    Task SetAsync(string key, T value, TimeSpan? expiration = null);

    Task RemoveAsync(string key);
}