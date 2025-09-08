using log4net;
using MirrorWeb.Models;
using MirrorWeb.ViewModels;
using MirrorWeb.ViewModels.Manage;
using MirrorRepository.SnowTableApi;
using MirrorRepository.Model.SnowDbSyncMgnt;
using MirrorRepository.Model;
using MirrorRepository.Synchronization;
using MirrorRepository.Data.SnowDbSyncMgnt;
using MirrorRepository.SnowTableApi.TableDefinitions;
using System;
using System.Data.SqlClient;
using System.Web;
using System.Web.Security;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Mvc;
using MirrorRepository;
using MirrorRepository.Enums;
using MirrorRepository.NotificationHelper;
using MirrorRepository.WindowsServiceController;

namespace MirrorWeb.Controllers
{
    [RequireHttps]
    public class ManageController : Controller
    {

        /// <summary>
        /// Member which holds the log4net logger
        /// </summary>
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        /// <summary>
        /// get dashboard data
        /// </summary>
        /// <returns></returns>
        public ActionResult Dashboard()
        {
            DashboardViewModel model = new DashboardViewModel();

            try
            {
                using (ServiceNowDbSyncMgntEntities entities = new ServiceNowDbSyncMgntEntities())
                {
                    model.Synchronizations = entities.Synchronization.OrderByDescending(o => o.StartDate).ToList();

                    model.TotalNumberOfSynchronizations = entities.SyncProcess.Count();

                    model.LastSuccessSync = entities.SyncProcess.Where(l => l.EndTime != null).OrderByDescending(o => o.EndTime).First().EndTime;

                    model.LastSuccessSynchronizations = entities.SyncProcess.Where(s => s.FinalErrorMessage == null && s.EndTime != null).OrderByDescending(d => d.EndTime).ToList();

                    model.FailedSynchronizations = entities.SyncProcess.Where(s => s.FinalErrorMessage != null).OrderByDescending(d => d.SyncTime).ToList();

                }
                
                return View(model);
            }
            catch (Exception ex)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: {ex.Message}");
            }

            return View(model);
        }

        /// <summary>
        /// retrieves enabled synchronizations
        /// </summary>
        /// <param name="syncId"></param>
        /// <param name="selectedInstanceId"></param>
        /// <returns></returns>
        [System.Web.Mvc.Authorize]
        public ActionResult RunningSynchronization(Guid? syncId, Guid? selectedInstanceId)
        {
            SyncQueueViewModel model = new SyncQueueViewModel();
            
            using (ServiceNowDbSyncMgntEntities syncMgntEntities = new ServiceNowDbSyncMgntEntities())
            {
                if (selectedInstanceId == null || selectedInstanceId == Guid.Empty)
                {
                    selectedInstanceId = syncMgntEntities.InstanzSettings.FirstOrDefault(i => i.InstanzName == "a1prod")?.Id;
                }

                List<Synchronization> syncList = syncMgntEntities.Synchronization.Where(s => s.InstanzSettingsId == selectedInstanceId).OrderBy(o => o.Name).ToList();
                
                if (syncList.Any())
                {
                    //get syncs for target SqldDb
                    var sqlTarget = syncMgntEntities.SyncTarget.FirstOrDefault(t => t.TargetType.Equals(EnumTargetType.Sql.ToString()));
                    model.SyncNameListSqlDb = syncList.Where(y => y.SyncTargetId == sqlTarget?.Id).Select(x =>
                        new SelectListItem()
                        {
                            Text = x.Name,
                            Value = x.Id.ToString()
                        }).ToList();

                    model.SyncNameListSqlDb.Insert(0, new SelectListItem() { Text = string.Empty });

                    if (syncId != null && syncId != Guid.Empty)
                    {
                        foreach (var sync in model.SyncNameListSqlDb)
                        {
                            if (sync.Value == syncId.ToString())
                            {
                                sync.Selected = true;
                            }
                        }

                        model.SelectedSynchronizationIdSqlDb = syncId;
                    }

                    //get syncs for target Kafka
                    List<Guid> kafkaTarget = syncMgntEntities.SyncTarget.Where(t => t.TargetType.Equals(EnumTargetType.Kafka.ToString())).Select(k => k.Id).ToList();
                    model.SyncNameListKafka = syncList.Where(k => kafkaTarget.Contains(k.SyncTargetId.Value)).Select(x =>
                        new SelectListItem()
                        {
                            Text = x.Name,
                            Value = x.Id.ToString()
                        }).ToList();

                    model.SyncNameListKafka.Insert(0, new SelectListItem() { Text = string.Empty });

                    if (syncId != null && syncId != Guid.Empty)
                    {
                        foreach (var sync in model.SyncNameListKafka)
                        {
                            if (sync.Value == syncId.ToString())
                            {
                                sync.Selected = true;
                            }
                        }

                        model.SelectedSynchronizationIdKafka = syncId;
                    }

                }
                else
                {
                    model.SyncNameListSqlDb = new List<SelectListItem>();
                }
            }
            
            //Instances
            model.InstanceList = new SynchronizationViewModel().AllInstanzSettings();
            model.SelectedInstanceId = selectedInstanceId;
            
            return View(model);
        }
        
