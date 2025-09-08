using log4net;
using MirrorRepository.Base;
using MirrorRepository.Data.SnowDbSyncMgnt;
using MirrorRepository.Enums;
using MirrorRepository.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using MirrorRepository.Constants;
using Microsoft.EntityFrameworkCore;

namespace MirrorRepository.Synchronization
{
    public class SyncScheduler
    {
        /// <summary>
        /// Member which holds the log4net logger
        /// </summary>
        protected readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);


        public SyncType GetOrCreateSyncType(string typeName)
        {
            SyncType st = null;
            try
            {
                using (var smeCtx = new ServiceNowDbSyncMgntEntities())
                {
                    st = (from t in smeCtx.SyncType where t.TypeName == typeName select t).FirstOrDefault();
                    if (st == null)
                    {
                        st = new SyncType() { Id = Guid.NewGuid(), TypeName = typeName, Created = DateTime.Now };
                        smeCtx.SyncType.Add(st);
                        smeCtx.SaveChanges();
                    }
                    return st;
                }
            }
            catch (Exception e)
            {
                Log.Info("cannot get: " + typeName, e.GetBaseException());
                SnowBase.LogEntityException(Log, e, st);
                return new SyncType() { Id = new Guid(), TypeName = typeName, Created = DateTime.Now }; // return default to let process work
            }
        }

        public SyncProcess Get(string key)
        {
            using (var smeCtx = new ServiceNowDbSyncMgntEntities())
            {
                return smeCtx.SyncProcess.Find(key);
            }
        }

        IQueryable<SyncProcess> GetSycnProcess(ServiceNowDbSyncMgntEntities ctx, string key)
        {
            return from s in ctx.SyncProcess
                   where s.Key == key
                   select s;
        }

        IQueryable<SyncProcess> GetSycnProcess(ServiceNowDbSyncMgntEntities ctx, string tableName, Guid synchronizationId)
        {
            return from s in ctx.SyncProcess
                            where s.SynchronizationId == synchronizationId && s.TableName == tableName
                            select s;
        }

        public SyncProcess GetOrCreate(string tableName, Guid synchronizationId, string key)
        {
            SyncProcess sp = null;
            try
            {
                using (var smeCtx = new ServiceNowDbSyncMgntEntities())
                {
                    sp = (GetSycnProcess(smeCtx, key)).FirstOrDefault();
                    if (sp == null)
                    {
                        sp = new SyncProcess() { Key = key, SynchronizationId = synchronizationId, TableName = tableName, 
                            Created = DateTime.Now, StartTime = DateTime.Now, RecordsFound = 0, RecordsSynchronized = 0 };
                        smeCtx.SyncProcess.Add(sp);
                        smeCtx.SaveChanges();
                    }
                    return sp;
                }
            } catch (Exception e)
            {
                Log.Info("cannot get: " + key, e);
                SnowBase.LogEntityException(Log, e, sp);
                return new SyncProcess() { Key = key, SynchronizationId = synchronizationId, TableName = tableName }; // return default to let process work
            }
        }

        public void Update(SyncProcess sp)
        {
            try
            {
                using (var smeCtx = new ServiceNowDbSyncMgntEntities())
                {
                    smeCtx.Entry(sp).State = EntityState.Modified;
                    smeCtx.SaveChanges();
                }
            } catch (Exception e)
            {
                Log.Info("cannot update: " + sp, e);
                SnowBase.LogEntityException(Log, e, sp);
            }
        }

        public List<SyncProcess> FindActive(Guid? syncId = null, DatabaseSettings currentDatabaseSettings = null)
        {
            return FindActive((List<string>)null, syncId, currentDatabaseSettings: currentDatabaseSettings);
        }

        public List<SyncProcess> FindActive(string tableName, Guid? syncId = null, DatabaseSettings currentDatabaseSettings = null)
        {
            return FindActive((tableName != null ? new List<string> { tableName } : null), syncId, currentDatabaseSettings: currentDatabaseSettings);
        }

