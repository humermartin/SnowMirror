using MirrorRepository.Data.SnowDbSyncMgnt;
using MirrorRepository.Model.SnowDbSyncMgnt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MirrorWeb.ViewModels.Manage
{
    public class BaseViewModel : BaseModel
    {

        public List<SelectListItem> AllDatabaseSettings()
        {
            var ds = All<DatabaseSettings>();
            return ds.Select(s => new SelectListItem()
            {
                Selected = false,
                Text = s.Instancename,
                Value = s.Id.ToString()
            }).ToList();
        }

        public List<SelectListItem> AllStagingDatabases()
        {
            var ds = All<DatabaseSettings>().Where(d => d.Id != Guid.Parse("E43C8A09-CAFD-4A2C-9F25-99A46AD7FA72"));
            return ds.Select(s => new SelectListItem()
            {
                Selected = false,
                Text = s.Instancename,
                Value = s.Id.ToString()
            }).ToList();
        }

        public List<SelectListItem> AllInstanzSettings()
        {
            var ds = All<InstanzSettings>();
            return ds.Select(s => new SelectListItem()
            {
                Selected = false,
                Text = s.InstanzName,
                Value = s.Id.ToString()
            }).ToList();
        }

        public List<SelectListItem> AllSyncTargets()
        {
            var ds = All<SyncTarget>().OrderByDescending(o => o.Targetname);
            return ds.Select(s => new SelectListItem()
            {
                Selected = false,
                Text = s.Targetname,
                Value = s.Id.ToString()
            }).ToList();
        }

        public List<SelectListItem> AllSynchronizations()
        {
            var ds = All<Synchronization>().OrderBy(o => o.Name);
            return ds.Select(s => new SelectListItem()
            {
                Selected = false,
                Text = s.Name,
                Value = s.Id.ToString()
            }).ToList();
        }
    }
}