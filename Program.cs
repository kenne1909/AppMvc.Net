using System.Net;
using App.ExtendMethods;
using App.Models;
using App.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>{
    // string? connectString=builder.Configuration.GetConnectionString("MyBlogContext");
    // options.UseSqlServer(connectString);

    options.UseSqlServer(builder.Configuration.GetConnectionString("AppMvcConnectionString"));
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();//đăng kí các dịch vụ liên quan đến razor
// builder.Services.AddTransient(typeof(ILogger<>),typeof(Logger<>));// dịch vụ này tự đăng kí

builder.Services.Configure<RazorViewEngineOptions>(options => {
    // /View/Controller/Action.cshtml mặc định
    // /MyView/Controller/Action.cshtml

    //{0} -> ten action; {1} -> ten controller; {2} -> ten area
    options.ViewLocationFormats.Add("/MyView/{1}/{0}"+RazorViewEngine.ViewExtension);//RazorViewEngine.ViewExtension phần mở rộng
});

builder.Services.AddSingleton<ProductService>();
// builder.Services.AddSingleton<ProductService,ProductService>();
// builder.Services.AddSingleton(typeof(ProductService));
// builder.Services.AddSingleton(typeof(ProductService),typeof(ProductService));

builder.Services.AddSingleton<PlanetService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.AddStatusCodePage();    // tùy biến respone lỗi: 404 -> 599

app.UseRouting();   //endpointRoutingMiddleware

app.UseAuthentication();//Xác thực danh tính
app.UseAuthorization();//Xác thực quyền truy cập

// //URL: /{controleer}/{action}/{id?}
// //Abc/Xyz => Controller=Abc, gojie method Xyz
// app.MapControllerRoute(
//     name: "default",
//     pattern: "{controller=Home}/{action=Index}/{id?}");// url mặc định

// app.MapRazorPages();//tạo ra điểm endpoint tới những trang razor trên ứng dụng

// /sayhi
app.MapGet("/sayhi",async (context) => {
        await context.Response.WriteAsync($"Hello ASP.NET {DateTime.Now}");
});

// app.MapControllers               // cấu hình để tạo ra các endpoint tới những controller, sau đó những điểm endpoint phải định nghĩa trực tiếp ở trong controller ở trong  attribute
// app.MapControllerRoute           // 
// app.MapDefaultControllerRoute
// app.MapAreaControllerRoute       // Tạo ra những điểm endpoint đến những controller mà controller nằm trong những area(mục riêng)

app.MapControllers();

app.MapControllerRoute(
    name: "First",
    pattern: "{url:regex(^((xemsanpham)|(viewproduct))$)}/{id:range(2,4)}", //{url} địa chỉ bất kì vd: asfasdfsadfasdf/3
    defaults: new {
        controller = "First",
        action = "ViewProduct"
    }
    // constraints: new{
    //     url = new RegexRouteConstraint(@"^((xemsanpham)|(viewproduct))$"),
    //     // id = new RangeRouteConstraint(1,4)// từ 1 -> 4
    //     //IRouteConstraints: new StringRouteConstraint("")
    //     // có thể sử dụng RegaxRouteConstraint
    // }   // đối tượng chỉ ra các ràng buộc
);

app.MapAreaControllerRoute(
    name: "product",
    pattern: "{controller}/{action=Index}/{id?}",
    areaName: "ProductManage"
);

//chỉ thực hiện trên những controller k có area
app.MapControllerRoute(
    name: "default",   //name
    pattern: "{controller=Home}/{action=Index}/{id?}"//URL: start-here/First/HelloView
    // defaults: new{
    //     //controller ="First",
    //     //action ="ViewProduct",
    //     //id=3// pattern: "start-here{id}" thì ở đây k có id vẫn đc và nếu pattern k có id thì nó sẽ mặc định là 3
    // }// kiểu vô danh chứa các tham số của route có các ket : controller; action;area
);


//sử dụng các attibute để tạo ra các route -> đc viết trực tiếp trong controller or action
// [AcceptVerbs]
// [Route]
// [HttpGet]
// [HttpPost]
// [HttpPut]
// [HttpHead]
// [HttpPatch]

app.MapRazorPages();
app.Run();


//dotnet aspnet-codegenerator -h
