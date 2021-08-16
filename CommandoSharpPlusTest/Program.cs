using System.Runtime.Loader;
using System.Threading.Tasks;

namespace CommandoSharpPlusTest
{
    public static class Program
    {
        private static async Task Main()
        {
            var bot = new CommandSharpPlusTestBot();
            await bot.Run();
            await Task.Delay(-1);
        }
    }
}