using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreIdentityExample.CustomValidations;
using AspNetCoreIdentityExample.Models.Authentication;
using AspNetCoreIdentityExample.Models.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCoreIdentityExample
{
    public class Startup
    {
        public IConfiguration Configuration { get; set; }
        public Startup(IConfiguration configuration) => Configuration = configuration;
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(_ => _.UseSqlServer(Configuration["ConnectionStrings:SqlServerConnectionString"]));
            services.AddIdentity<AppUser, AppRole>(_ =>
            {
                _.Password.RequiredLength = 5; //En az kaç karakterli olması gerektiğini belirtiyoruz.
                _.Password.RequireNonAlphanumeric = false; //Alfanumerik zorunluluğunu kaldırıyoruz.
                _.Password.RequireLowercase = false; //Küçük harf zorunluluğunu kaldırıyoruz.
                _.Password.RequireUppercase = false; //Büyük harf zorunluluğunu kaldırıyoruz.
                _.Password.RequireDigit = false; //0-9 arası sayısal karakter zorunluluğunu kaldırıyoruz.

                _.User.RequireUniqueEmail = true; //Email adreslerini tekilleştiriyoruz.
                _.User.AllowedUserNameCharacters = "abcçdefghiıjklmnoöpqrsştuüvwxyzABCÇDEFGHIİJKLMNOÖPQRSŞTUÜVWXYZ0123456789-._@+"; //Kullanıcı adında geçerli olan karakterleri belirtiyoruz.
            }).AddPasswordValidator<CustomPasswordValidation>()
              .AddUserValidator<CustomUserValidation>()
              .AddErrorDescriber<CustomIdentityErrorDescriber>().AddEntityFrameworkStores<AppDbContext>()
              .AddDefaultTokenProviders(); ;

            services.ConfigureApplicationCookie(_ =>
            {
                _.LoginPath = new PathString("/User/Login");
                _.Cookie = new CookieBuilder
                {
                    Name = "AspNetCoreIdentityExampleCookie", //Oluşturulacak Cookie'yi isimlendiriyoruz.
                    HttpOnly = false, //Kötü niyetli insanların client-side tarafından Cookie'ye erişmesini engelliyoruz.
                    Expiration = TimeSpan.FromMinutes(2), //Oluşturulacak Cookie'nin vadesini belirliyoruz.
                    SameSite = SameSiteMode.Lax, //Top level navigasyonlara sebep olmayan requestlere Cookie'nin gönderilmemesini belirtiyoruz.
                    SecurePolicy = CookieSecurePolicy.Always //HTTPS üzerinden erişilebilir yapıyoruz.
                };
                _.SlidingExpiration = true; //Expiration süresinin yarısı kadar süre zarfında istekte bulunulursa eğer geri kalan yarısını tekrar sıfırlayarak ilk ayarlanan süreyi tazeleyecektir.
                _.ExpireTimeSpan = TimeSpan.FromMinutes(2); //CookieBuilder nesnesinde tanımlanan Expiration değerinin varsayılan değerlerle ezilme ihtimaline karşın tekrardan Cookie vadesi burada da belirtiliyor.
            });

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseStatusCodePages();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvc(_ => _.MapRoute("Default", "{controller=Home}/{action=Index}/{id?}"));
        }
    }
}
