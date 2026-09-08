using System;
using System.Windows;
using WpfApp1.Models;

namespace WpfApp1.Views.UserControls
{
    public partial class VistaFormWindow : Window
    {
        public BalcaxVista Vista { get; private set; } = null!;
        public bool Guardado { get; private set; } = false;

        public VistaFormWindow(BalcaxVista? vistaExistente = null)
        {
            InitializeComponent();

            if (vistaExistente != null)
            {
                // Editar
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