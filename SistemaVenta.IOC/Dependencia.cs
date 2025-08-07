using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using SistemaVenta.DAL.DBContext;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.DAL.Implementacion;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.BLL.Implementacion;

namespace SistemaVenta.IOC
{

    // Inyecion de dependencias para el proyecto SistemaVenta
    public static class Dependencia
    {

        public static void InyectarDependencia(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddDbContext<DBVENTAContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("CadenaSQL")
                );
            });

            services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            services.AddScoped<IVentaRepository, VentaRepository>();

            services.AddScoped<ICorreoService, CorreoService>();

            services.AddScoped<IUtilidadesService, UtilidadesService>();

            services.AddScoped<IRolService, RolService>();

            services.AddScoped<ICloudinaryService, CloudinaryService>();

            services.AddScoped<IUsuarioService, UsuarioService>();

            services.AddScoped<INegocioService, NegocioService>();

            services.AddScoped<ICategoriaService, CategoriaService>();

        }

    }
}
