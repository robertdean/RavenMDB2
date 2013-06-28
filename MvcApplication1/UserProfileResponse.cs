using ServiceStack.ServiceInterface.ServiceModel;

namespace MvcApplication1
{
    public class UserProfileResponse
    {
        public UserProfile Result { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }
}