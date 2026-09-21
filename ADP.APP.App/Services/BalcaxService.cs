using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using SysAdm.Models;

namespace SysAdm.Services
{
    /// <summary>
    /// Servicio para consumir los endpoints de la API Balcax.
    /// </summary>
    public class BalcaxService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public BalcaxService(string baseUrl)
        {
            // Normalizar la URL a la raíz de la API si incluye sufijos
            string url = baseUrl.TrimEnd('/');
            if (url.EndsWith("/Balcan/Usuario", StringComparison.OrdinalIgnoreCase))
            {
                url = url[..^"/Balcan/Usuario".Length];
            }

            _baseUrl = url.EndsWith("/") ? url : url + "/";
            
            // Permitir certificados auto-firmados en desarrollo (localhost)
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(_baseUrl)
            };

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        #region MÉTODOS DE USUARIO

        /// <summary>
        /// Obtiene la lista de usuarios desde la API Balcax.
        /// </summary>
        public async Task<List<BalcaxUsuario>> ObtenerUsuariosAsync()
        {
            var response = await _httpClient.GetAsync("Balcan/Usuario");
            response.EnsureSuccessStatusCode();

            var resultado = await response.Content
                .ReadFromJsonAsync<BalcaxApiResponse<List<BalcaxUsuario>>>(_jsonOptions);

            if (resultado is null || !resultado.Success)
                throw new InvalidOperationException(resultado?.Message ?? "Respuesta inválida del servidor.");

            return resultado.Data ?? new List<BalcaxUsuario>();
        }

        /// <summary>
        /// Busca usuarios utilizando el endpoint de búsqueda general o filtros específicos en la API Balcax.
        /// </summary>
        public async Task<List<BalcaxUsuario>> BuscarUsuariosAsync(string filtro, string valor)
        {
            string endpoint = "";
            if (string.IsNullOrWhiteSpace(valor))
            {
                return await ObtenerUsuariosAsync();
            }

            switch (filtro.ToLower())
            {
                case "id":
                    var uId = await GetByIdAsync(valor);
                    return uId != null ? new List<BalcaxUsuario> { uId } : new List<BalcaxUsuario>();
                case "nombre completo":
                    endpoint = $"Balcan/Usuario/by-fullname/{Uri.EscapeDataString(valor)}";
                    break;
                case "correo":
                    endpoint = $"Balcan/Usuario/by-email/{Uri.EscapeDataString(valor)}";
                    break;
                case "telefono":
                    endpoint = $"Balcan/Usuario/by-phone/{Uri.EscapeDataString(valor)}";
                    break;
                case "avatar":
                    endpoint = $"Balcan/Usuario/by-avatar/{Uri.EscapeDataString(valor)}";
                    break;
                default:
                    endpoint = $"Balcan/Usuario/search/{Uri.EscapeDataString(valor)}";
                    break;
            }

            var response = await _httpClient.GetAsync(endpoint);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<BalcaxUsuario>();
            }

            response.EnsureSuccessStatusCode();

            var resultado = await response.Content
                .ReadFromJsonAsync<BalcaxApiResponse<JsonElement>>(_jsonOptions);

            if (resultado is null || !resultado.Success)
                throw new InvalidOperationException(resultado?.Message ?? "Fallo en la búsqueda.");

            var usuarios = new List<BalcaxUsuario>();
            if (resultado.Data.ValueKind == JsonValueKind.Array)
            {
                var lista = JsonSerializer.Deserialize<List<BalcaxUsuario>>(resultado.Data.GetRawText(), _jsonOptions);
                if (lista != null) usuarios.AddRange(lista);
            }
            else if (resultado.Data.ValueKind == JsonValueKind.Object)
            {
                var item = JsonSerializer.Deserialize<BalcaxUsuario>(resultado.Data.GetRawText(), _jsonOptions);
                if (item != null) usuarios.Add(item);
            }

