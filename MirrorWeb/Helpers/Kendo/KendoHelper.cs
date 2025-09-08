using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using MirrorRepository.Model;

namespace MirrorWeb.Helpers.Kendo
{
    public class KendoHelper
    {
        public List<SnowTables> HandleFilter(SyncSchedulerModel model, string filter)
        {
            List<SnowTables> filtered = null;
            KendoFilter kendoFilter = JsonConvert.DeserializeObject<KendoFilter>(filter);

            if (model != null && model.SnowTables.Any())
            {
                bool isSinglColDualFieldFilter = SingleColDualFieldFilter(kendoFilter);

                //we have only on column filter with an or logical
                KendoFilterOperator conditionSingle = new KendoFilterOperator();
                KendoFilterOperator conditionLeft = new KendoFilterOperator();
                KendoFilterOperator conditionRight = new KendoFilterOperator();

                int i = 1;
                foreach (var kfSub in kendoFilter.Filter)
                {
                    if (kfSub.Filters != null)
                    {
                        foreach (var kf in kfSub.Filters)
                        {
                            switch (i)
                            {
                                case 1:
                                    conditionLeft = kf;
                                    break;
                                case 2:
                                    conditionRight = kf;
                                    break;
                            }
                            i++;
                        }
                        if (conditionLeft != null || conditionRight != null)
                        {
                            //call filter method
                            filtered = GetFilteredByCondition(kfSub.Logic, conditionLeft, conditionRight, model.SnowTables);
                            model.SnowTableNames = string.Join(";", filtered.Select(t => t.Name));
                        }
                    }
                    else
                    {
                        if (isSinglColDualFieldFilter)
                        {
                            switch (i)
                            {
                                case 1:
                                    if (conditionLeft != null)
                                    {
                                        conditionLeft.Field = kfSub.Field;
                                        conditionLeft.Operator = kfSub.Operator;
                                        conditionLeft.Value = kfSub.Value;
                                    }
                                    break;
                                case 2:
                                    if (conditionRight != null)
                                    {
                                        conditionRight.Field = kfSub.Field;
                                        conditionRight.Operator = kfSub.Operator;
                                        conditionRight.Value = kfSub.Value;
                                    }
                                    break;
                            }
                            i++;
                        }
                        else
                        {
                            conditionSingle.Field = kfSub.Field;
                            conditionSingle.Operator = kfSub.Operator;
                            conditionSingle.Value = kfSub.Value;

                            filtered = GetFilteredBySingleCondition(conditionSingle, model.SnowTables);
                            model.SnowTableNames = string.Join(";", filtered.Select(t => t.Name));
                        }
                    }
                }

                if (isSinglColDualFieldFilter)
                {
                    if (conditionLeft != null || conditionRight != null)
                    {
                        //call filter method
                        filtered = GetFilteredByCondition(kendoFilter.Logic, conditionLeft, conditionRight, model.SnowTables);
                        model.SnowTableNames = string.Join(";", filtered.Select(t => t.Name));
                    }
                }
            }

            return filtered;
        }

        /// <summary>
        /// kendo filter json does not wrap a single column dual field filter in any array
        /// so we have to check such filter and return marker as left and right condition
        /// </summary>
        /// <param name="kendoFilter"></param>
        /// <returns></returns>
        private bool SingleColDualFieldFilter(KendoFilter kendoFilter)
        {
            if (kendoFilter != null && kendoFilter.Filter.Count == 2)
            {
                var filtersCount = kendoFilter.Filter.Where(f => f.Filters != null);
                if (!filtersCount.Any() && kendoFilter.Filter.First().Field.Equals(kendoFilter.Filter.Last().Field))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get clients filtered by single condition
        /// </summary>
        /// <param name="conditionSingle"></param>
        /// <param name="snowTables"></param>
        /// <returns></returns>
        private List<SnowTables> GetFilteredBySingleCondition(KendoFilterOperator conditionSingle, List<SnowTables> snowTables)
        {
            List<SnowTables> filteredList = new List<SnowTables>();

            switch (conditionSingle.Operator)
            {
                case "eq":

                    filteredList = snowTables.Where(s => s.Name == conditionSingle.Value).ToList();
                    break;

                case "neq":

                    filteredList = snowTables.Where(s => s.Name != conditionSingle.Value).ToList();
                    break;

                case "isnull":

                    filteredList = snowTables.Where(s => s.Name == null).ToList();
                    break;

                case "contains":

                    filteredList = snowTables.Where(s => conditionSingle.Value.Contains(s.Name)).ToList();
                    break;

                case "doesnotcontain":

                    filteredList = snowTables.Where(s => !conditionSingle.Value.Contains(s.Name)).ToList();
                    break;

                case "startswith":

                    filteredList = snowTables.Where(s => s.Name.StartsWith(conditionSingle.Value)).ToList();
                    break;

                case "endswith":

                    filteredList = snowTables.Where(s => s.Name.EndsWith(conditionSingle.Value)).ToList();
                    break;

                case "doesnotstartwith":

                    filteredList = snowTables.Where(s => !s.Name.StartsWith(conditionSingle.Value)).ToList();
                    break;

                case "doesnotendwith":

                    filteredList = snowTables.Where(s => !s.Name.EndsWith(conditionSingle.Value)).ToList();
                    break;

                case "isempty":

                    filteredList = snowTables.Where(s => s.Name == string.Empty).ToList();
                    break;

                case "isnotempty":

                    filteredList = snowTables.Where(s => s.Name != string.Empty).ToList();
                    break;
            }

            return filteredList;
        }

        /// <summary>
        /// Get clients filtered by conditions
        /// </summary>
        /// <param name="conditionLogic"></param>
        /// <param name="conditionLeft"></param>
        /// <param name="conditionRight"></param>
        /// <param name="snowTables"></param>
        /// <returns></returns>
        private List<SnowTables> GetFilteredByCondition(string conditionLogic, KendoFilterOperator conditionLeft, KendoFilterOperator conditionRight, List<SnowTables> snowTables)
        {
            //Todo - not needed at the moment
            var filteredSnowtables = snowTables;
            return filteredSnowtables;
        }
    }
}