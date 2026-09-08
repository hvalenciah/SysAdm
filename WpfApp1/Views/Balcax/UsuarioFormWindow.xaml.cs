using System.Windows;
using WpfApp1.Models;

namespace WpfApp1.Views.UserControls
{
    public partial class UsuarioFormWindow : Window
    {
        public BalcaxUsuario Usuario { get; private set; } = null!;
        public bool Guardado { get; private set; } = false;

        public UsuarioFormWindow(BalcaxUsuario? usuarioExistente = null)
        {
            InitializeComponent();

            if (usuarioExistente != null)
            {
                // Editar
                txtId.Text = usuarioExistente.Id.ToString();
                txtNombre.Text = usuarioExistente.Nombre;
                txtApellidos.Text = usuarioExistente.Apellidos;
                txtCorreo.Text = usuarioExistente.Correo;
                txtTelefono.Text = usuarioExistente.Telefono;
                txtContrasena.Text = usuarioExistente.Contrasena;
                txtToken.Text = usuarioExistente.Token;
                txtAvatar.Text = usuarioExistente.Avatar;
                txtIdEmpresa.Text = usuarioExistente.IdEmpresa.ToString();
                chkHabilitado.IsChecked = usuarioExistente.Habilitado;
                
                Usuario = usuarioExistente;
            }
            else
            {
                // Crear
                Usuario = new BalcaxUsuario
                {
                    Registrado = DateTime.Now
                };
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrWhiteSpace(txtApellidos.Text) ||
                string.IsNullOrWhiteSpace(txtCorreo.Text))
            {
                MessageBox.Show("Los campos Nombre, Apellidos y Correo son obligatorios.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtIdEmpresa.Text, out int idEmpresa))
            {
                MessageBox.Show("Id Empresa debe ser un número entero válido.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Usuario.Nombre = txtNombre.Text.Trim().ToUpper();
            Usuario.Apellidos = txtApellidos.Text.Trim().ToUpper();
            Usuario.Correo = txtCorreo.Text.Trim().ToLower();
            Usuario.Telefono = string.IsNullOrWhiteSpace(txtTelefono.Text) ? null : txtTelefono.Text.Trim();
            Usuario.Contrasena = string.IsNullOrWhiteSpace(txtContrasena.Text) ? null : txtContrasena.Text.Trim();
            Usuario.Token = string.IsNullOrWhiteSpace(txtToken.Text) ? null : txtToken.Text.Trim();
            Usuario.Avatar = string.IsNullOrWhiteSpace(txtAvatar.Text) ? null : txtAvatar.Text.Trim().ToUpper();
            Usuario.IdEmpresa = idEmpresa;
            Usuario.Habilitado = chkHabilitado.IsChecked ?? false;

            if (Usuario.Id == 0)
            {
                Usuario.UsuarioFechaCreacion = DateTime.Now;
            }
            else
            {
                Usuario.UsuarioFechaActualizacion = DateTime.Now;
            }

            Guardado = true;
            DialogResult = true;
            Close();
        }
    }
}
