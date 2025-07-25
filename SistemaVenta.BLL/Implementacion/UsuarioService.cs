using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using System.Net;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.Entity.Entities;
using SistemaVenta.DAL.Interfaces;

namespace SistemaVenta.BLL.Implementacion
{
    public class UsuarioService : IUsuarioService
    {

        private readonly IGenericRepository<Usuario> _repositorio;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IUtilidadesService _utilidadesService;
        private readonly ICorreoService _correoService;

        public UsuarioService(
            IGenericRepository<Usuario> repositorio,
            ICloudinaryService cloudinaryService,
            IUtilidadesService utilidadesService,
            ICorreoService correoService)
        {
            _repositorio = repositorio;
            _cloudinaryService = cloudinaryService;
            _utilidadesService = utilidadesService;
            _correoService = correoService;
        }

        //Traer la lista de usuarios junto con los roles
        public async Task<List<Usuario>> Lista()
        {
            IQueryable<Usuario> query = await _repositorio.Consultar();
            return query.Include(r => r.IdRolNavigation).ToList();
        }

        public async Task<Usuario> Crear(Usuario entidad, Stream? Foto = null, string Nombrefoto = "", string UrlPlantillaCorreo = "")
        {
            //Valdar existencia de usuario
            Usuario usuario_existe = await _repositorio.Obtener(u => u.Correo == entidad.Correo);
            if(usuario_existe != null)
            {
                throw new TaskCanceledException("El correo ya existe");
            }

            try{
                //Generar clave y convertirla a SHA256
                string clave_generada = _utilidadesService.GenerarClave();
                entidad.Clave = _utilidadesService.ConvertirSha256(clave_generada);

                entidad.NombreFoto = Nombrefoto;

                //Si se proporciona una foto, subirla al almacenamiento en la nube
                if (Foto != null)
                {
                    string urlFoto = await _cloudinaryService.SubirStorage(Foto, "carpeta_usuario", Nombrefoto);
                    entidad.UrlFoto = urlFoto;

                }

                //Crear el usuario en la base de datos
                Usuario usuario_creado = await _repositorio.Crear(entidad);

                if(usuario_creado.IdUsuario == 0)
                {
                    throw new TaskCanceledException("No se pudo crear el usuario");
                }

                //Si se proporciona una URL de plantilla de correo, enviar el correo al usuario
                if (UrlPlantillaCorreo != "")
                {
                    //Reemplazar los datos en la plantilla de correo
                    UrlPlantillaCorreo = UrlPlantillaCorreo.Replace("[correo]", usuario_creado.Correo).Replace("[clave]", clave_generada);

                    //inicializar la variable para almacenar el contenido del correo
                    string htmlCorreo = "";

                    //Realizar la solicitud HTTP para obtener el contenido de la plantilla de correo
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UrlPlantillaCorreo);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    //Verificar si la respuesta es exitosa
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        //Leer el contenido de la respuesta
                        //el uso de using garantiza que los recursos se liberen correctamente
                        using (Stream dataStream = response.GetResponseStream())
                        {
                            StreamReader readerStream = null;

                            if (response.CharacterSet == null)
                            {
                                readerStream = new StreamReader(dataStream);
                            }
                            else
                            {
                                readerStream = new StreamReader(dataStream, Encoding.GetEncoding(response.CharacterSet));
                            }

                            htmlCorreo = readerStream.ReadToEnd();
                            response.Close();
                            readerStream.Close();

                        }
                    }

                    if(htmlCorreo != "")
                    {
                        await _correoService.EnviarCorreo(usuario_creado.Correo,
                            "Cuenta Creada",
                            htmlCorreo
                        );
                    }
                }

                IQueryable<Usuario> query = await _repositorio.Consultar(u => u.IdUsuario == usuario_creado.IdUsuario);
                usuario_creado = query.Include(r => r.IdRolNavigation).First();

