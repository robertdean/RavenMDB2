using System.Data;
using System.Linq;
using MvcApplication1.App_Start;
using ServiceStack.OrmLite;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;

namespace MvcApplication1
{
    public abstract class AppServiceBase : Service
    {
        public CustomUserSession UserSession
        {
            get
            {
                return SessionAs<CustomUserSession>();
            }
        }

    }
}