using System.Collections.Generic;
using MirrorRepository.Model;

namespace MirrorWeb.ViewModels.Manage
{
    public class ScriptCommandViewModel
    {
        /// <summary>
        /// Gets or sets the script command list
        /// </summary>
        public List<ScriptCommand> ScriptCommandList { get; set; }

        /// <summary>
        /// Gets or sets the total count
        /// </summary>
        public int ScriptCommandListTotalCount { get; set; }
    }
}