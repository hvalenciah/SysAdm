namespace WpfApp1.Models
{
    public class ActualizarModuloVistasDTO
    {
        public int IdModulo { get; set; }
        public List<int> VistasIds { get; set; } = new List<int>();
    }
}