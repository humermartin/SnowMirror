using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MirrorWeb.ViewModels
{
    public class SnowDbSyncRegisterViewModel
    {
        /// <summary>
        /// Gets or sets the UserName
        /// </summary>
        [Display(Name = "UserName")]
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the Password
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the ConfirmPassword
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        
    }
}