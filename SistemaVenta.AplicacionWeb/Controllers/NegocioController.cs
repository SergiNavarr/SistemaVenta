using Microsoft.AspNetCore.Mvc;

using AutoMapper;
using Newtonsoft.Json;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.AplicacionWeb.Models.ViewModels;
using SistemaVenta.AplicacionWeb.Utilidades.Response;
using SistemaVenta.Entity.Entities;


namespace SistemaVenta.AplicacionWeb.Controllers
{
    // Controlador MVC para la entidad "Negocio"
    public class NegocioController : Controller
    {
        // Inyección de dependencias:
        // IMapper se utiliza para mapear entidades a ViewModels y viceversa
        private readonly IMapper _mapper;

        // Servicio que contiene la lógica de negocio para la entidad "Negocio"
        private readonly INegocioService _negocioService;

        // Constructor que inicializa las dependencias inyectadas
        public NegocioController(IMapper mapper, INegocioService negocioService)
        {
            _mapper = mapper;
            _negocioService = negocioService;
        }

        // Acción por defecto que devuelve la vista principal
        public IActionResult Index()
        {
            return View();
        }

        // Acción HTTP GET que obtiene los datos del negocio
        [HttpGet]
        public async Task<IActionResult> Obtener()
        {
            // Crea una respuesta genérica que contendrá el estado, mensaje y objeto devuelto
            GenericResponse<VMNegocio> gResponse = new GenericResponse<VMNegocio>();

            try
            {
                // Llama al servicio para obtener el negocio y lo mapea a la ViewModel
                VMNegocio vmNegocio = _mapper.Map<VMNegocio>(await _negocioService.Obtener());

                // Indica que la operación fue exitosa y asigna el objeto mapeado
                gResponse.Estado = true;
                gResponse.Objeto = vmNegocio;
            }
            catch (Exception ex)
            {
                // En caso de error, indica que falló y guarda el mensaje de excepción
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }

            // Retorna una respuesta HTTP 200 con el contenido de la respuesta genérica
            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpPost]
        // Acción del controlador que guarda los cambios del negocio
        // Recibe:
        // - un archivo (logo) desde el formulario como IFormFile
        // - un string con los datos del modelo serializado en JSON (modelo)
        public async Task<IActionResult> GuardarCambios([FromForm] IFormFile logo, [FromForm] string modelo)
        {
            // Estructura de respuesta genérica con estado, mensaje y objeto
            GenericResponse<VMNegocio> gResponse = new GenericResponse<VMNegocio>();

            try
            {
                // Deserializa el string JSON recibido a un objeto VMNegocio
                VMNegocio vmNegocio = JsonConvert.DeserializeObject<VMNegocio>(modelo);

                // Variables para el nombre del logo e imagen en stream
                string nombreLogo = "";
                Stream logoStream = null;

                // Si el logo fue enviado
                if (logo != null)
                {
                    // Genera un nombre único para el archivo usando GUID
                    string nombre_en_codigo = Guid.NewGuid().ToString("N");
                    string extension = Path.GetExtension(logo.FileName);
                    nombreLogo = string.Concat(nombre_en_codigo, extension);

                    // Obtiene el stream del archivo (para subirlo)
                    logoStream = logo.OpenReadStream();
                }

                // Llama al servicio para guardar los cambios, mapeando la ViewModel a entidad
                Negocio negocio_editado = await _negocioService.GuardarCambios(
                    _mapper.Map<Negocio>(vmNegocio),
                    logoStream,
                    nombreLogo
                );

                // Mapea la entidad devuelta nuevamente a VMNegocio para la respuesta
                vmNegocio = _mapper.Map<VMNegocio>(negocio_editado);

                // Marca la respuesta como exitosa
                gResponse.Estado = true;
                gResponse.Objeto = vmNegocio;
            }
            catch (Exception ex)
            {
                // En caso de error, devuelve el mensaje y marca la respuesta como fallida
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }

            // Retorna una respuesta HTTP 200 con el objeto o el mensaje de error
            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

    }
}
