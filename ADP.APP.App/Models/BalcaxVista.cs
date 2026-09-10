namespace SysAdm.Models
{
    /// <summary>
    /// Representa una vista dentro del sistema Balcax.
    /// </summary>
    public class BalcaxVista
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string? RouterLink { get; set; }
        public int? IdModulo { get; set; }
        public int? IdVistaPadre { get; set; }
        public int Nivel { get; set; }
        public bool Visible { get; set; } = true;
    }
}
