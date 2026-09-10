using System;
using System.Linq;
using System.Text;
using System.Windows;
using SysAdm.Models;

namespace SysAdm.Views.UserControls
{
    public partial class ContrasenaFormWindow : Window
    {
        public string NuevaContrasena { get; private set; } = string.Empty;
        public bool Guardado { get; private set; } = false;
        private bool _mostrandoPasswordActual = false;

        public ContrasenaFormWindow(BalcaxUsuario usuario)
        {
            InitializeComponent();
            lblUsuario.Text = $"{usuario.Nombre} {usuario.Apellidos}".Trim();
            lblCorreo.Text = usuario.Correo ?? "-";

            // Asignar contraseña actual proveniente del objeto si existe
            if (!string.IsNullOrEmpty(usuario.Contrasena))
            {
                txtContrasenaActual.Password = usuario.Contrasena;
                txtContrasenaActualRevelada.Text = usuario.Contrasena;
            }
        }

        private void BtnTogglePassword_Click(object sender, RoutedEventArgs e)
        {
            _mostrandoPasswordActual = !_mostrandoPasswordActual;

            if (_mostrandoPasswordActual)
            {
                txtContrasenaActualRevelada.Text = txtContrasenaActual.Password;
                txtContrasenaActual.Visibility = Visibility.Collapsed;
                txtContrasenaActualRevelada.Visibility = Visibility.Visible;
                btnTogglePassword.Content = "🙈";
            }
            else
            {
                txtContrasenaActual.Password = txtContrasenaActualRevelada.Text;
                txtContrasenaActualRevelada.Visibility = Visibility.Collapsed;
                txtContrasenaActual.Visibility = Visibility.Visible;
                btnTogglePassword.Content = "👁";
            }
        }

        private void BtnGenerarContrasena_Click(object sender, RoutedEventArgs e)
        {
            string passwordGenerada = GenerarPasswordRandom(12);
            
            txtNuevaContrasena.Password = passwordGenerada;
            txtConfirmarContrasena.Password = passwordGenerada;

            // Copiar al portapapeles de Windows
            Clipboard.SetText(passwordGenerada);

            MessageBox.Show($"Se ha generado y copiado la contraseña {passwordGenerada} al portapapeles.", 
                            "Contraseña Generada", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private string GenerarPasswordRandom(int longitud)
        {
            const string minusculas = "abcdefghijklmnopqrstuvwxyz";
            const string mayusculas = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string numeros = "0123456789";
            const string especiales = "!@#$%^&*";

            var random = new Random();
            var charPool = minusculas + mayusculas + numeros + especiales;
            
            var builder = new StringBuilder();
            builder.Append(minusculas[random.Next(minusculas.Length)]);
            builder.Append(mayusculas[random.Next(mayusculas.Length)]);
            builder.Append(numeros[random.Next(numeros.Length)]);
            builder.Append(especiales[random.Next(especiales.Length)]);

            for (int i = 4; i < longitud; i++)
            {
                builder.Append(charPool[random.Next(charPool.Length)]);
            }

            return new string(builder.ToString().ToCharArray().OrderBy(s => random.Next()).ToArray());
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            string pass1 = txtNuevaContrasena.Password;
            string pass2 = txtConfirmarContrasena.Password;

            if (string.IsNullOrWhiteSpace(pass1))
            {
                MessageBox.Show("Por favor, ingrese la nueva contraseña.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNuevaContrasena.Focus();
                return;
            }

            if (pass1 != pass2)
            {
                MessageBox.Show("Las contraseñas no coinciden. Por favor, verifíquelas.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtConfirmarContrasena.Focus();
                return;
            }

            NuevaContrasena = pass1;
            Guardado = true;
            DialogResult = true;
            Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}