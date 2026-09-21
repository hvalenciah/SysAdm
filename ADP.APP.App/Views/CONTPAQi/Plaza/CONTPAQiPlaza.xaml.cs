using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SysAdm.Services;

namespace SysAdm.Views.UserControls
{
    public partial class CONTPAQiPlaza : UserControl
    {
        private readonly MainWindow _parent;
        private ContpaqiService? _servicio;

        public CONTPAQiPlaza(MainWindow parent)
        {
            InitializeComponent();
            _parent = parent;
            Loaded += CONTPAQiPlaza_Loaded;
        }

        private void CONTPAQiPlaza_Loaded(object sender, RoutedEventArgs e)
        {
            InicializarServicio();
            CargarPlazas();
        }

        private void dgvPlazas_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void InicializarServicio()
        {
            try
            {
                string apiUrl = App.Configuration["ApiSettings:ContpaqiAPI"]
                                ?? App.Configuration["ApiSettings:BalcaxAPI"]
                                ?? throw new InvalidOperationException("URL de API no configurada en appsettings.json.");
                _servicio = new ContpaqiService(apiUrl);
            }
            catch (Exception ex)
            {
                _parent.LogError("ERR_CONTPAQI_CONFIG", $"Error al inicializar servicio CONTPAQi: {ex.Message}");
            }
        }

        public async void CargarPlazas()
        {
            if (_servicio == null) return;

            _parent.LogOutput("Consultando plazas CONTPAQi...");
            try
            {
                var plazas = await _servicio.ObtenerPlazasAsync();
                dgvPlazas.ItemsSource = plazas;
                _parent.LogOutput($"Plazas cargadas: {plazas.Count} registro(s).");
            }
            catch (Exception ex)
            {
                _parent.LogError("ERR_CONTPAQI_GET", $"Error al obtener la lista de plazas: {ex.Message}");
            }
        }

        private async void BtnBuscar_Click(object sender, RoutedEventArgs e)
        {
            if (_servicio == null) return;

            string filtro = (cmbFiltro.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Todas las plazas";
            string valor = txtBusqueda.Text.Trim();

            _parent.LogOutput($"Buscando plazas ({filtro}: '{valor}')...");
            try
            {
                var plazas = await _servicio.BuscarPlazasAsync(filtro, valor);
                dgvPlazas.ItemsSource = plazas;

                if (plazas == null || plazas.Count == 0)
                {
                    _parent.LogOutput("Búsqueda finalizada: No se encontraron registros.");
                    MessageBox.Show("No se encontraron resultados para la búsqueda realizada.", "Sin resultados", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _parent.LogOutput($"Búsqueda finalizada: {plazas.Count} registro(s) encontrado(s).");
                }
            }
            catch (Exception ex)
            {
                _parent.LogError("ERR_CONTPAQI_SEARCH", $"Error en la búsqueda: {ex.Message}");
            }
        }

        private void TxtBusqueda_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnBuscar_Click(sender, e);
            }
        }

        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            txtBusqueda.Clear();
            cmbFiltro.SelectedIndex = 0;
            CargarPlazas();
        }
    }
}