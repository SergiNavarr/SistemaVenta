using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity.Entities;

namespace SistemaVenta.BLL.Implementacion
{
    // Clase de servicio que implementa la lógica de negocio para la entidad "Negocio"
    public class NegocioService : INegocioService
    {
        // Repositorio genérico para acceder a datos de la entidad "Negocio"
        private readonly IGenericRepository<Negocio> _repositorio;

        // Servicio encargado de subir archivos a Cloudinary
        private readonly ICloudinaryService _cloudinaryService;

        // Constructor: recibe las dependencias mediante inyección
        public NegocioService(IGenericRepository<Negocio> repositorio, ICloudinaryService cloudinaryService)
        {
            _repositorio = repositorio;
            _cloudinaryService = cloudinaryService;
        }

        // Método que obtiene los datos del negocio (solo uno con Id = 1)
        public async Task<Negocio> Obtener()
        {
            try
            {
                // Busca el negocio con IdNegocio = 1 en la base de datos
                Negocio negocio_encontrado = await _repositorio.Obtener(n => n.IdNegocio == 1);
                return negocio_encontrado;
            }
            catch
            {
                // Re-lanza la excepción para que sea manejada por capas superiores
                throw;
            }
        }

        // Método para guardar los cambios del negocio (actualiza los datos + imagen si corresponde)
        public async Task<Negocio> GuardarCambios(Negocio entidad, Stream? Logo = null, string NombreLogo = "")
        {
            try
            {
                // Obtiene el negocio actual desde el repositorio (Id = 1)
                Negocio negocio_encontrado = await _repositorio.Obtener(n => n.IdNegocio == 1);

                // Actualiza los campos con los valores recibidos en la entidad
                negocio_encontrado.NumeroDocumento = entidad.NumeroDocumento;
                negocio_encontrado.Nombre = entidad.Nombre;
                negocio_encontrado.Correo = entidad.Correo;
                negocio_encontrado.Direccion = entidad.Direccion;
                negocio_encontrado.Telefono = entidad.Telefono;
                negocio_encontrado.PorcentajeImpuesto = entidad.PorcentajeImpuesto;
                negocio_encontrado.SimboloMoneda = entidad.SimboloMoneda;

                // Si no tenía un nombre de logo, se le asigna el nuevo
                negocio_encontrado.NombreLogo = negocio_encontrado.NombreLogo == "" ? NombreLogo : negocio_encontrado.NombreLogo;

                // Si se proporciona una imagen (Logo), se sube al almacenamiento en la nube
                if (Logo != null)
                {
                    // Sube la imagen a Cloudinary y obtiene la URL
                    string urlLogo = await _cloudinaryService.SubirStorage(Logo, "carpeta_logo", negocio_encontrado.NombreLogo);

                    // Actualiza la URL del logo en el negocio
                    negocio_encontrado.UrlLogo = urlLogo;
                }

                // Guarda los cambios en el repositorio
                await _repositorio.Editar(negocio_encontrado);

                // Retorna la entidad modificada
                return negocio_encontrado;
            }
            catch
            {
                // Re-lanza cualquier excepción
                throw;
            }
        }
    }
}
