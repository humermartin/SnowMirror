using System;
using System.Linq;
using System.Reflection;
using log4net;
using Newtonsoft.Json;
using MirrorRepository.Constants;
using MirrorRepository.Data.SnowDbSyncMgnt;

namespace MirrorRepository.Model
{
    public class SqlSessionSettings
    {
        /// <summary>
        /// Member which holds the log4net logger
        /// </summary>
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        /// <summary>
        /// Gets or sets the enable kill session value
        /// </summary>
        public bool EnableKilleSession { get; set; }

        /// <summary>
        /// Gets or sets the sql user name
        /// </summary>
        public string SqlUserName { get; set; }

        /// <summary>
        /// Gets or sets the stored procedure name
        /// </summary>
        public string StoredProcedure { get; set; }


        /// <summary>
        /// update sql session values
        /// </summary>
        /// <param name="sqlSessionSettingsModel"></param>
        public void AddOrUpdateSqlSessionChanges(SqlSessionSettings sqlSessionSettingsModel)
        {
            try
            {
                if (sqlSessionSettingsModel != null)
                {
                    using (ServiceNowDbSyncMgntEntities ctx = new ServiceNowDbSyncMgntEntities())
                    {
                        var sqlSession = ctx.AppSettings.FirstOrDefault(a => a.Key == SnowDbSyncConstants.SqlSessionSettings);

                        if (sqlSession != null)
                        {
                            SqlSessionSettings sqlSessionSettings = JsonConvert.DeserializeObject<SqlSessionSettings>(sqlSession?.Value);

                            sqlSessionSettings.EnableKilleSession = sqlSessionSettingsModel.EnableKilleSession;
                            sqlSessionSettings.SqlUserName = sqlSessionSettingsModel.SqlUserName;
                            sqlSessionSettings.StoredProcedure = sqlSessionSettingsModel.StoredProcedure;

                            var serializedSqlSessionSettings = JsonConvert.SerializeObject(sqlSessionSettings);

                            sqlSession.Value = serializedSqlSessionSettings;

                            ctx.SaveChanges();
                        }
                        else
                        {
                            AppSettings appSettings = new AppSettings();

                            var serializedSqlSessionSettings = JsonConvert.SerializeObject(sqlSessionSettingsModel);

                            appSettings.Id = Guid.NewGuid();
                            appSettings.Key = SnowDbSyncConstants.SqlSessionSettings;
                            appSettings.Value = serializedSqlSessionSettings;
                            appSettings.Created = DateTime.Now;

                            ctx.AppSettings.Add(appSettings);
                            ctx.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}: {ex.Message}{ex.InnerException}");
            }
        }
    }
}
