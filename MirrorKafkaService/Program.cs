using System.ServiceProcess;

namespace MirrorKafkaService
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
                new MirrorKafkaService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
