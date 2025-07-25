﻿namespace SistemaVenta.AplicacionWeb.Models.ViewModels
{
    public class VMDashBoard
    {
        public int TotalVentas { get; set; }
        public int TotalIngresos { get; set; }
        public int TotalProductos { get; set; }
        public int TotalCategorias { get; set; }

        public List<VMVentasSemana>? VentasUltimaSemana { get; set; } 
        public List<VmProductosSemana>? ProductosTopUltimaSemana { get; set; }
    }
}
