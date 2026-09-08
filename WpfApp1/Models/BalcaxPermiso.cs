using System.Collections.Generic;

namespace WpfApp1.Models
{
    public class BalcaxPermiso
    {
        public int IdVista { get; set; }
        public bool Consultar { get; set; }
        public bool Agregar { get; set; }
        public bool Actualizar { get; set; }
        public bool Borrar { get; set; }
        public bool Autorizar { get; set; }
    }

    public class GuardarPermisosRequest
    {
        public int IdUsuario { get; set; }
        public List<BalcaxPermiso> Permisos { get; set; } = new();
    }

    public class UsuarioPermisosDTO
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public List<ModuloPermisoDTO> Modulos { get; set; } = new();
    }

    public class ModuloPermisoDTO
    {
        public int IdModulo { get; set; }
        public string NombreModulo { get; set; } = string.Empty;
        public List<PermisoNodoModel> Vistas { get; set; } = new();
    }
}
