/* .NET 10 | JWT | Identity | Swagger | SQL Server
   Seed: papéis, empresa Valoriza e AdminValoriza vinculado a ela */

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Valoriza.API.Data;
using Valoriza.API.Filters;
using Valoriza.API.Models;
using Valoriza.API.Services;

var builder = WebApplication.CreateBuilder(args);

// BANCO (SQL Server)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// IDENTITY
// Hash de senha: PBKDF2 (padrão do Identity)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// SERVIÇOS DE DOMÍNIO
builder.Services.AddScoped<ProgressoService>();
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>(); // auditoria (denúncias etc.)

// JWT
var jwt = builder.Configuration.GetSection("Jwt");
var key = jwt["Key"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwt["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwt["Audience"],
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// CORS (mobile / front)
builder.Services.AddCors(o => o.AddPolicy("AllowAll",
    p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// CONTROLLERS + FILTROS GLOBAIS
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
    options.Filters.Add<GlobalExceptionFilter>();
});
builder.Services.Configure<ApiBehaviorOptions>(o =>
    o.SuppressModelStateInvalidFilter = true);

// SWAGGER
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Valoriza API",
        Version = "v1",
        Description = "API da plataforma Valoriza – DEI"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT no header. Ex.: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// SEED
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    // await context.Database.MigrateAsync(); // opcional em dev

    // Papéis
    foreach (var role in new[] { "AdminValoriza", "AdminEmpresa", "GestorDEI", "Colaborador" })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Empresa Valoriza
    var empresaValoriza = await context.Empresas
        .FirstOrDefaultAsync(e =>
            e.NomeFantasia == "Valoriza" ||
            e.RazaoSocial.Contains("Valoriza"));

    if (empresaValoriza == null)
    {
        empresaValoriza = new Empresa
        {
            RazaoSocial = "Valoriza Tecnologia LTDA",
            NomeFantasia = "Valoriza",
            Cnpj = "12.345.678/0001-90",
            Segmento = "Tecnologia",
            Plano = "Profissional",
            ValorAssinatura = 1299.00m,
            CicloCobranca = "Mensal",
            StatusAssinatura = "Ativa",
            Ativa = true,
            DataInicioAssinatura = DateTime.UtcNow
        };
        context.Empresas.Add(empresaValoriza);
        await context.SaveChangesAsync();
        Console.WriteLine("Empresa Valoriza criada.");
    }

    // AdminValoriza vinculado à empresa Valoriza
    var seed = builder.Configuration.GetSection("SeedAdmin");
    var email = seed["Email"] ?? "oliver@valoriza.com";
    var senha = seed["Senha"] ?? "Senha@123";
    var nome = seed["Nome"] ?? "Oliver Valentim Carvalho Santos";

    var admin = await userManager.FindByEmailAsync(email);
    if (admin == null)
    {
        admin = new ApplicationUser
        {
            UserName = email,
            Email = email,
            NomeCompleto = nome,
            EmailConfirmed = true,
            Ativo = true,
            EmpresaId = empresaValoriza.Id,
            DataCadastro = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(admin, senha);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "AdminValoriza");
            Console.WriteLine($"Admin criado: {email} / empresa Valoriza (Id={empresaValoriza.Id})");
        }
        else
        {
            Console.WriteLine("Falha ao criar admin: " +
                string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }
    else if (admin.EmpresaId == null)
    {
        admin.EmpresaId = empresaValoriza.Id;
        await userManager.UpdateAsync(admin);
        Console.WriteLine($"Admin {email} vinculado à empresa Valoriza (Id={empresaValoriza.Id})");
    }
}

// PIPELINE
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection(); // HTTPS
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();