        public List<SyncProcess> FindActive(List<string> tableNames, Guid? syncId = null, DatabaseSettings currentDatabaseSettings = null)
        {
            using (var smeCtx = new ServiceNowDbSyncMgntEntities())
            {
                var dateMin1 = DateTime.Now.AddMinutes(-1);
                var dateMin5 = DateTime.Now.AddMinutes(-5);
                var sqry = from s in smeCtx.SyncProcess
                           where s.EndTime == null && (s.StartTime > dateMin1 || s.SyncTime > dateMin5)
                           select s;
                if (currentDatabaseSettings != null)
                {
                    sqry = from s in sqry
                           join sync in smeCtx.Synchronization on syncId equals sync.Id
                           join db in smeCtx.DatabaseSettings on sync.DatabaseSettings.Id equals db.Id
                           where db.Id == currentDatabaseSettings.Id
                           select s;
                }
                if (syncId != null)
                    sqry = from s in sqry where s.SynchronizationId == syncId.Value select s;
                if (tableNames != null && tableNames.Count > 0)
                    sqry = from s in sqry where tableNames.Contains(s.TableName) select s;
                
                return (sqry).ToList().Select(s => s.Copy()).ToList();
            }
        }

        /// <summary>
        /// Adds new synchronization
        /// </summary>
        /// <param name="model"></param>
        /// <param name="user"></param>
        public SyncSchedulerModel AddOrUpdateSynchronization(SyncSchedulerModel model, IPrincipal user)
        {
            try
            {
                using (ServiceNowDbSyncMgntEntities syncMgntEntities = new ServiceNowDbSyncMgntEntities())
                {
                    Data.SnowDbSyncMgnt.Synchronization synchronization;
                    if (model.SynchronizationId != null)
                    {
                        synchronization = syncMgntEntities.Synchronization.Find(model.SynchronizationId);

                        if (synchronization != null)
                        {
                            var userUpdateIdentity = (ClaimsIdentity)user.Identity;
                            synchronization.UpdatedBy = userUpdateIdentity.Claims.FirstOrDefault(f => f.Type == ClaimTypes.Name)?.Value.ToUpper();
                        }
                    }
                    else
                    {
                        synchronization = new Data.SnowDbSyncMgnt.Synchronization();
                        synchronization.Id = Guid.NewGuid();
                        var userCreateIdentity = (ClaimsIdentity)user.Identity;
                        synchronization.CreatedBy = userCreateIdentity.Claims.FirstOrDefault(f => f.Type == ClaimTypes.Name)?.Value.ToUpper();

                        syncMgntEntities.Synchronization.Add(synchronization);
                    }

                    synchronization.Enabled = true;
                    
                    if (model.SyncTarget != null && model.SyncTarget.Id != Guid.Empty)
                    {
                        synchronization.SyncTargetId = model.SyncTarget.Id;
                    }

                    synchronization.RunImmediately = model.RunImmediately;
                    synchronization.Name = model.SynchronizationName;
                    synchronization.SyncInterval = model.SelectedInterval.ToString();
                    if (model.SyncType != null && model.SyncType.Id != Guid.Empty)
                    {
                        synchronization.SyncTypeId = model.SyncType.Id;
                    }

                    if (model.SyncTarget?.TargetType == EnumTargetType.Sql.ToString())
                    {
                        synchronization.DatabaseSettingsId = model.SelectedDatabaseSettings.Id;
                    }
                    
                    synchronization.InstanzSettingsId = model.SelectedInstanzSettings.Id;
                    synchronization.MaxThreads = model.MaxThreads > 0 ? model.MaxThreads : 20;
                    synchronization.ThreadsPerTable = model.ThreadsPerTable > 0 ? model.ThreadsPerTable : 10;
                    synchronization.ThreadSleepTime = model.ThreadSleepTime > 0 ? model.ThreadSleepTime : 1;
                    synchronization.PageSize = model.PageSize > 0 ? model.PageSize : 1000;
                    synchronization.KafkaBlockSize = model.KafkaBlockSize > 0 ? model.KafkaBlockSize : 50;
                    synchronization.KafkaMode = model.KafkaMode;
                    synchronization.RequestTimeout = model.RequestTimeout > 0 ? model.RequestTimeout : 30;
                    if (model.SelectedDaysOfWeek != null)
                    {
                        synchronization.SetActiveDays(model.SelectedDaysOfWeek.Select(s => s.Day));
                    }
                    if (!string.IsNullOrEmpty(model.ActiveSince))
                    {
                        synchronization.SyncActiveSinceDate = SnowBase.FormatDateTime(SnowBase.ParseDateTime(model.ActiveSince));
                    }
                    synchronization.SyncStartTime = SnowBase.FormatTime(SnowBase.ParseTime(model.SyncTime));
                    synchronization.CustomDeltaStart = model.CustomDeltaStart;
                    synchronization.SubtractMinutesFromDelta = model.SubtractMinutesFromDelta;

                    //Step 2: set sync scheduler data
                    switch (model.SelectedInterval)
                    {
                        case EnumInterval.Daily:
                            break;

                        case EnumInterval.Weekly:
                            break;

                        case EnumInterval.Periodically:
                            synchronization.PeriodInterval = model.IntervalInMinutes;
                            break;

                        case EnumInterval.TwoWeeks:
                            break;

                        case EnumInterval.ThreeWeeks:
                            break;

                        case EnumInterval.FourWeeks:
                            break;

                        case EnumInterval.FiveWeeks:
                            break;
                        
                    }

                    synchronization.RunImmediately = model.RunImmediately;

                    if (!string.IsNullOrWhiteSpace(model.SnowTableNames))
                    {
                        synchronization.SnowTables = model.SnowTableNames;
                    }
                    
                    synchronization.AutoSchemaUpdate = model.AutoSchemaUpdate;
                    synchronization.Created = DateTime.Now;

                    syncMgntEntities.SaveChanges();

                    model.SynchronizationId = synchronization.Id;

                    return model;
                }
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var entityValidationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in entityValidationErrors.ValidationErrors)
                    {
                        Log.Error($"{MethodBase.GetCurrentMethod()?.Name}: Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}: Error: {ex},InnerException: {ex.InnerException?.Message}");

                
            }
            return null;
        }

