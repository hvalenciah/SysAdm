namespace SysAdm.Models
{
    public class TrabajadorDTO
    {
        public int IdInfoTrabajador { get; set; }
        public string? CodigoTrabajador { get; set; }
        public string? NombreTrabajador { get; set; }
        public string? ApellidoPaterno { get; set; }
        public string? ApellidoMaterno { get; set; }
        public string? CorreoElectronico { get; set; }        
        public string? Sexo { get; set; }
        public string? NssTrabajador { get; set; }        
    }
}