using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//imports necesarios para el servicio de correo
using System.Net;
using System.Net.Mail;

using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity.Entities;
using System.Runtime.CompilerServices;

namespace SistemaVenta.BLL.Implementacion
{
    public class CorreoService : ICorreoService
    {
        //Se usa un repositorio genérico para acceder a la base de datos y obtener la configuración
        //de correo electrónico (host, puerto, usuario, clave, etc.).
        private readonly IGenericRepository<Configuracion> _repositorio;

        public CorreoService(IGenericRepository<Configuracion> repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task<bool> EnviarCorreo(string CorreoDestino, string Asunto, string Mensaje)
        {

            try
            {

                //Se consulta la configuración del correo electrónico desde la base de datos
                IQueryable<Configuracion> query = await _repositorio.Consultar(c => c.Recurso.Equals("Servicio_Correo"));
                Dictionary<string, string> Config = query.ToDictionary(keySelector: c => c.Propiedad, elementSelector: c => c.Valor);

                //Se crea un objeto NetworkCredential con las credenciales del correo electrónico
                var credenciales = new NetworkCredential(Config["correo"], Config["clave"]);

                //Se crea un objeto MailMessage con los detalles del correo electrónico
                var correo = new MailMessage
                {
                    From = new MailAddress(Config["correo"], Config["alias"]),
                    Subject = Asunto,
                    Body = Mensaje,
                    IsBodyHtml = true
                };
                correo.To.Add(new MailAddress(CorreoDestino));

                //Configuración del cliente SMTP
                var clienteServidor = new SmtpClient()
                {
                    Host = Config["host"],
                    Port = int.Parse(Config["puerto"]),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = credenciales,
                    UseDefaultCredentials = false,
                    EnableSsl = true
                };

                // Se envía el correo electrónico
                clienteServidor.Send(correo);
                return true;

            }
            catch
            {
                return false;
            }
        }

    }
}