        // GET: Synchronization
        [System.Web.Mvc.Authorize(Roles = "Administrator")]
        public ActionResult Synchronization(SynchronizationViewModel model)
        {
            if (model == null)
            {
                model = new SynchronizationViewModel();
                
            }
            
            model.Synchronizations = model.AllSynchronizations();
            model.DatabaseSettings = model.AllStagingDatabases();
            model.InstanzSettings = model.AllInstanzSettings();
            model.SyncTargets = model.AllSyncTargets();

            return View(model);
        }

        /// <summary>
        /// Sets the syncName and TableList
        /// </summary>
        /// <param name="syncId"></param>
        /// <returns></returns>
        [System.Web.Mvc.Authorize(Roles = "Administrator")]
        public ActionResult SyncSettings(Guid syncId)
        {
            SynchronizationViewModel model = new SynchronizationViewModel {SynchronizationId = syncId};
            model.Init();
            
            return View(model);
        }

        /// <summary>
        /// Sync scheduler for single page synchronization
        /// </summary>
        /// <returns></returns>
        [System.Web.Mvc.Authorize(Roles = "Administrator")]
        public ActionResult SyncScheduler(Guid syncId)
        {
            SyncSchedulerViewModel model = new SyncSchedulerViewModel();
            
            if (syncId != Guid.Empty)
            {
                SyncSchedulerModel schedulerModel = new SyncSchedulerModel();
                var synchronization = schedulerModel.FindInternal<Synchronization>(syncId);
                schedulerModel.Init(synchronization);

                model.SynchronizationId = syncId;
                model.SynchronizationName = schedulerModel.SynchronizationName;
                
                model.SelectedDatabaseSettings = schedulerModel.SelectedDatabaseId;
                model.SelectedInstanzSettings = schedulerModel.SelectedInstanceId;
                model.PageSize = schedulerModel.PageSize;
                model.KafkaBlockSize = schedulerModel.KafkaBlockSize;
                if (!string.IsNullOrWhiteSpace(schedulerModel.KafkaMode))
                {
                    int valKafkaMode = (int)Enum.Parse(typeof(EnumKafkaMode), schedulerModel.KafkaMode);
                    model.SelectedKafkaMode = valKafkaMode.ToString();
                }
                
                model.ThreadSleepTime = schedulerModel.ThreadSleepTime;
                model.ThreadsPerTable = schedulerModel.ThreadsPerTable;
                model.SelectedInterval = schedulerModel.SelectedInterval;
                if (schedulerModel.SyncType != null)
                {
                    model.SelectedSyncType = schedulerModel.SyncType.Id;
                }

                using (ServiceNowDbSyncMgntEntities ctx = new ServiceNowDbSyncMgntEntities())
                {
                    if (schedulerModel.SyncTarget != null)
                    {
                        model.SelectedSyncTarget = schedulerModel.SyncTarget.Id;
                    }
                    else
                    {
                        //set default SqlDb
                        model.SelectedSyncTarget = ctx.SyncTarget.FirstOrDefault(t => t.TargetType == EnumTargetType.Sql.ToString())?.Id;

                    }

                    var target = ctx.SyncTarget.FirstOrDefault(t => t.Id == model.SelectedSyncTarget);
                    if (target != null)
                    {
                        model.SelectedSyncTargetType = ((int)Enum.Parse(typeof(EnumTargetType), target.TargetType)).ToString();

                        var syncTargets = (from s in ctx.SyncTarget where s.TargetType == target.TargetType
                                           select new SelectListItem()
                            {
                                Selected = false,
                                Text = s.Targetname,
                                Value = s.Id.ToString()
                            }).ToList();

                        model.SyncTargets = syncTargets;
                    }
                    
                }
                
                model.SelectedDaysOfWeek = schedulerModel.SelectedDaysOfWeek ?? new List<SnowDayOfWeek>();
                model.Time = schedulerModel.SyncTime;
                model.IntervalInMinutes = schedulerModel.IntervalInMinutes ?? 0;
                model.RequestTimeout = schedulerModel.RequestTimeout;

                if (!string.IsNullOrWhiteSpace(schedulerModel.ActiveSince))
                {
                    DateTime activeSince = DateTime.Parse(schedulerModel.ActiveSince, new CultureInfo("de-DE", false));
                    model.ActiveSince = DateTime.SpecifyKind(activeSince, DateTimeKind.Utc).ToString();
                }
                else
                {
                    model.ActiveSince = null;
                }

                if (schedulerModel.CustomDeltaStart != null)
                {
                    try
                    {
                        Log.Info($"{MethodBase.GetCurrentMethod()?.Name} - CustomDeltaStart - Before parsed: {schedulerModel.CustomDeltaStart.ToString()}");
                        DateTime custDeltaStart = DateTime.Parse(schedulerModel.CustomDeltaStart.ToString(), CultureInfo.InvariantCulture);
                        Log.Info($"{MethodBase.GetCurrentMethod()?.Name} - CustomDeltaStart  - after parsed: {custDeltaStart}");
                        var convDateTime = custDeltaStart.ToString("d.MM.yyyy HH:mm:ss");
                        Log.Info($"{MethodBase.GetCurrentMethod()?.Name} - CustomDeltaStart  - after convert: {convDateTime}");
                        model.CustomDeltaStart = convDateTime;
                        
                    }
                    catch (Exception e)
                    {
                        Log.Error($"{MethodBase.GetCurrentMethod()?.Name} - Error reading CustomDeltaStart={schedulerModel.CustomDeltaStart} - {e.Message} {e.InnerException}");
                    }
                    
                }

                model.SubtractMinutesFromDelta = schedulerModel.SubtractMinutesFromDelta;
            }
           
            return View(model);
        }

