## Controller
- Là một lớp kế thừa lớp Controller: Microsoft.AspNetCore.Mvc.Controller
- Action trong controller là một phương thức public (không được static)
- Action trả về bất kì kiểu dữ liệu nào, thường là IActionResult
- Các dich vụ inject vào controller qua hàm tạo
## View
- Là file .cshtml
- View cho Action lưu tại: /View/ControllerName/ActionName.cshtml
- Thêm thư mục lưu trữ View:
```
//{0} -> ten action; 
//{1} -> ten controller; 
//{2} -> ten area
options.ViewLocationFormats.Add("/MyView/{1}/{0}"+RazorViewEngine.ViewExtension);
```
## Truyền dữ liệu sang View
- Model
- ViewData
- ViewBag
- TempData

## Area
- Là tên dùng để routing
- Là cấu trúc thư mục chưa M.V.C
- Thiết lập Area cho controller bằng ```[Area("AreaName")]```
- Tạo thư mục cấu trúc
```
dotnet aspnet-codegenerator area Product
```

## Route
- endpoints.MapControllerRoute
- endpoints.MapAreaControllerRoute
- [AcceptVerbs("POST","GET")]
- [Route("pattern)]
- [HttpPost] [HttpGet]

## Url Generation
### UrlHelper: Action, ActionLink, RouteUrl, Link
```
Url.Action("PlanetInfo","Planet",new {id=1}, Context.Request.Scheme)

Url.RouteUrl("default",new {controller="First",action="HelloView",id =1,userName="LeHoan"})
```
### HtmlTagHelper: ```<a> <button> <form>```
sử dụng thuộc tính
```
<li>asp-area="Area</li>
<li>asp-action="Action</li>
<li>asp-controller="Product</li>
<li>asp-route-...="1234</li>
<li>asp-route="default</li>
```