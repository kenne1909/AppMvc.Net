using App.Services;
using Microsoft.AspNetCore.Mvc.Razor;

var builder = WebApplication.CreateBuilder(args);

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

app.UseRouting();

app.UseAuthentication();//Xác thực danh tính
app.UseAuthorization();//Xác thực quyền truy cập

//URL: /{controleer}/{action}/{id?}
//Abc/Xyz => Controller=Abc, gojie method Xyz
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");// url mặc định

app.MapRazorPages();//tạo ra điểm endpoint tới những trang razor trên ứng dụng

app.Run();