        // GET: Settings
        [System.Web.Mvc.Authorize(Roles = "Administrator")]
        public ActionResult Settings()
        {
            SettingsViewModel model = new SettingsViewModel();
            return View(model);
        }

        // GET: Database Settings
        [System.Web.Mvc.Authorize(Roles = "Administrator")]
        public ActionResult DatabaseSettings()
        {
            DatabaseSettingsViewModel model = new DatabaseSettingsViewModel();
            DatabaseSettingsModel dbModel = new DatabaseSettingsModel();
            
            var syncDbList = dbModel.GetAllDatabases();
            if (syncDbList != null && syncDbList.Any())
            {
                var selDbListItems = (from s in syncDbList
                                      select new SelectListItem()
                                        {
                                            Selected = false,
                                            Text = s.Instancename + ": " + s.Databasename,
                                            Value = s.Databasename + ";" + s.Id
                                        }).ToList();
                model.Databases = selDbListItems;
            }
            return View(model);
        }

        /// <summary>
        /// Set mailsettings
        /// </summary>
        /// <returns></returns>
        public ActionResult MailSettings()
        {
            MailAccountSettingsViewModel model = new MailAccountSettingsViewModel();
            AppSettingsModel appSettingsModel = new AppSettingsModel().GetAppSettingsModel();

            model.NotificationSettings = appSettingsModel.NotificationSettings;

            return View(model);
        }

