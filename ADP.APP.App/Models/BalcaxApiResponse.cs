namespace SysAdm.Models
{
    /// <summary>
    /// Wrapper genérico para las respuestas de la API Balcax.
    /// </summary>
    public class BalcaxApiResponse<T>
    {
        public bool    Success { get; set; }
        public string  Message { get; set; } = string.Empty;
        public T?      Data    { get; set; }
    }
}
