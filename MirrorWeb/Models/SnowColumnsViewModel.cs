using System.Collections.Generic;
using MirrorRepository.SnowTableApi;

namespace MirrorWeb.Models
{
    public class SnowColumnsViewModel
    {
        /// <summary>
        /// Gets or sets the ServiceNow available columnList
        /// </summary>
        public List<SnowColumns> SnowColumnList { get; set; }

        /// <summary>
        /// Gets or sets the ServiceNow available columns count
        /// </summary>
        public int SnowColumnListTotalCount { get; set; }

        /// <summary>
        /// Gets or sets the saved selected columns from table
        /// </summary>
        public List<string> SnowColumns { get; set; }
    }
}