using System.Collections.Generic;
using ServiceStack.Authentication.OpenId;
using ServiceStack.Common;
using ServiceStack.OrmLite;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;

namespace MvcApplication1.App_Start
{
    public class CustomUserSession : AuthUserSession
    {
        public string CustomId { get; set; }

        public override void OnAuthenticated(
            IServiceBase authService,
            IAuthSession session,
            IOAuthTokens tokens,
            Dictionary<string, string> authInfo)
        {
            base.OnAuthenticated(authService, session, tokens, authInfo);

            //Populate all matching fields from this session to your own custom User table
            var user = session.TranslateTo<User>();
            user.Id = int.Parse(session.UserAuthId);
            //user.GravatarImageUrl64 = !session.Email.IsNullOrEmpty() ? CreateGravatarUrl(session.Email, 64) : null;

            foreach (var authToken in session.ProviderOAuthAccess)
            {
                //    //if (authToken.Provider == FacebookAuthProvider.Name)
                //    //{
                //    //    user.FacebookName = authToken.DisplayName;
                //    //    user.FacebookFirstName = authToken.FirstName;
                //    //    user.FacebookLastName = authToken.LastName;
                //    //    user.FacebookEmail = authToken.Email;
                //    //}
                //    //else 
                if (authToken.Provider == GoogleOpenIdOAuthProvider.Name)
                {
                    user.GoogleUserId = authToken.UserId;
                    user.GoogleFullName = authToken.FullName;
                    user.GoogleEmail = authToken.Email;
                    if (user.Email == null) user.Email = user.GoogleEmail;
                    if (user.UserName == null) user.UserName = user.GoogleUserId;
                }
                else if (authToken.Provider == YahooOpenIdOAuthProvider.Name)
                {
                    user.YahooUserId = authToken.UserId;
                    user.YahooFullName = authToken.FullName;
                    user.YahooEmail = authToken.Email;
                }
            }


            var adminUserNames = AppHost.AppConfig.AdminUserNames;

            if (AppHost.AppConfig.AdminUserNames.Contains(session.UserAuthName)
                && !session.HasRole(RoleNames.Admin))
            {
                using (var assignRoles = authService.ResolveService<AssignRolesService>())
                {
                    assignRoles.Post(new AssignRoles
                        {
                            UserName = session.UserAuthName,
                            Roles = { RoleNames.Admin }
                        });
                    //authService.TryResolve<IDbConnectionFactory>().Run(db => db.Save(user));
                }
            }
            
            //Resolve the DbFactory from the IOC and persist the user info
            
        }
    }
}