using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MirrorRepository.Model;
using System;
using System.Web.Mvc;
using MirrorRepository.Data.SnowDbSyncMgnt;
using System.Linq;
using MirrorRepository;
using MirrorRepository.Enums;
using MirrorRepository.SnowTableApi.TableDefinitions;

namespace MirrorWeb.ViewModels.Manage
{
    public class SynchronizationViewModel : BaseViewModel
    {
        
        /// <summary>
        /// Member which holds the Target values
        /// </summary>
        private List<SelectListItem> _targetItem;

        /// <summary>
        /// the currently viewed Synchronization - or null if new
        /// </summary>
        public Guid? SynchronizationId { get; set; }
        
        /// <summary>
        /// Gets or sets the SnowTable values
        /// </summary>
        public List<SnowTables> SnowTables { get; protected set; } = new List<SnowTables>();

        string _snowTableNames;
        public string SnowTableNames
        {
            get { return _snowTableNames; }
            set
            {
                _snowTableNames = value;
                if (!string.IsNullOrEmpty(_snowTableNames))
                {
                    SnowTables = _snowTableNames.Split(';').ToList().Select(t => new SnowTables() { Name = t }).ToList();
                }
            }
        }

        /// <summary>
        /// Gets or sets the auto schema update value
        /// </summary>
        public bool AutoSchemaUpdate { get; set; }

        /// <summary>
        /// Gets or sets the new sync name
        /// </summary>
        public string SynchronizationName { get; set; }

        public SynchronizationViewModel Init()
        {
            if (SynchronizationId != null)
            {
                var s = FindInternal<Synchronization>(SynchronizationId.Value);


                if (s != null)
                {
                    SynchronizationName = s.Name;
                    if (string.IsNullOrWhiteSpace(s.UsedCoreTables))
                    {
                        SnowTableNames = s.SnowTables;
                    }
                    else
                    {
                        SnowTableNames = s.SnowTables + ";" + s.UsedCoreTables;
                    }
                    
                    AutoSchemaUpdate = s.AutoSchemaUpdate;
                }
            }
            return this;
        }

        /// <summary>
        /// Gets or sets the Target value
        /// </summary>
        public List<SelectListItem> Target
        {
            get => _targetItem = GetTargetFromEnum();
            set => _targetItem = value;
        }

        /// <summary>
        /// Gets or set the selected target
        /// </summary>
        public Guid? SelectedSyncTarget { get; set; }

        public Guid? SelectedDatabaseSettingsId { get; set; }

        public Guid? SelectedInstanzSettingsId { get; set; }

        public List<SelectListItem> SyncTargets { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Synchronizations { get; set; } = new List<SelectListItem>();

        public List<SelectListItem> DatabaseSettings { get; set; } = new List<SelectListItem>();

        public List<SelectListItem> InstanzSettings { get; set; } = new List<SelectListItem>();
        
        public List<SnowObject> TableSyncList { get; set; }

        public int TableSyncListTotalCount { get; set; }

        public string SelectedTable { get; set; }

        public List<SelectListItem> TableList
        {
            get
            {
                List<SelectListItem> selTables = new List<SelectListItem>();
                foreach (var table in SnowTables)
                {
                    SelectListItem tableItem = new SelectListItem {Value = table.Name, Text = table.Name};
                    selTables.Add(tableItem);
                }

                if (selTables.Any())
                {
                    selTables.Insert(0, new SelectListItem { Value = string.Empty, Text = string.Empty }); 
                }
                return selTables;
            } 

        }

        /// <summary>
        /// flag tables which are already used in other synchronization
        /// </summary>
        /// <param name="model"></param>
        /// <param name="selSynchronizationId"></param>
        /// <returns></returns>
        public SynchronizationViewModel GetUsedTablesInSynchronizations(SynchronizationViewModel model, Guid selSynchronizationId)
        {
            using (ServiceNowDbSyncMgntEntities entities = new ServiceNowDbSyncMgntEntities())
            {
                //check if table is already in sync
                var tbls = entities.Synchronization.Where(s => s.Id != selSynchronizationId &&
                                                               s.DatabaseSettingsId ==
                                                               model.SelectedDatabaseSettingsId &&
                                                               s.InstanzSettingsId == model.SelectedInstanzSettingsId).ToList();
                Dictionary<string, List<string>> tablesFromInstanceAndDatabase = tbls.ToDictionary(a => a.Name, b => b.SnowTables.Split(';').ToList());

                List<string> colUsedTables = new List<string>();
                foreach (var tblValues in tablesFromInstanceAndDatabase)
                {
                    foreach (var table in tblValues.Value)
                    {
                        if (!colUsedTables.Any(t => t.ToLower().Equals(table.ToLower())))
                        {
                            colUsedTables.Add(table);
                        }
                    }
                }
                
                foreach (var usedTable in colUsedTables)
                {
                    var syncNames = tablesFromInstanceAndDatabase.Where(s => s.Value.Any(t => t.ToLower().Equals(usedTable))).ToList();
                    
                    string syncNameList = string.Join(" - ", syncNames.Select(x => x.Key).ToArray());

                    SnowObject snowTable = model.TableSyncList.FirstOrDefault(t => usedTable.Equals(t.TableName));

                    if (snowTable != null)
                    {
                        snowTable.UsedInOtherSyncList = syncNameList;
                        snowTable.UsedInOtherSync = true;
                    }

                }
                
            }
               
            return model;
        }

        public List<SnowTables> GetColumnRestricedTables()
        {
            var tableColList = SnowTables;

            using (ServiceNowDbSyncMgntEntities ctx = new ServiceNowDbSyncMgntEntities())
            {
               var syncRecord = ctx.Synchronization.FirstOrDefault(i => i.Id == SynchronizationId);
             
               foreach (var table in tableColList)
               {
                   var tableDef = ctx.SnowTableDefinition.FirstOrDefault(d => d.InstanceId == syncRecord.InstanzSettingsId && d.Table == table.Name);
                   if (tableDef != null && !string.IsNullOrWhiteSpace(tableDef.Columns))
                   {
                       table.HasColumnRestriction = true;
                   }
               }

               tableColList.Insert(0, new SnowTables());
            }
            
            return tableColList;
        }

        /// <summary>
        /// Pass Target enum to SelectListItem
        /// </summary>
        /// <returns></returns>
        private List<SelectListItem> GetTargetFromEnum()
        {
            List<SelectListItem> targetItem = Enum.GetValues(typeof(EnumTarget)).Cast<EnumTarget>().Select(v => new SelectListItem
            {
                Text = v.ToString(),
                Value = ((int)v).ToString()
            }).ToList();

            return targetItem;
        }
    }
}