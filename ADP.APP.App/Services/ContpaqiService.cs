using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using SysAdm.Models;

namespace SysAdm.Services
{
    public class ContpaqiService
    {
        private readonly HttpClient _httpClient;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        #region MÉTODOS DE PLAZAS
        public ContpaqiService(string baseUrl)
        {
            string url = baseUrl.TrimEnd('/') + "/";

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(url)
            };

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<List<PlazaTrabajadorDTO>> ObtenerPlazasAsync()
        {
            var response = await _httpClient.GetAsync("CONTPAQi/PlazaTrabajador");
            response.EnsureSuccessStatusCode();

            var resultado = await response.Content.ReadFromJsonAsync<BalcaxApiResponse<List<PlazaTrabajadorDTO>>>(_jsonOptions);
            if (resultado is null || !resultado.Success)
                throw new InvalidOperationException(resultado?.Message ?? "Respuesta inválida del servidor.");

            return resultado.Data ?? new List<PlazaTrabajadorDTO>();
        }

        public async Task<List<PlazaTrabajadorDTO>> BuscarPlazasAsync(string filtro, string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return await ObtenerPlazasAsync();
            }

            string endpoint = filtro.ToLower() switch
            {
                "código de plaza" => $"CONTPAQi/PlazaTrabajador/by-plaza/{Uri.EscapeDataString(valor)}",
                "código de trabajador" => $"CONTPAQi/PlazaTrabajador/by-trabajador/{Uri.EscapeDataString(valor)}",
                "nombre completo" => $"CONTPAQi/PlazaTrabajador/by-fullname/{Uri.EscapeDataString(valor)}",
                _ => $"CONTPAQi/PlazaTrabajador/by-plaza/{Uri.EscapeDataString(valor)}"
            };

            var response = await _httpClient.GetAsync(endpoint);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<PlazaTrabajadorDTO>();
            }

            response.EnsureSuccessStatusCode();

            var resultado = await response.Content.ReadFromJsonAsync<BalcaxApiResponse<JsonElement>>(_jsonOptions);
            if (resultado is null || !resultado.Success)
                throw new InvalidOperationException(resultado?.Message ?? "Error al procesar la consulta.");

            var plazas = new List<PlazaTrabajadorDTO>();
            if (resultado.Data.ValueKind == JsonValueKind.Array)
            {
                var lista = JsonSerializer.Deserialize<List<PlazaTrabajadorDTO>>(resultado.Data.GetRawText(), _jsonOptions);
                if (lista != null) plazas.AddRange(lista);
            }
            else if (resultado.Data.ValueKind == JsonValueKind.Object)
            {
                var item = JsonSerializer.Deserialize<PlazaTrabajadorDTO>(resultado.Data.GetRawText(), _jsonOptions);
                if (item != null) plazas.Add(item);
            }

