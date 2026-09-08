using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfApp1.Models
{
    public class PermisoNodoModel : INotifyPropertyChanged
    {
        private int _id;
        private int? _idVistaPadre;
        private string _nombre = string.Empty;
        private bool _isChecked;
        private PermisoNodoModel? _padre;

        public int Id 
        { 
            get => _id; 
            set { _id = value; OnPropertyChanged(); } 
        }

        public int? IdVistaPadre 
        { 
            get => _idVistaPadre; 
            set { _idVistaPadre = value; OnPropertyChanged(); } 
        }

        public string Nombre 
        { 
            get => _nombre; 
            set { _nombre = value; OnPropertyChanged(); } 
        }

        public bool IsChecked 
        { 
            get => _isChecked; 
            set 
            { 
                if (_isChecked != value)
                {
                    _isChecked = value; 
                    OnPropertyChanged(); 
                    
                    // Propagar hacia abajo (hijos)
                    CascadeDown(value);
                    
                    // Propagar hacia arriba (padre)
                    _padre?.CascadeUp();
                }
            } 
        }

        public PermisoNodoModel? Padre
        {
            get => _padre;
            set { _padre = value; OnPropertyChanged(); }
        }

        public void SetIsCheckedSilently(bool value)
        {
            _isChecked = value;
            OnPropertyChanged(nameof(IsChecked));
        }

        public ObservableCollection<PermisoNodoModel> Hijos { get; set; } = new();

        private bool _isCascading = false;

        private void CascadeDown(bool value)
        {
            if (_isCascading) return;
            _isCascading = true;
            foreach (var hijo in Hijos)
            {
                hijo.IsChecked = value;
            }
            _isCascading = false;
        }

        private void CascadeUp()
        {
            if (_isCascading) return;
            _isCascading = true;

            // Si algún hijo está seleccionado, el padre se selecciona.
            // Si ningún hijo está seleccionado, el padre se deselecciona.
            bool anyChecked = false;
            foreach (var hijo in Hijos)
            {
                if (hijo.IsChecked)
                {
                    anyChecked = true;
                    break;
                }
            }

            if (_isChecked != anyChecked)
            {
                _isChecked = anyChecked;
                OnPropertyChanged(nameof(IsChecked));
                _padre?.CascadeUp();
            }

            _isCascading = false;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}