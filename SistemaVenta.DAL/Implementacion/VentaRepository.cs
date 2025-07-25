using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SistemaVenta.DAL.DBContext;
using SistemaVenta.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using SistemaVenta.Entity.Entities;

namespace SistemaVenta.DAL.Implementacion
{
    public class VentaRepository : GenericRepository<Venta>, IVentaRepository
    {
        private readonly DBVENTAContext _dbContext;

        public VentaRepository(DBVENTAContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Venta> Registrar(Venta entidad)
        {
            Venta ventaGenerada = new Venta();

            // Iniciar una transacción para asegurar la atomicidad de la operación
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    // Recorrer cada detalle de la venta y actualizar el stock del producto
                    foreach (DetalleVenta dv in entidad.DetalleVenta)
                    {
                        Producto producto_encontrado = _dbContext.Productos.Where(p => p.IdProducto == dv.IdProducto).First();

                        producto_encontrado.Stock -= dv.Cantidad;

                        _dbContext.Productos.Update(producto_encontrado);
                    }

                    await _dbContext.SaveChangesAsync();

                    // Obtengo el correlativo de venta y actualizo su último número
                    NumeroCorrelativo correlativo = _dbContext.NumeroCorrelativos.Where(n => n.Gestion == "venta").First();

                    correlativo.UltimoNumero += 1;
                    correlativo.FechaActualizacion = DateTime.Now;


                    _dbContext.NumeroCorrelativos.Update(correlativo);
                    await _dbContext.SaveChangesAsync();

                    // Generar el número de venta con el formato adecuado

                    string ceros = string.Concat(Enumerable.Repeat("0", correlativo.CantidadDigitos.Value));
                    string numeroVenta = ceros + correlativo.UltimoNumero.ToString();
                    numeroVenta = numeroVenta.Substring(numeroVenta.Length - correlativo.CantidadDigitos.Value, correlativo.CantidadDigitos.Value);

                    entidad.NumeroVenta = numeroVenta;

                    await _dbContext.Venta.AddAsync(entidad);
                    await _dbContext.SaveChangesAsync();


                    // Completar la entidad de venta generada con los detalles
                    ventaGenerada = entidad;

                    // Completar la transacción
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("Error al registrar la venta", ex);
                }

                return ventaGenerada;
            }
        }

        public async Task<List<DetalleVenta>> Reporte(DateTime FechaInicio, DateTime FechaFin)
        {
            // Obtener el resumen de ventas entre las fechas especificadas
            // Con los include y theninclude añado a la lista de detalleventa las entidades relacionadas
            List<DetalleVenta> listaResumen = await _dbContext.DetalleVenta
                .Include(v => v.IdVentaNavigation)
                .ThenInclude(u => u.IdUsuarioNavigation)
                .Include(v => v.IdVentaNavigation)
                .ThenInclude(tdv => tdv.IdTipoDocumentoVentaNavigation)
                .Where(dv => dv.IdVentaNavigation.FechaRegistro.Value.Date >= FechaInicio.Date && 
                dv.IdVentaNavigation.FechaRegistro.Value.Date <= FechaFin.Date).ToListAsync();

            return listaResumen;
        }
    }

}
