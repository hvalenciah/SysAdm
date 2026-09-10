using System;
using System.Windows;
using WpfApp1.Models;

namespace WpfApp1.Views.UserControls
{
    public partial class VistaFormWindow : Window
    {
        public BalcaxVista Vista { get; private set; } = null!;
        public bool Guardado { get; private set; } = false;

        public VistaFormWindow(BalcaxVista? vistaExistente = null, bool esSoloLectura = false)
        {
            InitializeComponent();

            if (vistaExistente != null)
            {
                // Editar o Ver
                txtId.Text = vistaExistente.Id.ToString();
                txtNombre.Text = vistaExistente.Nombre;
                txtRouterLink.Text = vistaExistente.RouterLink;
                txtIcon.Text = vistaExistente.Icon;
                txtIdModulo.Text = vistaExistente.IdModulo.ToString();
                txtIdVistaPadre.Text = vistaExistente.IdVistaPadre?.ToString() ?? string.Empty;
                txtNivel.Text = vistaExistente.Nivel.ToString();
                chkVisible.IsChecked = vistaExistente.Visible;

                Vista = vistaExistente;
            }
            else
            {
                // Crear
                Vista = new BalcaxVista();
            }

            if (esSoloLectura)
            {
                AplicarModoSoloLectura();
            }
        }

        private void AplicarModoSoloLectura()
        {
            Title = "Consulta de Vista";

            var bc = new System.Windows.Media.BrushConverter();
            var fondoDeshabilitado = (System.Windows.Media.Brush)bc.ConvertFrom("#F0F0F0")!;

            txtNombre.IsReadOnly = true;
            txtNombre.Background = fondoDeshabilitado;

            txtRouterLink.IsReadOnly = true;
            txtRouterLink.Background = fondoDeshabilitado;

            txtIcon.IsReadOnly = true;
            txtIcon.Background = fondoDeshabilitado;

            txtIdModulo.IsReadOnly = true;
            txtIdModulo.Background = fondoDeshabilitado;

            txtIdVistaPadre.IsReadOnly = true;
            txtIdVistaPadre.Background = fondoDeshabilitado;

            txtNivel.IsReadOnly = true;
            txtNivel.Background = fondoDeshabilitado;

            chkVisible.IsEnabled = false;

            // Ocultar el botón Guardar y renombrar Cancelar a Cerrar
            btnGuardar.Visibility = Visibility.Collapsed;
            btnCancelar.Content = "Cerrar";
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El campo Nombre es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNombre.Focus();
                return;
            }

            // Validación para IdModulo: null si está en blanco
            int? idModulo = null;
            if (!string.IsNullOrWhiteSpace(txtIdModulo.Text))
            {
                if (int.TryParse(txtIdModulo.Text.Trim(), out int parsedModulo))
                {
                    idModulo = parsedModulo;
                }
                else
                {
                    MessageBox.Show("El ID de Módulo debe ser un número entero válido o estar vacío.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtIdModulo.Focus();
                    return;
                }
            }

            // Validación para IdVistaPadre: null si está en blanco
            int? idVistaPadre = null;
            if (!string.IsNullOrWhiteSpace(txtIdVistaPadre.Text))
            {
                if (int.TryParse(txtIdVistaPadre.Text.Trim(), out int parsedPadre))
                {
                    idVistaPadre = parsedPadre;
                }
                else
                {
                    MessageBox.Show("El ID de Vista Padre debe ser un entero válido o estar vacío.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtIdVistaPadre.Focus();
                    return;
                }
            }

            int nivel = 0;
            if (!string.IsNullOrWhiteSpace(txtNivel.Text) && !int.TryParse(txtNivel.Text.Trim(), out nivel))
            {
                MessageBox.Show("El Nivel debe ser un número entero válido.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNivel.Focus();
                return;
            }

            // Asignación al objeto Vista
            Vista.Nombre = txtNombre.Text.Trim();
            
            // Si están en blanco, asignamos null
            Vista.RouterLink = string.IsNullOrWhiteSpace(txtRouterLink.Text) ? null : txtRouterLink.Text.Trim();
            Vista.Icon = string.IsNullOrWhiteSpace(txtIcon.Text) ? null : txtIcon.Text.Trim();
            
            Vista.IdModulo = idModulo;
            Vista.IdVistaPadre = idVistaPadre;
            
            Vista.Nivel = nivel;
            Vista.Visible = chkVisible.IsChecked ?? false;

            Guardado = true;
            DialogResult = true;
            Close();
        }
    }
}