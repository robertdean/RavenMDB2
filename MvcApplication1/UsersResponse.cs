    using System.Collections.Generic;
using ServiceStack.ServiceInterface.ServiceModel;

namespace MvcApplication1
{
    public class UsersResponse : IHasResponseStatus
    {
        public UsersResponse()
        {
            this.ResponseStatus = new ResponseStatus();
        }

        public List<UserInfo> Results { get; set; }

        public ResponseStatus ResponseStatus { get; set; }
    }
}