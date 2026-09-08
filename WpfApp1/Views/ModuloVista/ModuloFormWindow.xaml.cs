using System;
using System.Windows;
using WpfApp1.Models;

namespace WpfApp1.Views.UserControls
{
    public partial class ModuloFormWindow : Window
    {
        public BalcaxModulo Modulo { get; private set; } = null!;
        public bool Guardado { get; private set; } = false;

        public ModuloFormWindow(BalcaxModulo? moduloExistente = null)
        {
            InitializeComponent();

            if (moduloExistente != null)
            {
                // Editar
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