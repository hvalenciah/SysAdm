using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SysAdm.Services;

namespace SysAdm.Views.UserControls
{
    public partial class CONTPAQiTrabajador : UserControl
    {
        private readonly MainWindow _parent;
        private ContpaqiService? _servicio;

        public CONTPAQiTrabajador(MainWindow parent)
        {
            InitializeComponent();
            _parent = parent;
            Loaded += CONTPAQiTrabajador_Loaded;
        }

        private void CONTPAQiTrabajador_Loaded(object sender, RoutedEventArgs e)
        {
            InicializarServicio();
            CargarTrabajadores();
        }

        private void dgvTrabajadores_LoadingRow(object sender, DataGridRowEventArgs e)
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
                _parent.LogError("ERR_CONTPAQI_CONFIG", $"Error al inicializar servicio CONTPAQi Trabajador: {ex.Message}");
            }
        }

        public async void CargarTrabajadores()
        {
            if (_servicio == null) return;

            _parent.LogOutput("Consultando trabajadores CONTPAQi...");
            try
            {
                var trabajadores = await _servicio.ObtenerTrabajadoresAsync();
                dgvTrabajadores.ItemsSource = trabajadores;
                _parent.LogOutput($"Trabajadores cargados: {trabajadores.Count} registro(s).");
            }
            catch (Exception ex)
            {
                _parent.LogError("ERR_CONTPAQI_GET", $"Error al obtener la lista de trabajadores: {ex.Message}");
            }
        }

        private async void BtnBuscar_Click(object sender, RoutedEventArgs e)
        {
            if (_servicio == null) return;

            string filtro = (cmbFiltro.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Todos los trabajadores";
            string valor = txtBusqueda.Text.Trim();

            _parent.LogOutput($"Buscando trabajadores ({filtro}: '{valor}')...");
            try
            {
                var trabajadores = await _servicio.BuscarTrabajadoresAsync(filtro, valor);
                dgvTrabajadores.ItemsSource = trabajadores;

                if (trabajadores == null || trabajadores.Count == 0)
                {
                    _parent.LogOutput("Búsqueda finalizada: No se encontraron registros.");
                    MessageBox.Show("No se encontraron resultados para la búsqueda realizada.", "Sin resultados", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _parent.LogOutput($"Búsqueda finalizada: {trabajadores.Count} registro(s) encontrado(s).");
                }
            }
            catch (Exception ex)
            {
                _parent.LogError("ERR_CONTPAQI_SEARCH", $"Error en la búsqueda de trabajadores: {ex.Message}");
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
            CargarTrabajadores();
        }
    }
}