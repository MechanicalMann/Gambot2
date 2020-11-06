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

        /// <summary>
        /// Gets the key-value pair associated with a single-value key.
        /// </summary>
        /// <returns>
        /// The value associated with <paramref name="key" /> if <paramref name="key" /> exists and has only one value associated with it in the data store; otherwise <c>null</c>
        /// </returns>
        Task<DataStoreValue> GetSingle(string key);

        /// <summary>
        /// Adds <paramref name="value" /> to the data store as the only value associated with the given <paramref name="key" />.
        /// If <paramref name="key" /> already has a value associated, that value will be overwritten.
        /// If <paramref name="key" /> has multiple values associated, they will be removed.
        /// </summary>
        /// <returns><c>True</c> if the operation succeeded; otherwise <c>false</c></returns>
        Task<bool> SetSingle(string key, string value);

        /// <summary>
        /// Returns the number of values associated with the given key
        /// <paramref name="key" /> The key to count values for
        /// </summary>
        Task<int> GetCount(string key);

        /// <summary>
        /// Returns whether the given value has been associated with the given key
        /// <param name="key">The key to look for</param>
        /// <param name="value">The value to look for</param>
        /// </summary>
        Task<bool> Contains(string key, string value);
    }
}