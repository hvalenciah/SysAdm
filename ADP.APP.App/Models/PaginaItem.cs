namespace SysAdm.Models
{
    /// <summary>
    /// Modelo de página para el ComboBox de paginación en el Control Paperless.
    /// </summary>
    public class PaginaItem
    {
        public string Label { get; set; } = "";
        public int Pagina { get; set; }
        public override string ToString() => Label;
    }
}
