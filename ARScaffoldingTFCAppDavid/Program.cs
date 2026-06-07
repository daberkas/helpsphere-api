using API_TFCAppDavid.Contexto;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace API_TFCAppDavid
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var firebaseProjectId = builder.Configuration["Firebase:ProjectId"];

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
           
            builder.Services.AddDbContext<ASPContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddHttpClient();

            builder.Services.AddCors(options => options.AddPolicy("AllowWebClient", policy =>
            {
                policy.WithOrigins("http://localhost:5500",
                                    "http://127.0.0.1:5500",
                                    "https://daberkas.com",
                                    "https://app.daberkas.com"
                                    )
                                    .AllowAnyHeader()
                                    .AllowAnyMethod();
            }));

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = $"https://securetoken.google.com/{firebaseProjectId}";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = $"https://securetoken.google.com/{firebaseProjectId}",
                        ValidateAudience = true,
                        ValidAudience = firebaseProjectId,
                        ValidateLifetime = true
                    };
                });

            builder.Services.AddAuthorization();

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c => {   
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "HelpSphere API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Introduce: Bearer {token de Firebase}",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT"
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

            // Configure the HTTP request pipeline.
            // Swagger se mantiene habilitado en todos los entornos por motivos académicos,
            // permitiendo la consulta y prueba de los endpoints durante la evaluación del proyecto.
            // En un entorno de producción real, esta funcionalidad debería limitarse al entorno
            // de desarrollo para evitar exponer información interna de la API.
//            if (app.Environment.IsDevelopment())
//            {
                app.UseSwagger();
                app.UseSwaggerUI();
            //            }
            // La terminación HTTPS se realiza mediante Cloudflare Tunnel y Traefik.
            // Por este motivo no se utiliza UseHttpsRedirection en la configuración de la API,
            // ya que el tráfico llega a la API a través de HTTP desde el túnel seguro.
            //            app.UseHttpsRedirection();

            app.UseCors("AllowWebClient");

            app.UseAuthentication();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
