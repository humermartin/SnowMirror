using MirrorRepository.Data.SnowDbSyncMgnt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MirrorRepository.Helpers
{
    public class ReflectionHelper
    {
        public static TEntity CopyProps<TEntity>(TEntity me)
        {
            TEntity clone = (TEntity)me.GetType().GetConstructor(new Type[] { }).Invoke(new object[] { });
            foreach (var p in clone.GetType().GetProperties())
            {
                if (p.CanWrite && p.CanWrite)
                {
                    object val = p.GetValue(me);
                    if (val != null)
                    {
                        var cpif = val.GetType().GetInterfaces().FirstOrDefault(i => i.Name == typeof(ICopyable<>).Name);
                        if (cpif != null)
                        {
                            MethodInfo method = null;
                            object copy = null;
                            try
                            {
                                method = cpif.GetMethod("Copy");
                                copy = method.Invoke(val, new object[] { });
                                p.SetValue(clone, copy);
                            }
                            catch (Exception e)
                            {
                                throw new Exception("cannot set from: " + me + ", prop:" + p.Name + ", meth=" + method + ", val=" + val.GetType() + ":" + val.ToString()
                                    + ", cpy=" + copy + " on:" + clone, e);
                            }
                        }
                        else
                        {
                            try
                            {
                                p.SetValue(clone, val);
                            }
                            catch (Exception e)
                            {
                                throw new Exception("cannot setfrom: " + me + ", prop:" + p.Name + ", val=" + val.GetType() + ":" + val.ToString() + " on:" + clone, e);
                            }
                        }
                    }
                }
            }
            return clone;
        }
    }
}
