using System.Configuration;
using System.Web.Configuration;

namespace MirrorWeb
{
    public class WebConfig
    {

        public static void Register()
        {
            EncryptConnString();
        }

        /// <summary>
        /// Encrypt web.config sections
        /// </summary>
        private static void EncryptConnString()
        {
            Configuration config = WebConfigurationManager.OpenWebConfiguration("/");
            ConfigurationSection appSection = config.GetSection("appSettings");

            if (!appSection.SectionInformation.IsProtected)
            {
                appSection.SectionInformation.ProtectSection("RsaProtectedConfigurationProvider");
                config.Save();
            }
        }

    }
}