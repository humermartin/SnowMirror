using log4net;
using Microsoft.EntityFrameworkCore;
using MirrorRepository.Data.SnowDbSyncMgnt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MirrorRepository.Model.SnowDbSyncMgnt
{
    public class BaseModel
    {
        protected readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        public List<TEntity> AllCopy<TEntity>() where TEntity : class
        {
            using (ServiceNowDbSyncMgntEntities snowEntities = new ServiceNowDbSyncMgntEntities())
            {
                return snowEntities.Set<TEntity>().ToList()
                    .Select(e => (e is ICopyable<TEntity> ? (TEntity)((ICopyable<TEntity>)e).Copy() : e))
                    .ToList();
            }
        }

        public List<TEntity> All<TEntity>() where TEntity : class
        {
            using (ServiceNowDbSyncMgntEntities snowEntities = new ServiceNowDbSyncMgntEntities())
            {
                return snowEntities.Set<TEntity>().ToList();
            }
        }

        public TEntity Find<TEntity>(Guid id) where TEntity : class
        {
            using (ServiceNowDbSyncMgntEntities snowEntities = new ServiceNowDbSyncMgntEntities())
            {
                var entity = snowEntities.Set<TEntity>().Find(id);
                if (entity is ICopyable<TEntity>)
                    return ((ICopyable<TEntity>)entity).Copy();
                return entity;
            }
        }

        public TEntity FindInternal<TEntity>(Guid guid) where TEntity : class
        {
            object entity = Find<TEntity>(guid);
            if (entity is Data.SnowDbSyncMgnt.Synchronization) {
                Data.SnowDbSyncMgnt.Synchronization sync = (Data.SnowDbSyncMgnt.Synchronization)entity;
                if (sync.InstanzSettings != null)
                {
                    sync.InstanzSettings.Password = Decryptdata(sync.InstanzSettings.Password);
                    sync.InstanzSettings.ProxyUserPassword = Decryptdata(sync.InstanzSettings.ProxyUserPassword);
                }
                if (sync.DatabaseSettings != null)
                {
                    sync.DatabaseSettings.Password = Decryptdata(sync.DatabaseSettings.Password);
                }
            }
            return (TEntity)entity;
        }

        public TEntity FindInstanceSetting<TEntity>(Guid guid) where TEntity : class
        {
            object entity = Find<TEntity>(guid);
            if (entity is Data.SnowDbSyncMgnt.InstanzSettings)
            {
                Data.SnowDbSyncMgnt.InstanzSettings snowInstance = (Data.SnowDbSyncMgnt.InstanzSettings)entity;
                if (snowInstance != null)
                {
                    snowInstance.Password = Decryptdata(snowInstance.Password);
                    snowInstance.ProxyUserPassword = Decryptdata(snowInstance.ProxyUserPassword);
                }
                
            }
            return (TEntity)entity;
        }

        public TEntity FindDatabaseSetting<TEntity>(Guid guid) where TEntity : class
        {
            object entity = Find<TEntity>(guid);
            if (entity is Data.SnowDbSyncMgnt.DatabaseSettings)
            {
                Data.SnowDbSyncMgnt.DatabaseSettings dbSettings = (Data.SnowDbSyncMgnt.DatabaseSettings)entity;
                if (dbSettings != null)
                {
                    dbSettings.Password = Decryptdata(dbSettings.Password);
                }

            }
            return (TEntity)entity;
        }

        public TEntity Update<TEntity>(TEntity entity) where TEntity : class
        {
            try
            {
                using (var smeCtx = new ServiceNowDbSyncMgntEntities())
                {
                    smeCtx.Entry(entity).State = EntityState.Modified;
                    smeCtx.SaveChanges();
                    return entity;
                }
            }
            catch (Exception e)
            {
                Log.Info("cannot update: " + entity, e.GetBaseException());
                throw;
            }
        }



        /// <summary>
        /// Return a list with all DatabaseSettings sets
        /// </summary>
        /// <returns></returns>
        public IList<DatabaseSettings> GetAllDatabases()
        {
            using (ServiceNowDbSyncMgntEntities snowEntities = new ServiceNowDbSyncMgntEntities())
            {
                return snowEntities.DatabaseSettings.ToList();
            }
        }

        /// <summary>
        /// Return a list with all InstanzSettings sets
        /// </summary>
        /// <returns></returns>
        public IList<InstanzSettings> GetAllInstanzSettings()
        {
            using (ServiceNowDbSyncMgntEntities snowEntities = new ServiceNowDbSyncMgntEntities())
            {
                return snowEntities.InstanzSettings.ToList();
            }
        }


        /// <summary>
        /// algorithm to decrypt an encrypted string
        /// </summary>
        /// <param name="encryptpwd"></param>
        /// <returns></returns>
        public static string Decryptdata(string encryptpwd)
        {
            if (string.IsNullOrEmpty(encryptpwd))
                return null;
            string decryptpwd = string.Empty;
            UTF8Encoding encodepwd = new UTF8Encoding();
            Decoder Decode = encodepwd.GetDecoder();
            byte[] todecode_byte = Convert.FromBase64String(encryptpwd);
            int charCount = Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
            char[] decoded_char = new char[charCount];
            Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
            decryptpwd = new String(decoded_char);
            return decryptpwd;
        }

        /// <summary>
        /// algorith to encrypt a plain password
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string Encryptdata(string password)
        {
            if (string.IsNullOrEmpty(password))
                return null;
            string strmsg = string.Empty;
            byte[] encode = new byte[password.Length];
            encode = Encoding.UTF8.GetBytes(password);
            strmsg = Convert.ToBase64String(encode);
            return strmsg;
        }
    }
}