        /// <summary>
        /// Gets a list of active scheduled synchronizations
        /// </summary>
        public List<SyncSchedulerModel> GetActiveSynchronizations()
        {
            List<SyncSchedulerModel> syncSchedulerList = new List<SyncSchedulerModel>();
            
            using (ServiceNowDbSyncMgntEntities syncMgntEntities = new ServiceNowDbSyncMgntEntities())
            {
                List<Data.SnowDbSyncMgnt.Synchronization> activeSynchronizations = syncMgntEntities.Synchronization.ToList();

                foreach (var activeSynchronization in activeSynchronizations)
                {
                    SyncSchedulerModel model = new SyncSchedulerModel().Init(activeSynchronization);
                    syncSchedulerList.Add(model);
                }

            }

            return syncSchedulerList;
        }

        /// <summary>
        /// get active single SyncScheduler model
        /// </summary>
        /// <param name="syncId"></param>
        /// <returns></returns>
        public SyncSchedulerModel GetActiveSynchronization(Guid syncId)
        {
            using (ServiceNowDbSyncMgntEntities syncMgntEntities = new ServiceNowDbSyncMgntEntities())
            {
                Data.SnowDbSyncMgnt.Synchronization activeSynchronization = syncMgntEntities.Synchronization.FirstOrDefault(s => s.Id == syncId);

               return new SyncSchedulerModel().Init(activeSynchronization);
               
            }
        }

        /// <summary>
        /// returns the value if interface monitoring is enabled/disabled
        /// </summary>
        public MonitoringSettings InterfaceMonitoringEnabled()
        {
            try
            {
                using (ServiceNowDbSyncMgntEntities syncMgntEntities = new ServiceNowDbSyncMgntEntities())
                {
                    var appSettingInferfaceMonitoring = syncMgntEntities.AppSettings.FirstOrDefault(i => i.Key == SnowDbSyncConstants.Monitoring);
                    if (appSettingInferfaceMonitoring?.Value != null)
                    {
                        var monitoringSettings = JsonConvert.DeserializeObject<MonitoringSettings>(appSettingInferfaceMonitoring.Value);
                        return monitoringSettings;
                    }
                    
                    return null;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets a single scheduled synchronization by name
        /// </summary>
        public SyncSchedulerModel GetSynchronizationByName(string selSyncName)
        {
            using (ServiceNowDbSyncMgntEntities syncMgntEntities = new ServiceNowDbSyncMgntEntities())
            {
                Data.SnowDbSyncMgnt.Synchronization activeSynchronization = syncMgntEntities.Synchronization.Single(s => s.Name.Equals(selSyncName));

                if (activeSynchronization != null)
                {
                    return new SyncSchedulerModel().Init(activeSynchronization);
                }
            }

            return null;
        }
    }
}
