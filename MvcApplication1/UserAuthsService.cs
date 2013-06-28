using ServiceStack.OrmLite;
using ServiceStack.ServiceInterface.Auth;

namespace MvcApplication1
{
    public class UserAuthsService : AppServiceBase
    {
        public object Any(UserAuths request)
        {
            var response = new UserAuthsResponse
                {
                    UserSession = base.UserSession,
                    Users = Db.Select<User>(),
                    UserAuths = Db.Select<UserAuth>(),
                    OAuthProviders = Db.Select<UserOAuthProvider>(),
                };

            response.UserAuths.ForEach(x => x.PasswordHash = "[Redacted]");
            response.OAuthProviders.ForEach(x => x.AccessToken = x.AccessTokenSecret = x.RequestTokenSecret = "[Redacted]");
            if (response.UserSession != null)
                response.UserSession.ProviderOAuthAccess.ForEach(x => x.AccessToken = x.AccessTokenSecret = x.RequestTokenSecret = "[Redacted]");

            return response;
        }
    }
}