namespace WpfApp1.Models
{
    /// <summary>
    /// Representa un módulo del sistema Balcax.
    /// </summary>
    public class BalcaxModulo
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Habilitado { get; set; }
    }
}
