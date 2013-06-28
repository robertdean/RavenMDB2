using System;
using System.Configuration;
using System.Web.Mvc;
using Funq;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;
using ServiceStack.Api.Swagger;
using ServiceStack.Authentication.RavenDb;
using ServiceStack.CacheAccess;
using ServiceStack.CacheAccess.Providers;
using ServiceStack.Configuration;
using ServiceStack.FluentValidation;
using ServiceStack.Mvc;
using ServiceStack.OrmLite;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.Text;
using ServiceStack.WebHost.Endpoints;
using ServiceStack.Authentication.OpenId;


[assembly: WebActivator.PreApplicationStartMethod(typeof(MvcApplication1.App_Start.AppHost), "Start")]
namespace MvcApplication1.App_Start
{
	//A customizeable typed UserSession that can be extended with your own properties
	//To access ServiceStack's Session, Cache, etc from MVC Controllers inherit from ControllerBase<CustomUserSession>

    public class AppHost
		: AppHostBase
	{		
		public AppHost(IDocumentStore store) //Tell ServiceStack the name and where to find your web services
		    : base("StarterTemplate ASP.NET Host", typeof (HelloService).Assembly)
		{
		    Store = store;
		}

	    public static IDocumentStore Store;
        
        public static AppConfig AppConfig;

	    public override void Configure(Container container)
		{
            var appSettings = new AppSettings();
            AppConfig = new AppConfig(appSettings);
            container.Register(AppConfig);

            container.Register<ICacheClient>(new MemoryCacheClient());

			//Set JSON web services to return idiomatic JSON camelCase properties
			JsConfig.EmitCamelCaseNames = true;
            Plugins.Add(new SwaggerFeature());

			//Configure User Defined REST Paths
	        Routes
	            .Add<Hello>("/hello")
	            .Add<Hello>("/hello/{Name*}")
	            //Custom services for this application
	            .Add<Users>("/users/{UserIds}")
	            .Add<UserProfile>("/profile");
			//Uncomment to change the default ServiceStack configuration
			//SetConfig(new EndpointHostConfig {
			//});

			//Enable Authentication
			ConfigureAuth(container);

			//Register all your dependencies
            container.Register(new TodoRepository());			

			//Set MVC to use the same Funq IOC as ServiceStack
			ControllerBuilder.Current.SetControllerFactory(new FunqControllerFactory(container));
		}

		/* Uncomment to enable ServiceStack Authentication and CustomUserSession*/
		private void ConfigureAuth(Container container)
		{
			var appSettings = new AppSettings();

			//Default route: /auth/{provider}
			Plugins.Add(new AuthFeature(() => new CustomUserSession(),
				new IAuthProvider[] {
					new CredentialsAuthProvider(appSettings), 
					//new FacebookAuthProvider(appSettings), 
					//new TwitterAuthProvider(appSettings), 
					//new BasicAuthProvider(appSettings), 
                    new GoogleOpenIdOAuthProvider(appSettings), //Sign-in with Google OpenId
                    new YahooOpenIdOAuthProvider(appSettings), //Sign-in with Yahoo OpenId
                    new OpenIdOAuthProvider(appSettings), 
				})); 

			//Default route: /register
			Plugins.Add(new RegistrationFeature()); 
		   

			//Requires ConnectionString configured in Web.Config
            container.RegisterAs<CustomRegistrationValidator, IValidator<Registration>>();
            
            container.Register<IUserAuthRepository>(c => new RavenUserAuthRepository(Store));
            CreateAdminIfNotPresent(container);
		    
		}

	    private void CreateAdminIfNotPresent(Container container)
	    {
            var auth = container.Resolve<IUserAuthRepository>();
	        var user = auth.GetUserAuthByUserName("admin") ?? auth.CreateUserAuth(
                new UserAuth
                    {
                        UserName = "admin", 
                        Email = "admin@inq.com", 
                        PrimaryEmail = "admin@inq.com"
                    },
	            "test");
	    }

	    /**/

		public static void Start()
		{
		    var store = new DocumentStore
		        {
		            Url = "http://localhost:8080/",
                    DefaultDatabase = "svcStack"
		        }
                .Initialize();

			new AppHost(store).Init();
		}
	}
}