            return usuarios;
        }

        private async Task<BalcaxUsuario?> GetByIdAsync(string idString)
        {
            var response = await _httpClient.GetAsync($"Balcan/Usuario/by-id/{idString}");
            if (!response.IsSuccessStatusCode) return null;

            var resultado = await response.Content
                .ReadFromJsonAsync<BalcaxApiResponse<BalcaxUsuario>>(_jsonOptions);

            if (resultado != null && resultado.Success)
            {
                return resultado.Data;
            }
            return null;
        }

        /// <summary>
        /// Crea un nuevo usuario Balcax.
        /// </summary>
        public async Task CrearUsuarioAsync(BalcaxUsuario usuario)
        {
            var response = await _httpClient.PostAsJsonAsync("Balcan/Usuario/create", usuario);
            response.EnsureSuccessStatusCode();

            var resultado = await response.Content
                .ReadFromJsonAsync<BalcaxApiResponse<object>>(_jsonOptions);

            if (resultado is null || !resultado.Success)
                throw new InvalidOperationException(resultado?.Message ?? "Error al crear usuario.");
        }

        /// <summary>
        /// Actualiza un usuario Balcax usando POST /update.
        /// </summary>
        public async Task ActualizarUsuarioAsync(BalcaxUsuario usuario)
        {
            var response = await _httpClient.PostAsJsonAsync("Balcan/Usuario/update", usuario);
            response.EnsureSuccessStatusCode();

            var resultado = await response.Content
                .ReadFromJsonAsync<BalcaxApiResponse<object>>(_jsonOptions);

            if (resultado is null || !resultado.Success)
                throw new InvalidOperationException(resultado?.Message ?? "Error al actualizar usuario.");
        }

        /// <summary>
        /// Elimina un usuario Balcax usando POST /delete/{id}.
        /// </summary>
        public async Task EliminarUsuarioAsync(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"Balcan/Usuario/delete/{id}");
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var resultado = await response.Content
                .ReadFromJsonAsync<BalcaxApiResponse<object>>(_jsonOptions);

            if (resultado is null || !resultado.Success)
                throw new InvalidOperationException(resultado?.Message ?? "Error al eliminar usuario.");
        }

        /// <summary>
        /// Obtiene la jerarquía y permisos de un usuario por su ID.
        /// GET /Balcan/Permiso/usuario/{idUsuario}
        /// </summary>
        public async Task<UsuarioPermisosDTO> ObtenerPermisosUsuarioAsync(int id)
        {
            var response = await _httpClient.GetAsync($"Balcan/Permiso/usuario/{id}");
            response.EnsureSuccessStatusCode();

            var resultado = await response.Content
                .ReadFromJsonAsync<BalcaxApiResponse<UsuarioPermisosDTO>>(_jsonOptions);

            if (resultado is null || !resultado.Success)
                throw new InvalidOperationException(resultado?.Message ?? "No se pudieron obtener los permisos del usuario.");

            return resultado.Data ?? new UsuarioPermisosDTO();
        }

        /// <summary>
        /// Guarda o actualiza los permisos de un usuario.
        /// POST /Balcan/Permiso/guardar
        /// </summary>
        public async Task GuardarPermisosUsuarioAsync(GuardarPermisosRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("Balcan/Permiso/save", request);
            response.EnsureSuccessStatusCode();

            var resultado = await response.Content
                .ReadFromJsonAsync<BalcaxApiResponse<object>>(_jsonOptions);

            if (resultado is null || !resultado.Success)
                throw new InvalidOperationException(resultado?.Message ?? "Error al guardar los permisos del usuario.");
        }

        #endregion

        #region MÉTODOS DE MÓDULOS

        /// <summary>
        /// Obtiene la lista general de módulos.
        /// GET /Balcan/Modulo
        /// </summary>
        public async Task<List<BalcaxModulo>> ObtenerModulosAsync()
        {
            var response = await _httpClient.GetAsync("Balcan/Modulo");
            response.EnsureSuccessStatusCode();

            var resultado = await response.Content
                .ReadFromJsonAsync<BalcaxApiResponse<List<BalcaxModulo>>>(_jsonOptions);

            if (resultado is null || !resultado.Success)
                throw new InvalidOperationException(resultado?.Message ?? "Error al obtener la lista de módulos.");

            return resultado.Data ?? new List<BalcaxModulo>();
        }

        /// <summary>
        /// Obtiene un módulo por su ID.
        /// GET /Balcan/Modulo/by-id/{id}
        /// </summary>
        public async Task<BalcaxModulo?> ObtenerModuloPorIdAsync(int idModulo)
        {
            var response = await _httpClient.GetAsync($"Balcan/Modulo/by-id/{idModulo}");
            if (!response.IsSuccessStatusCode) return null;

            var resultado = await response.Content
                .ReadFromJsonAsync<BalcaxApiResponse<BalcaxModulo>>(_jsonOptions);

            if (resultado != null && resultado.Success)
            {
                return resultado.Data;
            }
            return null;
        }

        /// <summary>
        /// Crea un nuevo módulo.
        /// POST /Balcan/Modulo/create
        /// </summary>
        public async Task CrearModuloAsync(BalcaxModulo modulo)
        {
            var response = await _httpClient.PostAsJsonAsync("Balcan/Modulo/create", modulo);
            response.EnsureSuccessStatusCode();

            var resultado = await response.Content
                .ReadFromJsonAsync<BalcaxApiResponse<object>>(_jsonOptions);

            if (resultado is null || !resultado.Success)
                throw new InvalidOperationException(resultado?.Message ?? "Error al crear el módulo.");
        }

        /// <summary>
        /// Actualiza un módulo existente.
        /// POST /Balcan/Modulo/update
        /// </summary>
        public async Task ActualizarModuloAsync(BalcaxModulo modulo)
        {
            var response = await _httpClient.PostAsJsonAsync("Balcan/Modulo/update", modulo);
            response.EnsureSuccessStatusCode();

            var resultado = await response.Content
                .ReadFromJsonAsync<BalcaxApiResponse<object>>(_jsonOptions);

            if (resultado is null || !resultado.Success)
                throw new InvalidOperationException(resultado?.Message ?? "Error al actualizar el módulo.");
        }

        /// <summary>
        /// Elimina un módulo por su ID.
        /// POST /Balcan/Modulo/delete/{id}
        /// </summary>
        public async Task EliminarModuloAsync(int idModulo)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"Balcan/Modulo/delete/{idModulo}");
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var resultado = await response.Content
                .ReadFromJsonAsync<BalcaxApiResponse<object>>(_jsonOptions);

            if (resultado is null || !resultado.Success)
                throw new InvalidOperationException(resultado?.Message ?? "Error al eliminar el módulo.");
        }

        #endregion

        #region MÉTODOS DE VISTAS

        /// <summary>
        /// Obtiene el catálogo completo de vistas disponibles.
        /// GET /Balcan/Vista
        /// </summary>
        public async Task<List<BalcaxVista>> ObtenerVistasAsync()
        {
            var response = await _httpClient.GetAsync("Balcan/Vista");
            response.EnsureSuccessStatusCode();

            var resultado = await response.Content
                .ReadFromJsonAsync<BalcaxApiResponse<List<BalcaxVista>>>(_jsonOptions);

            if (resultado is null || !resultado.Success)
                throw new InvalidOperationException(resultado?.Message ?? "Error al obtener la lista de vistas.");

            return resultado.Data ?? new List<BalcaxVista>();
        }

        /// <summary>
        /// Obtiene una vista por su ID.
        /// GET /Balcan/Vista/by-id/{id}
        /// </summary>
        public async Task<BalcaxVista?> ObtenerVistaPorIdAsync(int idVista)
        {
            var response = await _httpClient.GetAsync($"Balcan/Vista/by-id/{idVista}");
            if (!response.IsSuccessStatusCode) return null;

            var resultado = await response.Content
                .ReadFromJsonAsync<BalcaxApiResponse<BalcaxVista>>(_jsonOptions);

            if (resultado != null && resultado.Success)
            {
                return resultado.Data;
            }
            return null;
        }

        /// <summary>
        /// Crea una nueva vista en el catálogo general.
        /// POST /Balcan/Vista/create
        /// </summary>
        public async Task CrearVistaAsync(BalcaxVista vista)
        {
            var response = await _httpClient.PostAsJsonAsync("Balcan/Vista/create", vista);
            response.EnsureSuccessStatusCode();

            var resultado = await response.Content
                .ReadFromJsonAsync<BalcaxApiResponse<object>>(_jsonOptions);

            if (resultado is null || !resultado.Success)
                throw new InvalidOperationException(resultado?.Message ?? "Error al crear la vista.");
        }

        /// <summary>
        /// Actualiza una vista del catálogo.
        /// POST /Balcan/Vista/update
        /// </summary>
        public async Task ActualizarVistaAsync(BalcaxVista vista)
        {
            var response = await _httpClient.PostAsJsonAsync("Balcan/Vista/update", vista);
            response.EnsureSuccessStatusCode();

            var resultado = await response.Content
                .ReadFromJsonAsync<BalcaxApiResponse<object>>(_jsonOptions);

            if (resultado is null || !resultado.Success)
                throw new InvalidOperationException(resultado?.Message ?? "Error al actualizar la vista.");
        }

        /// <summary>
        /// Elimina una vista del catálogo.
        /// POST /Balcan/Vista/delete/{id}
        /// </summary>
        public async Task EliminarVistaAsync(int idVista)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"Balcan/Vista/delete/{idVista}");
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var resultado = await response.Content
                .ReadFromJsonAsync<BalcaxApiResponse<object>>(_jsonOptions);

            if (resultado is null || !resultado.Success)
                throw new InvalidOperationException(resultado?.Message ?? "Error al eliminar la vista.");
        }

        #endregion

        #region MÉTODOS DE ASOCIACIÓN MÓDULO - VISTA (Consumiendo ModuloVistaController)

        /// <summary>
        /// Obtiene las vistas pertenecientes a un módulo según el controlador ModuloVista.
        /// GET /Balcan/ModuloVista/{idModulo}
        /// </summary>
        public async Task<ModuloConVistasDTO?> ObtenerVistasPorModuloAsync(int idModulo)
        {
            var response = await _httpClient.GetAsync($"Balcan/ModuloVista/{idModulo}");
            if (!response.IsSuccessStatusCode) return null;

            var resultado = await response.Content
                .ReadFromJsonAsync<BalcaxApiResponse<ModuloConVistasDTO>>(_jsonOptions);

            if (resultado is null || !resultado.Success)
                return null;

            return resultado.Data;
        }

        /// <summary>
        /// Asocia una vista individual a un módulo.
        /// POST /Balcan/ModuloVista/create
        /// </summary>
        public async Task AsociarVistaAModuloAsync(int idModulo, int idVista)
        {
            var payload = new { IdModulo = idModulo, IdVista = idVista };
            var response = await _httpClient.PostAsJsonAsync("Balcan/ModuloVista/create", payload);
            response.EnsureSuccessStatusCode();

            var resultado = await response.Content
                .ReadFromJsonAsync<BalcaxApiResponse<object>>(_jsonOptions);

            if (resultado is null || !resultado.Success)
                throw new InvalidOperationException(resultado?.Message ?? "Error al asociar la vista al módulo.");
        }

        /// <summary>
        /// Sincroniza la lista completa de IDs de vistas asociadas a un módulo.
        /// POST /Balcan/ModuloVista/update
        /// </summary>
        public async Task SincronizarVistasModuloAsync(int idModulo, List<int> vistasIds)
        {
            var payload = new { IdModulo = idModulo, VistasIds = vistasIds };
            var response = await _httpClient.PostAsJsonAsync("Balcan/ModuloVista/update", payload);
            response.EnsureSuccessStatusCode();

            var resultado = await response.Content
                .ReadFromJsonAsync<BalcaxApiResponse<object>>(_jsonOptions);

            if (resultado is null || !resultado.Success)
                throw new InvalidOperationException(resultado?.Message ?? "Error al sincronizar las vistas del módulo.");
        }

        /// <summary>
        /// Remueve la asociación de una vista específica con un módulo.
        /// POST /Balcan/ModuloVista/delete
        /// </summary>
        public async Task DesasociarVistaDeModuloAsync(int idModulo, int idVista)
        {
            var payload = new { IdModulo = idModulo, IdVista = idVista };
            var response = await _httpClient.PostAsJsonAsync("Balcan/ModuloVista/delete", payload);
            response.EnsureSuccessStatusCode();

            var resultado = await response.Content
                .ReadFromJsonAsync<BalcaxApiResponse<object>>(_jsonOptions);

            if (resultado is null || !resultado.Success)
                throw new InvalidOperationException(resultado?.Message ?? "Error al desasociar la vista del módulo.");
        }

        #endregion

        #region PERMISOS
        
        /// <summary>
        /// Obtiene todos los Trabajadores-Usuarios.
        /// GET /Balcan/TrabajadorUsuario
        /// </summary>
        public async Task<List<TrabajadorUsuarioDTO>> ObtenerTrabajadoresUsuariosAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<BalcaxApiResponse<List<TrabajadorUsuarioDTO>>>("Balcan/TrabajadorUsuario", _jsonOptions);
            return response?.Data ?? new List<TrabajadorUsuarioDTO>();
        }

        /// <summary>
        /// Busca un Trabajador-Usuario por IdUsuario.
        /// GET /Balcan/TrabajadorUsuario/Usuario/{id}
        /// </summary>
        public async Task<List<TrabajadorUsuarioDTO>> BuscarTrabajadorUsuarioPorIdAsync(int idUsuario)
        {
            // Se mapea la respuesta a un objeto único
            var response = await _httpClient.GetFromJsonAsync<BalcaxApiResponse<TrabajadorUsuarioDTO>>($"Balcan/TrabajadorUsuario/Usuario/{idUsuario}", _jsonOptions);

            if (response?.Data != null)
            {
                return new List<TrabajadorUsuarioDTO> { response.Data };
            }

            return new List<TrabajadorUsuarioDTO>();
        }

        /// <summary>
        /// Busca Trabajadores-Usuarios por Código de Trabajador.
        /// GET /Balcan/TrabajadorUsuario/Trabajador/{codigo_trabajador}
        /// </summary>
        public async Task<List<TrabajadorUsuarioDTO>> BuscarTrabajadorUsuarioPorCodigoAsync(string codigoTrabajador)
        {
            // Se mapea la respuesta a un objeto único
            var response = await _httpClient.GetFromJsonAsync<BalcaxApiResponse<TrabajadorUsuarioDTO>>($"Balcan/TrabajadorUsuario/Trabajador/{codigoTrabajador}", _jsonOptions);

            if (response?.Data != null)
            {
                return new List<TrabajadorUsuarioDTO> { response.Data };
            }

            return new List<TrabajadorUsuarioDTO>();
        }

        #endregion
    }
}