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
                txtRuta.Text = vistaExistente.Ruta;
                txtDescripcion.Text = vistaExistente.Descripcion;
                chkHabilitado.IsChecked = vistaExistente.Habilitado;

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
            Title = "Consulta de Módulo";

            var bc = new System.Windows.Media.BrushConverter();
            var fondoDeshabilitado = (System.Windows.Media.Brush)bc.ConvertFrom("#F0F0F0")!;

            txtNombre.IsReadOnly = true;
            txtNombre.Background = fondoDeshabilitado;
            
            txtDescripcion.IsReadOnly = true;
            txtDescripcion.Background = fondoDeshabilitado;
            
            chkHabilitado.IsEnabled = false;

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
            if (string.IsNullOrWhiteSpace(txtNombre.Text) || string.IsNullOrWhiteSpace(txtRuta.Text))
            {
                MessageBox.Show("Los campos Nombre y Ruta son obligatorios.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Vista.Nombre = txtNombre.Text.Trim();
            Vista.Ruta = txtRuta.Text.Trim();
            Vista.Descripcion = string.IsNullOrWhiteSpace(txtDescripcion.Text) ? null : txtDescripcion.Text.Trim();
            Vista.Habilitado = chkHabilitado.IsChecked ?? false;

            Guardado = true;
            DialogResult = true;
            Close();
        }
    }
}