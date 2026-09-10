using System;
using System.Windows;
using SysAdm.Models;

namespace SysAdm.Views.UserControls
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
                MessageBox.Show("El campo Nombre del Módulo es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNombre.Focus();
                return;
            }

            Modulo.Nombre = txtNombre.Text.Trim();

            Guardado = true;
            DialogResult = true;
            Close();
        }
    }
}