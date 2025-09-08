using log4net;
using Newtonsoft.Json;
using MirrorRepository.Data.SnowDbSyncMgnt;
using MirrorRepository.Model;
using MirrorRepository.Model.SyncParams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MirrorRepository.Helpers
{
    public class TableHierarchy
    {
        protected readonly ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        public TableHierarchy(string tableName, string myTag, SnowDbContext ctx, Guid syncId)
        {
            DerivedFromParent = false;
            IsDerived(tableName, myTag, ctx, syncId);
        }
        public bool DerivedFromParent { get; private set; }
        public bool InheritanceTableSyncEnabled { get; set; }
        public string TableName { get; private set; }
        public string ParentTable { get; private set; }
        public string ChildTableFilter { get; private set; }

        public List<string> SysIDs { get; set; }

        public override string ToString()
        {
            return string.Format("TabHier: derived={0} for: {1}/{2}, filter={3}", DerivedFromParent, TableName, ParentTable, ChildTableFilter);
        }

        /// <summary>
        /// handle inhertied tablesyncs defined in appsettings
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="myTag"></param>
        /// <param name="ctx"></param>
        /// <param name="syncId"></param>
        /// <returns></returns>
        public bool IsDerived(string tableName, string myTag, SnowDbContext ctx, Guid syncId)
        {
            AppSettingsModel appSettingsModel = new AppSettingsModel().GetAppSettingsModel();
             
            var snowInheritTableModel = appSettingsModel.InheritanceSettings;
            if (snowInheritTableModel == null)
            {
                return false;
            }

            var tblInheritanceEnabled = TableInheritanceEnabled(tableName, syncId);
            InheritanceTableSyncEnabled = tblInheritanceEnabled == true;

            foreach (var pcModel in snowInheritTableModel.Where(p => p.SnowTableChildren.Count > 0))
            {
                var child = pcModel.SnowTableChildren.FirstOrDefault(c => c.TableName.Equals(tableName));
                if (child != null)
                {
                    Log.Info(InheritanceTableSyncEnabled
                        ? $"{myTag}: found derived. inheritance table sync is enabled : {pcModel.TableName}:{child.TableName} for: {tableName}"
                        : $"{myTag}: found derived. inheritance table sync is disabled : {pcModel.TableName}:{child.TableName} for: {tableName}");

                    DerivedFromParent = true;
                    this.TableName = tableName;
                    ParentTable = pcModel.TableName;
                    using (var sCtx = ctx.SyncedContext)
                    {
                        ChildTableFilter = (from c in sCtx.cmdb where TableName == c.SysClassName select c.SysClassPath)
                                           .Take(1).FirstOrDefault();
                        DerivedFromParent = ChildTableFilter != null;

                        if (ChildTableFilter != null)
                        {

                            var query = from c in sCtx.cmdb
                                        where c.SysClassPath.StartsWith(ChildTableFilter)
                                        orderby c.SysUpdatedOn, c.SysId
                                        select c.SysId;
                            SysIDs = query.ToList();
                        }
                        if (!DerivedFromParent)
                        {
                            Log.Info($"{myTag}: failed to find Filter for : {pcModel.TableName}:{child.TableName} for: {tableName}");
                        }
                        else
                        {
                            Log.Info(InheritanceTableSyncEnabled
                                ? $"{myTag}: found derived. Inheritance table sync is enabled : {pcModel.TableName}:{child.TableName} for: {tableName}"
                                : $"{myTag}: found derived. Inheritance table sync is disabled : {pcModel.TableName}:{child.TableName} for: {tableName}");
                        }
                    }
                    return DerivedFromParent;
                }
                Log.Info($"{myTag}: not derived in : {pcModel.TableName}:{String.Join(",", pcModel.SnowTableChildren.Select(c => c.TableName))} for: {tableName}");
            }
            return false;
        }

        /// <summary>
        /// Is sync enabled to interhitance table
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="syncId"></param>
        /// <returns></returns>
        public bool TableInheritanceEnabled(string tableName, Guid syncId)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(tableName) && syncId != Guid.Empty)
                {
                    using (ServiceNowDbSyncMgntEntities snowEntities = new ServiceNowDbSyncMgntEntities())
                    {
                        var synchronization = snowEntities.Synchronization.FirstOrDefault(s => s.Id == syncId);
                        if (synchronization == null) return false;

                        SnowTableDefinition tableDefinitionEntity = snowEntities.SnowTableDefinition.FirstOrDefault(t => t.Table.Equals(tableName));

                        if (tableDefinitionEntity != null && !string.IsNullOrWhiteSpace(tableDefinitionEntity.TableParams))
                        {
                            List<TableParam> tblParams = JsonConvert.DeserializeObject<List<TableParam>>(tableDefinitionEntity.TableParams);
                            var tableParam = tblParams.FirstOrDefault(t => t.InstanceId == synchronization.InstanzSettingsId);
                            if (tableParam != null)
                            {
                                SyncParameter syncParams = tableParam.SynchronizationTypes.FirstOrDefault(t => t.SyncTypeId == synchronization.SyncTypeId)?.SyncParameter;

                                if (syncParams?.TableInheritance != null && syncParams.TableInheritance == true)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: {e.Message}");
                return false;
            }
        }
    }
}
