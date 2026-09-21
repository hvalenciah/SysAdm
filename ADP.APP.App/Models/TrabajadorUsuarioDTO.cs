using System;

namespace SysAdm.Models
{
    public class TrabajadorUsuarioDTO
    {
        // Atributos de Usuario
        public int IdUsuario { get; set; }
        public string Correo { get; set; } = null!;
        public string? Telefono { get; set; }
        public bool Habilitado { get; set; }

        // Atributos de Trabajador
        public string? CodigoTrabajador { get; set; }
        public string? NombreTrabajador { get; set; }
        public string? ApellidoPaterno { get; set; }
        public string? ApellidoMaterno { get; set; }
        public DateTime? FechaIngreso { get; set; }
        public string? Estado { get; set; }
        public string? TipoEmpleado { get; set; }
        public int? CodigoPuesto { get; set; }
        public string? NombrePuesto { get; set; }
        public int? CodigoDepartamento { get; set; }
        public string? NombreDepartamento { get; set; }
        public int? CodigoGerencia { get; set; }
        public string? NombreGerencia { get; set; }
        public int? CodigoDireccion { get; set; }
        public string? NombreDireccion { get; set; }

        // Propiedad calculada para mostrar el Nombre Completo en la interfaz
        public string NombreCompleto => $"{NombreTrabajador} {ApellidoPaterno} {ApellidoMaterno}".Trim();
    }
}