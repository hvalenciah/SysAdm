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
                // Ejemplo de consulta de datos desde el servicio
                // Ajustar las llamadas a métodos del servicio según corresponda
                var modulos = await _servicio.ObtenerModulosAsync();
                var vistas = await _servicio.ObtenerVistasAsync();

                lstModulos.ItemsSource = modulos;
                lstVistas.ItemsSource = vistas;
                lstVistasDisponibles.ItemsSource = vistas;

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
                var vistasAsignadas = await _servicio.ObtenerVistasPorModuloAsync(moduloId);
                lstVistasAsignadas.ItemsSource = vistasAsignadas;
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

        private async void BtnModificarModulo_Click(object sender, RoutedEventArgs e)
        {
            var moduloSel = lstModulos.SelectedItem as BalcaxModulo;
            if (moduloSel == null)
            {
                MessageBox.Show("Seleccione un módulo para modificar.", "Atención", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var ventana = new ModuloFormWindow(moduloSel) { Owner = Window.GetWindow(this) };
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

        private async void BtnQuitarModulo_Click(object sender, RoutedEventArgs e)
        {
            var moduloSel = lstModulos.SelectedItem as BalcaxModulo;
            if (moduloSel == null)
            {
                MessageBox.Show("Seleccione un módulo para eliminar.", "Atención", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            // Lógica de eliminación
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

            var ventana = new VistaFormWindow(vistaSel) { Owner = Window.GetWindow(this) };
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

        private async void BtnQuitarVista_Click(object sender, RoutedEventArgs e)
        {
            var vistaSel = lstVistas.SelectedItem as BalcaxVista;
            if (vistaSel == null)
            {
                MessageBox.Show("Seleccione una vista para eliminar.", "Atención", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            // Lógica de eliminación
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
                _parent.LogOutput($"Asociando {vistasSeleccionadas.Count} vista(s) al módulo {modulo.Nombre}...");
                // Ejemplo: await _servicio.AsociarVistasAModuloAsync(modulo.Id, vistasSeleccionadas.Select(v => v.Id));
                CargarVistasDelModulo(modulo.Id);
            }
            catch (Exception ex)
            {
                _parent.LogError("ERR_ADD_ASOCIACION", $"Error al asociar vistas: {ex.Message}");
            }
        }

        private async void BtnQuitarAsociacion_Click(object sender, RoutedEventArgs e)
        {
            var modulo = lstModulos.SelectedItem as BalcaxModulo;
            if (modulo == null)
            {
                MessageBox.Show("Seleccione un módulo primero.", "Atención", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var vistasSeleccionadas = lstVistasAsignadas.SelectedItems.Cast<BalcaxVista>().ToList();
            if (!vistasSeleccionadas.Any())
            {
                MessageBox.Show("Seleccione al menos una vista asignada para remover.", "Atención", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                _parent.LogOutput($"Removiendo {vistasSeleccionadas.Count} vista(s) del módulo {modulo.Nombre}...");
                // Ejemplo: await _servicio.RemoverVistasDeModuloAsync(modulo.Id, vistasSeleccionadas.Select(v => v.Id));
                CargarVistasDelModulo(modulo.Id);
            }
            catch (Exception ex)
            {
                _parent.LogError("ERR_REMOVE_ASOCIACION", $"Error al remover vistas: {ex.Message}");
            }
        }
    }
}