using System.Net;

namespace App.ExtendMethods
{
    public static class AppExtends //phương thức mở rộng
    {
        public static void  AddStatusCodePage(this IApplicationBuilder app)
        {
            app.UseStatusCodePages(appError => {
                appError.Run(async context => {
                    var response =context.Response;
                    var code = response.StatusCode;//mã lỗi
                    var content= @$"<html>
                                    <head>
                                        <meta charset='UTF-8'/>
                                        <title>Lỗi {code}</title>
                                    </head>
                                    <body>
                                        <p style='color:red; font-size: 30px'>
                                            Có lỗi xảy ra: {code} - {(HttpStatusCode)code}
                                        </p>
                                    <body>
                                    </html>";
                    await response.WriteAsync(content);
                });  
            });// khi ứng dụng xảy ra lỗi từ 404 -> 599: trang này tạo ra các reposne trả về
        }
    }
}