        /// <summary>
        /// Set general settings
        /// </summary>
        /// <returns></returns>
        [System.Web.Mvc.Authorize(Roles = "Administrator")]
        public ActionResult GeneralSettings()
        {
            GeneralSettingsViewModel model = new GeneralSettingsViewModel();
            AppSettingsModel appSettingsModel = new AppSettingsModel().GetAppSettingsModel();
            model.MonitoringSettings = appSettingsModel.MonitoringSettings;
            model.SqlSessionSettings = appSettingsModel.SqlSessionSettings;
            model.AlertNotifySettings = appSettingsModel.AlertNotifySettings;
            model.SchemaChangeNotifySettings = appSettingsModel.SchemaChangeNotifySettings;
            model.ProcessSettings = appSettingsModel.ProcessSettings;
            model.GridSettings = appSettingsModel.GridSettings;

            return View(model);
        }

        /// <summary>
        /// Set sync targets settings
        /// </summary>
        /// <returns></returns>
        [System.Web.Mvc.Authorize(Roles = "Administrator")]
        public ActionResult SyncTargetSettings(Guid? targetId)
        {
            SyncTargetViewModel model = new SyncTargetViewModel();
            SyncTargetModel dbModel = new SyncTargetModel();

            var syncTargetList = dbModel.GetSyncTargetSettings();
            if (syncTargetList != null && syncTargetList.Any())
            {
                var selDbListItems = (from s in syncTargetList
                    select new SelectListItem()
                    {
                        Selected = s.Id == targetId,
                        Text = s.Targetname,
                        Value = s.Id.ToString()
                    }).ToList();
                model.SyncTargets = selDbListItems;
            }
            return View(model);

        }

        public ActionResult InheritanceSettings()
        {
            InheritanceSettingsViewModel model = new InheritanceSettingsViewModel();
            return View(model);
        }