            return plazas;
        }
        #endregion

        #region MÉTODOS DE TRABAJADORES
        /// <summary>
        /// Obtiene la lista general de trabajadores.
        /// GET /CONTPAQi/Trabajador
        /// </summary>
        public async Task<List<TrabajadorDTO>> ObtenerTrabajadoresAsync()
        {
            var response = await _httpClient.GetAsync("CONTPAQi/Trabajador");
            response.EnsureSuccessStatusCode();

            var resultado = await response.Content.ReadFromJsonAsync<BalcaxApiResponse<List<TrabajadorDTO>>>(_jsonOptions);
            if (resultado is null || !resultado.Success)
                throw new InvalidOperationException(resultado?.Message ?? "Respuesta inválida del servidor.");

            return resultado.Data ?? new List<TrabajadorDTO>();
        }

        /// <summary>
        /// Busca trabajadores según el filtro seleccionado.
        /// </summary>
        public async Task<List<TrabajadorDTO>> BuscarTrabajadoresAsync(string filtro, string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return await ObtenerTrabajadoresAsync();
            }

            string endpoint = filtro.ToLower() switch
            {
                "id" => $"CONTPAQi/Trabajador/by-id/{Uri.EscapeDataString(valor)}",
                "código de trabajador" => $"CONTPAQi/Trabajador/by-trabajador/{Uri.EscapeDataString(valor)}",
                "nombre completo" => $"CONTPAQi/Trabajador/by-fullname/{Uri.EscapeDataString(valor)}",
                _ => $"CONTPAQi/Trabajador/by-trabajador/{Uri.EscapeDataString(valor)}"
            };

            var response = await _httpClient.GetAsync(endpoint);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<TrabajadorDTO>();
            }

            response.EnsureSuccessStatusCode();

            var resultado = await response.Content.ReadFromJsonAsync<BalcaxApiResponse<JsonElement>>(_jsonOptions);
            if (resultado is null || !resultado.Success)
                throw new InvalidOperationException(resultado?.Message ?? "Error al procesar la consulta.");

            var lista = new List<TrabajadorDTO>();
            if (resultado.Data.ValueKind == JsonValueKind.Array)
            {
                var deserialized = JsonSerializer.Deserialize<List<TrabajadorDTO>>(resultado.Data.GetRawText(), _jsonOptions);
                if (deserialized != null) lista.AddRange(deserialized);
            }
            else if (resultado.Data.ValueKind == JsonValueKind.Object)
            {
                var item = JsonSerializer.Deserialize<TrabajadorDTO>(resultado.Data.GetRawText(), _jsonOptions);
                if (item != null) lista.Add(item);
            }

            return lista;
        }
        #endregion

        #region MÉTODOS DE VACACIONES
        /// <summary>
        /// Obtiene el catálogo completo de vacaciones activas.
        /// GET /CONTPAQi/Vacaciones
        /// </summary>
        public async Task<List<VacacionesDTO>> ObtenerVacacionesAsync()
        {
            var response = await _httpClient.GetAsync("CONTPAQi/Vacaciones");
            response.EnsureSuccessStatusCode();

            var resultado = await response.Content.ReadFromJsonAsync<BalcaxApiResponse<List<VacacionesDTO>>>(_jsonOptions);
            if (resultado is null || !resultado.Success)
                throw new InvalidOperationException(resultado?.Message ?? "Respuesta inválida del servidor.");

            return resultado.Data ?? new List<VacacionesDTO>();
        }

        /// <summary>
        /// Busca registros de vacaciones por ID o código de trabajador.
        /// </summary>
        public async Task<List<VacacionesDTO>> BuscarVacacionesAsync(string filtro, string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return await ObtenerVacacionesAsync();
            }

            string endpoint = filtro.ToLower() switch
            {
                "id" => $"CONTPAQi/Vacaciones/by-id/{Uri.EscapeDataString(valor)}",
                "número de trabajador" => $"CONTPAQi/Vacaciones/by-trabajador/{Uri.EscapeDataString(valor)}",
                _ => $"CONTPAQi/Vacaciones/by-trabajador/{Uri.EscapeDataString(valor)}"
            };

            var response = await _httpClient.GetAsync(endpoint);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<VacacionesDTO>();
            }

            response.EnsureSuccessStatusCode();

            var resultado = await response.Content.ReadFromJsonAsync<BalcaxApiResponse<JsonElement>>(_jsonOptions);
            if (resultado is null || !resultado.Success)
                throw new InvalidOperationException(resultado?.Message ?? "Error al procesar la consulta.");

            var lista = new List<VacacionesDTO>();
            if (resultado.Data.ValueKind == JsonValueKind.Array)
            {
                var deserialized = JsonSerializer.Deserialize<List<VacacionesDTO>>(resultado.Data.GetRawText(), _jsonOptions);
                if (deserialized != null) lista.AddRange(deserialized);
            }
            else if (resultado.Data.ValueKind == JsonValueKind.Object)
            {
                var item = JsonSerializer.Deserialize<VacacionesDTO>(resultado.Data.GetRawText(), _jsonOptions);
                if (item != null) lista.Add(item);
            }

            return lista;
        }

        /// <summary>
        /// Actualiza un registro de vacaciones por su ID.
        /// POST /CONTPAQi/Vacaciones/update
        /// </summary>
        public async Task ActualizarVacacionesAsync(VacacionesDTO vacaciones)
        {
            var response = await _httpClient.PostAsJsonAsync("CONTPAQi/Vacaciones/update", vacaciones);
            
            if (response.IsSuccessStatusCode)
            {
                var resultado = await response.Content.ReadFromJsonAsync<BalcaxApiResponse<object>>(_jsonOptions);
                if (resultado is null || !resultado.Success)
                    throw new InvalidOperationException(resultado?.Message ?? "Error al actualizar el registro de vacaciones.");
            }
            else
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Error HTTP {(int)response.StatusCode} ({response.ReasonPhrase}): {errorContent}");
            }
        }

        #endregion
    }
}