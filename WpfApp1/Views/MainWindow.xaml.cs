using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using WpfApp1.Models;
using WpfApp1.Services;
using Microsoft.Extensions.Configuration;

namespace WpfApp1.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<ErrorItem> _listaErrores = new();

        public MainWindow()
        {
            InitializeComponent();

            dgvErrores.ItemsSource = _listaErrores;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LogMessage("[INFO] Construcción del módulo Balcax Usuarios.");
            ValidarConexiónApi();
        }

        private async void ValidarConexiónApi()
        {
            // ENDPOINT /api/Home
            try
            {
                string baseUrl = App.Configuration?["ApiSettings:BalcaxAPI"] ?? "https://localhost:8443/";
                string apiUrl = $"{baseUrl.TrimEnd('/')}/api/home";

                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };

                using var client = new HttpClient(handler);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string rawContent = await response.Content.ReadAsStringAsync();
                    bool success = true;
                    string mensaje = "API de Propósito General";

                    try
                    {
                        using var doc = JsonDocument.Parse(rawContent);
                        var root = doc.RootElement;

                        if (root.ValueKind == JsonValueKind.Object)
                        {
                            if (root.TryGetProperty("success", out var propSuccess) ||
                                root.TryGetProperty("Success", out propSuccess))
                            {
                                success = propSuccess.ValueKind == JsonValueKind.True;
                            }

                            if (root.TryGetProperty("message", out var propMessage) ||
                                root.TryGetProperty("Message", out propMessage))
                            {
                                mensaje = propMessage.GetString() ?? mensaje;
                            }
                            else if (root.TryGetProperty("data", out var propData) ||
                                     root.TryGetProperty("Data", out propData))
                            {
                                mensaje = propData.GetString() ?? mensaje;
                            }
                        }
                        else if (root.ValueKind == JsonValueKind.String)
                        {
                            mensaje = root.GetString() ?? mensaje;
                        }
                    }
                    catch (JsonException)
                    {
                        if (!string.IsNullOrWhiteSpace(rawContent))
                        {
                            mensaje = rawContent.Trim('"', ' ', '\r', '\n');
                        }
                    }

                    if (success)
                    {
                        LogOutput($"[INFO] {mensaje} cargada correctamente.");
                    }
                    else
                    {
                        LogDanger("ERR_COMPAT", $"Error de compatibilidad: {mensaje}");
                    }
                }
                else
                {
                    LogDanger("ERR_CONEXION_API", $"Error al conectar con la API (Código HTTP {(int)response.StatusCode}: {response.ReasonPhrase})");
                }
            }
            catch (HttpRequestException ex)
            {
                LogDanger("ERR_CONEXION_API", $"Error de conexión HTTP con la API: {ex.Message}");
            }
            catch (Exception ex)
            {
                LogDanger("ERR_CONEXION_API", $"Error al validar conexión con la API: {ex.Message}");
            }
        }

        private void ActualizarPosicionCelda(DataGrid dataGrid)
        {
            if (dataGrid.SelectedCells.Count > 0)
            {
                var cellInfo = dataGrid.SelectedCells[0];
                int fila = dataGrid.Items.IndexOf(cellInfo.Item) + 1;
                int columna = cellInfo.Column != null ? cellInfo.Column.DisplayIndex + 1 : 1;

                if (fila > 0)
                {
                    // Formato en un solo texto
                    sbiPosicion.Text = $"Fil: {fila}, Col: {columna}";
                }
            }
        }

        private DataGrid? ObjetoGrillaActual()
        {
            if (tabControlEditor.SelectedItem is TabItem selectedTab)
            {
                return selectedTab.Content as DataGrid;
            }
            return null;
        }

        private void tabControlEditor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var dgv = ObjetoGrillaActual();
            if (dgv != null)
            {
                ActualizarPosicionCelda(dgv);
                ActualizarTotalLineas(dgv);
            }
            else
            {
                sbiLineas.Text = "Filas: 0";
            }
        }

        private void btnBalcaxUsuario_Click(object sender, RoutedEventArgs e)
        {
            AbrirOActivarPestana<UserControls.BalcaxUsers>("Usuario Balcax", () => new UserControls.BalcaxUsers(this));
        }

        private void btnBalcaxModuloVista_Click(object sender, RoutedEventArgs e)
        {
            AbrirOActivarPestana<UserControls.ModuloVista>("Módulo-Vista Balcax", () => new UserControls.ModuloVista(this));
        }

        private void btnSalir_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            txtSalida.Clear();
            txtMensajes.Clear();
            _listaErrores.Clear();
        }

        /// <summary>
        /// Retorna la lista de tabs del editor que contienen datos (DataGrid con DataTable cargado).
        /// Usada por los UserControls de procesamiento para mostrar el selector de archivo lote.
        /// </summary>
        public List<ArchivoTabItem> ObtenerTabsDeDatos()
        {
            var lista = new List<ArchivoTabItem>();
            foreach (TabItem tab in tabControlEditor.Items)
            {
                if (tab.Content is DataGrid dataGrid)
                {
                    string titulo = "";
                    if (tab.Header is StackPanel panel)
                    {
                        var tb = panel.Children.OfType<TextBlock>().FirstOrDefault();
                        titulo = tb?.Text ?? "Sin título";
                    }
                    else
                    {
                        titulo = tab.Header?.ToString() ?? "Sin título";
                    }

                    DataTable? datos = (dataGrid.ItemsSource as DataView)?.Table;
                    lista.Add(new ArchivoTabItem { Titulo = titulo, Datos = datos });
                }
            }
            return lista;
        }

        /// <summary>
        /// Abre una nueva pestaña con el UserControl indicado, o la activa si ya existe.
        /// La búsqueda se hace por el título de la pestaña para evitar duplicados.
        /// Usa un factory Func para permitir que cada UserControl reciba los parámetros que necesite.
        /// </summary>
        private void AbrirOActivarPestana<TControl>(string titulo, Func<TControl> factory) where TControl : System.Windows.Controls.UserControl
        {
            // Buscar si ya hay una pestaña con ese título
            foreach (TabItem item in tabControlEditor.Items)
            {
                // Caso A: El Header es el StackPanel personalizado (generado por CrearHeaderConBotonCerrar)
                if (item.Header is StackPanel panel)
                {
                    var textBlock = panel.Children.OfType<TextBlock>().FirstOrDefault();
                    if (textBlock != null && textBlock.Text == titulo)
                    {
                        tabControlEditor.SelectedItem = item; // Activar y salir
                        return;
                    }
                }
                // Caso B: El Header es texto simple
                else if (item.Header?.ToString() == titulo)
                {
                    tabControlEditor.SelectedItem = item; // Activar y salir
                    return;
                }
            }

            // Crear nueva pestaña usando el factory
            var control = factory();

            var header = new System.Windows.Controls.StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal
            };
            header.Children.Add(new System.Windows.Controls.TextBlock
            {
                Text = titulo,
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            });

            // Botón de cierre ✕
            var btnCerrar = new System.Windows.Controls.Button
            {
                Content = "✕",
                FontSize = 10,
                Width = 16,
                Height = 16,
                Padding = new System.Windows.Thickness(0),
                Margin = new System.Windows.Thickness(6, 0, 0, 0),
                Background = System.Windows.Media.Brushes.Transparent,
                BorderThickness = new System.Windows.Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                VerticalContentAlignment = System.Windows.VerticalAlignment.Center,
                HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center
            };

            var tabItem = new TabItem { Content = control };
            btnCerrar.Click += (s, e) => 
            {
                tabControlEditor.Items.Remove(tabItem);
                VerificarEstadoDocumentosAbiertos();
            };
            header.Children.Add(btnCerrar);
            tabItem.Header = header;

            tabControlEditor.Items.Add(tabItem);
            tabControlEditor.SelectedItem = tabItem;
        }

        private void btnAcercaDe_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("SysAdm v1.0.0\n\n",
                "Acerca de SysAdm", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // --- MÉTODOS AUXILIARES DE LOGGING ---

        public void LogOutput(string mensaje)
        {
            txtSalida.AppendText($"[{DateTime.Now:HH:mm:ss}] {mensaje}\n");
            txtSalida.ScrollToEnd();
        }

        public void LogMessage(string mensaje)
        {
            txtMensajes.AppendText($"[{DateTime.Now:HH:mm:ss}] {mensaje}\n");
            txtMensajes.ScrollToEnd();
        }

        /// <summary>
        /// Registra un evento en la tabla de errores/mensajes de la interfaz.
        /// </summary>
        public void LogError(string codigo, string descripcion, LogSeverity? severidad = null, int fila = 0, int columna = 0)
        {
            var nivel = severidad ?? LogSeverity.Danger;

            _listaErrores.Add(new ErrorItem
            {
                Tipo = nivel.Tipo,
                Titulo = codigo,
                Descripcion = descripcion
            });

            // Cambiar a la pestaña de Errores cuando es Warning o Danger/Error
            tabControlSalida.SelectedIndex = 1; 
        }

        /// <summary>
        /// Registra una advertencia (no bloqueante).
        /// </summary>
        public void LogWarning(string codigo, string descripcion, LogSeverity? severidad = null, int fila = 0, int columna = 0)
        {
            LogError(codigo, descripcion, LogSeverity.Warning);
            LogOutput($"[WARNING] {codigo} {descripcion}");
        }

        /// <summary>
        /// Registra un error crítico o falla grave.
        /// </summary>
        public void LogDanger(string codigo, string descripcion, LogSeverity? severidad = null, int fila = 0, int columna = 0)
        {
            LogError(codigo, descripcion, LogSeverity.Warning);
            LogOutput($"[DANGER] {codigo} {descripcion}");
        }

        private void ActualizarTotalLineas(DataGrid dataGrid)
        {
            if (dataGrid?.ItemsSource is DataView dataView)
            {
                int totalFilas = dataView.Count;
                sbiLineas.Text = $"Líneas: {totalFilas}";
            }
            else if (dataGrid != null)
            {
                sbiLineas.Text = $"Líneas: {dataGrid.Items.Count}";
            }
            else
            {
                sbiLineas.Text = "Líneas: 0";
            }
        }

        // --- CONTROLES DE VISIBILIDAD (MENÚ VER) ---

        /// <summary>
        /// Oculta o muestra toda la bandeja de barras de herramientas (ToolBarTray).
        /// </summary>
        private void menuVerBarraHerramientas_Click(object sender, RoutedEventArgs e)
        {
            bool visible = menuVerBarraHerramientas.IsChecked;

            toolBarTrayPrincipal.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;

            // Sincronizar el estado marcado/desmarcado de los submenús individuales
            menuBalcax.IsEnabled = visible;
        }

        /// <summary>
        /// Alterna la visibilidad de cada ToolBar individual según la opción seleccionada.
        /// </summary>
        private void menuVerToolBar_Click(object sender, RoutedEventArgs e)
        {
            tbBalcax.Visibility = menuBalcax.IsChecked ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Muestra u oculta la barra de estado (StatusBar).
        /// </summary>
        private void menuVerBarraEstado_Click(object sender, RoutedEventArgs e)
        {
            statusBarPrincipal.Visibility = menuVerBarraEstado.IsChecked ? Visibility.Visible : Visibility.Collapsed;
        }

        // Variable global dentro de MainWindow para recordar la altura previa de la fila
        private GridLength _alturaPreviaSalida = new GridLength(1, GridUnitType.Star);

        /// <summary>
        /// Muestra u oculta el TabControl inferior (Salida/Diagnóstico/Errores) liberando el espacio en el Grid.
        /// </summary>
        private void menuVerTabSalida_Click(object sender, RoutedEventArgs e)
        {
            bool visible = menuVerTabSalida.IsChecked;

            if (visible)
            {
                // Restaurar visibilidad
                tabControlSalida.Visibility = Visibility.Visible;
                splitterSalida.Visibility = Visibility.Visible;

                // Restaurar altura original y MinHeight
                gridRowSalida.Height = _alturaPreviaSalida;
                gridRowSalida.MinHeight = 80;
            }
            else
            {
                // Guardar la altura actual antes de colapsar
                if (gridRowSalida.Height.Value > 0)
                {
                    _alturaPreviaSalida = gridRowSalida.Height;
                }

                // Ocultar controles
                tabControlSalida.Visibility = Visibility.Collapsed;
                splitterSalida.Visibility = Visibility.Collapsed;

                // Colapsar la fila del Grid eliminando su altura y su tamaño mínimo
                gridRowSalida.MinHeight = 0;
                gridRowSalida.Height = new GridLength(0);
            }
        }

        /// <summary>
        /// Evalúa si aún existen pestañas con datos/hojas abiertas y actualiza los botones.
        /// </summary>
        private void VerificarEstadoDocumentosAbiertos()
        {
            // Verifica si hay al menos una pestaña cuyo contenido sea un DataGrid (hoja con datos)
            bool hayHojasAbiertas = tabControlEditor.Items
                .OfType<TabItem>()
                .Any(tab => tab.Content is DataGrid);
        }
    }
}
