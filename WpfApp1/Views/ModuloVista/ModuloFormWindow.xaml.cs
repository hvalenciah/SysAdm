using System;
using System.Windows;
using WpfApp1.Models;

namespace WpfApp1.Views.UserControls
{
    public partial class ModuloFormWindow : Window
    {
        public BalcaxModulo Modulo { get; private set; } = null!;
        public bool Guardado { get; private set; } = false;

        public ModuloFormWindow(BalcaxModulo? moduloExistente = null, bool esSoloLectura = false)
        {
            InitializeComponent();

            if (moduloExistente != null)
            {
                // Editar o Ver
                txtId.Text = moduloExistente.Id.ToString();
                txtNombre.Text = moduloExistente.Nombre;
                txtDescripcion.Text = moduloExistente.Descripcion;
                chkHabilitado.IsChecked = moduloExistente.Habilitado;

                Modulo = moduloExistente;
            }
            else
            {
                // Crear
                Modulo = new BalcaxModulo();
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
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El campo Nombre es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Modulo.Nombre = txtNombre.Text.Trim();
            Modulo.Descripcion = string.IsNullOrWhiteSpace(txtDescripcion.Text) ? null : txtDescripcion.Text.Trim();
            Modulo.Habilitado = chkHabilitado.IsChecked ?? false;

            Guardado = true;
            DialogResult = true;
            Close();
        }
    }
}