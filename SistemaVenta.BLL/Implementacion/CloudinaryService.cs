using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity.Entities;

using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace SistemaVenta.BLL.Implementacion
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly IGenericRepository<Configuracion> _repositorio;

        public CloudinaryService(IGenericRepository<Configuracion> repositorio)
        {
            _repositorio = repositorio;
        }

        private async Task<Cloudinary> ObtenerCliente()
        {
            // Obtener la configuración de Cloudinary desde la base de datos
            var query = await _repositorio.Consultar(c => c.Recurso == "Cloudinary");

            //Transformar la consulta en un diccionario
            var config = query.ToDictionary(c => c.Propiedad, c => c.Valor);

            //Crear una instancia de Account con los datos obtenidos
            var cuenta = new Account(
                config["cloudName"],
                config["apiKey"],
                config["apiSecret"]
            );

            return new Cloudinary(cuenta);

        }


        public async Task<string> SubirStorage(Stream StreamArchivo, string CarpetaDestino, string NombreArchivo)
        {

            //obtener el cliente de Cloudinary
            var cloudinary = await ObtenerCliente();

            //Preparar los parámetros de subida
            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(NombreArchivo, StreamArchivo),
                Folder = CarpetaDestino,
                PublicId = Path.GetFileNameWithoutExtension(NombreArchivo),
                Overwrite = true, // Sobrescribir el archivo si ya existe
            };

            // Subir el archivo a Cloudinary
            var resultado = await cloudinary.UploadAsync(uploadParams);

            if (resultado.StatusCode == HttpStatusCode.OK)
            {
                return resultado.SecureUrl.ToString();
            }
            else
            {
                throw new Exception($"Error al subir el archivo: {resultado.Error.Message}");
            }
        }
        public async Task<string> EliminarStorage(string CarpetaDestino, string NombreArchivo)
        {
            //obtener el cliente de Cloudinary
            var cloudinary = await ObtenerCliente();

            //Preparar la ruta del archivo a eliminar
            var publicId = $"{CarpetaDestino}/{Path.GetFileNameWithoutExtension(NombreArchivo)}";

            // Preparar los parámetros de eliminación
            var deleteParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Image // Especificar el tipo de recurso
            };

            // Eliminar el archivo de Cloudinary
            var resultado = await cloudinary.DestroyAsync(deleteParams);

            // Verificar el resultado de la eliminación y devolver un mensaje adecuado
            if (resultado.StatusCode == HttpStatusCode.OK)
            {
                return resultado.Result == "ok" ? "Archivo eliminado correctamente." : "No se pudo eliminar el archivo.";
            }
            else
            {
                throw new Exception($"Error al eliminar el archivo: {resultado.Error.Message}");
            }

        }

        
    }
}
