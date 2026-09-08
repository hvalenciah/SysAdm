namespace WpfApp1.Models
{
    /// <summary>
    /// Representa un usuario del sistema Balcax obtenido desde la API.
    /// </summary>
    public class BalcaxUsuario
    {
        public int      Id                          { get; set; }
        public string   Nombre                      { get; set; } = string.Empty;
        public string   Apellidos                   { get; set; } = string.Empty;
        public string   Correo                      { get; set; } = string.Empty;
        public string?  Telefono                    { get; set; }
        public bool     Habilitado                  { get; set; }
        public string?  Token                       { get; set; }
        public string?  Contrasena                  { get; set; }
        public string?  Avatar                      { get; set; }
        public int      IdEmpresa                   { get; set; }
        public DateTime Registrado                  { get; set; }
        public DateTime? UsuarioFechaCreacion       { get; set; }
        public DateTime? UsuarioFechaActualizacion  { get; set; }
        public DateTime? UsuarioFechaEliminacion    { get; set; }
        public int?     UsuarioFkModificado         { get; set; }
    }
}
