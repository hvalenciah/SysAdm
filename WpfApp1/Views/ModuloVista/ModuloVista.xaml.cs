using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.Models;
using WpfApp1.Services;

namespace WpfApp1.Views.UserControls
{
    public partial class ModuloVista : UserControl
    {
        private readonly MainWindow _parent;
        private BalcaxService? _servicio;

        public ModuloVista(MainWindow parent)
        {
            InitializeComponent();
            _parent = parent;
            Loaded += ModuloVista_Loaded;
        }

        private void ModuloVista_Loaded(object sender, RoutedEventArgs e)
        {
            InicializarServicio();
            CargarDatos();
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
                _parent.LogError("ERR_MODULO_VISTA_CONFIG", $"Error al inicializar servicio: {ex.Message}");
            }
        }

        private async void CargarDatos()
        {
            if (_servicio == null) return;

            _parent.LogOutput("Cargando módulos y vistas...");
            try
            {
                var selectedModuloId = (lstModulos.SelectedItem as BalcaxModulo)?.Id;
                var selectedVistaId = (lstVistas.SelectedItem as BalcaxVista)?.Id;

                var modulos = await _servicio.ObtenerModulosAsync();
                var vistas = await _servicio.ObtenerVistasAsync();

                lstModulos.ItemsSource = modulos;
                lstVistas.ItemsSource = vistas;
                lstVistasDisponibles.ItemsSource = vistas;

                if (selectedModuloId.HasValue)
                {
                    lstModulos.SelectedItem = modulos.FirstOrDefault(m => m.Id == selectedModuloId.Value);
                }
                if (selectedVistaId.HasValue)
                {
                    lstVistas.SelectedItem = vistas.FirstOrDefault(v => v.Id == selectedVistaId.Value);
                }

                _parent.LogOutput($"Módulos ({modulos.Count}) y Vistas ({vistas.Count}) cargados correctamente.");
            }
            catch (Exception ex)
            {
                _parent.LogError("ERR_MODULO_VISTA_GET", $"Error al cargar estructura: {ex.Message}");
            }
        }

        // ==================== EVENTOS LADO IZQUIERDO ====================

