using ServiceStack.Common;
using ServiceStack.OrmLite;
using ServiceStack.ServiceInterface;

namespace MvcApplication1
{
    public class UsersService : Service
    {
        public object Get(Users request)
        {
            var response = new UsersResponse();

            if (request.UserIds.Length == 0)
                return response;

            var users = Db.Ids<User>(request.UserIds);

            var userInfos = users.ConvertAll(x => x.TranslateTo<UserInfo>());

            return new UsersResponse
                {
                    Results = userInfos
                };
        }
    }
}