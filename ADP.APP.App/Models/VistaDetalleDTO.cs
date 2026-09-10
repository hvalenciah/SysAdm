namespace SysAdm.Models
{
    public class VistaDetalleDTO
    {
        public int IdVista { get; set; }
        public string? Nombre { get; set; }
        public string? RouterLink { get; set; }
        public string? Icon { get; set; }
        public bool Visible { get; set; }
        public int? IdVistaPadre { get; set; }
    }
}