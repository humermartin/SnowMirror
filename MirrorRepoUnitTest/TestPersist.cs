using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MirrorRepository.Base;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MirrorRepository;

namespace MirrorRepoUnitTest
{

    [TestClass]
    public class TestPersist
    {
        [TestMethod]
        public void TestPersistSnowBase()
        {
            var entity = new SnowBase { sys_id_str = "abcd1234abcd1234abcd1234abcd1234" };
            using (var ctx = new SnowSyncInMemoryContext())
            {
                List<SnowBase> sbs = (from sb in ctx.SnowBases where sb.sys_id != null select sb).ToList();
                ctx.SnowBases.RemoveRange(sbs);
                ctx.SnowBases.Add(entity);
                ctx.SaveChanges();
                sbs = (from sb in ctx.SnowBases where sb.sys_id != null select sb).ToList();
            }
        }

        [TestMethod]
        public void TestNew()
        {
            var ctx = new SnowDbContext{ DBHOST = "localhost", DBUSER = "DBUSER", DBPWD = "DBPWD" };
            Assert.IsTrue(ctx.Database.GetDbConnection().ConnectionString.Contains("User Id=DBUSER;Password=DBPWD"));

            var ctx22 = new SnowDbContext() { DBHOST = "localhost", DBUSER = "DBUSER22", DBPWD = "DBPWD" }.New();
            Assert.IsTrue(ctx22.Database.GetDbConnection().ConnectionString.Contains("User Id=DBUSER22;Password=DBPWD"));

            ctx.DBUSER = "DUMMYUSER";
            var ntx = ctx.New();
            Assert.IsTrue(ntx.Database.GetDbConnection().ConnectionString.Contains("User Id=DUMMYUSER;Password=DBPWD"));
        }
    }
}
