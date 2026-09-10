using System.Data;

namespace SysAdm.Models
{
    /// <summary>
    /// Representa un tab de datos abierto en el editor, disponible para los UserControls de procesamiento.
    /// </summary>
    public class ArchivoTabItem
    {
        public string Titulo { get; set; } = "";
        public DataTable? Datos { get; set; }
        public override string ToString() => Titulo;
    }
}
