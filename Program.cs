using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebCodesBares.Data;
using WebCodesBares.Services;
using WebCodesBares.Data.Service;
using WebCodesBares.Data.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 📧 Email
builder.Services.AddScoped<IEmailSender, EmailService>();

// 💾 Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 🔐 Identity + Rôles
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// 🔐 Autorisation Admin
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// 🧠 Services métier
builder.Services.AddScoped<PanierService>();
builder.Services.AddScoped<PayPalService>();
builder.Services.AddScoped<LicenceService>();

// 🌐 Access, Cookie & Session
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true; // ✅ Consentement RGPD obligatoire
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
});
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // ✅ Session fonctionne même sans consentement
});

// 📄 MVC + Pages + API
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin", "AdminOnly");
});
builder.Services.AddControllers();

// ➕ Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebCodesBares API", Version = "v1" });
});

var app = builder.Build();

// 🛡️ Error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCookiePolicy(); // ✅ Politique cookies RGPD
app.UseSession();      // ✅ Sessions utilisateur

// ➕ Swagger Middleware
app.UseSwagger();
app.UseSwaggerUI();

// 🔓 Custom gate (hors API)
app.Use(async (context, next) =>
{
    var path = context.Request.Path;

    if (path.StartsWithSegments("/api") ||
        path.StartsWithSegments("/gate") ||
        path.StartsWithSegments("/css") ||
        path.StartsWithSegments("/js") ||
        path.Value.Contains(".") ||
        path.StartsWithSegments("/_framework"))
    {
        await next();
        return;
    }

    if (context.Session.GetString("AccessGranted") == "true")
    {
        await next();
        return;
    }

    context.Response.Redirect("/gate");
});

app.UseAuthentication();
app.UseAuthorization();

// Redirection page d'accueil
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("/KundenBarCodes/Home");
        return;
    }
    await next();
});

app.MapRazorPages();
app.MapControllers();

app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

// 🔧 HTTP/2 support (si besoin)
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

app.Run();
