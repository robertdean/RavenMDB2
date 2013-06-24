using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Configuration;
using System.Collections.Generic;
using ServiceStack.Configuration;
using ServiceStack.OrmLite;
using ServiceStack.Common;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.ServiceInterface.ServiceModel;
using ServiceStack.Text;
using ServiceStack.WebHost.Endpoints;

namespace MvcApplication1
{
    //REST Resource DTO
    
    [Route("/todos",Summary = "Hello Text Service", Notes = "More description about Hello Text Service.")]
    [Route("/todos/{Ids}")]
    public class Todos : IReturn<List<Todo>>
    {
        public long[] Ids { get; set; }
        public Todos(params long[] ids)
        {
            this.Ids = ids;
        }
    }

    [Route("/todos", "POST")]
    [Route("/todos/{Id}", "PUT")]
    public class Todo : IReturn<Todo>
    {
        public long Id { get; set; }
        public string Content { get; set; }
        public int Order { get; set; }
        public bool Done { get; set; }
    }

    public class TodosService : Service
    {
        public TodoRepository Repository { get; set; }  //Injected by IOC

        public object Get(Todos request)
        {
            return request.Ids.IsEmpty()
                ? Repository.GetAll()
                : Repository.GetByIds(request.Ids);
        }

        public object Post(Todo todo)
        {
            return Repository.Store(todo);
        }

        public object Put(Todo todo)
        {
            return Repository.Store(todo);
        }

        public void Delete(Todos request)
        {
            Repository.DeleteByIds(request.Ids);
        }
    }
    
    public class TodoRepository
    {
        List<Todo> todos = new List<Todo>();
        
        public List<Todo> GetByIds(long[] ids)
        {
            return todos.Where(x => ids.Contains(x.Id)).ToList();
        }

        public List<Todo> GetAll()
        {
            return todos;
        }

        public Todo Store(Todo todo)
        {
            var existing = todos.FirstOrDefault(x => x.Id == todo.Id);
            if (existing == null)
            {
                var newId = todos.Count > 0 ? todos.Max(x => x.Id) + 1 : 1;
                todo.Id = newId;
                todos.Add(todo);
            }
            else
            {
                existing.PopulateWith(todo);
            }
            return todo;
        }

        public void DeleteByIds(params long[] ids)
        {
            todos.RemoveAll(x => ids.Contains(x.Id));
        }
    }


/*  Example calling above Service with ServiceStack's C# clients:

	var client = new JsonServiceClient(BaseUri);
	List<Todo> all = client.Get(new Todos());           // Count = 0

	var todo = client.Post(
	    new Todo { Content = "New TODO", Order = 1 });      // todo.Id = 1
	all = client.Get(new Todos());                      // Count = 1

	todo.Content = "Updated TODO";
	todo = client.Put(todo);                            // todo.Content = Updated TODO

	client.Delete(new Todos(todo.Id));
	all = client.Get(new Todos());                      // Count = 0

*/


    //Define Request and Response DTOs
    [Route("/hellotext/{Name}", Summary = "Hello Text Service", Notes = "More description about Hello Text Service.")]
    public class HelloText
    {
        [ApiMember(Name = "Name", Description = "Name Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string Name { get; set; }
    }

    [Route("/helloimage/{Name}", Summary = "Hello Image Service", Notes = "More description about Hello Image Service.")]
    public class HelloImage
    {
        [ApiMember(Name = "Name", Description = "Name Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string Name { get; set; }

        [ApiMember(Name = "Width", ParameterType = "path", DataType = "int", IsRequired = false)]
        public int? Width { get; set; }
        [ApiMember(Name = "Height", ParameterType = "path", DataType = "int", IsRequired = false)]
        public int? Height { get; set; }
        public int? FontSize { get; set; }
        public string Foreground { get; set; }
        public string Background { get; set; }
    }

    [Route("/hello/{Name}", Summary = "Hello Service", Notes = "More description about Hello Service")]
    public class Hello : IReturn<HelloResponse>
    {
        [ApiMember(Name = "Name", Description = "Name Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string Name { get; set; }
    }

    public class HelloResponse
    {
        public string Result { get; set; }
    }

    //Implementation
    public class HelloService : Service
    {
        [AddHeader(ContentType = "text/plain")]
        public object Get(HelloText request)
        {
            return "<h1>Hello, {0}!</h1>".Fmt(request.Name);
        }

        [AddHeader(ContentType = "image/png")]
        public object Get(HelloImage request)
        {
            var width = request.Width.GetValueOrDefault(640);
            var height = request.Height.GetValueOrDefault(360);
            var bgColor = request.Background != null ? Color.FromName(request.Background) : Color.ForestGreen;
            var fgColor = request.Foreground != null ? Color.FromName(request.Foreground) : Color.White;

            var image = new Bitmap(width, height);
            using (var g = Graphics.FromImage(image))
            {
                g.Clear(bgColor);

                var drawString = "Hello, {0}!".Fmt(request.Name);
                var drawFont = new Font("Times", request.FontSize.GetValueOrDefault(40));
                var drawBrush = new SolidBrush(fgColor);
                var drawRect = new RectangleF(0, 0, width, height);

                var drawFormat = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Center
                };

                g.DrawString(drawString, drawFont, drawBrush, drawRect, drawFormat);

                var ms = new MemoryStream();
                image.Save(ms, ImageFormat.Png);
                return ms;
            }
        }

        public object Get(Hello request)
        {
            return new HelloResponse { Result = "Hello, {0}!".Fmt(request.Name) };

            //C# client can call with:
            //var response = client.Get(new Hello { Name = "ServiceStack" });
        }
    }
}
