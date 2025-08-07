using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SistemaVenta.Entity.Entities;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;

namespace SistemaVenta.BLL.Implementacion
{
    public class CategoriaService : ICategoriaService
    {
        private readonly IGenericRepository<Categoria> _repositorio;

        // Constructor
        public CategoriaService(IGenericRepository<Categoria> repositorio)
        {
            _repositorio = repositorio;
        }

        // Método para obtener la lista de todas las categorías
        public async Task<List<Categoria>> Lista()
        {
            IQueryable<Categoria> query = await _repositorio.Consultar();

            return query.ToList();
        }

        // Método para crear una nueva categoría
        public async Task<Categoria> Crear(Categoria entidad)
        {
            try
            {
                Categoria categoria_creada = await _repositorio.Crear(entidad);

                // Si no se asignó un Id válido, se considera que la creación falló
                if (categoria_creada.IdCategoria == 0)
                {
                    throw new TaskCanceledException("No se pudo crear la categoria");
                }

                return categoria_creada;
            }
            catch
            {
                throw;
            }
        }

        // Método para editar una categoría existente
        public async Task<Categoria> Editar(Categoria entidad)
        {
            try
            {
                Categoria categoria_encontrada = await _repositorio.Obtener(c => c.IdCategoria == entidad.IdCategoria);

                // Actualiza los campos de la categoría encontrada
                categoria_encontrada.Descripcion = entidad.Descripcion;
                categoria_encontrada.EsActivo = entidad.EsActivo;

                // Llama al repositorio para guardar los cambios
                bool respuesta = await _repositorio.Editar(categoria_encontrada);

                // Si no se pudo editar, lanza una excepción
                if (!respuesta)
                    throw new TaskCanceledException("No se pudo editar la categoria");

                return categoria_encontrada;
            }
            catch
            {
                throw;
            }
        }

        // Método para eliminar una categoría
        public async Task<bool> Eliminar(int idCategoria)
        {
            try
            {
                Categoria categoria_encontrada = await _repositorio.Obtener(c => c.IdCategoria == idCategoria);

                // Si no existe, lanza excepción de clave no encontrada
                if (categoria_encontrada == null)
                {
                    throw new KeyNotFoundException("Categoria no encontrada");
                }

                // Llama al repositorio para eliminar la categoría
                bool respuesta = await _repositorio.Eliminar(categoria_encontrada);

                // Si no se pudo eliminar, lanza excepción
                if (!respuesta)
                {
                    throw new TaskCanceledException("No se pudo eliminar la categoria");
                }

                return respuesta;
            }
            catch
            {
                throw;
            }
        }
    }

}
