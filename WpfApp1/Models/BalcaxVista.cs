namespace WpfApp1.Models
{
    /// <summary>
    /// Representa una vista dentro del sistema Balcax.
    /// </summary>
    public class BalcaxVista
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int? IdModulo { get; set; }
        public string Ruta { get; set; }
        public bool Habilitado { get; set; }
    }
}
