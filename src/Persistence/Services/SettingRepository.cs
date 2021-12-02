using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;
using Persistence.Interfaces;

namespace Persistence.Services
{
    public class SettingRepository : GenericRepository<Setting>, ISettingRepository
    {
        public SettingRepository(DefaultContext context, ILoggerFactory logger, IConfiguration configuration) : base(context, logger, configuration)
        {
        }

        public string GetValue(string key)
        {
            return _dbSet.FirstOrDefault(m => m.Key == key)?.Value;
        }

        public TValue GetValue<TValue>(string key) where TValue : struct
        {
            var currencyCulture = new CultureInfo(Configuration["CultureInfo:CurrencyCulture"] ?? CultureInfo.CurrentCulture.Name);
            object value = _dbSet.FirstOrDefault(m => m.Key == key)?.Value;
            if (value != null) return (TValue)Convert.ChangeType(value, typeof(TValue), currencyCulture);
            return default;
        }

        public async Task<string> GetValueAsync(string key)
        {
            return await _dbSet.Where(m => m.Key == key).Select(m => m.Value).FirstOrDefaultAsync();
        }

        public async Task<TValue> GetValueAsync<TValue>(string key) where TValue : struct
        {
            var currencyCulture = new CultureInfo(Configuration["CultureInfo:CurrencyCulture"] ?? CultureInfo.CurrentCulture.Name);
            object value = await _dbSet.Where(m => m.Key == key).Select(m => m.Value).FirstOrDefaultAsync();
            if (value != null) return (TValue)Convert.ChangeType(value, typeof(TValue), currencyCulture);
            return default;
        }
    }
}
