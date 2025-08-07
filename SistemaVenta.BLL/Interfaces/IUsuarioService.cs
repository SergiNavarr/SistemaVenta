using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SistemaVenta.Entity.Entities;

namespace SistemaVenta.BLL.Interfaces
{
    public interface IUsuarioService
    {

        Task<List<Usuario>> Lista();
        Task<Usuario> Crear(Usuario entidad, Stream? Foto = null, string Nombrefoto = "", string UrlPlantillaCorreo = "");
        Task<Usuario> Editar(Usuario entidad, Stream? Foto = null, string Nombrefoto = "");
        Task<bool> Eliminar(int IdUsuario);
        Task<Usuario> ObtenerPorCredenciales(string Correo, string Clave);
        Task<Usuario> ObtenerPorId(int IdUsuario);
        Task<bool> GuardarPerfil(Usuario Entidad);
        Task<bool> CambiarClave(int IdUsuario, string ClaveActual, string NuevaClave);
        Task<bool> RestablecerClave(string Correo, string UrlPlantillaCorreo);
    }
}
