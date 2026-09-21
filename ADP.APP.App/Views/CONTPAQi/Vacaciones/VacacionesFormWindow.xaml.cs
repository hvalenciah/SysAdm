using System;
using System.Windows;
using SysAdm.Models;

namespace SysAdm.Views.UserControls
{
    public partial class VacacionesFormWindow : Window
    {
        public VacacionesDTO Vacaciones { get; private set; }
        public bool Guardado { get; private set; } = false;

        public VacacionesFormWindow(VacacionesDTO vacacionesExistentes)
        {
            InitializeComponent();
            Vacaciones = vacacionesExistentes ?? throw new ArgumentNullException(nameof(vacacionesExistentes));

            // Cargar datos en los controles de la UI
            txtId.Text = Vacaciones.IdVacacionesActivas.ToString();
            txtNumeroTrabajador.Text = Vacaciones.NumeroTrabajador.ToString();
            txtPeriodoVacacional.Text = Vacaciones.PeriodoVacacional;
            txtDiasOtorgados.Text = Vacaciones.DiasOtorgados.ToString();
            txtDiasGozados.Text = Vacaciones.DiasGozados?.ToString() ?? "0";
            txtDiasPendientes.Text = Vacaciones.DiasPendientes.ToString();
            dtpVigencia.SelectedDate = Vacaciones.Vigencia.ToDateTime(TimeOnly.MinValue);
            txtUsuarioModificacion.Text = Vacaciones.UsuarioModificacion;
            chkActivo.IsChecked = Vacaciones.Activo;
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(txtPeriodoVacacional.Text) ||
                string.IsNullOrWhiteSpace(txtUsuarioModificacion.Text) ||
                !dtpVigencia.SelectedDate.HasValue)
            {
                MessageBox.Show("Los campos Período, Usuario y Vigencia son obligatorios.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtDiasOtorgados.Text, out int diasOtorgados) ||
                !int.TryParse(txtDiasPendientes.Text, out int diasPendientes))
            {
                MessageBox.Show("Los días otorgados y pendientes deben ser números enteros válidos.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int? diasGozados = null;
            if (!string.IsNullOrWhiteSpace(txtDiasGozados.Text))
            {
                if (int.TryParse(txtDiasGozados.Text, out int dg))
                {
                    diasGozados = dg;
                }
                else
                {
                    MessageBox.Show("Los días gozados deben ser un valor numérico entero.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            DateOnly fechaDateOnly = DateOnly.FromDateTime(dtpVigencia.SelectedDate.Value);

            // Mapeo de valores de vuelta al DTO manteniendo la referencia del IdVacacionesActivas
            Vacaciones.PeriodoVacacional = txtPeriodoVacacional.Text.Trim();
            Vacaciones.DiasOtorgados = diasOtorgados;
            Vacaciones.DiasGozados = diasGozados;
            Vacaciones.DiasPendientes = diasPendientes;
            Vacaciones.Vigencia = fechaDateOnly;
            Vacaciones.UsuarioModificacion = txtUsuarioModificacion.Text.Trim();
            Vacaciones.Activo = chkActivo.IsChecked ?? false;
            Vacaciones.FechaModificacion = DateTime.Now;

            MessageBox.Show($"Fecha de vigencia: {fechaDateOnly}", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

            Guardado = true;
            DialogResult = true;
            Close();
        }
    }
}