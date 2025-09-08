using Newtonsoft.Json;

namespace MirrorRepository.NotificationHelper
{
    public class NotificationSettings
    {
        [JsonProperty("MailSendEnabled")]
        public bool MailSendEnabled { get; set; }

        [JsonProperty("MailServer")]
        public string MailServer { get; set; }

        [JsonProperty("MailServerPort")]
        public int MailServerPort { get; set; }

        [JsonProperty("UseSsl")]
        public bool UseSsl { get; set; }

        [JsonProperty("MailBoxUserName")]
        public string MailBoxUserName { get; set; }

        [JsonProperty("MailBoxSender")]
        public string MailBoxSender { get; set; }

        [JsonProperty("SmtpUserName")]
        public string SmtpUserName { get; set; }

        [JsonProperty("SmtpPassword")]
        public string SmtpPassword { get; set; }
    }
}
