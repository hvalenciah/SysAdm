namespace SysAdm.Models
{
    public class PlazaTrabajadorDTO
    {
        private static readonly DateTime FechaLimite = new DateTime(2010, 12, 30);

        public int Id { get; set; }
        public string? CodigoPlaza { get; set; }
        public string? CodigoTrabajador { get; set; }
        public string? Nombre { get; set; }
        public string? Paterno { get; set; }
        public string? Materno { get; set; }
        
        // Propiedades de Fecha Reales
        public DateTime? FechaIngreso { get; set; }
        public DateTime? FechaReIngreso { get; set; }
        public DateTime? FechaBaja { get; set; }

        // Propiedades Formateadas para el DataGrid (Sustituyen el Binding en XAML)
        public string FechaIngresoDisplay => (FechaIngreso.HasValue && FechaIngreso.Value > FechaLimite) 
            ? FechaIngreso.Value.ToString("dd-MM-yyyy") 
            : string.Empty;

        public string FechaReIngresoDisplay => (FechaReIngreso.HasValue && FechaReIngreso.Value > FechaLimite) 
            ? FechaReIngreso.Value.ToString("dd-MM-yyyy") 
            : string.Empty;

        public string FechaBajaDisplay => (FechaBaja.HasValue && FechaBaja.Value > FechaLimite) 
            ? FechaBaja.Value.ToString("dd-MM-yyyy") 
            : string.Empty;

        public string? CausaBaja { get; set; }
        public string? Estado { get; set; }
        public string? TipoEmpleado { get; set; }
        public int? SegmentoNegocio { get; set; }
        public int? CodigoPuesto { get; set; }
        public string? NombrePuesto { get; set; }
        public int? CodigoDepartamento { get; set; }
        public string? NombreDepartamento { get; set; }
        public int? CodigoGerencia { get; set; }
        public string? NombreGerencia { get; set; }
        public int? CodigoDireccion { get; set; }
        public string? NombreDireccion { get; set; }
    }
}