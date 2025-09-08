using log4net;
using MirrorRepository;
using MirrorRepository.Constants;
using MirrorRepository.Data.SnowDbSyncMgnt;
using MirrorRepository.Enums;
using MirrorRepository.Model;
using MirrorRepository.Model.SnowDbSyncMgnt;
using MirrorRepository.Model.SyncParams;
using MirrorRepository.SnowTableApi;
using MirrorRepository.SnowTableApi.TableDefinitions;
using MirrorRepository.Synchronization;
using MirrorWeb.AdAuthorizationFilter;
using MirrorWeb.Helpers.Kendo;
using MirrorWeb.Models;
using MirrorWeb.ViewModels;
using MirrorWeb.ViewModels.Manage;
using MirrorWeb.ViewModels.Monitoring;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.UI.WebControls;


namespace MirrorWeb.Controllers
{
    
    [System.Web.Http.RoutePrefix("api/SnowApi")]
    public class SnowApiController : ApiController
    {

        /// <summary>
        /// Member which holds the log4net logger
        /// </summary>
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        protected static readonly ILog WebApiLog = LogManager.GetLogger("WebApiLogger");

        /// <summary>
        /// set service now restapi call params
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.Authorize]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("InitializeTableSyncGrid")]
        public HttpResponseMessage InitializeTableSyncGrid(Guid selectedInstanceId, Guid selSynchronizationId)
        {
            SynchronizationViewModel model = new SynchronizationViewModel();
            InstanzSettings instanceSetting = null;
            
            if (selectedInstanceId != Guid.Empty)
            {
                instanceSetting = model.FindInstanceSetting<InstanzSettings>(selectedInstanceId);
            }
            
            if (instanceSetting != null) 
            {
                //read table objects
                TableApiClient<SnowObject> initSnowApi = new TableApiClient<SnowObject>("sys_db_object", instanceSetting);
                RestQueryResponse<SnowObject> snowTables = initSnowApi.GetFull();

                model.SnowTableNames = string.Join(";", snowTables.Result.OrderBy(o => o.TableName).Select(o => o.TableName).ToList());

                model.TableSyncList = snowTables.Result.OrderBy(o => o.TableName).ToList();
                model.TableSyncListTotalCount = snowTables.ResultCount;

                //load existing synchronization
                if (selSynchronizationId != Guid.Empty)
                {
                    var synchronization = model.FindInternal<Synchronization>(selSynchronizationId);
                    model.SelectedInstanzSettingsId = synchronization.InstanzSettingsId;
                    model.SelectedDatabaseSettingsId = synchronization.DatabaseSettingsId;

                    //get used tables in synchronization
                    //model.GetUsedTablesInSynchronizations(model, selSynchronizationId);
                    
                    //check selected tables
                    if (synchronization.SnowTables != null)
                    {
                        List<string> syncSnowTables = synchronization.SnowTables.Split(';').ToList();

                        if (synchronization.UsedCoreTables != null)
                        {
                            List<string> usedCoreTables = synchronization.UsedCoreTables.Split(';').ToList();
                            syncSnowTables.AddRange(usedCoreTables);
                        }
                        
                        foreach (var table in syncSnowTables)
                        {
                            var tblFound = model.TableSyncList.FirstOrDefault(t => t.TableName.Equals(table));
                            if (tblFound != null)
                            {
                                tblFound.Selected = true;
                            }
                        }
                    }
                }
            }
            
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, model, JsonMediaTypeFormatter.DefaultMediaType);
            return response;
        }

        /// <summary>
        /// Post selected tables for synchronizations
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("POST")]
        [System.Web.Http.Route("SaveTableSelectionAndRedirect")]
        public HttpResponseMessage SaveTableSelectionAndRedirect(WorkingSyncCreatorModel model)
        {
            if (model != null && model.SynchronizationId != Guid.Empty && model.SnowTables != null)
            {
                using (ServiceNowDbSyncMgntEntities entities = new ServiceNowDbSyncMgntEntities())
                {
                    var synchronization = entities.Synchronization.FirstOrDefault(s => s.Id == model.SynchronizationId);

                    if (synchronization != null)
                    {
                        //synchronization tables
                        if (model.SnowTables != null)
                        {
                            string snowTables;
                            
                            if (model.SnowTables.Count > 1)
                            {
                                snowTables = String.Join(";", model.SnowTables.OrderBy(o => o.Name).Select(t => t.Name.Trim()));
                            }
                            else
                            {
                                snowTables = model.SnowTables.OrderBy(o => o.Name).Select(t => t.Name.Trim()).First();
                            }
                            synchronization.SnowTables = snowTables;
                        }
                        
                        //check and add table in table definition
                        foreach (var table in model.SnowTables)
                        {
                            var tblDef= entities.SnowTableDefinition.FirstOrDefault(t => t.Table.Equals(table.Name));
                            if (tblDef == null)
                            {
                                SnowTableDefinition tblDefinition = new SnowTableDefinition
                                {
                                    Id = Guid.NewGuid(),
                                    Table = table.Name,
                                    CreateTime = DateTime.Now
                                };

                                TableParam tableParam = new TableParam();
                                var tableParams = tableParam.Init();
                                
                                tblDefinition.TableParams = tableParams;

                                entities.SnowTableDefinition.Add(tblDefinition);
                            }
                        }

                        //updated by
                        var userUpdateIdentity = (ClaimsIdentity)User.Identity;
                        synchronization.UpdatedBy = userUpdateIdentity.Claims.FirstOrDefault(f => f.Type == ClaimTypes.Name)?.Value.ToUpper();

                        entities.SaveChanges();
                    }
                    else
                    {
                        var response = Request.CreateResponse(HttpStatusCode.InternalServerError, $"Could not add/update Tables. Synchronization not found. SyncId:{model.SynchronizationId}", JsonMediaTypeFormatter.DefaultMediaType);
                        return response;
                    }
                }

                var newUrl = this.Url.Link("Default", new
                {
                    Controller = "Manage",
                    Action = "SyncSettings",
                    SyncId = model.SynchronizationId
                });
                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true, RedirectUrl = newUrl });
            }

            return null;
        }

        /// <summary>
        /// Post selected tables for synchronization
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("POST")]
        [System.Web.Http.Route("SaveColumnSelection")]
        public HttpResponseMessage SaveColumnSelection(WorkingSyncCreatorModel model)
        {
            try
            {
                if (model == null || model.SynchronizationId == Guid.Empty || model.SnowColumns == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { Success = false });
                }

                using (ServiceNowDbSyncMgntEntities ctx = new ServiceNowDbSyncMgntEntities())
                {
                    var synchronization = ctx.Synchronization.FirstOrDefault(s => s.Id == model.SynchronizationId);
                    SnowTableDefinition snowTableDefinition = ctx.SnowTableDefinition.FirstOrDefault(s => s.Table == model.TableName);

                    if (synchronization != null)
                    {
                        
                        //add/update new table/columns definition
                        if (model.SnowColumns.Any())
                        {
                            if (snowTableDefinition != null && !string.IsNullOrWhiteSpace(snowTableDefinition.TableParams))
                            {
                                
                                List<TableParam> tblParams = snowTableDefinition.TableParameters;
                                var instance = ctx.InstanzSettings.FirstOrDefault(i => i.Id == synchronization.InstanzSettingsId);

                                var tableParam = tblParams.FirstOrDefault(t => t.InstanceName == instance?.InstanzName);

                                if (tableParam != null)
                                {
                                    tableParam.SnowColummns = model.SnowColumns;
                                }

                                var serTableParams = JsonConvert.SerializeObject(tblParams);
                                snowTableDefinition.TableParams = serTableParams;

                                ctx.SnowTableDefinition.Update(snowTableDefinition);
                            }

                        }
                        
                        ctx.SaveChanges();
                    }
                    
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception ex)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: {ex.Message}");
                return Request.CreateResponse(HttpStatusCode.OK, new { Success = false });
            }
            

        }

        /// <summary>
        /// Get table column schema from single table
        /// or returns the table selection
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("POST")]
        [System.Web.Http.Route("UpdateSyncSettings")]
        public HttpResponseMessage UpdateSyncSettings(WorkingSyncCreatorModel model)
        {
            if (model != null && model.SynchronizationId != Guid.Empty)
            {
                using (ServiceNowDbSyncMgntEntities entities = new ServiceNowDbSyncMgntEntities())
                {
                    var synchronization = entities.Synchronization.FirstOrDefault(s => s.Id == model.SynchronizationId);

                    if (synchronization != null)
                    {
                        synchronization.AutoSchemaUpdate = model.AutoSchemaUpdate;

                        //updated by
                        var userUpdateIdentity = (ClaimsIdentity)User.Identity;
                        synchronization.UpdatedBy = userUpdateIdentity.Claims.FirstOrDefault(f => f.Type == ClaimTypes.Name)?.Value.ToUpper();

                        entities.SaveChanges();
                    }
                    else
                    {
                        var response = Request.CreateResponse(HttpStatusCode.InternalServerError, $"Could not update synchronization settings. Synchronization not found. SyncId:{model.SynchronizationId}", JsonMediaTypeFormatter.DefaultMediaType);
                        return response;
                    }
                }

                var newUrl = this.Url.Link("Default", new
                {
                    Controller = "Manage",
                    Action = "SyncScheduler",
                    SyncId = model.SynchronizationId
                });
                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true, RedirectUrl = newUrl });
            }

            return null;
            }

        /// <summary>
        /// Get scheduler settings and add data to snchronization database
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("POST")]
        [System.Web.Http.Route("SetFinishedSchedulerParams")]
        public HttpResponseMessage SetFinishedSchedulerParams(SyncSchedulerViewModel syncSchedulerModel)
        {
            //Step 1: map new synchronization parameters
            SyncSchedulerModel model = new SyncSchedulerModel();
            using (var snowMgntEntities = new ServiceNowDbSyncMgntEntities())
            {
                if (syncSchedulerModel.SynchronizationId == Guid.Empty)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError,"Cannot add/update Synchronization because of null SynchronizationId.", JsonMediaTypeFormatter.DefaultMediaType);
                }

                var synchronization = snowMgntEntities.Synchronization.FirstOrDefault(s => s.Id == syncSchedulerModel.SynchronizationId);

                if (synchronization != null)
                {
                    model.SynchronizationId = synchronization.Id;
                    model.SynchronizationName = synchronization.Name;
                    model.SnowTableNames = synchronization.SnowTables;
                    model.AutoSchemaUpdate = synchronization.AutoSchemaUpdate;
                }

                model.SyncType = snowMgntEntities.SyncType.FirstOrDefault(s => s.Id == syncSchedulerModel.SelectedSyncType);
                if (syncSchedulerModel.SelectedDatabaseSettings != null)
                {
                    model.SelectedDatabaseSettings = snowMgntEntities.DatabaseSettings.FirstOrDefault(s => s.Id == syncSchedulerModel.SelectedDatabaseSettings.Value);
                }
                if (syncSchedulerModel.SelectedInstanzSettings != null)
                {
                    model.SelectedInstanzSettings = snowMgntEntities.InstanzSettings.FirstOrDefault(s => s.Id == syncSchedulerModel.SelectedInstanzSettings.Value);
                }
                model.SyncTarget = snowMgntEntities.SyncTarget.FirstOrDefault(s => s.Id == syncSchedulerModel.SelectedSyncTarget);
                model.SelectedInterval = syncSchedulerModel.SelectedInterval;
                model.ThreadsPerTable = syncSchedulerModel.ThreadsPerTable;
                model.ThreadSleepTime = syncSchedulerModel.ThreadSleepTime;
                model.RequestTimeout = syncSchedulerModel.RequestTimeout;
                model.PageSize = syncSchedulerModel.PageSize;
                model.KafkaBlockSize = syncSchedulerModel.KafkaBlockSize;
                if (!string.IsNullOrWhiteSpace(syncSchedulerModel.KafkaMode))
                {
                    int intKafkaMode = Int32.Parse(syncSchedulerModel.KafkaMode);
                    var kafkaMode = (EnumKafkaMode)intKafkaMode;
                    model.KafkaMode = kafkaMode.ToString();
                }
                model.ActiveSince = syncSchedulerModel.ActiveSince;
                model.SyncTime = syncSchedulerModel.Time;
                model.SelectedDaysOfWeek = syncSchedulerModel.SelectedDaysOfWeek;
                model.IntervalInMinutes = syncSchedulerModel.IntervalInMinutes;
                model.SubtractMinutesFromDelta = syncSchedulerModel.SubtractMinutesFromDelta;

                if (!string.IsNullOrWhiteSpace(syncSchedulerModel.CustomDeltaStart))
                {
                    model.CustomDeltaStart = DateTime.Parse(syncSchedulerModel.CustomDeltaStart, new CultureInfo("de-DE", false));
                }
                else
                {
                    model.CustomDeltaStart = null;
                }
                
            }

            //Step 2: add new synchronization
            SyncScheduler syncScheduler = new SyncScheduler();
            syncScheduler.AddOrUpdateSynchronization(model, User);

            //Step 3: redirect
            var newUrl = this.Url.Link("Default", new
            {
                Controller = "Manage",
                Action = "Dashboard"
            });
            return Request.CreateResponse(HttpStatusCode.OK, new { Success = true, RedirectUrl = newUrl });
        }

        /// <summary>
        /// set service now restapi call params
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.Authorize]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("InitializeSyncQueueGrid")]
        public HttpResponseMessage InitializeSyncQueueGrid(Guid synchronizationId, int skip, int take, string filter)
        {
            SyncSchedulerModel model = new SyncSchedulerModel();
            
            if (synchronizationId != Guid.Empty)
            {
                var synchronization = model.FindInternal<Synchronization>(synchronizationId);
                model.Init(synchronization);

                InstanzSettings snowInstance = model.SelectedInstanzSettings;
                snowInstance.Password = BaseModel.Decryptdata(snowInstance.Password);
                snowInstance.ProxyUserPassword = BaseModel.Decryptdata(snowInstance.ProxyUserPassword);
                TableApiClient<SnowObject> snowTableApi = new TableApiClient<SnowObject>("sys_dictionary", snowInstance);

                if (!string.IsNullOrWhiteSpace(filter) && !filter.Equals("[]") && !filter.Equals("undefined"))
                {
                    KendoHelper kHelper = new KendoHelper();
                    List<SnowTables> filtered = null;

                    filtered = kHelper.HandleFilter(model, filter);

                    if (filtered != null && filtered.Any())
                    {
                        List<SnowTables> filteredResult = filtered.Skip(skip).Take(take).ToList();

                        model.SnowTables = filteredResult.OrderBy(n => n.Name).ToList();
                        
                        //add UsedCoreTables
                        if (synchronization.UsedCoreTables != null && !string.IsNullOrWhiteSpace(synchronization.UsedCoreTables))
                        {
                            List<string> usedCoreTables = synchronization.UsedCoreTables.Split(';').ToList();
                            foreach (var coreTable in usedCoreTables)
                            {
                                SnowTables snowTable = new SnowTables
                                {
                                    Name = coreTable,
                                    UsedInOtherSync = true
                                };
                                model.SnowTables.Add(snowTable);
                            }
                        }

                        model.ProcessRunning = SetProgressAndCount(synchronizationId, model.SnowTables, snowTableApi);
                        model.SyncTarget = synchronization.SyncTarget;
                        model.SnowTablesCount = filtered.Count();
                    }
                }
                else
                {
                    int totalCount = model.SnowTables.Count;

                    //add UsedCoreTables
                    if (synchronization.UsedCoreTables != null && !string.IsNullOrWhiteSpace(synchronization.UsedCoreTables))
                    {
                        List<string> usedCoreTables = synchronization.UsedCoreTables.Split(';').ToList();
                        foreach (var coreTable in usedCoreTables)
                        {
                            SnowTables snowTable = new SnowTables
                            {
                                Name = coreTable,
                                UsedInOtherSync = true
                            };

                            model.SnowTables.Add(snowTable);
                        }
                    }
                    
                    model.SnowTables = model.SnowTables.Skip(skip).Take(take).OrderBy(n => n.Name).ToList();
                    
                    model.ProcessRunning = SetProgressAndCount(synchronizationId, model.SnowTables, snowTableApi);
                    model.SnowTablesCount = totalCount;
                }

                model.IsAdmin = User.IsInRole("Administrator");
            }
           
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, model, JsonMediaTypeFormatter.DefaultMediaType);
            return response;
        }

        /// <summary>
        /// Update model values from snowprocessor
        /// </summary>
        /// <param name="syncId"></param>
        /// <param name="snowTables"></param>
        /// <param name="tableApi"></param>
        /// <returns></returns>
        protected bool SetProgressAndCount(Guid syncId, List<SnowTables> snowTables, TableApiClient<SnowObject> tableApi = null)
        {
            using (ServiceNowDbSyncMgntEntities snowEntities = new ServiceNowDbSyncMgntEntities())
            {
                SyncScheduler syncScheduler = new SyncScheduler();
                SnowProcessor snowProc = new SnowProcessor();
                bool enableServiceNowRowCount = false;
                bool enableRecordCount = false;
                bool enableSqlRowCount = false;

                var appGridSetting = snowEntities.AppSettings.FirstOrDefault(a => a.Key == SnowDbSyncConstants.GridSettings);
                if (appGridSetting?.Value != null)
                {
                    GridSettings gridSettings = JsonConvert.DeserializeObject<GridSettings>(appGridSetting.Value);
                    if (gridSettings != null)
                    {
                        enableServiceNowRowCount = gridSettings.EnableColumnSnowCount;
                        enableRecordCount = gridSettings.EnableColumnRecordCount;
                        enableSqlRowCount = gridSettings.EnableColumnSqlCount;
                    }
                }

                var synchronization = snowEntities.Synchronization.FirstOrDefault(s => s.Id == syncId);
                SyncTarget syncTarget = snowEntities.SyncTarget.FirstOrDefault(t => t.Id == synchronization.SyncTargetId);

                foreach (var snowTable in snowTables.Where(u => u.UsedInOtherSync == false))
                {
                    
                    var progress = (from s in snowEntities.SyncProcess
                                    where s.SynchronizationId == syncId && s.TableName == snowTable.Name
                                    select s).FirstOrDefault();

                    if (progress != null)
                    {
                        //set column visibility according app settings
                        if (tableApi != null && enableServiceNowRowCount && syncTarget != null && syncTarget.TargetType.Equals(EnumTargetType.Sql.ToString()))
                        {
                            snowTable.SnowCount = tableApi.GetRowCount(snowTable).Result;
                        }

                        if (enableRecordCount || syncTarget.TargetType.Equals(EnumTargetType.Kafka.ToString()))
                        {
                            snowTable.RowCount = progress.RecordsFound;
                        }
                        
                        if (enableSqlRowCount)
                        {
                            snowTable.SqlCount = snowProc.GetSqlRowCount(synchronization, snowEntities, snowTable);
                        }

                        snowTable.Progress = progress.RecordsSynchronized;
                        snowTable.Failures = progress.Failures;
                        snowTable.Inserted = progress.RecordsInserted;
                        snowTable.Updated = progress.RecordsUpdated;
                        snowTable.Deleted = progress.RecordsDeleted;
                        snowTable.SysId = progress.SysId;
                        snowTable.EndTime = progress.EndTime;
                        var targetId = snowEntities.Synchronization.FirstOrDefault(s => s.Id == syncId)?.SyncTargetId;
                        var target = snowEntities.SyncTarget.FirstOrDefault(t => t.Id == targetId);
                        snowTable.TargetType = target.TargetType;

                        StringBuilder sb = new StringBuilder();
                        foreach (var message in progress.LogMessages)
                        {
                            sb.AppendLine($"{message.Key}: {message.Message}");
                        }
                        snowTable.ProcessMessage = sb.ToString();

                        var activeSync = syncScheduler.FindActive(snowTable.Name, syncId).FirstOrDefault();
                        if (activeSync != null)
                        {
                            snowTable.SyncState = EnumSyncProcessState.Running;

                            if (progress.StartTime != null)
                            {
                                snowTable.StartTime = progress.StartTime.Value.ToString("dd.MM.yyyy HH:mm:ss");
                                TimeSpan diff = DateTime.Now - progress.StartTime.Value;
                                snowTable.Duration = $"{diff.Hours:00}:{diff.Minutes:00}:{diff.Seconds:00}";
                            }

                            if (progress.SuspendProcess)
                            {
                                snowTable.SyncState = EnumSyncProcessState.Suspended;
                            }
                        }
                        else
                        {
                            if (progress.StartTime != null)
                            {
                                snowTable.StartTime = progress.StartTime.Value.ToString("dd.MM.yyyy HH:mm:ss");
                            }
                            
                            if (progress.StartTime != null && progress.EndTime != null)
                            {
                                TimeSpan diff = progress.EndTime.Value - progress.StartTime.Value;
                                snowTable.Duration = $"{diff.Hours:00}:{diff.Minutes:00}:{diff.Seconds:00}";
                            }

                            if (progress.EndTime != null)
                            {
                                snowTable.SyncState = EnumSyncProcessState.Finished;
                            }
                            else if (progress.RecordsSynchronized == 0)
                            { 
                                snowTable.SyncState = EnumSyncProcessState.NotStarted;
                            }

                            if (progress.FinalMessage != null && progress.FinalMessage.ToLower().Contains("interrupted") || progress.StopProcess == true && progress.RecordsFound != progress.RecordsInserted)
                            {
                                snowTable.SyncState = EnumSyncProcessState.Interrupted;
                            }

                            if (progress.EndTime == null && progress.SyncTime < DateTime.Now.AddMinutes(-5))
                            {
                                snowTable.SyncState = EnumSyncProcessState.Canceled;
                                sb.AppendLine($"Last synchronization activity: {progress.SyncTime}");
                                snowTable.ProcessMessage =  sb.ToString();
                            }
                                
                            if (progress.FinalErrorMessage != null && !string.IsNullOrWhiteSpace(progress.FinalErrorMessage))
                            {
                                snowTable.SyncState = EnumSyncProcessState.FinalError;
                                snowTable.ProcessMessage = progress.FinalErrorMessage;
                            }
                            

                        }
                    }
                    else
                    {
                        //table does not exist in syncprocess - set default values 
                        if (tableApi != null && enableServiceNowRowCount && syncTarget != null && syncTarget.TargetType.Equals(EnumTargetType.Sql.ToString()))
                        {
                            snowTable.SnowCount = tableApi.GetRowCount(snowTable).Result;
                        }
                        if (enableRecordCount || syncTarget.TargetType.Equals(EnumTargetType.Kafka.ToString()))
                        {
                            snowTable.RowCount = 0;
                        }

                        if (enableSqlRowCount)
                        {
                            snowTable.SqlCount = snowProc.GetSqlRowCount(synchronization, snowEntities, snowTable);
                        }
                        snowTable.Progress = 0;
                        snowTable.Failures = 0;
                        snowTable.Inserted = 0;
                        snowTable.Updated = 0;
                        snowTable.Deleted = 0;
                        snowTable.SysId = null;
                        snowTable.EndTime = DateTime.Now;
                    }
                    
                }

                var runningProcesses = syncScheduler.FindActive(syncId);
                if (runningProcesses != null)
                {
                    return true;
                }
                return false;
            }
        }
        
        /// <summary>
        /// run sync process manually
        /// </summary>
        /// <param name="synchronizationId"></param>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("StartSelectedSyncProcess")]
        public HttpResponseMessage StartSelectedSyncProcess(Guid synchronizationId)
        {
            try
            {
                SyncSchedulerModel model = new SyncSchedulerModel();
                if (synchronizationId != Guid.Empty)
                {
                    var synchronization = model.FindInternal<Synchronization>(synchronizationId);
                    model.Init(synchronization);

                    var identity = (ClaimsIdentity)User.Identity;
                    var givenName = identity.Claims.FirstOrDefault(f => f.Type == ClaimTypes.GivenName);
                    var surName = identity.Claims.FirstOrDefault(f => f.Type == ClaimTypes.Surname);
                    var userAbbreviation = identity.Claims.FirstOrDefault(f => f.Type == ClaimTypes.Name);
                    var userShortName = $"{givenName?.Value} {surName?.Value}({userAbbreviation?.Value.ToUpper()})";

                    if (!string.IsNullOrWhiteSpace(userShortName))
                    {
                        Log.Info($"{MethodBase.GetCurrentMethod()?.Name}. User: {userShortName} started synchronization: {synchronization.Name}.");
                    }

                    var snowExec = new SnowProcessorRunner()
                    {
                        SynchronizationId = synchronizationId,
                        Invocation = EnumInvocation.ManualFull
                    };
                    snowExec.RunAsync();
                }
            }
            catch (Exception e)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: {e.InnerException}");
            }
            
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, JsonMediaTypeFormatter.DefaultMediaType);
            return response;
        }

        /// <summary>
        /// Load existing synchronization for further paramter update
        /// </summary>
        /// <param name="synchronizationId"></param>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("LoadExistingSynchronization")]
        public HttpResponseMessage LoadExistingSynchronization(Guid synchronizationId)
        {
            SyncSchedulerModel model = new SyncSchedulerModel();
            var synchronization = model.FindInternal<Synchronization>(synchronizationId);
            model.Init(synchronization);
            if (!string.IsNullOrWhiteSpace(synchronization.UsedCoreTables))
            {
                List<string> coreTables = synchronization.UsedCoreTables.Split(';').ToList();
                foreach (var coreTable in coreTables)
                {
                    SnowTables snowTable = new SnowTables
                    {
                        Name = coreTable,
                        UsedInOtherSync = true
                    };
                    model.SnowTables.Add(snowTable);
                }
            }
            
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, model, JsonMediaTypeFormatter.DefaultMediaType);
            return response;
        }

        /// <summary>
        /// Update grid values in interval
        /// </summary>
        /// <param name="synchronizationId"></param>
        /// <param name="skipCount"></param>
        /// <param name="takeCount"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("UpdateSyncProgress")]
        public HttpResponseMessage UpdateSyncProgress(Guid? synchronizationId, int skipCount, int takeCount, string filter)
        {
            SyncSchedulerModel model = new SyncSchedulerModel();

            if (synchronizationId != null)
            {
                var synchronization = model.FindInternal<Synchronization>(synchronizationId.Value);
                model.Init(synchronization);
                model.SyncTarget = synchronization.SyncTarget;
                
                if (!string.IsNullOrWhiteSpace(filter) && !filter.Equals("[]") && !filter.Equals("undefined"))
                {
                    KendoHelper kHelper = new KendoHelper();

                    var filtered = kHelper.HandleFilter(model, filter);

                    if (filtered != null && filtered.Any())
                    {
                        List<SnowTables> filteredResult = filtered.Skip(skipCount).Take(takeCount).ToList();

                        model.SnowTables = filteredResult;
                        model.ProcessRunning = SetProgressAndCount(synchronizationId.Value, model.SnowTables, null);
                        model.SnowTablesCount = filtered.Count();

                    }
                }
                else
                {
                    int totalCount = model.SnowTables.Count;
                    model.SnowTables = model.SnowTables.Skip(skipCount).Take(takeCount).ToList();
                    model.ProcessRunning = SetProgressAndCount(synchronizationId.Value, model.SnowTables, null);
                    model.SnowTablesCount = totalCount;
                }
            }

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, model, JsonMediaTypeFormatter.DefaultMediaType);
            return response;
        }

        /// <summary>
        /// Stopp selected running synchronization
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="syncId"></param>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("StopSyncProcess")]
        public HttpResponseMessage StopSyncProcess(string tableName, Guid syncId)
        {
            if (!string.IsNullOrEmpty(tableName))
            {
                SyncSchedulerModel model = new SyncSchedulerModel();
                model.StopRunningProcess(tableName, syncId);
            }

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, JsonMediaTypeFormatter.DefaultMediaType);
            return response;
        }

        /// <summary>
        /// Suspend selected running synchronization
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="syncId"></param>
        /// <param name="syncState"></param>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("SuspendOrContinueProcess")]
        public HttpResponseMessage SuspendOrContinueProcess(string tableName, Guid syncId, int syncState)
        {
            if (!string.IsNullOrWhiteSpace(tableName) && syncId != Guid.Empty)
            {
                if (syncState == (int)EnumSyncProcessState.Suspended)
                {
                    SyncSchedulerModel model = new SyncSchedulerModel();
                    model.ContinueSuspendedProcess(tableName, syncId);

                }
                else if (syncState == (int)EnumSyncProcessState.Running)
                {
                    SyncSchedulerModel model = new SyncSchedulerModel();
                    model.SuspendRunningProcess(tableName, syncId);
                }
            }
            
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, JsonMediaTypeFormatter.DefaultMediaType);
            return response;
        }

        /// <summary>
        /// CleanUp Process - delete process from table SyncProcess
        /// do this only if flag StopProcess is true
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="syncId"></param>
        /// <param name="syncState"></param>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("CleanUpProcess")]
        public HttpResponseMessage CleanUpProcess(string tableName, Guid syncId, int syncState)
        {
            if (!string.IsNullOrWhiteSpace(tableName) && syncId != Guid.Empty)
            {
                using (ServiceNowDbSyncMgntEntities snowEntities = new ServiceNowDbSyncMgntEntities())
                {
                    List<SyncProcess> processTables = new List<SyncProcess>();
                    if (syncState == (int) EnumSyncProcessState.Interrupted)
                    {
                        processTables = snowEntities.SyncProcess.Where(p => p.SynchronizationId == syncId && p.TableName.ToLower() == tableName.ToLower() && p.StopProcess == true).ToList();
                    }
                    else if (syncState == (int)EnumSyncProcessState.Canceled)
                    {
                        processTables = snowEntities.SyncProcess.Where(p => p.SynchronizationId == syncId && p.TableName.ToLower() == tableName.ToLower()).ToList();
                    }
                    
                    if (processTables.Any())
                    {
                        snowEntities.SyncProcess.RemoveRange(processTables);
                        snowEntities.SaveChanges();
                        Log.Info($"{MethodBase.GetCurrentMethod()?.Name}. Table: {tableName} removed from SyncProcess");
                    }
                    else
                    {
                        Log.Warn($"{MethodBase.GetCurrentMethod()?.Name}. Could not remove process for table: {tableName} because table not found with flag StopProcess=1");
                    }
                }
            }

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, JsonMediaTypeFormatter.DefaultMediaType);
            return response;
        }

        /// <summary>
        /// Try a retry synchronization from selected tables. Only tables with syncstate not running are available for retry
        /// </summary>
        /// <param name="retrySyncModel"></param>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("POST")]
        [System.Web.Http.Route("RetrySyncProcess")]
        public HttpResponseMessage RetrySyncProcess(WorkingSyncCreatorModel retrySyncModel)
        {
            if (retrySyncModel != null && retrySyncModel.SnowTables.Any() && retrySyncModel.SynchronizationId != null)
            {
                SyncSchedulerModel model = new SyncSchedulerModel();
                if (retrySyncModel.SynchronizationId != Guid.Empty && retrySyncModel.SnowTables.Any())
                {
                    var identity = (ClaimsIdentity)User.Identity;
                    var givenName = identity.Claims.FirstOrDefault(f => f.Type == ClaimTypes.GivenName);
                    var surName = identity.Claims.FirstOrDefault(f => f.Type == ClaimTypes.Surname);
                    var userAbbreviation = identity.Claims.FirstOrDefault(f => f.Type == ClaimTypes.Name);
                    var userShortName = $"{givenName?.Value} {surName?.Value}({userAbbreviation?.Value.ToUpper()})";

                    SnowProcessor processor = new SnowProcessor();
                    processor.RetrySyncProcess(retrySyncModel.SynchronizationId.Value, retrySyncModel.SnowTables, userShortName);
                }
            }
            else
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}: cannot retry process. SyncId is null or model is empty.");
            }

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, retrySyncModel, JsonMediaTypeFormatter.DefaultMediaType);
            return response;
        }

        /// <summary>
        /// Load table definitions
        /// </summary>
        ///<param name="synchronizationId"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("LoadSyncProcessParams")]
        public HttpResponseMessage LoadSyncProcessParams(Guid synchronizationId, string tableName)
        {
            SyncProcessOverrideModel syncProcessOverrideModel = new SyncProcessOverrideModel();
            
            if (synchronizationId != Guid.Empty && !string.IsNullOrWhiteSpace(tableName))
            {
                syncProcessOverrideModel.SynchronizationId = synchronizationId;
                syncProcessOverrideModel.TableName = tableName;

                SyncSchedulerModel schedulerModel = new SyncSchedulerModel();
                var synchronization = schedulerModel.FindInternal<Synchronization>(synchronizationId);

                using (ServiceNowDbSyncMgntEntities snowEntities = new ServiceNowDbSyncMgntEntities())
                {
                    SnowTableDefinition tableDefinitionEntity = snowEntities.SnowTableDefinition.FirstOrDefault(t => t.Table.Equals(tableName));

                    if (tableDefinitionEntity != null && !string.IsNullOrWhiteSpace(tableDefinitionEntity.TableParams))
                    {
                        List<TableParam> tblParams = JsonConvert.DeserializeObject<List<TableParam>>(tableDefinitionEntity.TableParams);
                        var syncType = snowEntities.SyncType.FirstOrDefault(t => t.Id == synchronization.SyncTypeId);
                        var instance = snowEntities.InstanzSettings.FirstOrDefault(i => i.Id == synchronization.InstanzSettingsId);
                        var tableParam = tblParams?.FirstOrDefault(t => t.InstanceName == instance?.InstanzName);
                        if (tableParam != null)
                        {
                            SyncParameter syncParams = tableParam.SynchronizationTypes.FirstOrDefault(t => t.SyncTypeName == syncType?.TypeName)?.SyncParameter;

                            if (syncParams != null)
                            {
                                syncProcessOverrideModel.Enabled = syncParams.Enabled ?? false;
                                syncProcessOverrideModel.ThreadsPerTable = syncParams.ThreadsPerTable;
                                syncProcessOverrideModel.ThreadSleepTime = syncParams.ThreadSleepTime;
                                syncProcessOverrideModel.PageSize = syncParams.PageSize;
                                syncProcessOverrideModel.RequestTimeout = syncParams.RequestTimeout;

                                AppSettingsModel appSettingsModel = new AppSettingsModel().GetAppSettingsModel();
                                SnowTableChild tableIsInherited = appSettingsModel.InheritanceSettings.SelectMany(i => i.SnowTableChildren).FirstOrDefault(f => f.TableName == tableName);
                                if (tableIsInherited != null)
                                {
                                    syncProcessOverrideModel.TableInheritanceEnabled = true;
                                }
                                else
                                {
                                    syncProcessOverrideModel.TableInheritanceEnabled = false;
                                }
                                syncProcessOverrideModel.TableInheritance = syncParams.TableInheritance ?? false;
                                
                                syncProcessOverrideModel.IsDelta = syncType?.TypeName.Equals("Delta");
                                if (syncParams.CustomDeltaStart != null)
                                {
                                    var customDeltaStart = DateTime.Parse(syncParams.CustomDeltaStart.ToString());
                                    var convDateTime = customDeltaStart.ToString("d.MM.yyyy HH:mm:ss");
                                    syncProcessOverrideModel.CustomDeltaStart = convDateTime;
                                }
                            }
                        }
                    }
                }
            }

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, syncProcessOverrideModel, JsonMediaTypeFormatter.DefaultMediaType);
            return response;
        }

        /// <summary>
        /// load custom delta start time for synchronization
        /// </summary>
        /// <param name="synchronizationId"></param>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("LoadCustomDeltaStart")]
        public HttpResponseMessage LoadCustomDeltaStart(Guid synchronizationId)
        {
            CustomDeltaStartModel model = new CustomDeltaStartModel();

            if (synchronizationId != Guid.Empty)
            {
                using (ServiceNowDbSyncMgntEntities snowEntities = new ServiceNowDbSyncMgntEntities())
                {
                    var synchronization = snowEntities.Synchronization.FirstOrDefault(s => s.Id == synchronizationId);

                    if (synchronization != null)
                    {
                        
                        if (synchronization.CustomDeltaStart != null)
                        {
                            var customDeltaStart = DateTime.Parse(synchronization.CustomDeltaStart.ToString());
                            var convDateTime = customDeltaStart.ToString("d.MM.yyyy HH:mm:ss");
                            model.CustomDeltaTime = convDateTime;
                        }

                    }
                }
            }
            else
            {
                Log.Warn($"{MethodBase.GetCurrentMethod()?.Name}: Cannot load custom delta start time. SynchronizationId not found: {synchronizationId}");
            }

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, model, JsonMediaTypeFormatter.DefaultMediaType);
            return response;
        }

        /// <summary>
        /// set custom delta start time for synchronization
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("POST")]
        [System.Web.Http.Route("SetCustomDeltaStart")]
        public HttpResponseMessage SetCustomDeltaStart(CustomDeltaStartModel model)
        {
            
            if (model.SynchronizationId != Guid.Empty)
            {
                using (ServiceNowDbSyncMgntEntities snowEntities = new ServiceNowDbSyncMgntEntities())
                {
                    var synchronization = snowEntities.Synchronization.FirstOrDefault(s => s.Id == model.SynchronizationId);

                    if (synchronization != null)
                    {
                        //update custom delta start
                        if (!string.IsNullOrWhiteSpace(model.CustomDeltaTime))
                        {
                            var customDeltaStart = DateTime.Parse(model.CustomDeltaTime, new CultureInfo("de-DE", false));
                            synchronization.CustomDeltaStart = customDeltaStart;
                        }
                        else
                        {
                            synchronization.CustomDeltaStart = null;
                        }

                        snowEntities.SaveChanges();
                        
                    }
                }
            }
            else
            {
                Log.Warn($"{MethodBase.GetCurrentMethod()?.Name}: Cannot update custom delta start time. SynchronizationId not found: {model.SynchronizationId}, Name: {model.SynchronizationName}");
            }
            
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, "", JsonMediaTypeFormatter.DefaultMediaType);
            return response;
        }

        /// <summary>
        /// Update SyncProcess parameters for single table
        /// in tabledefinition
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("POST")]
        [System.Web.Http.Route("UpdateSyncProcessParams")]
        public HttpResponseMessage UpdateSyncProcessParams(SyncProcessOverrideModel model)
        {
            if (model.SynchronizationId != Guid.Empty)
            {
                SyncSchedulerModel schedulerModel = new SyncSchedulerModel();
                var synchronization = schedulerModel.FindInternal<Synchronization>(model.SynchronizationId);
                
                using (ServiceNowDbSyncMgntEntities snowEntities = new ServiceNowDbSyncMgntEntities())
                {
                    SnowTableDefinition tableDefinitionEntity = snowEntities.SnowTableDefinition.FirstOrDefault(t => t.Table.Equals(model.TableName));

                    if (tableDefinitionEntity != null)
                    {
                        if (string.IsNullOrWhiteSpace(tableDefinitionEntity.TableParams))
                        {
                            TableParam tblParam = new TableParam();
                            var serTableParams = tblParam.Init();
                            tableDefinitionEntity.TableParams = serTableParams;
                            snowEntities.SaveChanges();
                            tableDefinitionEntity = snowEntities.SnowTableDefinition.FirstOrDefault(t => t.Table.Equals(model.TableName));
                        }
                        
                        if (tableDefinitionEntity != null && !string.IsNullOrWhiteSpace(tableDefinitionEntity.TableParams))
                        {
                            List<TableParam> tblParams = JsonConvert.DeserializeObject<List<TableParam>>(tableDefinitionEntity.TableParams);
                            var instance = snowEntities.InstanzSettings.FirstOrDefault(i => i.Id == synchronization.InstanzSettingsId);
                            var tableParam = tblParams?.FirstOrDefault(t => t.InstanceName != null && t.InstanceName == instance?.InstanzName);
                            if (tableParam == null)
                            {
                                tableParam = tblParams?.FirstOrDefault(t => t.InstanceId == instance?.Id);
                            }

                            if (tableParam != null)
                            {
                                tableParam.InstanceName = instance?.InstanzName;
                                var syncType = snowEntities.SyncType.FirstOrDefault(s => s.Id == synchronization.SyncTypeId);
                                SynchronizationType syncDefType = tableParam.SynchronizationTypes.FirstOrDefault(t => t.SyncTypeName != null && t.SyncTypeName == syncType?.TypeName);
                                if (syncDefType == null)
                                {
                                    syncDefType = tableParam.SynchronizationTypes.FirstOrDefault(t => t.SyncTypeId != null && t.SyncTypeId == syncType?.Id);
                                }
                                if (syncDefType != null)
                                {
                                    syncDefType.SyncTypeName = syncType?.TypeName;
                                }
                                SyncParameter syncParams = syncDefType?.SyncParameter;
                                
                                if (syncParams != null)
                                {
                                    syncParams.Enabled = model.Enabled;
                                    syncParams.PageSize = model.PageSize;
                                    syncParams.ThreadsPerTable = model.ThreadsPerTable;
                                    syncParams.ThreadSleepTime = model.ThreadSleepTime;
                                    syncParams.RequestTimeout = model.RequestTimeout;
                                    syncParams.TableInheritance = model.TableInheritance ?? false;
                                    
                                    if (syncType != null && syncType.TypeName.Equals("Delta"))
                                    {
                                        if (!string.IsNullOrWhiteSpace(model.CustomDeltaStart))
                                        {
                                            var customDeltaStart = DateTime.Parse(model.CustomDeltaStart, new CultureInfo("de-DE", false));
                                            syncParams.CustomDeltaStart = customDeltaStart;
                                        }
                                        else
                                        {
                                            syncParams.CustomDeltaStart = null;
                                        }
                                    }
                                }

                                var serTableParams = JsonConvert.SerializeObject(tblParams);
                                tableDefinitionEntity.TableParams = serTableParams;
                            }
                        }
                        
                        snowEntities.SaveChanges();
                    }
                    else
                    {
                        Log.Warn($"{MethodBase.GetCurrentMethod()?.Name}: Table: {model.TableName} not found in SnowTableDefinition.");
                    }
                }
            }
            
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, "", JsonMediaTypeFormatter.DefaultMediaType);
            return response;
        }

        /// <summary>
        /// Create new synchronization
        /// </summary>
        /// <param name="syncName"></param>
        /// <param name="syncInstanceId"></param>
        /// <param name="syncDatabaseId"></param>
        /// <param name="syncTargetId"></param>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("CreateNewSynchronization")]
        public HttpResponseMessage CreateNewSynchronization(string syncName, Guid syncInstanceId, Guid? syncDatabaseId, Guid? syncTargetId)
        {
            
            //Step 1: check if SyncName already exists
            using (ServiceNowDbSyncMgntEntities entities = new ServiceNowDbSyncMgntEntities())
            {
                var synxExist = entities.Synchronization.FirstOrDefault(s => s.Name.Equals(syncName));
                if (synxExist != null)
                {
                    HttpResponseMessage errResponse = Request.CreateResponse(HttpStatusCode.InternalServerError, "Synchronization Name already exists", JsonMediaTypeFormatter.DefaultMediaType);
                    return errResponse;
                }
            }
            
            //Step 1: map new synchronization parameters
            SyncSchedulerModel model = new SyncSchedulerModel();
            using (var snowMgntEntities = new ServiceNowDbSyncMgntEntities())
            {
                model.SynchronizationName = syncName;

                if (syncTargetId != null)
                {
                    model.SyncTarget = snowMgntEntities.SyncTarget.FirstOrDefault(t => t.Id == syncTargetId);
                }
                else
                {
                    model.SyncTarget = snowMgntEntities.SyncTarget.FirstOrDefault(t => t.TargetType == EnumTargetType.Sql.ToString());
                }
                
                if (model.SyncTarget != null && model.SyncTarget.TargetType.Equals(EnumTargetType.Sql.ToString()))
                {
                    model.SelectedDatabaseSettings = snowMgntEntities.DatabaseSettings.FirstOrDefault(s => s.Id == syncDatabaseId);
                }
                model.SelectedInstanzSettings = snowMgntEntities.InstanzSettings.FirstOrDefault(s => s.Id == syncInstanceId);
                
            }

            //Step 2: add new synchronization
            SyncScheduler syncScheduler = new SyncScheduler();
            SyncSchedulerModel syncModel = syncScheduler.AddOrUpdateSynchronization(model, User);

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, syncModel, JsonMediaTypeFormatter.DefaultMediaType);
            return response;
        }

        /// <summary>
        /// Validate AD User
        /// </summary>
        /// <param name="adUser"></param>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("ValidateAdUser")]
        public HttpResponseMessage ValidateAdUser(string adUser)
        {
            PrincipalModel principalModel = new PrincipalModel();
            string message = string.Empty;
            bool result = false;

            if (!string.IsNullOrWhiteSpace(adUser))
            {
                using (ServiceNowDbSyncMgntEntities entities = new ServiceNowDbSyncMgntEntities())
                {
                    var principals = entities.Principals.FirstOrDefault(p => p.UserName.ToLower().Equals(adUser.ToLower()));
                    
                    if (principals == null)
                    {
                        if (AdAuthenticationService.principalContext != null)
                        {
                            using (var adUserFound = UserPrincipal.FindByIdentity(AdAuthenticationService.principalContext, adUser))
                            {
                                if (adUserFound != null)
                                {
                                    try
                                    {
                                        DirectoryEntry de = adUserFound.GetUnderlyingObject() as DirectoryEntry;
                                        principalModel.SamAccountName = $"{de.Properties["samAccountName"].Value}";
                                        principalModel.FirstName = $"{de.Properties["givenName"].Value}";
                                        principalModel.LastName = $"{de.Properties["sn"].Value}";

                                        //many details
                                        result = true;
                                    }
                                    catch (Exception ex)
                                    {
                                        message = $"AdUser {adUser} found with error: {ex.Message}";
                                    }
                                }
                            }
                        }
                        else
                        {
                            message = "Not connected to AD. Please 'Log Out' and 'Log In' again to connect to Active Directory.";
                        }
                        
                    }
                    else
                    {
                        message = $"AdUser {adUser} is already permitted";
                    }
                }
            }

            var validateUidResult = new
            {
                ValidateUIDResult = result, PrincipalModel = principalModel, Message = message
            };
            
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, validateUidResult, JsonMediaTypeFormatter.DefaultMediaType);
            return response;
        }

        /// <summary>
        /// Register new principal AD User
        /// </summary>
        /// <param name="adUser"></param>
        /// <param name="managementRole"></param>
        /// <returns></returns>
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("RegisterPrincipals")]
        [System.Web.Http.Authorize(Roles = "Administrator")]
        public HttpResponseMessage RegisterPrincipals(string adUser, string managementRole)
        {
            string message = string.Empty;
            bool result = false;

            if (!string.IsNullOrWhiteSpace(adUser) && !string.IsNullOrWhiteSpace(managementRole))
            {
                using (ServiceNowDbSyncMgntEntities entities = new ServiceNowDbSyncMgntEntities())
                {
                    var principal = entities.Principals.FirstOrDefault(p => p.UserName.ToLower().Equals(adUser.ToLower()));
                    Guid.TryParse(managementRole, out Guid roleGuid);
                    
                    //validate aduser
                    if (AdAuthenticationService.principalContext != null)
                    {
                        using (var adUserFound = UserPrincipal.FindByIdentity(AdAuthenticationService.principalContext, adUser))
                        {
                            if (adUserFound == null)
                            {
                                message = $"AdUser={adUser} is not a valid Active Directory account";
                            }
                            else
                            {
                                if (principal == null)
                                {
                                    try
                                    {
                                        Principals principals = new Principals
                                        {
                                            Id = Guid.NewGuid(),
                                            UserName = adUser,
                                            RoleId = roleGuid,
                                            Active = true,
                                            CreateTime = DateTime.Now
                                        };
                                        entities.Principals.Add(principals);
                                        entities.SaveChanges();
                                        result = true;
                                    }
                                    catch (Exception ex)
                                    {
                                        message = $"Error saving AdUser={adUser}. {ex.Message}";
                                    }
                                }
                                else
                                {
                                    message = $"AdUser={adUser} already assigned to MirrorWeb";
                                }
                            }
                        }
                    }
                    else
                    {
                        message = "Not connected to AD. Please 'Log Out' and 'Log In' again to connect to Active Directory.";
                    }
                }
            }
            else
            {
                message = "Please set AdUser and Role";
            }

            var validateUidResult = new { AddAdUserResult = result, Message = message };

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, validateUidResult, JsonMediaTypeFormatter.DefaultMediaType);
            return response;
        }

        /// <summary>
        /// update adUser activation status
        /// </summary>
        /// <param name="principalId"></param>
        /// <param name="active"></param>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("UpdateAdUserActivation")]
        public HttpResponseMessage UpdateAdUserActivation(Guid principalId, bool active)
        {
            string message = string.Empty;
            bool result = false;

            try
            {
                if (principalId != Guid.Empty)
                {
                    using (ServiceNowDbSyncMgntEntities entities = new ServiceNowDbSyncMgntEntities())
                    {
                        var principal = entities.Principals.FirstOrDefault(p => p.Id == principalId);

                        if (principal != null)
                        {
                            principal.Active = active;
                            entities.SaveChanges();
                            message = active ? $"Aduser {principal.UserName} is now active." : $"Aduser {principal.UserName} is now deactivated.";
                            Log.Info($"{MethodBase.GetCurrentMethod()?.Name}. User={User.Identity.Name} changed User State. {message}");
                            result = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: {message}");
            }

            var validateResult = new { UpdateActivationResult = result, Message = message };

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, validateResult, JsonMediaTypeFormatter.DefaultMediaType);
            return response;

        }

        /// <summary>
        /// update adUser activation status
        /// </summary>
        /// <param name="principalId"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("UpdateAdUserRole")]
        public HttpResponseMessage UpdateAdUserRole(Guid principalId, Guid roleId)
        {

            string message = string.Empty;
            bool result = false;

            if (principalId != Guid.Empty && roleId != Guid.Empty)
            {
                using (ServiceNowDbSyncMgntEntities entities = new ServiceNowDbSyncMgntEntities())
                {
                    var principal = entities.Principals.FirstOrDefault(p => p.Id == principalId);

                    if (principal != null)
                    {
                        var previousRole = principal.ManagementRole.RoleName;
                        principal.RoleId = roleId;
                        entities.SaveChanges();

                        var newRole = entities.ManagementRole.FirstOrDefault(r => r.Id == roleId);
                        message = $"Role changed for AdUser {principal.UserName} from {previousRole} to {newRole?.RoleName}.";
                        Log.Info($"{MethodBase.GetCurrentMethod()?.Name}. User={User.Identity.Name} changed UserRole. {message}");
                        result = true;
                    }
                    else
                    {
                        message = "Cannot change Role. AdUser not found.";
                        Log.Info($"{MethodBase.GetCurrentMethod()?.Name}. {message}");
                    }
                }

            }
            var validateResult = new { UpdateAdUserRoleResult = result, Message = message };

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, validateResult, JsonMediaTypeFormatter.DefaultMediaType);
            return response;
        }

        /// <summary>
        /// update adUser activation status
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("InitializeAdUsersGrid")]
        public HttpResponseMessage InitializeAdUsersGrid()
        {
            PrincipalSettingsViewModel model = new PrincipalSettingsViewModel();
            try
            {
                PrincipalModel principalModel = new PrincipalModel();
                
                model.Principals = principalModel.GetPrincipals();
                model.PrincipalsTotalCount = model.Principals.Count();
            }
            catch (Exception e)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}: Error: {e.Message}, {e.InnerException}");
            }


            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, model, JsonMediaTypeFormatter.DefaultMediaType);
            return response;
        }

        /// <summary>
        /// Get roles
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("GetManagementRoles")]
        public HttpResponseMessage GetManagementRoles()
        {
            PrincipalSettingsViewModel model = new PrincipalSettingsViewModel();
            try
            {

                ManagementRoleModel roleModel = new ManagementRoleModel();
               
                var roles = roleModel.GetManagementRoles();

                if (roles != null && roles.Any())
                {
                    List<SelectListItem> listItems = new List<SelectListItem>();

                    foreach (var role in roles)
                    {
                        listItems.Add(new SelectListItem
                        {
                            Value = role.Id.ToString(),
                            Text = role.RoleName.ToString()
                        });
                    }
                    listItems.Insert(0, new SelectListItem() { Value = "", Text = "" });
                    model.ManagementRoles = listItems;
                }

                
            }
            catch (Exception e)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}: Error: {e.Message}, {e.InnerException}");
            }


            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, model, JsonMediaTypeFormatter.DefaultMediaType);
            return response;
        }

        /// <summary>
        /// get synchronizations
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("InitializeSyncGrid")]
        public HttpResponseMessage InitializeSyncGrid()
        {
            SynchronizationListModel model = new SynchronizationListModel();
            List<SynchronizationModel> synchronizationModels = new List<SynchronizationModel>();

            using (ServiceNowDbSyncMgntEntities snowEntities = new ServiceNowDbSyncMgntEntities())
            {
                List<Synchronization> allSynchronizations = snowEntities.Synchronization.OrderBy(n => n.Name).ToList();

                foreach (var synchronization in allSynchronizations)
                {
                    SynchronizationModel syncModel = new SynchronizationModel
                    {
                        Id = synchronization.Id,
                        Name = synchronization.Name,
                        SyncUrl = Url.Route("Default", new { controller = "Manage", action = "RunningSynchronization", syncId = synchronization.Id }),
                        Enabled = synchronization.Enabled,
                        StartTime = $"{synchronization.StartDate:F}",
                        EndTime = $"{synchronization.EndDate:F}"
                    };
                    var syncProcesses = snowEntities.SyncProcess.Where(p => p.SynchronizationId == synchronization.Id);
                    syncModel.Running = syncProcesses.Any(r => r.EndTime == null);
                    
                    synchronizationModels.Add(syncModel);
                }

                model.SynchronizationCount = allSynchronizations.Count;
                model.Synchronizations = synchronizationModels;

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, model, JsonMediaTypeFormatter.DefaultMediaType);
                return response;
            }

            
        }

        /// <summary>
        /// Toogle Synchronizaiton
        /// </summary>
        /// <param name="synchronizationId"></param>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("ToogleSynchronization")]
        public HttpResponseMessage ToogleSynchronization(Guid synchronizationId)
        {
            bool syncEnabled = false;

            using (ServiceNowDbSyncMgntEntities entities = new ServiceNowDbSyncMgntEntities())
            {
                var synchronization = entities.Synchronization.FirstOrDefault(g => g.Id == synchronizationId);

                if (synchronization != null)
                {
                    synchronization.Enabled = !synchronization.Enabled;
                    syncEnabled = synchronization.Enabled;
                    entities.SaveChanges();
                }
            }

            var result = new { Enabled = syncEnabled };

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, result, JsonMediaTypeFormatter.DefaultMediaType);
            return response;
        }

        /// <summary>
        /// Stop all processes Synchronizaiton
        /// </summary>
        /// <param name="synchronizationId"></param>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("StopAllProcessFromSync")]
        public HttpResponseMessage StopAllProcessFromSync(Guid synchronizationId)
        {
            bool processStopped = false;

            using (ServiceNowDbSyncMgntEntities snowEntities = new ServiceNowDbSyncMgntEntities())
            {
                var runningTables = snowEntities.SyncProcess.Where(p => p.SynchronizationId == synchronizationId && p.EndTime == null).ToList();

                SyncSchedulerModel model = new SyncSchedulerModel();
                ListDictionary tableListDictionary = new ListDictionary();
                foreach (var runningTable in runningTables)
                {
                    tableListDictionary.Add(runningTable.TableName, runningTable.SynchronizationId);
                }
                if (tableListDictionary.Count > 0)
                {
                    model.StopRunningProcess(tableListDictionary);
                    processStopped = true;
                }
            }

            var result = new { ProcessStopped = processStopped };

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, result, JsonMediaTypeFormatter.DefaultMediaType);
            return response;


        }

        /// <summary>
        /// Delete Synchronization and beneath processes
        /// </summary>
        /// <param name="synchronizationId"></param>
        /// <param name="delOptionFull"></param>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("DeleteSynchronization")]
        public HttpResponseMessage DeleteSynchronization(Guid synchronizationId, bool delOptionFull)
        {
            string delSyncName = string.Empty;

            try
            {
                using (ServiceNowDbSyncMgntEntities snowEntities = new ServiceNowDbSyncMgntEntities())
                {
                    var processTables = snowEntities.SyncProcess.Where(p => p.SynchronizationId == synchronizationId).ToList();

                    if (processTables.Any())
                    {
                        snowEntities.SyncProcess.RemoveRange(processTables);
                    }

                    if (delOptionFull)
                    {
                        var synchronization = snowEntities.Synchronization.FirstOrDefault(s => s.Id == synchronizationId);
                        if (synchronization != null)
                        {
                            delSyncName = synchronization.Name;
                            snowEntities.Synchronization.Remove(synchronization);
                        }
                    }
                  
                    snowEntities.SaveChanges();
                    Log.Info(delOptionFull
                        ? $"{MethodBase.GetCurrentMethod()?.Name}: Synchronization '{delSyncName}' and all underlying processes are successfully deleted."
                        : $"{MethodBase.GetCurrentMethod()?.Name}: All processes for Synchronization '{delSyncName}' are successfully deleted.");
                }

                var result = new { SyncDeleted = true };

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, result, JsonMediaTypeFormatter.DefaultMediaType);
                return response;
            }
            catch (Exception ex)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}: Error deleting Synchronization '{synchronizationId}'. {ex.Message}, {ex.InnerException}");

                var result = new { SyncDeleted = false };
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, result, JsonMediaTypeFormatter.DefaultMediaType);
                return response;
            }
        }

        /// <summary>
        /// get ServiceNow columsn from selected table
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="synchronizationId"></param>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("InitializeColumnGrid")]
        public HttpResponseMessage InitializeColumnGrid(string tableName, Guid synchronizationId)
        {
            try
            {
                SnowColumnsViewModel model = new SnowColumnsViewModel();

                using (ServiceNowDbSyncMgntEntities ctx = new ServiceNowDbSyncMgntEntities())
                {
                    Synchronization synchronization = ctx.Synchronization.FirstOrDefault(s => s.Id == synchronizationId);
                    
                    if (synchronization != null)
                    {
                        var snowInstance = ctx.InstanzSettings.FirstOrDefault(i => i.Id == synchronization.InstanzSettingsId);
                        snowInstance.Password = BaseModel.Decryptdata(snowInstance.Password);
                        snowInstance.ProxyUserPassword = BaseModel.Decryptdata(snowInstance.ProxyUserPassword);
                        TableApiClient<SnowObject> initSnowApi = new TableApiClient<SnowObject>("sys_dictionary", snowInstance);
                        ColumnResponse columns = initSnowApi.GetColumns(tableName);

                        model.SnowColumnList = columns.SnowColumns;
                        model.SnowColumnListTotalCount = columns.SnowColumns.Count;

                        var snowTableDefinitions = ctx.SnowTableDefinition.FirstOrDefault(s => s.Table == tableName && s.InstanceId == synchronization.InstanzSettings.Id);
                        if (snowTableDefinitions != null && !string.IsNullOrWhiteSpace(snowTableDefinitions.Columns))
                        {
                            model.SnowColumns = snowTableDefinitions.Columns.Split(',').ToList();

                            foreach (var column in model.SnowColumns)
                            {
                                var colFound = model.SnowColumnList.FirstOrDefault(c => c.Element.Value.Equals(column));
                                if (colFound != null)
                                {
                                    colFound.Selected = true;
                                }
                            }
                        }
                        
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, model, JsonMediaTypeFormatter.DefaultMediaType);
                        return response;

                    }
                }

                return null;

            }
            catch (Exception ex)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}: Error get columns from table: {tableName} and Synchronization '{synchronizationId}'. {ex.Message}, {ex.InnerException}");

                var result = new { SyncDeleted = false };
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, result, JsonMediaTypeFormatter.DefaultMediaType);
                return response;
            }
        }

        /// <summary>
        /// get ServiceNow columsn from selected table
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="synchronizationId"></param>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("InitializeScriptCommandsGrid")]
        public HttpResponseMessage InitializeScriptCommandsGrid(string tableName, Guid synchronizationId)
        {
            try
            {
                ScriptCommandViewModel model = new ScriptCommandViewModel();

                using (ServiceNowDbSyncMgntEntities ctx = new ServiceNowDbSyncMgntEntities())
                {
                    var synchronization = ctx.Synchronization.FirstOrDefault(s => s.Id == synchronizationId);

                    if (synchronization != null)
                    {
                        var snowTableDefinition = ctx.SnowTableDefinition.FirstOrDefault(s => s.InstanceId == synchronization.InstanzSettingsId && s.Table == tableName);

                        if (snowTableDefinition != null && !string.IsNullOrWhiteSpace(snowTableDefinition.PostScripts))
                        {
                            List<ScriptCommand> scriptCommandList = JsonConvert.DeserializeObject<List<ScriptCommand>>(snowTableDefinition.PostScripts);
                            model.ScriptCommandList = scriptCommandList;
                            model.ScriptCommandListTotalCount = scriptCommandList.Count;
                        }
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, model, JsonMediaTypeFormatter.DefaultMediaType);
                        return response;

                    }
                }

                return null;

            }
            catch (Exception ex)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}: Error get script commands from table: {tableName} and Synchronization '{synchronizationId}'. {ex.Message}, {ex.InnerException}");

                var result = new { SyncDeleted = false };
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, result, JsonMediaTypeFormatter.DefaultMediaType);
                return response;
            }
        }

        /// <summary>
        /// Post selected tables for synchronization
        /// </summary>
        /// <param name="scriptCommandPostModel"></param>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("POST")]
        [System.Web.Http.Route("AddScriptCommand")]
        public HttpResponseMessage AddScriptCommand(ScriptCommandPostModel scriptCommandPostModel)
        {
            if (scriptCommandPostModel.SynchronizationId != Guid.Empty && !string.IsNullOrWhiteSpace(scriptCommandPostModel.TableName) && !string.IsNullOrWhiteSpace(scriptCommandPostModel.Command))
            {
                using (ServiceNowDbSyncMgntEntities ctx = new ServiceNowDbSyncMgntEntities())
                {
                    var synchronization = ctx.Synchronization.FirstOrDefault(s => s.Id == scriptCommandPostModel.SynchronizationId);

                    if (synchronization != null)
                    {
                        var snowTableDefinition = ctx.SnowTableDefinition.FirstOrDefault(s => s.InstanceId == synchronization.InstanzSettingsId && s.Table == scriptCommandPostModel.TableName);

                        if (snowTableDefinition != null)
                        {
                            ScriptCommand newCommand = new ScriptCommand
                            {
                                Id = Guid.NewGuid(),
                                Synchronization = synchronization,
                                TableName = scriptCommandPostModel.TableName,
                                Command = scriptCommandPostModel.Command,
                                Created = DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss")
                            };

                            if (!string.IsNullOrWhiteSpace(snowTableDefinition.PostScripts))
                            {
                                //add command to existing commandlist
                                List<ScriptCommand> scriptCommandList = JsonConvert.DeserializeObject<List<ScriptCommand>>(snowTableDefinition.PostScripts);

                                scriptCommandList.Add(newCommand);

                                var serializedCommandList = JsonConvert.SerializeObject(scriptCommandList);

                                snowTableDefinition.PostScripts = serializedCommandList;
                            }
                            else
                            {
                                List<ScriptCommand> newCmdList = new List<ScriptCommand> {newCommand};
                                var serializedCommandList = JsonConvert.SerializeObject(newCmdList);
                                snowTableDefinition.PostScripts = serializedCommandList;
                            }
                            
                            ctx.SaveChanges();
                        }
                        else
                        {
                            List<ScriptCommand> commandList = new List<ScriptCommand>();
                            ScriptCommand newCommand = new ScriptCommand
                            {
                                Id = Guid.NewGuid(),
                                Synchronization = synchronization,
                                TableName = scriptCommandPostModel.TableName,
                                Command = scriptCommandPostModel.Command,
                                Created = DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss")
                            };
                            commandList.Add(newCommand);
                            var serializedCommandList = JsonConvert.SerializeObject(commandList);

                            SnowTableDefinition entitySnowTableDefinition = new SnowTableDefinition
                            {
                                Id = Guid.NewGuid(),
                                InstanceId = (Guid) synchronization.InstanzSettingsId,
                                Table = scriptCommandPostModel.TableName,
                                CreateTime = DateTime.Now,
                                PostScripts = serializedCommandList
                            };

                            ctx.SnowTableDefinition.Add(entitySnowTableDefinition);
                            ctx.SaveChanges();
                        }

                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, "", JsonMediaTypeFormatter.DefaultMediaType);
                        return response;

                    }
                }
            }
            return null;
        }

        /// <summary>
        /// remove command from snow tabledefinition
        /// </summary>
        /// <param name="synchronizationId"></param>
        /// <param name="tableName"></param>
        /// <param name="scriptCommandId"></param>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("DeleteScriptCommand")]
        public HttpResponseMessage DeleteScriptCommand(Guid synchronizationId, string tableName, Guid scriptCommandId)
        {
            if (synchronizationId != Guid.Empty && !string.IsNullOrWhiteSpace(tableName) && scriptCommandId != Guid.Empty)
            {
                using (ServiceNowDbSyncMgntEntities ctx = new ServiceNowDbSyncMgntEntities())
                {
                    var synchronization = ctx.Synchronization.FirstOrDefault(s => s.Id == synchronizationId);

                    if (synchronization != null)
                    {
                        var snowTableDefinition = ctx.SnowTableDefinition.FirstOrDefault(s => s.InstanceId == synchronization.InstanzSettingsId && s.Table == tableName);

                        if (snowTableDefinition != null)
                        {
                            if (!string.IsNullOrWhiteSpace(snowTableDefinition.PostScripts))
                            {
                                //remove command from existing commandlist
                                List<ScriptCommand> scriptCommandList = JsonConvert.DeserializeObject<List<ScriptCommand>>(snowTableDefinition.PostScripts);

                                var commandToRemove = scriptCommandList.FirstOrDefault(c => c.Id == scriptCommandId);
                                scriptCommandList.Remove(commandToRemove);

                                var serializedCommandList = JsonConvert.SerializeObject(scriptCommandList);

                                snowTableDefinition.PostScripts = serializedCommandList;
                            }
                            
                            ctx.SaveChanges();
                        }
                        
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, "", JsonMediaTypeFormatter.DefaultMediaType);
                        return response;

                    }
                }
            }
            return null;
        }

        /// <summary>
        /// get ServiceNow replication sql database list
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("InitializeDatabaseGrid")]
        public HttpResponseMessage InitializeDatabaseGrid()
        {
            try
            {
                DatabaseSettingsViewModel model = new DatabaseSettingsViewModel();

                using (ServiceNowDbSyncMgntEntities ctx = new ServiceNowDbSyncMgntEntities())
                {
                    List<DatabaseSettings> replicationDbList = ctx.DatabaseSettings.ToList();

                    model.DatabaseList = replicationDbList;
                    foreach (var db in model.DatabaseList)
                    {
                        db.Password = BaseModel.Decryptdata(db.Password);
                    }
                    model.DatabaseListTotalCount = replicationDbList.Count;

                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, model, JsonMediaTypeFormatter.DefaultMediaType);
                    return response;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}: Error get databaselist. {ex.Message}, {ex.InnerException}");
                return null;
            }
        }

        /// <summary>
        /// Post new database
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("POST")]
        [System.Web.Http.Route("AddDatabase")]
        public HttpResponseMessage AddDatabase(DatabaseSettingsModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.Servername) && 
                !string.IsNullOrWhiteSpace(model.Port) && 
                !string.IsNullOrWhiteSpace(model.Databasename) && 
                !string.IsNullOrWhiteSpace(model.Instancename) && 
                !string.IsNullOrWhiteSpace(model.Username) && 
                !string.IsNullOrWhiteSpace(model.Password))
            {

                try
                {
                    DatabaseSettingsModel dbModel = new DatabaseSettingsModel
                    {
                        Databasename = model.Databasename,
                        Instancename = model.Instancename,
                        Password = BaseModel.Encryptdata(model.Password),
                        Port = model.Port,
                        Servername = model.Servername,
                        Username = model.Username,
                        Schemaname = model.Schemaname
                    };
                    dbModel.InsertOrUpdateData();

                    var result = new { AddUpdateOk = true, Error = "" };

                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, result, JsonMediaTypeFormatter.DefaultMediaType);
                    return response;
                }
                catch (Exception ex)
                {
                    var result = new { AddUpdateOk = false, Error = ex.Message};
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, result, JsonMediaTypeFormatter.DefaultMediaType);
                    return response;
                }
            }
            return null;
        }

        /// <summary>
        /// Post new database
        /// </summary>
        /// <param name="databaseId"></param>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("RemoveDatabase")]
        public HttpResponseMessage RemoveDatabase(Guid databaseId)
        {
            if (databaseId != Guid.Empty)
            {

                try
                {
                    using (ServiceNowDbSyncMgntEntities ctx = new ServiceNowDbSyncMgntEntities())
                    {
                        var databaseToRemove = ctx.DatabaseSettings.FirstOrDefault(d => d.Id == databaseId);

                        if (databaseToRemove != null)
                        {
                            ctx.DatabaseSettings.Remove(databaseToRemove);
                            ctx.SaveChanges();
                        }
                    }

                    var result = new { RemoveOk = true, Error = "" };

                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, result, JsonMediaTypeFormatter.DefaultMediaType);
                    return response;
                }
                catch (Exception ex)
                {
                    var result = new { RemoveOk = false, Error = ex.Message };
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, result, JsonMediaTypeFormatter.DefaultMediaType);
                    return response;
                }
            }
            return null;
        }

        /// <summary>
        /// reload dashboard
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("ReloadDashboard")]
        public HttpResponseMessage ReloadDashboard()
        {
            try 
            {
                using (ServiceNowDbSyncMgntEntities ctx = new ServiceNowDbSyncMgntEntities())
                {
                    List<Synchronization> fullSyncList = new List<Synchronization>();
                    List<Synchronization> deltaSyncList = new List<Synchronization>();
                    
                    var fullSyncs = ctx.Synchronization.Where(t => t.SyncType.TypeName.Equals("Full") && t.Enabled).OrderBy(o => o.InstanzSettings.InstanzName).ThenByDescending(d => d.DaysOfWeek).ToList();
                    var deltaSyncs = ctx.Synchronization.Where(t => t.SyncType.TypeName.Equals("Delta") && t.Enabled).OrderBy(o => o.InstanzSettings.InstanzName).ThenByDescending(d => d.DaysOfWeek).ToList();

                    foreach (var fullSync in fullSyncs)
                    {
                        Synchronization sync = new Synchronization
                        {
                            Id = fullSync.Id,
                            Name = fullSync.Name,
                            StartDate = fullSync.StartDate,
                            EndDate = fullSync.EndDate
                        };
                        fullSyncList.Add(sync);
                    }

                    foreach (var deltaSync in deltaSyncs)
                    {
                        Synchronization sync = new Synchronization
                        {
                            Id = deltaSync.Id,
                            Name = deltaSync.Name,
                            StartDate = deltaSync.StartDate,
                            EndDate = deltaSync.EndDate
                        };
                        deltaSyncList.Add(sync);
                    }

                    var result = new {Full = fullSyncList, Delta = deltaSyncList };
                    
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, result, JsonMediaTypeFormatter.DefaultMediaType);
                    return response;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error. {ex.Message}");
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, "", JsonMediaTypeFormatter.DefaultMediaType);
                return response;
            }
        }

        /// <summary>
        /// get planned full syncs
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("GetDashboardSyncModel")]
        public HttpResponseMessage GetDashboardSyncModel()
        {
            try
            {
                using (ServiceNowDbSyncMgntEntities entities = new ServiceNowDbSyncMgntEntities())
                {
                    DashboardModel dashboardModel = new DashboardModel();

                    //Step 1: Running Syncs
                    List<SyncViewModel> syncRunningSyncList = new List<SyncViewModel>();
                    
                    List<SyncProcess> runningSyncs = new SyncScheduler().FindActive();
                    foreach (var runSync in runningSyncs.OrderByDescending(s => s.StartTime))
                    {
                        var syncObject = entities.Synchronization.FirstOrDefault(s => s.Id == runSync.SynchronizationId);
                        var instanceSettings = entities.InstanzSettings.FirstOrDefault(i => i.Id == syncObject.InstanzSettingsId);
                        var syncTarget = entities.SyncTarget.FirstOrDefault(t => t.Id == syncObject.SyncTargetId);

                        runSync.Synchronization = syncObject;

                        SyncViewModel runningSyncModel = new SyncViewModel
                        {
                            Id = runSync.SynchronizationId,
                            Instance = instanceSettings?.InstanzName,
                            SyncName = syncObject?.Name,
                            TableName = runSync.TableName
                        };

                        if (runSync.StartTime.HasValue)
                        {
                            runningSyncModel.StartTime = runSync.StartTime.Value.ToString("dd.MM.yyyy HH:mm:ss");
                        }
                        runningSyncModel.RecordsFound = runSync.RecordsFound;

                        if (syncTarget != null && syncTarget.TargetType.Equals(EnumTargetType.Sql.ToString()))
                        {
                            runningSyncModel.RecordsUpdated = runSync.RecordsUpdated;
                            runningSyncModel.RecordsInserted = runSync.RecordsInserted;
                            runningSyncModel.RecordsPosted = 0;

                        } 
                        else if (syncTarget != null && syncTarget.TargetType.Equals(EnumTargetType.Kafka.ToString()))
                        {
                            runningSyncModel.RecordsUpdated = 0;
                            runningSyncModel.RecordsInserted = 0;
                            runningSyncModel.RecordsPosted = runSync.RecordsUpdated;
                        }
                        else
                        {
                            runningSyncModel.RecordsUpdated = runSync.RecordsUpdated;
                            runningSyncModel.RecordsInserted = runSync.RecordsInserted;
                        }

                        runningSyncModel.RecordsSynchronized = runSync.RecordsSynchronized;

                        syncRunningSyncList.Add(runningSyncModel);
                    }

                    //Step 2: Full Syncs
                    List<SyncViewModel> syncFullViewModelList = new List<SyncViewModel>();
                    Dictionary<Guid, Synchronization> activeFullSyncList = entities.Synchronization.Where(s => s.SyncType.TypeName.Equals("Full")).ToDictionary(a => a.Id, a => a);

                    //get latest fullSyncs
                    foreach (var dictFull in activeFullSyncList)
                    {
                        SyncViewModel syncFullViewModel = new SyncViewModel
                        {
                            Id = dictFull.Value.Id,
                            Enabled = dictFull.Value.Enabled
                        };

                        var instance =  entities.InstanzSettings.FirstOrDefault(i => i.Id == dictFull.Value.InstanzSettingsId);
                        syncFullViewModel.Instance = instance?.InstanzName;
                        syncFullViewModel.SyncName = dictFull.Value.Name;
                        int.TryParse(dictFull.Value.DaysOfWeek, out var iDayOfWeek);
                        if (dictFull.Value.SyncInterval.Equals("Daily") || dictFull.Value.SyncInterval.Equals("Manual"))
                        {
                            syncFullViewModel.PlannedWeekDay = dictFull.Value.SyncInterval;
                        }
                        else
                        {
                            syncFullViewModel.PlannedWeekDay = Enum.GetName(typeof(EnumDaysOfWeek), iDayOfWeek);
                        }
                        
                        syncFullViewModel.PlannedStart = dictFull.Value.SyncStartTime;

                        if (dictFull.Value.StartDate.HasValue)
                        {
                            syncFullViewModel.StartTime = dictFull.Value.StartDate.Value.ToString("dd.MM.yyyy HH:mm:ss");
                        }

                        if (dictFull.Value.EndDate.HasValue)
                        {
                            syncFullViewModel.EndTime = dictFull.Value.EndDate.Value.ToString("dd.MM.yyyy HH:mm:ss");
                        }
                        if (dictFull.Value.StartDate.HasValue && dictFull.Value.EndDate.HasValue)
                        {
                            TimeSpan durationTimeSpan = dictFull.Value.EndDate.Value.Subtract(dictFull.Value.StartDate.Value);
                            syncFullViewModel.Duration = durationTimeSpan.ToString(@"hh\:mm\:ss");
                        }

                        //next planned start
                        if (!dictFull.Value.SyncInterval.Equals("Daily"))
                        {
                            var date = DateTime.Now;
                            int daysUntilNextStart = 0;
                            if (string.IsNullOrWhiteSpace(dictFull.Value.DaysOfWeek))
                            {
                                continue;
                            }
                            var weekDay = int.Parse(dictFull.Value.DaysOfWeek);
                            if (dictFull.Value.SyncInterval.Equals("TwoWeeks"))
                            {
                                daysUntilNextStart = ((weekDay - (int)date.DayOfWeek + 7) % 7) + 14;
                            }
                            else if (dictFull.Value.SyncInterval.Equals("ThreeWeeks"))
                            {
                                daysUntilNextStart = ((weekDay - (int)date.DayOfWeek + 7) % 7) + 21;
                            }
                            else if (dictFull.Value.SyncInterval.Equals("FourWeeks"))
                            {
                                daysUntilNextStart = ((weekDay - (int)date.DayOfWeek + 7) % 7) + 28;
                            }
                            else
                            {
                                daysUntilNextStart = (weekDay - (int)date.DayOfWeek + 7) % 7;
                            }
                            
                            DateTime nextStart = date.AddDays(daysUntilNextStart);
                            
                            var calcedNextStart = nextStart.Date + TimeSpan.Parse(dictFull.Value.SyncStartTime);
                            if (calcedNextStart < DateTime.Now)
                            {
                                daysUntilNextStart = (weekDay - (int)date.DayOfWeek + 7) % 8;
                                nextStart = date.AddDays(daysUntilNextStart);
                                syncFullViewModel.NextStart = (nextStart.Date + TimeSpan.Parse(dictFull.Value.SyncStartTime)).ToString(CultureInfo.CurrentCulture);
                            }
                            else
                            {
                                syncFullViewModel.NextStart = (nextStart.Date + TimeSpan.Parse(dictFull.Value.SyncStartTime)).ToString(CultureInfo.CurrentCulture);
                            }
                            
                        }
                        else
                        {
                            var calcDate = DateTime.Now.Date + TimeSpan.Parse(dictFull.Value.SyncStartTime);

                            if (calcDate > DateTime.Now)
                            {
                                syncFullViewModel.NextStart = (DateTime.Now.Date + TimeSpan.Parse(dictFull.Value.SyncStartTime)).ToString(CultureInfo.CurrentCulture);
                            }
                            else
                            {
                                var date = DateTime.Now;
                                DateTime nextStart = date.AddDays(1);
                                syncFullViewModel.NextStart = (nextStart.Date + TimeSpan.Parse(dictFull.Value.SyncStartTime)).ToString(CultureInfo.CurrentCulture);
                            }
                            
                        }
                        
                        syncFullViewModelList.Add(syncFullViewModel);
                        
                    }

                    //Step 3: Delta Syncs
                    List<SyncViewModel> syncDeltaViewModelList = new List<SyncViewModel>();
                    Dictionary<Guid, Synchronization> activeDeltaSyncList = entities.Synchronization.Where(s => s.SyncType.TypeName.Equals("Delta") && s.SyncTarget.TargetType.Equals(EnumTargetType.Sql.ToString())).ToDictionary(a => a.Id, a => a);

                    //get latest deltaSyncs
                    foreach (var dictDelta in activeDeltaSyncList)
                    {
                        SyncViewModel syncDeltaViewModel = new SyncViewModel
                        {
                            Id = dictDelta.Value.Id,
                            Enabled = dictDelta.Value.Enabled,
                            Instance = dictDelta.Value.InstanzSettings.InstanzName,
                            SyncName = dictDelta.Value.Name
                        };

                        int.TryParse(dictDelta.Value.DaysOfWeek, out var iDayOfWeek);
                        syncDeltaViewModel.PlannedWeekDay = Enum.GetName(typeof(EnumDaysOfWeek), iDayOfWeek);
                        syncDeltaViewModel.PlannedStart = dictDelta.Value.SyncStartTime;
                        if (dictDelta.Value.PeriodInterval != null)
                        {
                            syncDeltaViewModel.Period = (int)dictDelta.Value.PeriodInterval;
                        }

                        syncDeltaViewModel.StartTime = dictDelta.Value.StartDate.HasValue ? dictDelta.Value.StartDate.Value.ToString("dd.MM.yyyy HH:mm:ss") : "";
                        syncDeltaViewModel.EndTime = dictDelta.Value.EndDate.HasValue ? dictDelta.Value.EndDate.Value.ToString("dd.MM.yyyy HH:mm:ss") : "";
                        
                        if (dictDelta.Value.StartDate.HasValue && dictDelta.Value.EndDate.HasValue)
                        {
                            TimeSpan durationTimeSpan = dictDelta.Value.EndDate.Value.Subtract(dictDelta.Value.StartDate.Value);
                            syncDeltaViewModel.Duration = durationTimeSpan.ToString(@"hh\:mm\:ss");
                        }
                        syncDeltaViewModelList.Add(syncDeltaViewModel);
                        
                    }

                    //Step 4: Kafka Delta Syncs
                    List<SyncViewModel> syncKafkaDeltaViewModelList = new List<SyncViewModel>();
                    Dictionary<Guid, Synchronization> activeKafkaDeltaSyncList = entities.Synchronization.Where(s => s.SyncType.TypeName.Equals("Delta") && s.SyncTarget.TargetType.Equals(EnumTargetType.Kafka.ToString())).ToDictionary(a => a.Id, a => a);

                    //get latest Kafka deltaSyncs
                    foreach (var dictKafkaDelta in activeKafkaDeltaSyncList)
                    {
                        SyncViewModel syncKafkaDeltaViewModel = new SyncViewModel
                        {
                            Id = dictKafkaDelta.Value.Id,
                            Enabled = dictKafkaDelta.Value.Enabled,
                            Instance = dictKafkaDelta.Value.InstanzSettings.InstanzName,
                            SyncName = dictKafkaDelta.Value.Name
                        };

                        int.TryParse(dictKafkaDelta.Value.DaysOfWeek, out var iDayOfWeek);
                        syncKafkaDeltaViewModel.PlannedWeekDay = Enum.GetName(typeof(EnumDaysOfWeek), iDayOfWeek);
                        syncKafkaDeltaViewModel.PlannedStart = dictKafkaDelta.Value.SyncStartTime;
                        if (dictKafkaDelta.Value.PeriodInterval != null)
                        {
                            syncKafkaDeltaViewModel.Period = (int)dictKafkaDelta.Value.PeriodInterval;
                        }

                        syncKafkaDeltaViewModel.StartTime = dictKafkaDelta.Value.StartDate.HasValue ? dictKafkaDelta.Value.StartDate.Value.ToString("dd.MM.yyyy HH:mm:ss") : "";
                        syncKafkaDeltaViewModel.EndTime = dictKafkaDelta.Value.EndDate.HasValue ? dictKafkaDelta.Value.EndDate.Value.ToString("dd.MM.yyyy HH:mm:ss") : "";

                        if (dictKafkaDelta.Value.StartDate.HasValue && dictKafkaDelta.Value.EndDate.HasValue)
                        {
                            TimeSpan durationTimeSpan = dictKafkaDelta.Value.EndDate.Value.Subtract(dictKafkaDelta.Value.StartDate.Value);
                            syncKafkaDeltaViewModel.Duration = durationTimeSpan.ToString(@"hh\:mm\:ss");
                        }
                        syncKafkaDeltaViewModelList.Add(syncKafkaDeltaViewModel);

                    }

                    dashboardModel.RunningSyncViewModel = syncRunningSyncList;
                    dashboardModel.RunningSyncViewModelTotalCount = syncRunningSyncList.Count;
                    dashboardModel.SyncFullViewModels = syncFullViewModelList;
                    dashboardModel.SyncFullViewModelsTotalCount = syncFullViewModelList.Count;
                    dashboardModel.SyncDeltaViewModels = syncDeltaViewModelList;
                    dashboardModel.SyncDeltaViewModelsTotalCount = syncDeltaViewModelList.Count;
                    dashboardModel.SyncKafkaDeltaViewModels = syncKafkaDeltaViewModelList;
                    dashboardModel.SyncKafkaDeltaViewModelsTotalCount = syncKafkaDeltaViewModelList.Count;

                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, dashboardModel, JsonMediaTypeFormatter.DefaultMediaType);
                    return response;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// update adUser activation status
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("InitializeTableInheritanceGrid")]
        public HttpResponseMessage InitializeTableInheritanceGrid()
        {
            TableInheritanceModel model = new TableInheritanceModel();
            try
            {
                using (ServiceNowDbSyncMgntEntities ctx = new ServiceNowDbSyncMgntEntities())
                {
                    var inheritanceRecord = ctx.AppSettings.FirstOrDefault(a => a.Key == SnowDbSyncConstants.InheritanceSettings);

                    if (inheritanceRecord != null)
                    {
                        List<SnowTableInheritance> snowTableInheritance = new List<SnowTableInheritance>();
                        List<SnowTableParent> inheritanceList = JsonConvert.DeserializeObject<List<SnowTableParent>>(inheritanceRecord.Value);
                        foreach (var inheritModel in inheritanceList)
                        {
                            SnowTableInheritance snowTableInheritModel = new SnowTableInheritance
                            {
                                ParentTable = inheritModel.TableName
                            };
                            var concateChildTables = string.Empty;
                            foreach (var childTable in inheritModel.SnowTableChildren)
                            {
                                concateChildTables += childTable.TableName + ",";
                            }
                            concateChildTables = concateChildTables.Remove(concateChildTables.Length - 1);
                            snowTableInheritModel.ChildTables = concateChildTables;
                            snowTableInheritance.Add(snowTableInheritModel);
                        }

                        model.SnowTableInheritance = snowTableInheritance;
                        model.SnowTableInheritanceTotalCount = inheritanceList.Count();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}: Error: {ex.Message}, {ex.InnerException}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"Error init Grid. {ex.Message}");
            }


            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, model, JsonMediaTypeFormatter.DefaultMediaType);
            return response;
        }

        /// <summary>
        /// add new parent child inheritance
        /// </summary>
        /// <param name="parentTableName"></param>
        /// <param name="childTableNames"></param>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("AddNewInheritance")]
        public HttpResponseMessage AddNewInheritance(string parentTableName, string childTableNames)
        {
            string message = string.Empty;
            bool result = false;

            try
            {
                if (!string.IsNullOrWhiteSpace(parentTableName) && !string.IsNullOrWhiteSpace(childTableNames))
                {
                    using (ServiceNowDbSyncMgntEntities ctx = new ServiceNowDbSyncMgntEntities())
                    {
                        List<SnowTableParent> inheritanceList = null;

                        //Step 1: get jsonObject
                        var inheritanceRecord = ctx.AppSettings.FirstOrDefault(a => a.Key == SnowDbSyncConstants.InheritanceSettings);

                        if (inheritanceRecord != null)
                        {
                            inheritanceList = JsonConvert.DeserializeObject<List<SnowTableParent>>(inheritanceRecord.Value);

                            SnowTableParent tableParentFound = inheritanceList.FirstOrDefault(s => s.TableName.ToLower() == parentTableName.ToLower());

                            if (tableParentFound != null)
                            {
                                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"A parent table called {parentTableName} exists already. Please delete existing before adding same parent.");
                            }
                        }
                        else
                        {
                            inheritanceList = new List<SnowTableParent>();
                        }
                            
                        var childTables = childTableNames.ToLower().Trim().Split(',').ToList();
                        List<SnowTableChild> snowTableChildren = new List<SnowTableChild>();
                        
                        foreach (var childTable in childTables)
                        {
                            SnowTableChild snowTableChild = new SnowTableChild {TableName = childTable};
                            snowTableChildren.Add(snowTableChild);
                        }
                        
                        SnowTableParent snowTableParent = new SnowTableParent
                        {
                            TableName = parentTableName,
                            SnowTableChildren = snowTableChildren
                        };
                        inheritanceList.Add(snowTableParent);

                        var serializeSnowTableInherit = JsonConvert.SerializeObject(inheritanceList);

                        if (inheritanceRecord == null) 
                        {
                            AppSettings appSettings = new AppSettings
                            {
                                Id = Guid.NewGuid(),
                                Key = SnowDbSyncConstants.InheritanceSettings,
                                Value = serializeSnowTableInherit,
                                Created = DateTime.Now
                            };

                            ctx.AppSettings.Add(appSettings);
                            ctx.SaveChanges();
                        }
                        else
                        {
                            inheritanceRecord.Value = serializeSnowTableInherit;
                            ctx.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: {ex.Message}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"Error add object '{parentTableName}'. {ex.Message}");
            }

            var updateResult = new { UpdateResult = result, Message = message };

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, updateResult, JsonMediaTypeFormatter.DefaultMediaType);
            return response;

        }

        /// <summary>
        /// remove table inheritance
        /// </summary>
        /// <param name="parentTableName"></param>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("DeleteTableInheritance")]
        public HttpResponseMessage DeleteTableInheritance(string parentTableName)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(parentTableName))
                {
                    using (ServiceNowDbSyncMgntEntities ctx = new ServiceNowDbSyncMgntEntities())
                    {

                        var inheritanceRecord = ctx.AppSettings.FirstOrDefault(a => a.Key == SnowDbSyncConstants.InheritanceSettings);

                        if (inheritanceRecord != null)
                        {
                            var inheritanceObject = JsonConvert.DeserializeObject<List<SnowTableParent>>(inheritanceRecord.Value);

                            SnowTableParent tableParentToRemove = inheritanceObject.FirstOrDefault(s => s.TableName.ToLower() == parentTableName.ToLower());

                            if (tableParentToRemove != null)
                            {
                                inheritanceObject.Remove(tableParentToRemove);
                                var serializeSnowTableInherit = JsonConvert.SerializeObject(inheritanceObject);

                                inheritanceRecord.Value = serializeSnowTableInherit;
                                ctx.SaveChanges();
                            }
                        }

                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, "", JsonMediaTypeFormatter.DefaultMediaType);
                        return response;


                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: {ex.Message}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"Error remove object '{parentTableName}'. {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// load monitoring alert notify recipients
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("InitializeNotifyRecipientGrid")]
        public HttpResponseMessage InitializeNotifyRecipientGrid()
        {
            AlertNotifySettings model = new AlertNotifySettings();
            try
            {
                using (ServiceNowDbSyncMgntEntities ctx = new ServiceNowDbSyncMgntEntities())
                {
                    var alertNotifySettings = ctx.AppSettings.FirstOrDefault(a => a.Key == SnowDbSyncConstants.AlertNotifySettings);

                    if (alertNotifySettings != null)
                    {
                        
                        var alertNotifyParam = JsonConvert.DeserializeObject<AlertNotifySettings>(alertNotifySettings.Value);

                        

                        model.EmailRecipients = alertNotifyParam.EmailRecipients;
                        model.EmailRecipientsTotalCount = alertNotifyParam.EmailRecipients.Count();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}: Error: {ex.Message}, {ex.InnerException}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"Error init Grid. {ex.Message}");
            }


            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, model, JsonMediaTypeFormatter.DefaultMediaType);
            return response;
        }

        /// <summary>
        /// add new notify recipient
        /// </summary>
        /// <param name="emailName"></param>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("AddNotifyRecipient")]
        public HttpResponseMessage AddNotifyRecipient(string emailName, string emailAddress)
        {
            string message = string.Empty;
            bool result = false;

            try
            {
                if (!string.IsNullOrWhiteSpace(emailName) && !string.IsNullOrWhiteSpace(emailAddress))
                {
                    using (ServiceNowDbSyncMgntEntities ctx = new ServiceNowDbSyncMgntEntities())
                    {
                        AlertNotifySettings model = new AlertNotifySettings();

                        var alertNotifySettings = ctx.AppSettings.FirstOrDefault(a => a.Key == SnowDbSyncConstants.AlertNotifySettings);

                        if (alertNotifySettings != null)
                        {
                            model = JsonConvert.DeserializeObject<AlertNotifySettings>(alertNotifySettings.Value);

                            EmailRecipient emailRecipientToAdd = new EmailRecipient
                            {
                                Name = emailName,
                                EmailAddress = emailAddress
                            };

                            var alreadyExist = model.EmailRecipients.FirstOrDefault(x => x.EmailAddress.Equals(emailRecipientToAdd.EmailAddress));
                            if (alreadyExist == null)
                            {
                                model.EmailRecipients.Add(emailRecipientToAdd);

                                var recipientsToJson = JsonConvert.SerializeObject(model);
                                alertNotifySettings.Value = recipientsToJson;
                                ctx.SaveChanges();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: {ex.Message}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"Error add notify recipient '{emailName},{emailAddress}'. {ex.Message}");
            }

            var updateResult = new { UpdateResult = result, Message = message };

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, updateResult, JsonMediaTypeFormatter.DefaultMediaType);
            return response;

        }

        /// <summary>
        /// remove notify recipient
        /// </summary>
        /// <param name="notifyRecipient"></param>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("DeleteNotifyRecipient")]
        public HttpResponseMessage DeleteNotifyRecipient(string notifyRecipient)
        {
            string message = string.Empty;
            bool result = false;

            try
            {
                if (!string.IsNullOrWhiteSpace(notifyRecipient))
                {
                    using (ServiceNowDbSyncMgntEntities ctx = new ServiceNowDbSyncMgntEntities())
                    {
                        AlertNotifySettings model = new AlertNotifySettings();

                        var alertNotifySettings = ctx.AppSettings.FirstOrDefault(a => a.Key == SnowDbSyncConstants.AlertNotifySettings);

                        if (alertNotifySettings != null)
                        {
                            model = JsonConvert.DeserializeObject<AlertNotifySettings>(alertNotifySettings.Value);

                            var emailToRemove = model.EmailRecipients.FirstOrDefault(e => e.EmailAddress.Equals(notifyRecipient));
                            model.EmailRecipients.Remove(emailToRemove);

                            var recipientsToJson = JsonConvert.SerializeObject(model);
                            alertNotifySettings.Value = recipientsToJson;
                            ctx.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: {ex.Message}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"Error remove notifiy recipient '{notifyRecipient}'. {ex.Message}");
            }

            var updateResult = new { UpdateResult = result, Message = message };

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, updateResult, JsonMediaTypeFormatter.DefaultMediaType);
            return response;
        }

        /// <summary>
        /// load monitoring alert notify recipients
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("InitializeSchemaChangeNotifyGrid")]
        public HttpResponseMessage InitializeSchemaChangeNotifyGrid()
        {
            SchemaChangeNotifySettings model = new SchemaChangeNotifySettings();
            try
            {
                using (ServiceNowDbSyncMgntEntities ctx = new ServiceNowDbSyncMgntEntities())
                {
                    var tableSchemaChange = ctx.AppSettings.FirstOrDefault(a => a.Key == SnowDbSyncConstants.TableSchemaChangeNotify);

                    if (tableSchemaChange != null)
                    {

                        var schemaChangeNotifyParam = JsonConvert.DeserializeObject<SchemaChangeNotifySettings>(tableSchemaChange.Value);



                        model.EmailRecipients = schemaChangeNotifyParam.EmailRecipients;
                        model.EmailRecipientsTotalCount = schemaChangeNotifyParam.EmailRecipients.Count();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}: Error: {ex.Message}, {ex.InnerException}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"Error init Grid. {ex.Message}");
            }


            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, model, JsonMediaTypeFormatter.DefaultMediaType);
            return response;
        }

        /// <summary>
        /// add new notify recipient
        /// </summary>
        /// <param name="emailName"></param>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("AddSchemaChangeNotifyRecipient")]
        public HttpResponseMessage AddSchemaChangeNotifyRecipient(string emailName, string emailAddress)
        {
            string message = string.Empty;
            bool result = false;

            try
            {
                if (!string.IsNullOrWhiteSpace(emailName) && !string.IsNullOrWhiteSpace(emailAddress))
                {
                    using (ServiceNowDbSyncMgntEntities ctx = new ServiceNowDbSyncMgntEntities())
                    {
                        SchemaChangeNotifySettings model = new SchemaChangeNotifySettings();

                        var schemaChangeNotifySettings = ctx.AppSettings.FirstOrDefault(a => a.Key == SnowDbSyncConstants.TableSchemaChangeNotify);

                        if (schemaChangeNotifySettings != null)
                        {
                            model = JsonConvert.DeserializeObject<SchemaChangeNotifySettings>(schemaChangeNotifySettings.Value);

                            EmailRecipient emailRecipientToAdd = new EmailRecipient
                            {
                                Name = emailName,
                                EmailAddress = emailAddress
                            };

                            var alreadyExist = model.EmailRecipients.FirstOrDefault(x => x.EmailAddress.Equals(emailRecipientToAdd.EmailAddress));
                            if (alreadyExist == null)
                            {
                                model.EmailRecipients.Add(emailRecipientToAdd);

                                var recipientsToJson = JsonConvert.SerializeObject(model);
                                schemaChangeNotifySettings.Value = recipientsToJson;
                                ctx.SaveChanges();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: {ex.Message}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"Error add schema change notify recipient '{emailName},{emailAddress}'. {ex.Message}");
            }

            var updateResult = new { UpdateResult = result, Message = message };

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, updateResult, JsonMediaTypeFormatter.DefaultMediaType);
            return response;

        }

        /// <summary>
        /// remove schema change notify recipient
        /// </summary>
        /// <param name="notifyRecipient"></param>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("DeleteSchemaChangeNotifyRecipient")]
        public HttpResponseMessage DeleteSchemaChangeNotifyRecipient(string notifyRecipient)
        {
            string message = string.Empty;
            bool result = false;

            try
            {
                if (!string.IsNullOrWhiteSpace(notifyRecipient))
                {
                    using (ServiceNowDbSyncMgntEntities ctx = new ServiceNowDbSyncMgntEntities())
                    {
                        SchemaChangeNotifySettings model = new SchemaChangeNotifySettings();

                        var schemaChangeNotifySettings = ctx.AppSettings.FirstOrDefault(a => a.Key == SnowDbSyncConstants.TableSchemaChangeNotify);

                        if (schemaChangeNotifySettings != null)
                        {
                            model = JsonConvert.DeserializeObject<SchemaChangeNotifySettings>(schemaChangeNotifySettings.Value);

                            var emailToRemove = model.EmailRecipients.FirstOrDefault(e => e.EmailAddress.Equals(notifyRecipient));
                            model.EmailRecipients.Remove(emailToRemove);

                            var recipientsToJson = JsonConvert.SerializeObject(model);
                            schemaChangeNotifySettings.Value = recipientsToJson;
                            ctx.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: {ex.Message}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"Error remove schema change notifiy recipient '{notifyRecipient}'. {ex.Message}");
            }

            var updateResult = new { UpdateResult = result, Message = message };

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, updateResult, JsonMediaTypeFormatter.DefaultMediaType);
            return response;
        }

        /// <summary>
        /// remove notify recipient
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("GetMonitoringModel")]
        public HttpResponseMessage GetMonitoringModel()
        {
            MonitoringViewModel model = new MonitoringViewModel();

            try
            {
                using (ServiceNowDbSyncMgntEntities ctx = new ServiceNowDbSyncMgntEntities())
                {
                    var startTime = DateTime.Now;
                    
                    List<TableRecordMonitoringModel> tblMonitoringModelList = new List<TableRecordMonitoringModel>();
                    DateTime maxMonitoringDate = DateTime.Now.AddDays(-30);
                    List<TableMonitoring> tableMonitoringList = ctx.TableMonitoring.Where(c => c.Created > maxMonitoringDate && c.SyncType.TypeName == "Delta").OrderByDescending(c => c.Created).ToList();

                    foreach (var tblRecord in tableMonitoringList)
                    {

                        TableRecordMonitoringModel tblMonitoringModel = new TableRecordMonitoringModel
                        {
                            Id = tblRecord.Id,
                            TableName = tblRecord.TableName,
                            Instance = tblRecord.InstanzSettings.InstanzName,
                            StartTime = tblRecord.StartTime,
                            EndTime = tblRecord.EndTime,
                            Duration = tblRecord.Duration
                        };

                        var prvStartTime = tableMonitoringList.FirstOrDefault(c => c.TableName == tblRecord.TableName &&
                                                                                   c.InstanzSettingsId == tblRecord.InstanzSettingsId &&
                                                                                   c.DatabaseSettingsId == tblRecord.DatabaseSettingsId &&
                                                                                   c.Created < tblRecord.Created);

                        if (tblRecord.StartTime.HasValue && prvStartTime?.StartTime != null)
                        {
                            TimeSpan periodTimeSpan = tblRecord.StartTime.Value.Subtract(prvStartTime.StartTime.Value);
                            tblMonitoringModel.Period = $"{(int) periodTimeSpan.TotalMinutes:D2}:{periodTimeSpan.Seconds:D2}";
                        }

                        tblMonitoringModel.GetDeltaRecordsFrom = tblRecord.GetDeltaRecordsFrom;
                        tblMonitoringModelList.Add(tblMonitoringModel);
                    }

                    model.LoadedInSeconds = Math.Round(DateTime.Now.Subtract(startTime).TotalSeconds).ToString();
                    model.TableRecords = tblMonitoringModelList;
                    model.TableRecordsTotalCount = tblMonitoringModelList.Count;
                }
            }
            catch (Exception e)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: {e.Message}");
            }

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, model, JsonMediaTypeFormatter.DefaultMediaType);
            return response;
        }

        /// <summary>
        /// set service now restapi call params
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("GetSyncTargetById")]
        public HttpResponseMessage GetSyncTargetById(Guid syncTargetId)
        {
            SyncTargetViewModel model = new SyncTargetViewModel();
            
            if (syncTargetId != Guid.Empty)
            {
                using (ServiceNowDbSyncMgntEntities entities = new ServiceNowDbSyncMgntEntities())
                {
                    var target = entities.SyncTarget.FirstOrDefault(i => i.Id == syncTargetId);

                    if (target != null)
                    {
                        model.Id = target.Id;
                        EnumTargetType enumTargetType = (EnumTargetType)Enum.Parse(typeof(EnumTargetType), target.TargetType);
                        model.TargetType = enumTargetType;
                        model.Targetname = target.Targetname;
                        model.Endpoint = target.Endpoint;
                        model.Username = target.User;
                        model.Password = target.Password;
                    }
                }
            }
            
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, model, JsonMediaTypeFormatter.DefaultMediaType);
            return response;
        }

        /// <summary>
        /// Get the targets by targetType
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="syncId"></param>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("GetSyncTargetsByType")]
        public HttpResponseMessage GetSyncTargetByType(string targetType, string syncId)
        {
            List<SyncTargetViewModel> targetList = new List<SyncTargetViewModel>();
            string selectedTargetName = string.Empty;

            if (!string.IsNullOrWhiteSpace(targetType))
            {
                using (ServiceNowDbSyncMgntEntities entities = new ServiceNowDbSyncMgntEntities())
                {
                    var targets = entities.SyncTarget.Where(i => i.TargetType == targetType).ToList();

                    foreach (var target in targets)
                    {
                        SyncTargetViewModel model = new SyncTargetViewModel
                        {
                            Id = target.Id,
                            Targetname = target.Targetname,
                            Endpoint = target.Endpoint,
                            Username = target.User,
                            Password = target.Password
                        };
                        EnumTargetType enumTargetType = (EnumTargetType)Enum.Parse(typeof(EnumTargetType), target.TargetType);
                        model.TargetType = enumTargetType;
                        
                        targetList.Add(model);
                    }

                    if (!string.IsNullOrWhiteSpace(syncId))
                    {
                        Guid.TryParse(syncId, out Guid syncGuid);
                        var synchronization = entities.Synchronization.FirstOrDefault(i => i.Id == syncGuid);
                        if (synchronization != null)
                        {
                            var selectedTarget = entities.SyncTarget.FirstOrDefault(t => t.Id == synchronization.SyncTargetId);
                            if (selectedTarget != null)
                            {
                                selectedTargetName = selectedTarget.Targetname;
                            }
                        }

                    }
                }
            }

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, new {TargetList = targetList, SelectedTargetName = selectedTargetName }, JsonMediaTypeFormatter.DefaultMediaType);
            return response;
        }

        /// <summary>
        /// set service now restapi call params
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("POST")]
        [System.Web.Http.Route("UpdateSyncTargetById")]
        public HttpResponseMessage UpdateSyncTargetById(SyncTargetModel syncTargetModel)
        {
            bool successResponse = true;

            if (syncTargetModel != null && syncTargetModel.Id != Guid.Empty)
            {
                try
                {
                    using (ServiceNowDbSyncMgntEntities entities = new ServiceNowDbSyncMgntEntities())
                    {
                        var target = entities.SyncTarget.FirstOrDefault(i => i.Id == syncTargetModel.Id);

                        if (target != null)
                        {
                            target.TargetType = Enum.GetName(typeof(EnumTargetType), Int32.Parse(syncTargetModel.TargetType));
                            target.Targetname = syncTargetModel.Targetname;
                            target.Endpoint = syncTargetModel.Endpoint;
                            target.User = syncTargetModel.Username;
                            target.Password = syncTargetModel.Password;
                            target.LastChanged = DateTime.Now;
                            entities.SaveChanges();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: {ex.Message}");
                    successResponse = false;
                }
                
            }
            
            return Request.CreateResponse(HttpStatusCode.OK, new { Success = successResponse });
            
        }

        /// <summary>
        /// set service now restapi call params
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("POST")]
        [System.Web.Http.Route("AddSyncTarget")]
        public HttpResponseMessage AddSyncTarget(SyncTargetModel syncTargetModel)
        {
            bool successResponse = true;
            Guid newSyncTargetId = Guid.NewGuid();

            if (syncTargetModel != null)
            {
                try
                {
                    using (ServiceNowDbSyncMgntEntities entities = new ServiceNowDbSyncMgntEntities())
                    {
                        SyncTarget syncTarget = new SyncTarget
                        {
                            Id = newSyncTargetId,
                            TargetType = Enum.GetName(typeof(EnumTargetType), Int32.Parse(syncTargetModel.TargetType)),
                            Targetname = syncTargetModel.Targetname,
                            Endpoint = syncTargetModel.Endpoint,
                            User = syncTargetModel.Username,
                            Password = syncTargetModel.Password,
                            Created = DateTime.Now,
                            LastChanged = DateTime.Now
                        };

                        entities.SyncTarget.Add(syncTarget);
                        entities.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: {ex.Message}");
                    successResponse = false;
                }

            }

            var newUrl = this.Url.Link("Default", new
            {
                Controller = "Manage",
                Action = "SyncTargetSettings",
                targetId = newSyncTargetId
            });

            return Request.CreateResponse(HttpStatusCode.OK, new { Success = successResponse, RedirectUrl = newUrl });

        }

        /// <summary>
        /// set service now restapi call params
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("ValidateTargetName")]
        public HttpResponseMessage ValidateTargetName(string syncTargetName)
        {

            bool successResponse = true;

            if (!string.IsNullOrEmpty(syncTargetName))
            {
                using (ServiceNowDbSyncMgntEntities entities = new ServiceNowDbSyncMgntEntities())
                {
                    var target = entities.SyncTarget.FirstOrDefault(i => i.Targetname.ToLower().Equals(syncTargetName.ToLower()));

                    if (target != null)
                    {
                        successResponse = false;
                    }
                }
            }
            
            return Request.CreateResponse(HttpStatusCode.OK, new { Success = successResponse });
            
        }

        /// <summary>
        /// get Service NOW nodes - sys_cluster_state
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("GetSnowNodesModel")]
        public HttpResponseMessage GetSnowNodesModel(string instanceName)
        {
            SynchronizationViewModel syncModel = new SynchronizationViewModel();
            SysClusterStateViewModel nodeViewModel = new SysClusterStateViewModel();
            try
            {
                using (ServiceNowDbSyncMgntEntities entities = new ServiceNowDbSyncMgntEntities())
                {
                    if (string.IsNullOrWhiteSpace(instanceName)) return null;
                    var instanceSettings = entities.InstanzSettings.FirstOrDefault(i => i.InstanzName == instanceName);
                    if (instanceSettings == null) return null;
                    var snowInstance = syncModel.FindInstanceSetting<InstanzSettings>(instanceSettings.Id);

                    TableApiClient<SysClusterState> initSnowApi = new TableApiClient<SysClusterState>("sys_cluster_state", snowInstance);
                    var jClusterNodes= initSnowApi.GetFull();
                    if (jClusterNodes.Result != null && jClusterNodes.Result.Any())
                    {
                        nodeViewModel.Nodes = jClusterNodes.Result.ToList();
                        nodeViewModel.NodesTotalCount = nodeViewModel.Nodes.Count;
                    }
                }

            }
            catch (Exception e)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: {e.Message}");
            }

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, nodeViewModel, JsonMediaTypeFormatter.DefaultMediaType);
            return response;
        }
        
        /// <summary>
        /// get Service NOW nodes - sys_cluster_state
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("GetColVisibilities")]
        public HttpResponseMessage GetColVisibilities()
        {
            GridSettings jGridSettings = null;

            try
            {
                using (ServiceNowDbSyncMgntEntities entities = new ServiceNowDbSyncMgntEntities())
                {
                    var gridSettings = entities.AppSettings.FirstOrDefault(g => g.Key == SnowDbSyncConstants.GridSettings);
                    if (gridSettings != null)
                    {
                        jGridSettings = JsonConvert.DeserializeObject<GridSettings>(gridSettings.Value);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: {e.Message}");
            }

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, jGridSettings, JsonMediaTypeFormatter.DefaultMediaType);
            return response;
        }

        //[System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("POST")]
        [System.Web.Http.Route("DeleteServiceNowRecord")]
        public HttpResponseMessage DeleteServiceNowRecord(SnowTableObjectList model)
        {
            
            List<string> respSysIds = new List<string>();
            HttpResponseMessage responseMsg = null;

            try
            {
                if (model != null && model.SnowTableObjects != null && model.SnowTableObjects.Any())
                {
                    var instance = model.SnowInstance;
                    var groupedObject = model.SnowTableObjects.GroupBy(g => g.TableName);
                    foreach (var grpTableObject in groupedObject)
                    {
                        var grpTableKey = grpTableObject.Key;
                        string sysIdList = null;
                        List<string> snowSysIdList = new List<string>();
                        foreach (var tableObject in grpTableObject)
                        {
                            sysIdList += $"'{tableObject.TableSysId }',";
                            snowSysIdList.Add(tableObject.SysId);
                        }

                        if (!string.IsNullOrWhiteSpace(sysIdList))
                        {
                            sysIdList = sysIdList.Remove(sysIdList.Length - 1);
                            var query = $"Delete from {grpTableKey} where sys_id in ({sysIdList})";

                            using (ServiceNowDbSyncMgntEntities snowEntities = new ServiceNowDbSyncMgntEntities())
                            {
                                //default instance
                                string instanceName = "ServiceNowDbSync_KIT";

                                switch (instance)
                                {
                                    case "a1prod":
                                        instanceName = "ServiceNowDbSync";
                                        break;

                                    case "a1int":
                                        instanceName = "ServiceNowDbSync_INT";
                                        break;

                                    case "a1kit":
                                        instanceName = "ServiceNowDbSync_KIT";
                                        break;
                                }

                                var dbSettings = snowEntities.DatabaseSettings.FirstOrDefault(d => d.Instancename == instanceName);
                                var mirrorDb = snowEntities.DatabaseSettings.FirstOrDefault(i => i.Id == dbSettings.Id);

                                if (mirrorDb != null)
                                {
                                    byte[] data = Convert.FromBase64String(mirrorDb.Password);
                                    string mirrorDbPwd = System.Text.Encoding.UTF8.GetString(data);

                                    var conString = $@"Server={mirrorDb.Servername},{mirrorDb.Port};Database={mirrorDb.Databasename};TrustServerCertificate=True;User Id={mirrorDb.Username};Password={mirrorDbPwd}";

                                    //build and execute command
                                    using (var con = new SqlConnection(conString))
                                    {
                                        if (con.State != System.Data.ConnectionState.Open) con.Open();

                                        using (var cmd = con.CreateCommand())
                                        {
                                            cmd.CommandText = query;
                                            cmd.CommandTimeout = 1200;

                                            try
                                            {
                                                var count = cmd.ExecuteNonQuery();
                                                WebApiLog.Info($"{MethodBase.GetCurrentMethod()?.Name}. Count: {count}. Query executed for table: {grpTableKey}. Database = {instanceName}. Query: {query}.");
                                            }
                                            catch (Exception ex)
                                            {
                                                WebApiLog.Error($"{MethodBase.GetCurrentMethod()?.Name}. Command execution failed in database = {instanceName} for table: {grpTableKey}. {ex.Message}, {ex.InnerException}. Query: {query}.");
                                            }
                                            finally
                                            {
                                                //add 
                                                respSysIds.AddRange(snowSysIdList);
                                            }
                                        }
                                    }

                                }
                                else
                                {
                                    WebApiLog.Warn($"{MethodBase.GetCurrentMethod()?.Name}. Could not get DatabaseSettings for '{instanceName}'");
                                }
                            }
                        }
                        else
                        {
                            WebApiLog.Warn($"{MethodBase.GetCurrentMethod()?.Name}. SysId list is empty for table: {grpTableKey}");
                        }


                    }

                    responseMsg = Request.CreateResponse(HttpStatusCode.OK, respSysIds, JsonMediaTypeFormatter.DefaultMediaType);

                }
            }
            catch (Exception ex)
            {
                WebApiLog.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: {ex.InnerException}");
                responseMsg = Request.CreateResponse(HttpStatusCode.BadRequest, "", JsonMediaTypeFormatter.DefaultMediaType);
            }
            
            return responseMsg;
        }

        /// <summary>
        /// get table information
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("GetTableMetaData")]
        public HttpResponseMessage GetTableMetaData(string tableName)
        {

            if (string.IsNullOrWhiteSpace(tableName))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "", JsonMediaTypeFormatter.DefaultMediaType);
            }

            TableMetaDataModel tableMetaDataModel = new TableMetaDataModel
            {
                TableName = tableName
            };

            try
            {

                //init Synchronization collection
                tableMetaDataModel.Synchronizations = new List<Synchronization>();

                using (ServiceNowDbSyncMgntEntities entities = new ServiceNowDbSyncMgntEntities())
                {
                    var synchronizations = entities.Synchronization.OrderBy(i => i.InstanzSettingsId).ToList();
                    foreach (var synchronization in synchronizations)
                    {
                        var snowTables = synchronization.SnowTables.Split(';').ToList();

                        if (snowTables.Contains(tableName))
                        {
                            var instance = entities.InstanzSettings.FirstOrDefault(i => i.Id == synchronization.InstanzSettingsId);
                            synchronization.InstanzSettings = instance;
                            tableMetaDataModel.Synchronizations.Add(synchronization);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: {e.Message}");
            }

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, tableMetaDataModel, JsonMediaTypeFormatter.DefaultMediaType);
            return response;
        }

        /// <summary>
        /// get Service NOW nodes - sys_cluster_state
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.Authorize]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("GetNodeStats")]
        public HttpResponseMessage GetNodeStats(string instanceName, string nodeId)
        {
            XmlStats nodeStat = null;

            try
            {
              
                if (string.IsNullOrWhiteSpace(nodeId)) return null;
                
                using (ServiceNowDbSyncMgntEntities entities = new ServiceNowDbSyncMgntEntities())
                {
                    if (string.IsNullOrWhiteSpace(instanceName)) return null;
                    var snowInstance = entities.InstanzSettings.FirstOrDefault(i => i.InstanzName == instanceName);

                    TableApiClient<SysClusterState> initSnowApi = new TableApiClient<SysClusterState>("sys_cluster_state", snowInstance);

                    var query = $"https://{snowInstance?.InstanzName}.service-now.com/sys_cluster_state.do?sys_id={nodeId}&sys_target=node_stats.stats&XML=&sysparm_stack=no";

                    nodeStat = initSnowApi.GetNodeStatFromUrl(query, true, snowInstance);

                }

            }
            catch (Exception e)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: {e.Message}");
            }

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, nodeStat, JsonMediaTypeFormatter.DefaultMediaType);
            return response;
        }

        /// <summary>
        /// get Service NOW nodes - sys_cluster_state
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.Authorize]
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("GetLogFile")]
        public HttpResponseMessage GetLogFile(string fileType)
        {
            string logContent = null;

            try
            {

                if (string.IsNullOrWhiteSpace(fileType)) return null;

                string filePath = System.Web.Hosting.HostingEnvironment.MapPath("~/Log/SnowMirrorUI.log");
                if (!File.Exists(filePath))
                {
                    return null;
                }

                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs, Encoding.Default))
                {
                    string content = sr.ReadToEnd();
                    logContent = content;
                }
                
            }
            catch (Exception e)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: {e.Message}");
            }

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, logContent, JsonMediaTypeFormatter.DefaultMediaType);
            return response;
        }

    }
}

