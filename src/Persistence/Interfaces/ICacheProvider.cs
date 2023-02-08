using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Persistence.Interfaces
{
	public interface ICacheProvider
	{
		Task<T> GetObjectFromCacheAsync<T>(string key, Func<T> nullValueCallback, long? expires = null) where T : class;
		Task<bool> KeyExistsAsync<T>(string key, Func<T> nullValueCallback, long? expires = null) where T : class;
		T GetObjectFromCache<T>(string key, Func<T> nullValueCallback, long? expires = null) where T : class;
		bool KeyExists<T>(string key, Func<T> nullValueCallback, long? expires = null) where T : class;
		bool KeyExists(string key);
		void InvalidateCache(string key);
		void InvalidateCache(IEnumerable<string> keys);
		void InvalidateCachePattern(string pattern);
	}
}
