using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVenta.BLL.Interfaces
{
    public interface ICloudinaryService
    {

        Task<string> SubirStorage(Stream StreamArchivo, string CarpetaDestino, string NombreArchivo);
        Task<string> EliminarStorage(string CarpetaDestino, string NombreArchivo);

    }
}
