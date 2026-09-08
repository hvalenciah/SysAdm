using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfApp1.Models;
using WpfApp1.Services;

namespace WpfApp1.Views.UserControls
{
    public partial class BalcaxUsers : UserControl
    {
        private readonly MainWindow _parent;
        private BalcaxService? _servicio;

        public BalcaxUsers(MainWindow parent)
        {
            InitializeComponent();
            _parent = parent;
            Loaded += BalcaxUsers_Loaded;
        }

        private void BalcaxUsers_Loaded(object sender, RoutedEventArgs e)
        {
            InicializarServicio();
            CargarUsuarios();
        }

        private void dgvUsuarios_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            // Asigna el número de fila (índice base 0 + 1) en la cabecera lateral (RowHeader)
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

        public async void CargarUsuarios()
        {
            if (_servicio == null) return;

            _parent.LogOutput("Consultando lista de usuario Balcax...");
            try
            {
                var usuarios = await _servicio.ObtenerUsuariosAsync();
                dgvUsuarios.ItemsSource = usuarios;
                _parent.LogOutput($"Usuarios cargados: {usuarios.Count} registro(s).");
            }
            catch (Exception ex)
            {
                _parent.LogError("ERR_BALCAX_GET", $"No se pudo obtener la lista de usuarios: {ex.Message}");
            }
        }

        private async void BtnBuscar_Click(object sender, RoutedEventArgs e)
        {
            if (_servicio == null) return;

            string filtro = (cmbFiltro.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "General";
            string valor = txtBusqueda.Text.Trim();

            _parent.LogOutput($"Buscando usuarios ({filtro}: '{valor}')...");
            try
            {
                var usuarios = await _servicio.BuscarUsuariosAsync(filtro, valor);
                dgvUsuarios.ItemsSource = usuarios;
                _parent.LogOutput($"Búsqueda finalizada: {usuarios.Count} registros coincidentes.");
            }
            catch (Exception ex)
            {
                _parent.LogError("ERR_BALCAX_SEARCH", $"Error al buscar: {ex.Message}");
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
            CargarUsuarios();
        }

        private async void BtnAgregar_Click(object sender, RoutedEventArgs e)
        {
            if (_servicio == null) return;

            var form = new UsuarioFormWindow() { Owner = Window.GetWindow(this) };
            if (form.ShowDialog() == true && form.Guardado)
            {
                _parent.LogOutput("Creando usuario Balcax...");
                try
                {
                    await _servicio.CrearUsuarioAsync(form.Usuario);
                    _parent.LogOutput($"Usuario {form.Usuario.Nombre} creado exitosamente.");
                    CargarUsuarios();
                }
                catch (Exception ex)
                {
                    _parent.LogError("ERR_BALCAX_ADD", $"No se pudo crear el usuario: {ex.Message}");
                }
            }
        }

        private async void BtnActualizar_Click(object sender, RoutedEventArgs e)
        {
            if (_servicio == null) return;

            var uSeleccionado = dgvUsuarios.SelectedItem as BalcaxUsuario;
            if (uSeleccionado == null)
            {
                MessageBox.Show("Por favor, seleccione un usuario de la lista.", "Actualizar", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Crear una copia para editar y evitar modificar la fila del Grid directamente antes de guardar
            var uEdit = new BalcaxUsuario
            {
                Id = uSeleccionado.Id,
                Nombre = uSeleccionado.Nombre,
                Apellidos = uSeleccionado.Apellidos,
                Correo = uSeleccionado.Correo,
                Telefono = uSeleccionado.Telefono,
                Habilitado = uSeleccionado.Habilitado,
                Token = uSeleccionado.Token,
                Contrasena = uSeleccionado.Contrasena,
                Avatar = uSeleccionado.Avatar,
                IdEmpresa = uSeleccionado.IdEmpresa
            };

            var form = new UsuarioFormWindow(uEdit) { Owner = Window.GetWindow(this) };
            if (form.ShowDialog() == true && form.Guardado)
            {
                _parent.LogOutput($"Actualizando usuario Balcax ID {uEdit.Id}...");
                try
                {
                    await _servicio.ActualizarUsuarioAsync(form.Usuario);
                    _parent.LogOutput($"Usuario ID {uEdit.Id} actualizado exitosamente.");
                    CargarUsuarios();
                }
                catch (Exception ex)
                {
                    _parent.LogError("ERR_BALCAX_UPDATE", $"Error al actualizar: {ex.Message}");
                }
            }
        }

        private async void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (_servicio == null) return;

            var uSeleccionado = dgvUsuarios.SelectedItem as BalcaxUsuario;
            if (uSeleccionado == null)
            {
                MessageBox.Show("Por favor, seleccione el usuario que desea eliminar.", "Eliminar", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show($"¿Está seguro que desea eliminar al usuario {uSeleccionado.Nombre} (ID: {uSeleccionado.Id})?", "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            _parent.LogOutput($"Eliminando usuario Balcax ID {uSeleccionado.Id}...");
            try
            {
                await _servicio.EliminarUsuarioAsync(uSeleccionado.Id);
                _parent.LogOutput($"Usuario {uSeleccionado.Nombre} (ID: {uSeleccionado.Id}) eliminado exitosamente.");
                CargarUsuarios();
            }
            catch (Exception ex)
            {
                _parent.LogError("ERR_BALCAX_DELETE", $"No se pudo eliminar el usuario: {ex.Message}");
            }
        }

        private void BtnPermiso_Click(object sender, RoutedEventArgs e)
        {
            if (_servicio == null) return;

            // 1. Obtener el usuario seleccionado del DataGrid
            var usuarioSeleccionado = dgvUsuarios.SelectedItem as BalcaxUsuario;

            // 2. Validar que se haya seleccionado un elemento
            if (usuarioSeleccionado == null)
            {
                MessageBox.Show(
                    "Por favor, seleccione un usuario de la lista para gestionar sus permisos.",
                    "Selección requerida",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
                return;
            }

            // 3. Instanciar la ventana pasando el usuario, servicio y estableciendo el Owner
            var form = new PermisoFormWindow(usuarioSeleccionado, _servicio)
            {
                Owner = Window.GetWindow(this)
            };

            // 4. Mostrar la ventana modal y evaluar si se guardaron cambios
            if (form.ShowDialog() == true && form.Actualizado)
            {
                _parent.LogOutput($"Permisos actualizados para el usuario ID {usuarioSeleccionado.Id} ({usuarioSeleccionado.Nombre}).");
                
                // Opcional: Recargar la lista si los permisos afectan la vista actual
                // CargarUsuarios();
            }
        }
    }
}
