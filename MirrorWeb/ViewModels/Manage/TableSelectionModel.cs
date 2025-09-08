using MirrorWeb.Models;
using System.Collections.Generic;
using MirrorRepository.Model;

namespace MirrorWeb.ViewModels.Manage
{
    public class TableSelectionViewModel
    {
        public List<SnowTables> SnowTables { get; set; } = new List<SnowTables>();
        
        public List<SnowTables> SnowColumns { get; set; } = new List<SnowTables>();

        public SyncSettingModel SyncSettingModel { get; set; } = new SyncSettingModel();

    }
}