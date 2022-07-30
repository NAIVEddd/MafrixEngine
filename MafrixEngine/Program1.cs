using Serilog;

namespace TmpNameSpace
{
    public class Tmpclass
    {
        public static void Tmpfunc()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("MafrixLogfile.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Console.WriteLine("Hello, World!");
            var logname = "MafrixEngine";
            Log.Debug($"Logger name is: {logname}");
        }
    }
}
