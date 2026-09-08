using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using WpfApp1.Models;
using WpfApp1.Services;

namespace WpfApp1.Views.UserControls
{
    public partial class PermisoFormWindow : Window
    {
        private readonly BalcaxUsuario _usuario;
        private readonly BalcaxService _servicio;

        public ObservableCollection<PermisoNodoModel> PermisosArbol { get; set; } = new();
        public bool Actualizado { get; private set; } = false;

        public PermisoFormWindow(BalcaxUsuario usuario, BalcaxService servicio)
        {
            InitializeComponent();
            _usuario = usuario;
            _servicio = servicio;

            // Asignar datos del usuario a la interfaz
            lblUsuario.Text = usuario.Nombre != null && usuario.Apellidos != null 
                ? $"{usuario.Nombre} {usuario.Apellidos}" 
                : "BERNARDO DE HUERTA BADILLO";
            lblCorreo.Text = usuario.Correo ?? "bdehuerta@aguapuebla.mx";

            Loaded += PermisoFormWindow_Loaded;
        }

        private async void PermisoFormWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Consultar permisos desde el API (obtiene toda la estructura jerárquica)
                var permisosDto = await _servicio.ObtenerPermisosUsuarioAsync(_usuario.Id);

                // 2. Construir la jerarquía para el TreeView
                PermisosArbol.Clear();
                foreach (var modulo in permisosDto.Modulos)
                {
                    // Cada módulo actúa como raíz en el TreeView.
                    // Para integrarlo usamos PermisoNodoModel con un ID representativo
                    var moduloNodo = new PermisoNodoModel
                    {
                        Id = -modulo.IdModulo, // ID negativo para diferenciarlo de vistas reales
                        Nombre = modulo.NombreModulo
                    };

                    foreach (var vista in modulo.Vistas)
                    {
                        moduloNodo.Hijos.Add(vista);
                    }

                    // Enlazar referencias de padres y calcular estados iniciales
                    EnlazarPadresYCalcularSeleccion(moduloNodo);

                    PermisosArbol.Add(moduloNodo);
                }

                // 3. Asignar al control TreeView
                treePermisos.ItemsSource = PermisosArbol;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los permisos: {ex.Message}", "Error de Carga", MessageBoxButton.OK, MessageBoxImage.Error);
                treePermisos.ItemsSource = PermisosArbol;
            }
        }

        private void EnlazarPadresYCalcularSeleccion(PermisoNodoModel nodo, PermisoNodoModel? padre = null)
        {
            nodo.Padre = padre;
            
            bool algunHijoChecked = false;
            foreach (var hijo in nodo.Hijos)
            {
                EnlazarPadresYCalcularSeleccion(hijo, nodo);
                if (hijo.IsChecked)
                {
                    algunHijoChecked = true;
                }
            }

            if (algunHijoChecked)
            {
                nodo.SetIsCheckedSilently(true);
            }
        }

        private List<BalcaxPermiso> RecopilarPermisos()
        {
            var actionNodes = new List<PermisoNodoModel>();
            foreach (var root in PermisosArbol)
            {
                ObtenerNodosAccion(root, actionNodes);
            }

            // Agrupar los nodos de acción por su IdVistaPadre para construir el DTO de guardado
            var permisos = actionNodes
                .GroupBy(n => n.IdVistaPadre)
                .Where(g => g.Key.HasValue)
                .Select(g =>
                {
                    var idVista = g.Key!.Value;
                    return new BalcaxPermiso
                    {
                        IdVista = idVista,
                        Consultar = g.FirstOrDefault(n => n.Nombre == "Consultar")?.IsChecked ?? false,
                        Agregar = g.FirstOrDefault(n => n.Nombre == "Agregar")?.IsChecked ?? false,
                        Actualizar = g.FirstOrDefault(n => n.Nombre == "Actualizar")?.IsChecked ?? false,
                        Borrar = g.FirstOrDefault(n => n.Nombre == "Borrar")?.IsChecked ?? false,
                        Autorizar = g.FirstOrDefault(n => n.Nombre == "Autorizar")?.IsChecked ?? false
                    };
                })
                .ToList();

            return permisos;
        }

        private void ObtenerNodosAccion(PermisoNodoModel nodo, List<PermisoNodoModel> actionNodes)
        {
            if (nodo.Nombre == "Consultar" || nodo.Nombre == "Agregar" || 
                nodo.Nombre == "Actualizar" || nodo.Nombre == "Borrar" || 
                nodo.Nombre == "Autorizar")
            {
                actionNodes.Add(nodo);
                return;
            }

            foreach (var hijo in nodo.Hijos)
            {
                ObtenerNodosAccion(hijo, actionNodes);
            }
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void BtnActualizar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var request = new GuardarPermisosRequest
                {
                    IdUsuario = _usuario.Id,
                    Permisos = RecopilarPermisos()
                };

                await _servicio.GuardarPermisosUsuarioAsync(request);

                Actualizado = true;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar los permisos: {ex.Message}", "Error al Actualizar", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}