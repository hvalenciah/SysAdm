namespace SysAdm.Models
{
    /// <summary>
    /// Representa un error de validación o procesamiento mostrado en el panel de errores.
    /// </summary>
    public class ErrorItem
    {
        public string Tipo { get; set; } = "";
        public string Titulo { get; set; } = "";
        public string Descripcion { get; set; } = "";
    }
}