                return usuario_creado;
            }
            catch(Exception ex)
            {
                throw new Exception($"Error al crear el usuario: {ex.Message}", ex);
            }
        }
        public async Task<Usuario> Editar(Usuario entidad, Stream? Foto = null, string Nombrefoto = "")
        {
            //Validar existencia de usuario
            Usuario usuario_existe = await _repositorio.Obtener(u => u.Correo == entidad.Correo && u.IdUsuario != entidad.IdUsuario);
            if (usuario_existe != null)
            {
                throw new TaskCanceledException("El correo ya existe");
            }

            try
            {
                //Buscar el usuario por IdUsuario
                IQueryable<Usuario> queryUsuario = await _repositorio.Consultar(u => u.IdUsuario == entidad.IdUsuario);

                //crear el nuevo usuario
                Usuario usuario_editar = queryUsuario.First();
                usuario_editar.Nombre = entidad.Nombre;
                usuario_editar.Correo = entidad.Correo;
                usuario_editar.Telefono = entidad.Telefono;
                usuario_editar.IdRol = entidad.IdRol;

                //Si no hay nombre de foto, usar el proporcionado
                if (usuario_editar.NombreFoto == "")
                    usuario_editar.NombreFoto = Nombrefoto;

                //Si se proporciona una foto, subirla al almacenamiento en la nube
                if (Foto!= null)
                {
                    string urlFoto = await _cloudinaryService.SubirStorage(Foto, "carpeta_usuario", usuario_editar.NombreFoto);
                    usuario_editar.UrlFoto = urlFoto;
                }

                //Realizar la edición del usuario
                bool respuesta = await _repositorio.Editar(usuario_editar);

                //Si la respuesta es falsa, lanzar una excepción
                if (!respuesta)
                    throw new TaskCanceledException("No se pudo modificar el usuario");

                //Obtener el usuario editado con su rol
                Usuario usuario_editado = queryUsuario.Include(r => r.IdRolNavigation).First();

                //retornar el usuario editado
                return usuario_editado;
            }
            catch
            {
                throw;
            }
        }
        public async Task<bool> Eliminar(int IdUsuario)
        {
            try
            {
                //Buscar el usuario por IdUsuario
                Usuario usuario_encontrado = await _repositorio.Obtener(u => u.IdUsuario == IdUsuario);

                //Si no se encuentra el usuario, lanzar una excepción
                if (usuario_encontrado == null)
                    throw new TaskCanceledException("El usuario no existe");

                //Se guarda el nombre de la foto para eliminarla del almacenamiento en la nube
                string nombreFoto = usuario_encontrado.NombreFoto;
                
                //Eliminar el usuario
                bool respuesta = await _repositorio.Eliminar(usuario_encontrado);

                //Si se elimino correctamente, eliminar la foto del almacenamiento en la nube
                if (respuesta)
                    await _cloudinaryService.EliminarStorage("carpeta_usuario", nombreFoto);

                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<Usuario> ObrenerPorCredenciales(string Correo, string Clave)
        {
            string clave_encriptada = _utilidadesService.ConvertirSha256(Clave);

            Usuario usuario_encontrado = await _repositorio.Obtener(u => u.Correo.Equals(Correo) && u.Clave.Equals(clave_encriptada));

            return usuario_encontrado;
        }
        public async Task<Usuario> ObtenerPorId(int IdUsuario)
        {
            IQueryable<Usuario> query = await _repositorio.Consultar(u => u.IdUsuario == IdUsuario);

            Usuario resultado = query.Include(r => r.IdRolNavigation).FirstOrDefault();

            return resultado;
        }
        public async Task<bool> GuardarPerfil(Usuario Entidad)
        {
            try
            {
                Usuario usuario_encontrado = await _repositorio.Obtener(u => u.IdUsuario == Entidad.IdUsuario);

                if(usuario_encontrado == null)
                {
                    throw new TaskCanceledException("El usuario no existe");
                }

                usuario_encontrado.Correo = Entidad.Correo;
                usuario_encontrado.Telefono = Entidad.Telefono;

                bool respuesta = await _repositorio.Editar(usuario_encontrado);

                return respuesta;
            }
            catch
            {
                throw;
            }
        }
        public async Task<bool> CambiarClave(int IdUsuario, string ClaveActual, string NuevaClave)
        {
            try
            {
                Usuario usuario_encontrado = await _repositorio.Obtener(u => u.IdUsuario == IdUsuario);

                if (usuario_encontrado == null)
                {
                    throw new TaskCanceledException("El usuario no existe");
                }

                if(usuario_encontrado.Clave != _utilidadesService.ConvertirSha256(ClaveActual))
                {
                    throw new TaskCanceledException("La clave actual es incorrecta");
                }

                usuario_encontrado.Clave = _utilidadesService.ConvertirSha256(NuevaClave);
                bool respuesta = await _repositorio.Editar(usuario_encontrado);
                return respuesta;

            }
            catch
            {
                throw;
            }
        }
        public async Task<bool> RestablecerClave(string Correo, string UrlPlantillaCorreo)
        {
            try
            {
                Usuario usuario_encontrado = await _repositorio.Obtener(u => u.Correo == Correo);

                if (usuario_encontrado == null)
                {
                    throw new TaskCanceledException("El se encontro usuario asociado al correo proporcionado");
                }

                //generar una nueva clave
                string clave_generada = _utilidadesService.GenerarClave();

                //Actualizar la clave del usuario encriptada
                usuario_encontrado.Clave = _utilidadesService.ConvertirSha256(clave_generada);


                //Reemplazar los datos en la plantilla de correo
                UrlPlantillaCorreo = UrlPlantillaCorreo.Replace("[clave]", clave_generada);

                //inicializar la variable para almacenar el contenido del correo
                string htmlCorreo = "";

                //Realizar la solicitud HTTP para obtener el contenido de la plantilla de correo
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UrlPlantillaCorreo);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                //Verificar si la respuesta es exitosa
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    //Leer el contenido de la respuesta
                    //el uso de using garantiza que los recursos se liberen correctamente
                    using (Stream dataStream = response.GetResponseStream())
                    {
                        StreamReader readerStream = null;

                        if (response.CharacterSet == null)
                        {
                            readerStream = new StreamReader(dataStream);
                        }
                        else
                        {
                            readerStream = new StreamReader(dataStream, Encoding.GetEncoding(response.CharacterSet));
                        }

                        htmlCorreo = readerStream.ReadToEnd();
                        response.Close();
                        readerStream.Close();

                    }
                }

                bool correo_enviado = false;

                if (htmlCorreo != "")
                {
                    correo_enviado = await _correoService.EnviarCorreo(Correo,
                        "Contraseña restablecida",
                        htmlCorreo
                    );
                }

                if(!correo_enviado)
                    throw new TaskCanceledException("No se pudo enviar el correo de restablecimiento de clave");

                bool respuesta = await _repositorio.Editar(usuario_encontrado);
                return respuesta;

            }
            catch
            {
                throw;
            }
        }
    }
}
