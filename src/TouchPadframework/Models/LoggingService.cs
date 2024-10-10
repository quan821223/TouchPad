using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TouchPadframework.Models
{
    public class LoggingService : ILogger
    {
        public LogLevel Level { get; set; }

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= Level;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (IsEnabled(logLevel))
            {
                var message = formatter(state, exception);
                // 在這裡輸出日誌，比如寫入控制台或檔案
                Console.WriteLine($"[{logLevel}] {message}");
            }
        }
    }
}
