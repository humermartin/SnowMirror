using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MirrorWeb.AdAuthorizationFilter
{
    public class AuthenticationResult
    {
        /// <summary>
        /// Gets or sets the Error message
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// Gets or sets the IsSuccess value
        /// </summary>
        public Boolean IsSuccess => String.IsNullOrEmpty(ErrorMessage);

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="errorMessage"></param>
        public AuthenticationResult(string errorMessage = null)
        {
            ErrorMessage = errorMessage;
        }
    }
}