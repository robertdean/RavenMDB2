using ServiceStack.ServiceHost;

namespace MvcApplication1
{
    [Route("/userauths")]
    public class UserAuths
    {
        public int[] Ids { get; set; }
    }
}