namespace SysAdm.Models
{
    public class LogSeverity
    {
        public string Nombre { get; set; } = string.Empty;
        public string Tipo => Nombre.ToUpper();

        public LogSeverity(string nombre)
        {
            Nombre = nombre;
        }

        // Instancias estáticas predefinidas
        public static readonly LogSeverity Info    = new LogSeverity("Info");
        public static readonly LogSeverity Warning = new LogSeverity("Warning");
        public static readonly LogSeverity Danger  = new LogSeverity("Danger");
        public static readonly LogSeverity Error   = new LogSeverity("Error");
    }
}