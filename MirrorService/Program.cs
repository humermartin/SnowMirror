using System.ServiceProcess;

namespace MirrorService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new MirrorService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
