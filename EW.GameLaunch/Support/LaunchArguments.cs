
namespace EW.Support
{
    public class LaunchArguments
    {
        public string Connect;

        public string URI;

        public string Replay;

        public bool Benchmark;



        public LaunchArguments(Arguments args)
        {
            if (args == null)
                return;

            foreach (var f in GetType().GetFields())
                if (args.Contains("Launch" + "." + f.Name))
                    FieldLoader.LoadField(this, f.Name, args.GetValue("Launch" + "." + f.Name, ""));
        }

        public string GetConnectAddress()
        {
            var connect = string.Empty;
            return connect;
        }
    }
}