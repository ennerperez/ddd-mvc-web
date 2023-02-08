using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.Configuration;
using Persistence.Interfaces;

namespace Persistence.Providers
{
	public sealed class MemoryCacheProvider : ICacheProvider, IDisposable
	{
		private readonly long _defaultExpiration;
		private readonly Timer _timer;

		private Dictionary<string, object> _cache;
		private Dictionary<string, DateTime> _expires;
		public MemoryCacheProvider(IConfiguration configuration)
		{
			_cache = new Dictionary<string, object>();
			_expires = new Dictionary<string, DateTime>();
			_defaultExpiration = configuration.GetValue<long?>("AppSettings:CacheControl") ?? 21600;
			_timer = new Timer(_defaultExpiration);
			_timer.Elapsed += TimerOnElapsed;
			_timer.Start();
		}

		private bool _isTimerRunning = false;
		private void TimerOnElapsed(object sender, ElapsedEventArgs e)
		{
			if (_isTimerRunning) return;
			_isTimerRunning = true;
			foreach (var key in _expires.Where(m => (DateTime.Now > m.Value)))
				InvalidateCache(key.Key);
			_isTimerRunning = false;
		}
		public T GetObjectFromCache<T>(string key, Func<T> nullValueCallback, long? expires = null) where T : class
		{
			try
			{
				if (_cache.ContainsKey(key))
				{
					var date = _expires[key];
					if (DateTime.Now > date) { InvalidateCache(key); }
					else
					{
						var data = (T)_cache[key];
						if (data != null)
							return data;
					}
				}

				return AddToCache(key, nullValueCallback(), expires);
			}
			catch
			{
				return nullValueCallback();
			}
		}
		public async Task<T> GetObjectFromCacheAsync<T>(string key, Func<T> nullValueCallback, long? expires = null) where T : class
		{
			try
			{
				if (_cache.ContainsKey(key))
				{
					var date = _expires[key];
					if (DateTime.Now > date)
					{
						InvalidateCache(key);
					}
					else
					{
						var data = (T)_cache[key];
						if (data != null)
							return data;
					}
				}

				return await AddToCacheAsync(key, nullValueCallback(), expires);
			}
			catch
			{
				return nullValueCallback();
			}
		}

		public Task<bool> KeyExistsAsync<T>(string key, Func<T> nullValueCallback, long? expires = null) where T : class
		{
			bool result;
			try
			{
				result = _cache.ContainsKey(key);
			}
			catch
			{
				result = false;
			}
			return Task.FromResult(result);
		}
		public bool KeyExists<T>(string key, Func<T> nullValueCallback, long? expires = null) where T : class
		{
			try
			{
				return _cache.ContainsKey(key);
			}
			catch
			{
				return false;
			}
		}
		public bool KeyExists(string key)
		{
			throw new NotImplementedException();
		}

		public void InvalidateCache(string key)
		{
			try
			{
				if (_cache.ContainsKey(key))
				{
					if (_cache[key].GetType().IsAssignableTo(typeof(IDisposable)))
						(_cache[key] as IDisposable)?.Dispose();
					_cache.Remove(key);
					_expires.Remove(key);
				}
				else
					throw new Exception($"Could not invalidate cache object: {key}");
			}
			catch (Exception)
			{
				// Ignore
			}
		}
		public void InvalidateCache(IEnumerable<string> keys)
		{
			foreach (var key in keys)
				InvalidateCache(key);
		}
		public void InvalidateCachePattern(string pattern)
		{
			throw new NotImplementedException();
		}

		private Task<T> AddToCacheAsync<T>(string key, T value, long? expires = null)
		{
			_cache.TryAdd(key, value);
			_expires.TryAdd(key, DateTime.Now.AddSeconds(expires ?? _defaultExpiration));
			return Task.FromResult(value);
		}
		private T AddToCache<T>(string key, T value, long? expires = null)
		{
			_cache.TryAdd(key, value);
			_expires.TryAdd(key, DateTime.Now.AddSeconds(expires ?? _defaultExpiration));

			return value;
		}
		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				_cache = null;
				_expires = null;
				_timer.Stop();
				_timer.Dispose();
			}
		}
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		~MemoryCacheProvider()
		{
			Dispose(false);
		}
	}
}
