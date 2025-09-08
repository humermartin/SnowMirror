using MirrorWeb.ViewModels.Manage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MirrorRepository.Data.SnowDbSyncMgnt;

namespace MirrorWeb.ViewModels
{
    public class DatabaseSettingsViewModel : BaseViewModel
    {
        public string Id { get; set; }
        public string Servername { get; set; }
        public string Port { get; set; }
        public string Instancename { get; set; }
        public string Databasename { get; set; }
        public string Schemaname { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        
        public string ConnOk { get; set; }

        public List<SelectListItem> Databases { get; set; } = new List<SelectListItem>();

        public List<DatabaseSettings> DatabaseList = new List<DatabaseSettings>();
        
        public int DatabaseListTotalCount { get; set;}
      
    }
}