        private void LstModulos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var moduloSeleccionado = lstModulos.SelectedItem as BalcaxModulo; // Adaptar según el nombre del Modelo
            if (moduloSeleccionado != null)
            {
                lblVistasAsignadas.Text = $"Asignadas a: {moduloSeleccionado.Nombre}";
                CargarVistasDelModulo(moduloSeleccionado.Id);
            }
            else
            {
                lblVistasAsignadas.Text = "Vistas Asignadas al Módulo:";
                lstVistasAsignadas.ItemsSource = null;
            }
        }

        private async void CargarVistasDelModulo(int moduloId)
        {
            if (_servicio == null) return;
            try
            {
                // Obtener el objeto ModuloConVistasDTO desde el servicio
                var resultado = await _servicio.ObtenerVistasPorModuloAsync(moduloId);
                
                // Asignar únicamente la lista de vistas a la ListBox
                lstVistasAsignadas.ItemsSource = resultado?.Vistas;
            }
            catch (Exception ex)
            {
                _parent.LogError("ERR_GET_VISTAS_MODULO", $"Error al cargar vistas asignadas: {ex.Message}");
            }
        }

        private async void BtnAgregarModulo_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new ModuloFormWindow { Owner = Window.GetWindow(this) };
            if (ventana.ShowDialog() == true && ventana.Guardado)
            {
                try
                {
                    _parent.LogOutput("Creando módulo...");
                    await _servicio!.CrearModuloAsync(ventana.Modulo);
                    _parent.LogOutput("Módulo creado exitosamente.");
                    CargarDatos();
                }
                catch (Exception ex)
                {
                    _parent.LogError("ERR_CREATE_MODULO", $"Error al crear módulo: {ex.Message}");
                }
            }
        }

        private async void BtnVerModulo_Click(object sender, RoutedEventArgs e)
        {
            var moduloSel = lstModulos.SelectedItem as BalcaxModulo;
            if (moduloSel == null)
            {
                MessageBox.Show("Seleccione un módulo para visualizar sus datos.", "Atención", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _parent.LogOutput($"Visualizando detalle del módulo ID: {moduloSel.Id}...");
            
            var moduloActualizado = await _servicio!.ObtenerModuloPorIdAsync(moduloSel.Id) ?? moduloSel;

            // Abrir formulario en modo solo lectura (esSoloLectura = true)
            var ventana = new ModuloFormWindow(moduloActualizado, esSoloLectura: true) 
            { 
                Owner = Window.GetWindow(this) 
            };
            ventana.ShowDialog();
        }

        private async void BtnModificarModulo_Click(object sender, RoutedEventArgs e)
        {
            var moduloSel = lstModulos.SelectedItem as BalcaxModulo;
            if (moduloSel == null)
            {
                MessageBox.Show("Seleccione un módulo para modificar.", "Atención", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _parent.LogOutput($"Consultando datos actualizados del módulo ID: {moduloSel.Id}...");
            var moduloActualizado = await _servicio!.ObtenerModuloPorIdAsync(moduloSel.Id) ?? moduloSel;

            var ventana = new ModuloFormWindow(moduloActualizado) { Owner = Window.GetWindow(this) };
            if (ventana.ShowDialog() == true && ventana.Guardado)
            {
                try
                {
                    _parent.LogOutput($"Actualizando módulo ID: {moduloSel.Id}...");
                    await _servicio!.ActualizarModuloAsync(ventana.Modulo);
                    _parent.LogOutput("Módulo actualizado exitosamente.");
                    CargarDatos();
                }
                catch (Exception ex)
                {
                    _parent.LogError("ERR_UPDATE_MODULO", $"Error al actualizar módulo: {ex.Message}");
                }
            }
        }

        private async void BtnEliminarModulo_Click(object sender, RoutedEventArgs e)
        {
            var moduloSel = lstModulos.SelectedItem as BalcaxModulo;
            if (moduloSel == null)
            {
                MessageBox.Show("Seleccione un módulo para eliminar.", "Atención", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var confirmacion = MessageBox.Show(
                $"¿Está seguro de que desea eliminar el módulo '{moduloSel.Nombre}'?\nEsta acción también podría remover sus asociaciones con las vistas.",
                "Confirmar Eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmacion != MessageBoxResult.Yes) return;

            try
            {
                _parent.LogOutput($"Eliminando módulo ID: {moduloSel.Id} ({moduloSel.Nombre})...");
                
                await _servicio!.EliminarModuloAsync(moduloSel.Id);
                
                _parent.LogOutput("Módulo eliminado exitosamente.");
                
                // Recargar datos y limpiar la selección
                CargarDatos();
                lstVistasAsignadas.ItemsSource = null;
                lblVistasAsignadas.Text = "Vistas Asignadas al Módulo:";
            }
            catch (Exception ex)
            {
                _parent.LogError("ERR_DELETE_MODULO", $"Error al eliminar el módulo: {ex.Message}");
                MessageBox.Show($"Ocurrió un error al eliminar el módulo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnVerVista_Click(object sender, RoutedEventArgs e)
        {
            var vistaSel = lstVistas.SelectedItem as BalcaxVista;
            if (vistaSel == null)
            {
                MessageBox.Show("Seleccione una vista para visualizar sus datos.", "Atención", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _parent.LogOutput($"Visualizando detalle de la vista ID: {vistaSel.Id}...");

            var vistaActualizada = await _servicio!.ObtenerVistaPorIdAsync(vistaSel.Id) ?? vistaSel;

            // Abrir formulario en modo solo lectura (esSoloLectura = true)
            var ventana = new VistaFormWindow(vistaActualizada, esSoloLectura: true) 
            { 
                Owner = Window.GetWindow(this) 
            };
            ventana.ShowDialog();
        }

        private async void BtnAgregarVista_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new VistaFormWindow { Owner = Window.GetWindow(this) };
            if (ventana.ShowDialog() == true && ventana.Guardado)
            {
                try
                {
                    _parent.LogOutput("Creando vista...");
                    await _servicio!.CrearVistaAsync(ventana.Vista);
                    _parent.LogOutput("Vista creada exitosamente.");
                    CargarDatos();
                }
                catch (Exception ex)
                {
                    _parent.LogError("ERR_CREATE_VISTA", $"Error al crear vista: {ex.Message}");
                }
            }
        }

        private async void BtnModificarVista_Click(object sender, RoutedEventArgs e)
        {
            var vistaSel = lstVistas.SelectedItem as BalcaxVista;
            if (vistaSel == null)
            {
                MessageBox.Show("Seleccione una vista para modificar.", "Atención", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _parent.LogOutput($"Consultando datos actualizados de la vista ID: {vistaSel.Id}...");
            var vistaActualizada = await _servicio!.ObtenerVistaPorIdAsync(vistaSel.Id) ?? vistaSel;

            var ventana = new VistaFormWindow(vistaActualizada) { Owner = Window.GetWindow(this) };
            if (ventana.ShowDialog() == true && ventana.Guardado)
            {
                try
                {
                    _parent.LogOutput($"Actualizando vista ID: {vistaSel.Id}...");
                    await _servicio!.ActualizarVistaAsync(ventana.Vista);
                    _parent.LogOutput("Vista actualizada exitosamente.");
                    CargarDatos();
                }
                catch (Exception ex)
                {
                    _parent.LogError("ERR_UPDATE_VISTA", $"Error al actualizar vista: {ex.Message}");
                }
            }
        }

        private async void BtnEliminarVista_Click(object sender, RoutedEventArgs e)
        {
            var vistaSel = lstVistas.SelectedItem as BalcaxVista;
            if (vistaSel == null)
            {
                MessageBox.Show("Seleccione una vista para eliminar.", "Atención", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var confirmacion = MessageBox.Show(
                $"¿Está seguro de que desea eliminar la vista '{vistaSel.Nombre}'?\nEsta acción la removerá de todos los módulos asociados.",
                "Confirmar Eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmacion != MessageBoxResult.Yes) return;

            try
            {
                _parent.LogOutput($"Eliminando vista ID: {vistaSel.Id} ({vistaSel.Nombre})...");
                
                await _servicio!.EliminarVistaAsync(vistaSel.Id);
                
                _parent.LogOutput("Vista eliminada exitosamente.");
                
                // Recargar datos actualizados del catálogo
                CargarDatos();

                // Si hay un módulo seleccionado actualmente, refrescar sus vistas asignadas
                if (lstModulos.SelectedItem is BalcaxModulo moduloSeleccionado)
                {
                    CargarVistasDelModulo(moduloSeleccionado.Id);
                }
            }
            catch (Exception ex)
            {
                _parent.LogError("ERR_DELETE_VISTA", $"Error al eliminar la vista: {ex.Message}");
                MessageBox.Show($"Ocurrió un error al eliminar la vista: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==================== EVENTOS LADO DERECHO (ASOCIACIÓN) ====================

        private async void BtnAgregarAsociacion_Click(object sender, RoutedEventArgs e)
        {
            var modulo = lstModulos.SelectedItem as BalcaxModulo;
            if (modulo == null)
            {
                MessageBox.Show("Seleccione un módulo primero.", "Atención", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var vistasSeleccionadas = lstVistasDisponibles.SelectedItems.Cast<BalcaxVista>().ToList();
            if (!vistasSeleccionadas.Any())
            {
                MessageBox.Show("Seleccione al menos una vista disponible para asociar.", "Atención", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                _parent.LogOutput($"Asociando {vistasSeleccionadas.Count} vista(s) al módulo '{modulo.Nombre}'...");
                
                foreach (var vista in vistasSeleccionadas)
                {
                    await _servicio!.AsociarVistaAModuloAsync(modulo.Id, vista.Id);
                }

                _parent.LogOutput("Asociación guardada correctamente.");
                CargarVistasDelModulo(modulo.Id);
                CargarDatos();
            }
            catch (Exception ex)
            {
                _parent.LogError("ERR_ADD_ASOCIACION", $"Error al asociar vistas: {ex.Message}");
            }
        }

        private async void BtnEliminarAsociacion_Click(object sender, RoutedEventArgs e)
        {
            var modulo = lstModulos.SelectedItem as BalcaxModulo;
            if (modulo == null)
            {
                MessageBox.Show("Seleccione un módulo primero.", "Atención", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var vistasSeleccionadas = lstVistasAsignadas.SelectedItems.Cast<VistaDetalleDTO>().ToList();
            if (!vistasSeleccionadas.Any())
            {
                MessageBox.Show("Seleccione al menos una vista asignada para remover.", "Atención", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                _parent.LogOutput($"Removiendo {vistasSeleccionadas.Count} vista(s) del módulo '{modulo.Nombre}'...");

                foreach (var vista in vistasSeleccionadas)
                {
                    await _servicio!.DesasociarVistaDeModuloAsync(modulo.Id, vista.IdVista);
                }

                _parent.LogOutput("Vista(s) desasociada(s) correctamente.");
                CargarVistasDelModulo(modulo.Id);
                CargarDatos();
            }
            catch (Exception ex)
            {
                _parent.LogError("ERR_REMOVE_ASOCIACION", $"Error al remover vistas: {ex.Message}");
            }
        }
    }
}