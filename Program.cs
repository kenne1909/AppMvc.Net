using System.Net;
using App.Data;
using App.ExtendMethods;
using App.Models;
using App.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();//đăng kí các dịch vụ liên quan đến razor
builder.Services.AddDbContext<AppDbcontext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AppDbcontext") ?? throw new InvalidOperationException("Connection string 'AppDbcontext' not found.")));
// builder.Services.AddTransient(typeof(ILogger<>),typeof(Logger<>));// dịch vụ này tự đăng kí

builder.Services.AddOptions();
var mailSetting  = builder.Configuration.GetSection("MailSettings");
builder.Services.Configure<MailSettings>(mailSetting);
builder.Services.AddSingleton<IEmailSender, SendMailService>();

builder.Services.AddSingleton<IdentityErrorDescriber,AppIdentityErrorDescriber>();

builder.Services.AddDbContext<AppDbContext>(options =>{
    // string? connectString=builder.Configuration.GetConnectionString("MyBlogContext");
    // options.UseSqlServer(connectString);

    options.UseSqlServer(builder.Configuration.GetConnectionString("AppMvcConnectionString"));
});

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

// đăng ký Identity
builder.Services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

// Truy cập IdentityOptions
builder.Services.Configure<IdentityOptions> (options => {
    // Thiết lập về Password
    options.Password.RequireDigit = false; // Không bắt phải có số
    options.Password.RequireLowercase = false; // Không bắt phải có chữ thường
    options.Password.RequireNonAlphanumeric = false; // Không bắt ký tự đặc biệt
    options.Password.RequireUppercase = false; // Không bắt buộc chữ in
    options.Password.RequiredLength = 3; // Số ký tự tối thiểu của password
    options.Password.RequiredUniqueChars = 1; // Số ký tự riêng biệt

    // Cấu hình Lockout - khóa user
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes (5); // Khóa 5 phút
    options.Lockout.MaxFailedAccessAttempts = 3; // Thất bại 3 lầ thì khóa
    options.Lockout.AllowedForNewUsers = true;

    // Cấu hình về User.
    options.User.AllowedUserNameCharacters = // các ký tự đặt tên user
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;  // Email là duy nhất

    // Cấu hình đăng nhập.
    options.SignIn.RequireConfirmedEmail = true;            // Cấu hình xác thực địa chỉ email (email phải tồn tại)
    options.SignIn.RequireConfirmedPhoneNumber = false;     // Xác thực số điện thoại
    options.SignIn.RequireConfirmedAccount = true;         // Người dùng phải xác nhận tài khoản
});

builder.Services.ConfigureApplicationCookie(options =>{
    options.LoginPath = "/login/";
    options.LogoutPath= "/logout/";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied"; //đường dẫn tới trang khi user bị cấm truy cập
});

builder.Services.AddAuthentication()
                .AddGoogle(options =>{
                    IConfigurationSection? gconfi = builder.Configuration.GetSection("Authentication:Google");

                    string clientId = gconfi["ClientId"] ?? throw new InvalidOperationException("Google ClientId chưa được định cấu hình.");
                    string clientSecret = gconfi["ClientSecret"] ?? throw new InvalidOperationException("Google ClientSecret chưa được định cấu hình.");
                    
                    options.ClientId =clientId;
                    options.ClientSecret = clientSecret;
                    // https://localhost:7134/signin-google nếu k thiết lập CallbackPath 
                    options.CallbackPath = "/dang-nhap-tu-google";
                })
                .AddFacebook(options => {
                    IConfigurationSection? gconfi = builder.Configuration.GetSection("Authentication:Facebook");
                    string appId = gconfi["AppId"] ?? throw new InvalidOperationException("Facebook AppId chưa được định cấu hình.");
                    string appSecret = gconfi["AppSecret"] ?? throw new InvalidOperationException("Facebook AppSecret chưa được định cấu hình.");

                    options.AppId =appId;
                    options.AppSecret = appSecret;
                    // https://localhost:7134/signin-google nếu k thiết lập CallbackPath 
                    options.CallbackPath = "/dang-nhap-tu-facebook";
                })
                // .AddFacebook()
                // .AddTwitter()
                // .AddMicrosoftAccount()
                ;

builder.Services.AddAuthorization(option =>{
    option.AddPolicy("ViewManageMenu",builder=>{
        builder.RequireAuthenticatedUser();
        builder.RequireRole(RoleName.Administrator);
    });
});

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
app.UseStaticFiles(new StaticFileOptions(){
    FileProvider= new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(),"Uploads")
    ),
    RequestPath = "/contents"//khi truy cập 1 file tĩnh  contents/1.jpg => Uploads/1.jpg
});

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
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");


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