        /// <summary>
        /// ReStart SyncService
        /// </summary>
        /// <returns></returns>
        public JsonResult ReStartService()
        {
            SnowDbSyncServiceController service = new SnowDbSyncServiceController("MirrorService");
            service.RestartService();
            
            var validateResult = new { ServiceRestart = true };
            return Json(validateResult, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// update mail settings
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult UpdateNotification([FromBody] MailAccountSettingsViewModel model)
        {
            NotificationHandler notificationHandler = new NotificationHandler();
            notificationHandler.UpdateNotificationChanges(model.NotificationSettings);
            
            //implement update method
            return RedirectToAction("MailSettings");
        }

        public ActionResult UpdateGeneralSettings([FromBody] GeneralSettingsViewModel model)
        {
            MonitoringHandler monitoringModel = new MonitoringHandler();
            monitoringModel.UpdateMonitoringChanges(model.MonitoringSettings);

            SqlSessionSettings sqlSessionSettingsModel = new SqlSessionSettings();
            sqlSessionSettingsModel.AddOrUpdateSqlSessionChanges(model.SqlSessionSettings);

            AlertNotifySettings alertNotifySettingsModel = new AlertNotifySettings();
            alertNotifySettingsModel.AddOrUpdateAlertNotifyChanges(model.AlertNotifySettings);

            SchemaChangeNotifySettings schemaChangeNotifyModel = new SchemaChangeNotifySettings();
            schemaChangeNotifyModel.AddOrUpdateSchemaChangeNotifyChanges(model.SchemaChangeNotifySettings);

            ProcessSettings processSettings = new ProcessSettings();
            processSettings.UpdateProcessChanges(model.ProcessSettings);

            GridSettings gridSettings = new GridSettings();
            gridSettings.UpdateGridChanges(model.GridSettings);

            //implement update method
            return RedirectToAction("GeneralSettings");
        }

        public ActionResult UpdateInheritanceSettings([FromBody] InheritanceSettingsViewModel model)
        {
            //implement update method
            return RedirectToAction("InheritanceSettings");
        }

        /// <summary>
        /// update synctarget settings
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult UpdateSyncTargetSettings([FromBody] SyncTargetViewModel model)
        {
            
            //implement update method
            return RedirectToAction("SyncTargetSettings");
        }

        // GET: ServiceNowInstance Settings
        [System.Web.Mvc.Authorize(Roles = "Administrator")]
        public ActionResult ServiceNowInstanceSettings()
        {
            ServiceNowInstanceSettingsViewModel model = new ServiceNowInstanceSettingsViewModel();
            InstanzSettingsModel snowModel = new InstanzSettingsModel();

            var syncSnowList = snowModel.GetAllInstances();
            if(syncSnowList != null && syncSnowList.Any())
            {
                var selSnowListItems = (from s in syncSnowList
                                        select new SelectListItem()
                                        {
                                            Selected = false,
                                            Text = s.InstanzName,
                                            Value = s.InstanzName + ";" + s.Id
                                        }).ToList();
                model.Instances = selSnowListItems;
            }
            return View(model);

        }

        /// <summary>
        /// Manage admin user settings
        /// </summary>
        /// <returns></returns>
        [System.Web.Mvc.Authorize(Roles = "Administrator")]
        public ActionResult PrincipalSettings()
        {
            PrincipalSettingsViewModel model = new PrincipalSettingsViewModel();
            try
            {
                
                ManagementRoleModel roleModel = new ManagementRoleModel();
                PrincipalModel principalModel = new PrincipalModel();

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

                    model.ManagementRoles = listItems;
                }

                model.Principals = principalModel.GetPrincipals();
            }
            catch (Exception e)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}: Error: {e.Message}, {e.InnerException}");
            }
            

