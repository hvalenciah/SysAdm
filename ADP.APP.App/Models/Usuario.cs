namespace SysAdm.Models
{
    /// <summary>
    /// Representa un usuario autenticado en la sesión activa.
    /// </summary>
    public class Usuario
    {
        public string Nombre { get; set; } = "";
        public DateTime FechaLogin { get; set; }
    }
}
