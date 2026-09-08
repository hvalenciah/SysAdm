using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using WpfApp1.Models;

namespace WpfApp1.Services
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
                case "nombre":
                    endpoint = $"Balcan/Usuario/by-name/{Uri.EscapeDataString(valor)}";
                    break;
                case "apellidos":
                    endpoint = $"Balcan/Usuario/by-lastname/{Uri.EscapeDataString(valor)}";
                    break;
                case "correo":
                    endpoint = $"Balcan/Usuario/by-email/{Uri.EscapeDataString(valor)}";
                    break;
                case "telefono":
                    endpoint = $"Balcan/Usuario/by-phone/{Uri.EscapeDataString(valor)}";
                    break;
                default: // Filtro General
                    endpoint = $"Balcan/Usuario/search/{Uri.EscapeDataString(valor)}";
                    break;
            }

            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            var resultado = await response.Content
                .ReadFromJsonAsync<BalcaxApiResponse<List<BalcaxUsuario>>>(_jsonOptions);

            if (resultado is null || !resultado.Success)
                throw new InvalidOperationException(resultado?.Message ?? "Fallo en la búsqueda.");

            return resultado.Data ?? new List<BalcaxUsuario>();
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
        public async Task<UsuarioPermisosDTO> ObtenerPermisosUsuarioAsync(int idUsuario)
        {
            var response = await _httpClient.GetAsync($"Balcan/Permiso/usuario/{idUsuario}");
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
            var response = await _httpClient.PostAsJsonAsync("Balcan/Permiso/guardar", request);
            response.EnsureSuccessStatusCode();

            var resultado = await response.Content
                .ReadFromJsonAsync<BalcaxApiResponse<object>>(_jsonOptions);

            if (resultado is null || !resultado.Success)
                throw new InvalidOperationException(resultado?.Message ?? "Error al guardar los permisos del usuario.");
        }
    }
}
