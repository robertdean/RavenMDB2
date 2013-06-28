using System.Linq;
using ServiceStack.Authentication.OpenId;
using ServiceStack.Common;
using ServiceStack.OrmLite;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;

namespace MvcApplication1
{
    [Authenticate]
    public class UserProfileService : AppServiceBase
    {
        public object Get(UserProfile request)
        {
            var session = base.UserSession;

            var userProfile = session.TranslateTo<UserProfile>();
            userProfile.Id = int.Parse(session.UserAuthId);

            var user = Db.QueryById<User>(userProfile.Id);
            userProfile.PopulateWith(user);

            var userAuths = Db.Select<UserOAuthProvider>("UserAuthId = {0}", session.UserAuthId.ToInt());

            var googleAuth = userAuths.FirstOrDefault(x => x.Provider == GoogleOpenIdOAuthProvider.Name);
            if (googleAuth != null)
            {
                userProfile.GoogleUserId = googleAuth.UserId;
                userProfile.GoogleFullName = googleAuth.FullName;
                userProfile.GoogleEmail = googleAuth.Email;
            }

            var yahooAuth = userAuths.FirstOrDefault(x => x.Provider == YahooOpenIdOAuthProvider.Name);
            if (yahooAuth != null)
            {
                userProfile.YahooUserId = yahooAuth.UserId;
                userProfile.YahooFullName = yahooAuth.FullName;
                userProfile.YahooEmail = yahooAuth.Email;
            }

            return new UserProfileResponse
                {
                    Result = userProfile
                };
        }
    }
}