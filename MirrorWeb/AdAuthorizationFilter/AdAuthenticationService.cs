using System;
using System.Configuration;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Claims;
using Microsoft.Owin.Security;
using MirrorRepository;
using MirrorRepository.Data.SnowDbSyncMgnt;

namespace MirrorWeb.AdAuthorizationFilter
{
    public class AdAuthenticationService
    {
        /// <summary>
        /// Sets the authentication manaager
        /// </summary>
        private readonly IAuthenticationManager authenticationManager;

        /// <summary>
        /// Holds the principal context
        /// </summary>
        public static PrincipalContext principalContext;

        public AdAuthenticationService(IAuthenticationManager authenticationManager)
        {
            this.authenticationManager = authenticationManager;
        }

        /// <summary>
        /// Check if username and password matches existing account in AD. 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="authentication"></param>
        /// <returns></returns>
        public AuthenticationResult SignIn(string username, string authentication)
        {
            // authenticates against your Domain AD
            ContextType authenticationType = ContextType.Domain;

            var domain = ConfigurationManager.AppSettings["Domain"];
            principalContext = new PrincipalContext(authenticationType, domain, username, authentication);
            bool isAuthenticated = false;
            UserPrincipal userPrincipal = null;
            
            try
            {
                userPrincipal = UserPrincipal.FindByIdentity(principalContext, username);
                if (userPrincipal != null)
                {

                    isAuthenticated = principalContext.ValidateCredentials(username, authentication, ContextOptions.Negotiate);
                }
            }
            catch (Exception)
            {
                return new AuthenticationResult("Username or Password is not correct");
            }

            using (ServiceNowDbSyncMgntEntities entities = new ServiceNowDbSyncMgntEntities())
            {
                var adUserContext =
                    entities.Principals.FirstOrDefault(p => p.UserName.ToLower().Equals(username.ToLower()));

                if (userPrincipal == null)
                {
                    return new AuthenticationResult("Username or Password is not correct");
                }

                if (userPrincipal.IsAccountLockedOut())
                {
                    // here can be a security related discussion wether it is worth 
                    // revealing this information. User is locked in Active Directory
                    return new AuthenticationResult("Your account is locked.");
                }

                if (userPrincipal.Enabled.HasValue && userPrincipal.Enabled.Value == false)
                {
                    // here can be a security related discussion weather it is worth 
                    // revealing this information
                    return new AuthenticationResult("Your account is disabled");
                }

                if (adUserContext != null && adUserContext.Active == false)
                {
                    return new AuthenticationResult("Your account is deactived.");
                }

                if (adUserContext == null)
                {
                    return new AuthenticationResult("Your account is not permitted.");
                }

            }

            var identity = CreateIdentity(userPrincipal);

            authenticationManager.SignOut(Startup.MyAuthentication.ApplicationCookie);
            authenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = false }, identity);


            return new AuthenticationResult();
        }

        /// <summary>
        /// Create Identity object
        /// </summary>
        /// <param name="userPrincipal"></param>
        /// <returns></returns>
        private ClaimsIdentity CreateIdentity(UserPrincipal userPrincipal)
        {
            var identity = new ClaimsIdentity(Startup.MyAuthentication.ApplicationCookie, ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            identity.AddClaim(new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "Active Directory"));
            identity.AddClaim(new Claim(ClaimTypes.Name, userPrincipal.SamAccountName));
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userPrincipal.SamAccountName));
            identity.AddClaim(new Claim(ClaimTypes.GivenName, userPrincipal.GivenName));
            
            using (ServiceNowDbSyncMgntEntities entities = new ServiceNowDbSyncMgntEntities())
            {
                string userPrincipalRole = string.Empty;
                Principals principal = entities.Principals.FirstOrDefault(p => p.UserName.ToLower().Equals(userPrincipal.SamAccountName.ToLower()));
                if (principal != null)
                {
                    ManagementRole userRole = entities.ManagementRole.FirstOrDefault(r => r.Id == principal.RoleId);
                    if (userRole != null)
                    {
                        userPrincipalRole = userRole.RoleName;
                    }
                }
                identity.AddClaim(new Claim(ClaimTypes.Role, userPrincipalRole));
                
            }

            if (!string.IsNullOrWhiteSpace(userPrincipal.EmailAddress))
            {
                identity.AddClaim(new Claim(ClaimTypes.Email, userPrincipal.EmailAddress));
            }

            return identity;
        }
    }
}