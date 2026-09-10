using System.ComponentModel;

namespace SysAdm.Models
{
    /// <summary>
    /// Modelo de fila para el DataGrid de resultados Paperless.
    /// Implementa INotifyPropertyChanged para que el CheckBox de selección se refleje en tiempo real.
    /// </summary>
    public class RegistroPaperless : INotifyPropertyChanged
    {
        public int Indice { get; set; }

        private bool _seleccionado;
        public bool Seleccionado
        {
            get => _seleccionado;
            set { _seleccionado = value; OnPropertyChanged(nameof(Seleccionado)); }
        }

        public DateTime? FechaCreacion { get; set; }
        public string Cuenta { get; set; } = "";
        public string Enlace { get; set; } = "";
        public string EstadoEnvio { get; set; } = "";
        public DateTime? FechaEnvio { get; set; }
        public string Medio { get; set; } = "";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string prop) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
