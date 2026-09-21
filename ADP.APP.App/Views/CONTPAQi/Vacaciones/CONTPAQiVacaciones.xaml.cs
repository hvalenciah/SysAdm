using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SysAdm.Models;
using SysAdm.Services;

namespace SysAdm.Views.UserControls
{
    public partial class CONTPAQiVacaciones : UserControl
    {
        private readonly MainWindow _parent;
        private ContpaqiService? _servicio;

        public CONTPAQiVacaciones(MainWindow parent)
        {
            InitializeComponent();
            _parent = parent;
            Loaded += CONTPAQiVacaciones_Loaded;
        }

        private void CONTPAQiVacaciones_Loaded(object sender, RoutedEventArgs e)
        {
            InicializarServicio();
            CargarVacaciones();
        }

        private void dgvVacaciones_LoadingRow(object sender, DataGridRowEventArgs e)
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
                _parent.LogError("ERR_CONTPAQI_CONFIG", $"Error al inicializar servicio CONTPAQi Vacaciones: {ex.Message}");
            }
        }

        public async void CargarVacaciones()
        {
            if (_servicio == null) return;

            _parent.LogOutput("Consultando lista de vacaciones CONTPAQi...");
            try
            {
                var vacaciones = await _servicio.ObtenerVacacionesAsync();
                dgvVacaciones.ItemsSource = vacaciones;
                _parent.LogOutput($"Vacaciones cargadas: {vacaciones.Count} registro(s).");
            }
            catch (Exception ex)
            {
                _parent.LogError("ERR_CONTPAQI_GET", $"No se pudo obtener la lista de vacaciones: {ex.Message}");
            }
        }

        private async void BtnBuscar_Click(object sender, RoutedEventArgs e)
        {
            if (_servicio == null) return;

            string filtro = (cmbFiltro.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Todas";
            string valor = txtBusqueda.Text.Trim();

            _parent.LogOutput($"Buscando vacaciones ({filtro}: '{valor}')...");
            try
            {
                var vacaciones = await _servicio.BuscarVacacionesAsync(filtro, valor);
                dgvVacaciones.ItemsSource = vacaciones;

                if (vacaciones == null || vacaciones.Count == 0)
                {
                    _parent.LogOutput("Búsqueda finalizada: No se encontraron resultados.");
                    MessageBox.Show("No se encontraron resultados para la búsqueda realizada.", "Sin resultados", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _parent.LogOutput($"Búsqueda finalizada: {vacaciones.Count} registros coincidentes.");
                }
            }
            catch (Exception ex)
            {
                _parent.LogError("ERR_CONTPAQI_SEARCH", $"Error al buscar vacaciones: {ex.Message}");
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
            CargarVacaciones();
        }

        private async void BtnActualizar_Click(object sender, RoutedEventArgs e)
        {
            if (_servicio == null) return;

            var vSeleccionado = dgvVacaciones.SelectedItem as VacacionesDTO;
            if (vSeleccionado == null)
            {
                MessageBox.Show("Por favor, seleccione un registro de vacaciones de la lista.", "Actualizar", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var vEdit = new VacacionesDTO
            {
                IdVacacionesActivas = vSeleccionado.IdVacacionesActivas,
                FechaCreacion = vSeleccionado.FechaCreacion,
                FechaModificacion = vSeleccionado.FechaModificacion,
                UsuarioModificacion = vSeleccionado.UsuarioModificacion,
                NumeroTrabajador = vSeleccionado.NumeroTrabajador,
                PeriodoVacacional = vSeleccionado.PeriodoVacacional,
                DiasOtorgados = vSeleccionado.DiasOtorgados,
                DiasGozados = vSeleccionado.DiasGozados,
                DiasPendientes = vSeleccionado.DiasPendientes,
                Vigencia = vSeleccionado.Vigencia,
                Activo = vSeleccionado.Activo
            };

            var form = new VacacionesFormWindow(vEdit) { Owner = Window.GetWindow(this) };
            if (form.ShowDialog() == true && form.Guardado)
            {
                _parent.LogOutput($"Actualizando vacaciones ID {vEdit.IdVacacionesActivas}...");
                try
                {
                    await _servicio.ActualizarVacacionesAsync(form.Vacaciones);
                    _parent.LogOutput($"Vacaciones ID {vEdit.IdVacacionesActivas} actualizadas exitosamente.");
                    CargarVacaciones();
                }
                catch (Exception ex)
                {
                    _parent.LogError("ERR_CONTPAQI_UPDATE", $"Error al actualizar vacaciones: {ex.Message}");
                }
            }
        }
    }
}