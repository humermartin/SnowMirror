using System.Collections.Generic;
namespace MirrorWeb.Helpers.Kendo
{
    /// <summary>
    /// class Kendo Filter operator mapper
    /// </summary>
    public class KendoFilterOperatorMapper
    {
        /// <summary>
        /// Kendo operator automapper
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static string Map(KendoFilterOperator condition)
        {
            string mappedOperatorAndValue = string.Empty;

            switch (condition.Operator)
            {
                case KendoConstants.KendoOperatorEqual:
                    mappedOperatorAndValue = $"= '{condition.Value}'";
                    break;

                case KendoConstants.KendoOperatorNotEqual:
                    mappedOperatorAndValue = $"<> '{condition.Value}'";
                    break;

                case KendoConstants.KendoOperatorGreaterThan:
                    mappedOperatorAndValue = $"> '{condition.Value}'";
                    break;

                case KendoConstants.KendoOperatorGreaterOrEqual:
                    mappedOperatorAndValue = $">= '{condition.Value}'";
                    break;

                case KendoConstants.KendoOperatorLesserThan:
                    mappedOperatorAndValue = $"< '{condition.Value}'";
                    break;

                case KendoConstants.KendoOperatorLesserOrEqual:
                    mappedOperatorAndValue = $"<= '{condition.Value}'";
                    break;

                case KendoConstants.KendoOperatorIsNull:
                    mappedOperatorAndValue = $"is null";
                    break;

                case KendoConstants.KendoOperatorIsNotNull:
                    mappedOperatorAndValue = $"is not null";
                    break;

                case KendoConstants.KendoOperatorContains:
                    mappedOperatorAndValue = $"like '%{condition.Value}%'";
                    break;

                case KendoConstants.KendoOperatorDoesNotContain:
                    mappedOperatorAndValue = $"not like '%{condition.Value}%'";
                    break;

                case KendoConstants.KendoOperatorStartsWith:
                    mappedOperatorAndValue = $"like '{condition.Value}%'";
                    break;

                case KendoConstants.KendoOperatorDoesNotStartWith:
                    mappedOperatorAndValue = $"not like '{condition.Value}%'";
                    break;

                case KendoConstants.KendoOperatorEndsWith:
                    mappedOperatorAndValue = $"like '%{condition.Value}'";
                    break;

                case KendoConstants.KendoOperatorDoesNotEndWith:
                    mappedOperatorAndValue = $"not like '%{condition.Value}'";
                    break;

                case KendoConstants.KendoOperatorIsEmpty:
                    mappedOperatorAndValue = $"= ''";
                    break;

                case KendoConstants.KendoOperatorIsNotEmpty:
                    mappedOperatorAndValue = $"<> ''";
                    break;

                default:
                    break;
            }

            return mappedOperatorAndValue;
        }

        /// <summary>
        /// cast varchar to date
        /// </summary>
        /// <param name="kendoFilterOperator"></param>
        /// <returns></returns>
        public static KendoFilterOperator ValidateFieldDataType(KendoFilterOperator kendoFilterOperator)
        {
            var dateFields = new List<string>{ "MigrationTime", "Inbetriebdatum", "FirstEmailSent", "LastEmailSent", "InstallStartTime", "InstallFinishTime", "LastChange", "JobImportTime" };
            if (dateFields.Contains(kendoFilterOperator.Field))
            {
                kendoFilterOperator.Field = $"cast({kendoFilterOperator.Field} as date)";
            }

            return kendoFilterOperator;
        }
    }
}