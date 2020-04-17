using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gambot.Data
{
    public interface IDataStore
    {
        /// <summary>
        /// Adds <paramref name="value"/> to the data store and associates it with <paramref name="key"/>.
        /// </summary>
        /// <returns><c>true</c> if the operation succeeded; <c>false</c> otherwise (such as when the value already exists in the data store).</returns>
        Task<bool> Add(string key, string value);

        /// <summary>
        /// Removes all values associated with <paramref name="key"/>.
        /// </summary>
        /// <returns>The number of values removed.</returns>
        Task<int> RemoveAll(string key);

        /// <summary>
        /// Removes <paramref name="value"/> from <paramref name="key"/>.
        /// </summary>
        /// <returns><c>true</c> if the operation succeeded; <c>false</c> otherwise (such as when the value does not exist in the data store).</returns>
        Task<bool> Remove(string key, string value);

        /// <summary>
        /// Removes the key-value pair with the specified ID.
        /// </summary>
        /// <returns><c>true</c>, if the operation succeeded; <c>false</c> otherwise.</returns>
        /// <param name="id">ID of the key-value pair.</param>
        Task<bool> Remove(int id);

        /// <summary>
        /// Gets all keys from the data store.
        /// </summary>
        Task<IEnumerable<string>> GetAllKeys();

        /// <summary>
        /// Gets all values associated with <paramref name="key"/>.
        /// </summary>
        Task<IEnumerable<DataStoreValue>> GetAll(string key);

        /// <summary>
        /// Gets a random value associated with <paramref name="key"/>.
        /// </summary>
        /// <returns>A random value associated with <paramref name="key"/> if <paramref name="key"/> exists as a key in the data store; <c>null</c> otherwise.</returns>
        Task<DataStoreValue> GetRandom(string key);

        /// <summary>
        /// Gets a random value associated with a random key.
        /// </summary>
        Task<DataStoreValue> GetRandom();

        /// <summary>
        /// Gets the key-value pair associated with the given ID.
        /// </summary>
        /// <param name="id">ID of the key-value pair.</param>
        Task<DataStoreValue> Get(int id);
    }
}