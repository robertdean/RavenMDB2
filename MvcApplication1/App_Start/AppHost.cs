using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Mvc;
using Funq;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;
using ServiceStack.Api.Swagger;
using ServiceStack.Authentication.RavenDb;
using ServiceStack.Common;
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
                }
            }
            
            //Resolve the DbFactory from the IOC and persist the user info
            authService.TryResolve<IDbConnectionFactory>().Run(db => db.Save(user));
            
        }
    }

    public class AppConfig
    {
        public AppConfig(IResourceManager appSettings)
        {
            this.Env = appSettings.Get("Env", Env.Local);
            this.EnableCdn = appSettings.Get("EnableCdn", false);
            this.CdnPrefix = appSettings.Get("CdnPrefix", "");
            this.AdminUserNames = appSettings.Get("AdminUserNames", new List<string>());
        }
        
        public Env Env { get; set; }
        public bool EnableCdn { get; set; }
        public string CdnPrefix { get; set; }
        public List<string> AdminUserNames { get; set; }
    }

    public enum Env
    {
        Local,
        Dev,
        Test,
        Prod,
    }

	public class AppHost
		: AppHostBase
	{		
		public AppHost() //Tell ServiceStack the name and where to find your web services
			: base("StarterTemplate ASP.NET Host", typeof(HelloService).Assembly) { }

	    public static IDocumentStore Store;
        
        public static AppConfig AppConfig;

	    public override void Configure(Container container)
		{
            var appSettings = new AppSettings();
            AppConfig = new AppConfig(appSettings);
            container.Register(AppConfig);
            
            //Store = new EmbeddableDocumentStore { RunInMemory = true};
            
            Store = new DocumentStore { ConnectionStringName = "RavenDB" };
	        Store.Initialize();

			//Set JSON web services to return idiomatic JSON camelCase properties
			JsConfig.EmitCamelCaseNames = true;
            Plugins.Add(new SwaggerFeature());

			//Configure User Defined REST Paths
			Routes
			  .Add<Hello>("/hello")
			  .Add<Hello>("/hello/{Name*}");

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
		    try
		    {

			//Requires ConnectionString configured in Web.Config
            container.RegisterAs<CustomRegistrationValidator, IValidator<Registration>>();
            container.Register<IUserAuthRepository>(c => new RavenUserAuthRepository(Store));
                CreateAdminIfNotPresent(container);
		    }
		    catch (Exception ex)
		    {
		        
                var test= ConfigurationManager.ConnectionStrings["RavendDB"].ToString()
		        throw new Exception(test);
		    }
		    
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
			new AppHost().Init();
		}
	}
    public class CustomRegistrationValidator : RegistrationValidator
    {
        public CustomRegistrationValidator()
        {
            RuleSet(ApplyTo.Post, () =>
            {
                RuleFor(x => x.DisplayName).NotEmpty();
            });
        }
    }


}