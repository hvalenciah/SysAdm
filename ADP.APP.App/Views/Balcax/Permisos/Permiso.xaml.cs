using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SysAdm.Models;
using SysAdm.Services;

namespace SysAdm.Views.UserControls
{
    public partial class Permisos : UserControl
    {
        private readonly MainWindow _parent;
        private BalcaxService? _servicio;

        public Permisos(MainWindow parent)
        {
            InitializeComponent();
            _parent = parent;
            Loaded += Permisos_Loaded;
        }

        private void Permisos_Loaded(object sender, RoutedEventArgs e)
        {
            InicializarServicio();
            // Se elimina CargarTrabajadoresUsuarios() para no hacer la consulta masiva al inicio
        }

        private void dgvTrabajadoresUsuarios_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void InicializarServicio()
        {
            try
            {
                string apiUrl = App.Configuration["ApiSettings:BalcaxAPI"]
                                ?? throw new InvalidOperationException("URL de API no configurada en appsettings.json.");
                _servicio = new BalcaxService(apiUrl);
            }
            catch (Exception ex)
            {
                _parent.LogError("ERR_BALCAX_CONFIG", $"Error al inicializar servicio Balcax: {ex.Message}");
            }
        }

        public async void CargarTrabajadoresUsuarios()
        {
            if (_servicio == null) return;

            _parent.LogOutput("Consultando lista completa de Trabajadores / Usuarios...");
            try
            {
                var lista = await _servicio.ObtenerTrabajadoresUsuariosAsync();
                dgvTrabajadoresUsuarios.ItemsSource = lista;
                _parent.LogOutput($"Registros cargados: {lista.Count} registro(s).");
            }
            catch (Exception ex)
            {
                _parent.LogError("ERR_BALCAX_GET", $"No se pudo obtener la lista de trabajadores/usuarios: {ex.Message}");
            }
        }

        private async void BtnBuscar_Click(object sender, RoutedEventArgs e)
        {
            if (_servicio == null) return;

            string filtro = (cmbFiltro.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Todos";
            string valor = txtBusqueda.Text.Trim();

            // Si se selecciona un filtro específico y el texto está vacío, se valida
            if (filtro != "Todos" && string.IsNullOrEmpty(valor))
            {
                MessageBox.Show("Por favor, ingrese un valor de búsqueda.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _parent.LogOutput($"Buscando Trabajador/Usuario ({filtro}: '{valor}')...");
            try
            {
                List<TrabajadorUsuarioDTO> resultado = new();

                switch (filtro)
                {
                    case "ID Usuario":
                        if (int.TryParse(valor, out int idUsuario))
                        {
                            resultado = await _servicio.BuscarTrabajadorUsuarioPorIdAsync(idUsuario);
                        }
                        else
                        {
                            MessageBox.Show("El ID de usuario debe ser un número entero válido.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        break;

                    case "Código Trabajador":
                        resultado = await _servicio.BuscarTrabajadorUsuarioPorCodigoAsync(valor);
                        break;

                    case "Todos":
                    default:
                        // Solo consultará a la API todos los usuarios cuando explícitamente se presione Buscar con la opción "Todos"
                        resultado = await _servicio.ObtenerTrabajadoresUsuariosAsync();
                        break;
                }

                dgvTrabajadoresUsuarios.ItemsSource = resultado;

                if (resultado == null || resultado.Count == 0)
                {
                    _parent.LogOutput("Búsqueda finalizada: No se encontraron resultados.");
                    MessageBox.Show("No se encontraron resultados para la búsqueda realizada.", "Sin resultados", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _parent.LogOutput($"Búsqueda finalizada: {resultado.Count} registros coincidentes.");
                }
            }
            catch (Exception ex)
            {
                _parent.LogError("ERR_BALCAX_SEARCH", $"Error al ejecutar la búsqueda: {ex.Message}");
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
            dgvTrabajadoresUsuarios.ItemsSource = null; // Limpia la tabla sin hacer peticiones a la API
        }

        private void BtnPermiso_Click(object sender, RoutedEventArgs e)
        {
            if (_servicio == null) return;

            var seleccionado = dgvTrabajadoresUsuarios.SelectedItem as TrabajadorUsuarioDTO;
            if (seleccionado == null)
            {
                MessageBox.Show("Por favor, seleccione un registro de la lista para gestionar sus permisos.", "Selección requerida", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var usuarioModal = new BalcaxUsuario
            {
                Id = seleccionado.IdUsuario,
                Nombre = seleccionado.NombreTrabajador,
                Apellidos = $"{seleccionado.ApellidoPaterno} {seleccionado.ApellidoMaterno}".Trim(),
                Correo = seleccionado.Correo,
                Telefono = seleccionado.Telefono,
                Habilitado = seleccionado.Habilitado
            };

            var form = new PermisoFormWindow(usuarioModal, _servicio)
            {
                Owner = Window.GetWindow(this)
            };

            if (form.ShowDialog() == true && form.Actualizado)
            {
                _parent.LogOutput($"Permisos actualizados para el Usuario ID {seleccionado.IdUsuario} ({seleccionado.NombreCompleto}).");
            }
        }
    }
}