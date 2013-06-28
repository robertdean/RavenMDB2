using System.Collections.Generic;
using MvcApplication1.App_Start;
using ServiceStack.ServiceInterface.Auth;

namespace MvcApplication1
{
    public class UserAuthsResponse
    {
        public UserAuthsResponse()
        {
            this.Users = new List<User>();
            this.UserAuths = new List<UserAuth>();
            this.OAuthProviders = new List<UserOAuthProvider>();
        }
        public CustomUserSession UserSession { get; set; }

        public List<User> Users { get; set; }

        public List<UserAuth> UserAuths { get; set; }

        public List<UserOAuthProvider> OAuthProviders { get; set; }
    }
}