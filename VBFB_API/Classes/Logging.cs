namespace VBFB_API.Classes
{
    public static class Logging
    {
        public enum LogLevel { Info, Warning, Error, Fatal }

        public static string FilePath { get; set; } = Path.Combine(AppContext.BaseDirectory, "Logs");

        private static readonly object _lock = new();

        public static void Log(LogLevel level, string message)
        {
            try
            {
                Directory.CreateDirectory(FilePath);
                string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}{Environment.NewLine}";
                lock (_lock)
                {
                    File.AppendAllText(Path.Combine(FilePath, $"VBFB_API_{DateTime.Now:yyyyMMdd}.log"), line);
                }
                Console.Write(line);
            }
            catch
            {
                // logging must never break a request
            }
        }
    }
}
