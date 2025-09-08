using log4net;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MirrorRepository.Helpers
{
    public class LogHelp
    {
        public static void Info(ILog Log, Exception e, object o)
        {
            if (e is DbEntityValidationException)
            {
                Log.Info("ValidationFailed: " + o + ": " +
                    string.Join("\n", ((DbEntityValidationException)e).EntityValidationErrors
                        .Select(ve => ve.Entry.GetType() + ":" + string.Join(",", ve.ValidationErrors.Select(err => err.ErrorMessage))))
                    );
            } else
            {
                Log.Info("exception", e);
            }
        }
    }
}