            return View(model);
        }

        /// <summary>
        /// Manage synchronization settings
        /// </summary>
        /// <returns></returns>
        [System.Web.Mvc.Authorize(Roles = "Administrator")]
        public ActionResult SynchronizationSettings()
        {
            SynchronizationSettingsViewModel model = new SynchronizationSettingsViewModel();
            return View(model);
        }

        /// <summary>
        /// Fill the Dropdownlist with all saved ServiceNowInstance Settings
        /// </summary>
        /// <param name="snowInstanceID"></param>
        /// <returns></returns>
        [System.Web.Mvc.Authorize(Roles = "Administrator")]
        public ActionResult GetServiceNowInstanceSettingsInfos(string snowInstanceID)
        {
            if(snowInstanceID != null)
            {
                InstanzSettingsModel model = new InstanzSettingsModel();
                model.Id = snowInstanceID;
                return Json(model.GetInstanceInfo(), JsonRequestBehavior.AllowGet);
            }
            return RedirectToAction("ServiceNowInstanceSettings", "Manage");
        }

        /// <summary>
        /// Fill the Dropdownlist with all saved Database Settings
        /// </summary>
        /// <param name="databaseId"></param>
        /// <returns></returns>
        [System.Web.Mvc.Authorize(Roles = "Administrator")]
        public ActionResult GetDatabaseSettingsInfos(string databaseId)
        {
            if (databaseId != null)
            {
                DatabaseSettingsModel model = new DatabaseSettingsModel();
                model.Id = databaseId;
                return Json(model.GetDatabaseInfo(), JsonRequestBehavior.AllowGet);
            }
            return RedirectToAction("DatabaseSettings", "Manage");
        }

        /// <summary>
        /// insert a new ServiceNowInstanceSettings set to the database
        /// </summary>
        /// <param name="model"></param>
        /// <param name="InsertOrNot"></param>
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        [System.Web.Mvc.Authorize(Roles = "Administrator")]
        public ActionResult InsertServiceNowInstanceSettings(InstanzSettingsModel model,string InsertOrNot)
        {
            model.InsertOrUpdateData();
            return RedirectToAction("ServiceNowInstanceSettings", "Manage");
        }

        /// <summary>
        /// Update one or more values of a specific ServiceNowInstanceSettings set 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [System.Web.Mvc.Authorize(Roles = "Administrator")]
        public ActionResult UpdateServiceNowInstanceSettings(InstanzSettingsModel model)
        {
            model.InsertOrUpdateData();
            return RedirectToAction("ServiceNowInstanceSettings", "Manage");
        }

        /// <summary>
        /// Remove one ServiceNowInstanceSettings set
        /// </summary>
        /// <param name="snowInstanceID"></param>
        /// <returns></returns>
        [System.Web.Mvc.Authorize(Roles = "Administrator")]
        public ActionResult RemoveServiceNowInstanceSettings(string snowInstanceID)
        {
            InstanzSettingsModel model = new InstanzSettingsModel();
            model.Id = snowInstanceID;
            model.RemoveData();
            return RedirectToAction("ServiceNowInstanceSettings", "Manage");
        }

        /// <summary>
        /// insert a new DatabaseSettings set to the database
        /// </summary>
        /// <param name="model"></param>
        /// <param name="InsertOrNot"></param>
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        [System.Web.Mvc.Authorize(Roles = "Administrator")]
        public ActionResult InsertDatabaseSettings(DatabaseSettingsViewModel model, string InsertOrNot)
        {
            DatabaseSettingsModel m = new DatabaseSettingsModel();
            m.Servername = model.Servername;
            m.Port = model.Port;
            m.Instancename = model.Instancename;
            m.Databasename = model.Databasename;
            m.Schemaname = model.Schemaname;
            m.Username = model.Username;
            m.Password = model.Password;
            m.InsertOrUpdateData();
            return RedirectToAction("DatabaseSettings", "Manage");
        }

        /// <summary>
        /// Update one or more values of a specific DatabaseSettings set 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        [System.Web.Mvc.Authorize(Roles = "Administrator")]
        public ActionResult UpdateDatabaseSettings(DatabaseSettingsModel model)
        {
            DatabaseSettingsModel m = new DatabaseSettingsModel();
            m.Id = model.Id;
            m.Servername = model.Servername;
            m.Port = model.Port;
            m.Instancename = model.Instancename;
            m.Databasename = model.Databasename;
            m.Schemaname = model.Schemaname;
            m.Username = model.Username;
            m.Password = model.Password;
            m.InsertOrUpdateData();
            return RedirectToAction("DatabaseSettings", "Manage");
        }

        /// <summary>
        /// Remove one DatabaseSettings set
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        [System.Web.Mvc.Authorize(Roles = "Administrator")]
        public ActionResult RemoveDatabaseSettings(DatabaseSettingsViewModel model)
        {
            DatabaseSettingsModel m = new DatabaseSettingsModel();
            m.Id = model.Id;
            m.RemoveData();
            return RedirectToAction("DatabaseSettings", "Manage");
        }

        /// <summary>
        /// Test connection
        /// </summary>
        /// <param name="server"></param>
        /// <param name="dbName"></param>
        /// <returns></returns>
        [System.Web.Mvc.Authorize(Roles = "Administrator")]
        public ActionResult TestDatabaseConnection(string server, string dbName)
        {
            if(!string.IsNullOrEmpty(server) && !string.IsNullOrEmpty(dbName))
            {
                string connString = null;
                connString += "data source=" + server.ToLower() + ";";
                connString += "initial catalog=" + dbName + ";integrated security=True;MultipleActiveResultSets=True;";
                using (SqlConnection cnn = new SqlConnection(connString))
                {
                    try
                    {
                        cnn.Open();
                        List<String> conn = new List<string>();
                        conn.Add("Connection successfull");
                        return Json(conn, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception e)
                    {
                        Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: {e.Message}, {e.InnerException}");
                        List<String> conn = new List<string>();
                        conn.Add("Connection failed");
                        return Json(conn, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            else
            {
                List<String> conn = new List<string>();
                conn.Add("Fields empty");
                return Json(conn, JsonRequestBehavior.AllowGet);
            }
            
        }

        /// <summary>
        /// Test snow connection
        /// </summary>
        /// <param name="instanceName"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [System.Web.Mvc.Authorize(Roles = "Administrator")]
        public ActionResult TestSnowConnection(string instanceName, string user, string password)
        {
            if(!string.IsNullOrEmpty(instanceName) && !string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(password))
            {
                using (ServiceNowDbSyncMgntEntities entities = new ServiceNowDbSyncMgntEntities())
                {
                    var instanceSettings = entities.InstanzSettings.FirstOrDefault(i => i.InstanzName == instanceName);
                    SynchronizationViewModel model = new SynchronizationViewModel();

                    if (instanceSettings != null)
                    {
                        var snowInstance = model.FindInstanceSetting<InstanzSettings>(instanceSettings.Id);

                        TableApiClient<SnowObject> initSnowApi = new TableApiClient<SnowObject>("sys_db_object", snowInstance);
                        RestQueryResponse<SnowObject> snowTables = initSnowApi.GetFull();

                        if (snowTables.Result.OrderBy(o => o.TableName).ToList().Count > 0)
                        {
                            List<String> conn = new List<string>();
                            conn.Add("Connection successfull");
                            return Json(conn, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            List<String> conn = new List<string>();
                            conn.Add("Connection failed");
                            return Json(conn, JsonRequestBehavior.AllowGet);
                        }
                    }
                }

                return null;
            }
            else
            {
                List<String> conn = new List<string>();
                conn.Add("Fields empty");
                return Json(conn, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Remove AdUser and his roles and not assigned credentials
        /// </summary>
        /// <param name="principalId"></param>
        /// <returns></returns>
        [System.Web.Http.Authorize(Roles = "Administrator")]
        [System.Web.Http.AcceptVerbs("GET")]
        public JsonResult RemoveAdUserAccount(Guid principalId)
        {
            string message = string.Empty;
            string redirectUrl = string.Empty;
            bool result = false;

            if (principalId != Guid.Empty)
            {
                try
                {
                    using (ServiceNowDbSyncMgntEntities entities = new ServiceNowDbSyncMgntEntities())
                    {
                        var principal = entities.Principals.FirstOrDefault(p => p.Id == principalId);
                        if (principal != null)
                        {
                            string adUserName = principal.UserName;

                            //Step 1: remove ad user principal
                            entities.Principals.Remove(principal);

                            //Step 2: commit
                            entities.SaveChanges();

                            //Step 8: if loggedInUser removes his own account. log off
                            if (adUserName.ToLower().Equals(User.Identity.Name.ToLower()))
                            {
                                //sign out
                                if (Request.Cookies["adCookie"] != null)
                                {
                                    var c = new HttpCookie("adCookie");
                                    c.Expires = DateTime.Now.AddDays(-1);
                                    Response.Cookies.Add(c);
                                    FormsAuthentication.SignOut();
                                }
                                redirectUrl = Url.Action("Login", "Account");
                            }
                            else
                            {
                                redirectUrl = Url.Action("PrincipalSettings", "Manage");
                            }

                            message = $"AdUser {adUserName} successfull removed. All credentials except other user assigned credentials are removed.";
                            Log.Info($"{MethodBase.GetCurrentMethod()?.Name}. User={User.Identity.Name} removed AdUser. {message}");
                            result = true;
                        }
                        else
                        {
                            message = "Remove AdUser not possible. User not found.";
                            Log.Info($"{MethodBase.GetCurrentMethod()?.Name}. User={User.Identity.Name} tried to removed AdUser. {message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                    Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: {message}");
                }

            }

            var validateResult = new { RemoveAdUserResult = result, Message = message, RedirectUrl = redirectUrl };
            return Json(validateResult, JsonRequestBehavior.AllowGet);
        }

        
    }

   
}

