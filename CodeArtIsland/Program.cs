using SharpDX.Diagnostics;
using System;

namespace OnTheOriginOfFishies
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                if (string.Equals(args[0], "--listadapters", StringComparison.InvariantCultureIgnoreCase))
                {
                    var factory = new SharpDX.DXGI.Factory1();
                    var adapters = factory.Adapters;
                    Console.WriteLine(adapters.Length + " adapters available.");
                    for (int i = 0; i < adapters.Length; i++)
                        Console.WriteLine(i + ": " + adapters[i].Description.Description);
                }
                else
                    Console.WriteLine("Invalid parameters.");
            }
            else if (args.Length == 2)
            {
                if (string.Equals(args[0], "--useadapter", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (!int.TryParse(args[1], out int adapterIndex))
                        Console.WriteLine("Adapter ID must be an integer.");
                    else
                        StartSim(adapterIndex);
                }
                else if (string.Equals(args[0], "--headless", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (!int.TryParse(args[1], out int simID))
                        Console.WriteLine("SimID must be an integer.");
                    else
                    {
                        while (true)
                        {
                            var h = new HeadlessSim(simID);
                            h.UpdateLoop(60 * 60 * 60);
                        }
                        Console.WriteLine("Finished.");
                        Console.ReadKey(true);
                    }
                }
                else
                    Console.WriteLine("Invalid parameters.");
            }
            else
                StartSim(-1);
        }

        private static void StartSim(int adapterIndex)
        {
            using (RenderBase rb = new RenderBase(adapterIndex))
            {
                rb.Run();
            }

            var liveObjs = ObjectTracker.FindActiveObjects();
            Console.WriteLine(liveObjs.Count + " live objects");
            foreach (var o in liveObjs)
            {
                Console.WriteLine(o.ToString());
            }
            //Console.WriteLine(ObjectTracker.ReportActiveObjects());
            //Console.ReadKey(true);
        }
    }
}
