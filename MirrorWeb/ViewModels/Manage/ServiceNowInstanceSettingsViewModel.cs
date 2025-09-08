using MirrorRepository.Model.SnowDbSyncMgnt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MirrorWeb.ViewModels.Manage
{
    public class ServiceNowInstanceSettingsViewModel : BaseViewModel
    {
        public string Id { get; set; }
        public string InstanceName { get; set; }
        public string UserName { get; set; }
        public string PW { get; set; }
        public string Servername { get; set; }
        public string Port { get; set; }
        public string ProxyPort { get; set; }
        public string ProxyHost { get; set; }
        public string ProxyUser { get; set; }
        public string ProxyPW { get; set; }

        public List<SelectListItem> Instances { get; set; } = new List<SelectListItem>();
    }
}