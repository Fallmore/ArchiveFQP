using ArchiveFqp.Models.AiExtractor;
using ArchiveFqp.Models.DTO.Attribute;
using ArchiveFqp.Models.Settings.SettingsArchive;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArchiveFqp.Services.AiExtractor
{
    public class AiExtractorService
    {
        private readonly HttpClient _http;
        private readonly SettingsArchive _settingsArchive;

        public AiExtractorService(HttpClient http, SettingsArchive settingsArchive)
        {
            _http = http;
            _settingsArchive = settingsArchive;
        }


        public async Task<HealthResponse> Health()
        {
            try
            {
                HealthResponse? response = await _http.GetFromJsonAsync<HealthResponse>("ai-extract/health");
                return response ?? new() { Status = "error" };
            }
            catch (Exception)
            {
                return new() { Status = "error" };
            }
        }

        // Получить список документов
        public async Task<DocumentsResponse> GetDocumentsAsync()
        {
            DocumentsResponse? response = await _http.GetFromJsonAsync<DocumentsResponse>("ai-extract/documents");
            return response ?? new() { Error = "Сервис недоступен" };
        }

        // Индексировать уже существующий PDF-файл в базу
        public async Task<IndexResponse> IndexDocumentAsync(string path, bool isPathRelative = true, bool forceRebuild = false)
        {
            string fullPath = path;
            if (isPathRelative)
            {
                string worksPath = Path.Combine(_settingsArchive.FilesRootPath, _settingsArchive.FolderDataName, _settingsArchive.FolderWorksName);
                string fileName = _settingsArchive.AllowedFiles.Keys.First(x => x == "Пояснительная записка (Pdf)");
                fullPath = Path.Combine(worksPath, path, fileName + ".pdf");
            }
            string encodedPath = Uri.EscapeDataString(fullPath);
            string url = $"ai-extract/documents/index/{encodedPath}?force_rebuild={forceRebuild.ToString().ToLower()}";

            var response = await _http.PostAsync(url, null);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IndexResponse>() ?? new() { Error = "Сервис недоступен" };
        }

        // Удалить документ из базы (файл остаётся на диске)
        public async Task<RemoveResponse> RemoveDocumentAsync(string path, bool isPathRelative = true)
        {
            string fullPath = path;
            if (isPathRelative)
            {
                string worksPath = Path.Combine(_settingsArchive.FilesRootPath, _settingsArchive.FolderDataName, _settingsArchive.FolderWorksName);
                string fileName = _settingsArchive.AllowedFiles.Keys.First(x => x == "Пояснительная записка (Pdf)");
                fullPath = Path.Combine(worksPath, path, fileName + ".pdf");
            }
            string encodedPath = Uri.EscapeDataString(fullPath);

            var response = await _http.DeleteAsync($"ai-extract/documents/remove/{encodedPath}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<RemoveResponse>() ?? new() { Error = "Сервис недоступен" };
        }

        // Запустить извлечение титульника
        public async Task<TaskStartResponse> StartTitleExtractionAsync(string path, bool isPathRelative = true, string? fileName = null, string modelAi = "llama3.2")
        {
            string fullPath = path;
            if (isPathRelative)
            {
                string worksPath = Path.Combine(_settingsArchive.FilesRootPath, _settingsArchive.FolderDataName, _settingsArchive.FolderWorksName);
                fileName ??= _settingsArchive.AllowedFiles.Keys.First(x => x == "Пояснительная записка (Pdf)") + ".pdf";
                fullPath = Path.Combine(worksPath, path, fileName);
            }
            var request = new { file_path = fullPath, model_ai = modelAi };
            var response = await _http.PostAsJsonAsync("ai-extract/extract-title/start", request);

            response.EnsureSuccessStatusCode();
            TaskStartResponse? result = await response.Content.ReadFromJsonAsync<TaskStartResponse>();
            return result ?? new() { Status = "error", Error = "Сервис недоступен" };
        }

        public async Task<ExtractTitleStatusResponse> GetTitleExtractionStatusAsync(string taskId)
        {
            return await _http.GetFromJsonAsync<ExtractTitleStatusResponse>($"ai-extract/extract-title/status/{taskId}")
                ?? new() { Error = "Сервис недоступен" };
        }

        public async Task CancelTitleExtractionAsync(string taskId)
        {
            await _http.PostAsync($"ai-extract/extract-title/cancel/{taskId}", null);
        }

        // Запустить поиск фиксированных атрибутов для проверки
        public async Task<TaskStartResponse> StartSearchAsync(string path, bool isPathRelative = true, string modelAi = "llama3.2")
        {
            string fullPath = path;
            if (isPathRelative)
            {
                string worksPath = Path.Combine(_settingsArchive.FilesRootPath, _settingsArchive.FolderDataName, _settingsArchive.FolderWorksName);
                string fileName = _settingsArchive.AllowedFiles.Keys.First(x => x == "Пояснительная записка (Pdf)") + ".pdf";
                fullPath = Path.Combine(worksPath, path, fileName);
            }

            var request = new { file_path = fullPath, model_ai = modelAi };
            var response = await _http.PostAsJsonAsync("ai-extract/search-attributes/start", request);

            response.EnsureSuccessStatusCode();
            TaskStartResponse? result = await response.Content.ReadFromJsonAsync<TaskStartResponse>();
            return result ?? new() { Status = "error", Error = "Сервис недоступен" };
        }

        public async Task<TaskStatusResponse> GetSearchStatusAsync(string taskId)
        {
            return await _http.GetFromJsonAsync<TaskStatusResponse>($"ai-extract/search-attributes/status/{taskId}")
                ?? new() { Error = "Сервис недоступен" };
        }

        public async Task CancelSearchAsync(string taskId)
        {
            await _http.PostAsync($"ai-extract/search-attributes/cancel/{taskId}", null);
        }

        // Запустить поиск атрибутов из бд
        public async Task<TaskStartResponse> StartProcessWorkAsync(string path, int workId, bool isPathRelative = true, string modelAi = "llama3.2")
        {
            string fullPath = path;
            if (isPathRelative)
            {
                string worksPath = Path.Combine(_settingsArchive.FilesRootPath, _settingsArchive.FolderDataName, _settingsArchive.FolderWorksName);
                string fileName = _settingsArchive.AllowedFiles.Keys.First(x => x == "Пояснительная записка (Pdf)") + ".pdf";
                fullPath = Path.Combine(worksPath, path, fileName);
            }

            var request = new { file_path = fullPath, work_id = workId, model_ai = modelAi };
            var response = await _http.PostAsJsonAsync("ai-extract/process-work/start", request);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<TaskStartResponse>();
            return result ?? new() { Status = "error", Error = "Сервис недоступен" };
        }

        public async Task<ProcessWorkStatusResponse> GetProcessWorkStatusAsync(string taskId)
        {
            return await _http.GetFromJsonAsync<ProcessWorkStatusResponse>($"ai-extract/process-work/status/{taskId}")
                ?? new() { Error = "Сервис недоступен" };
        }

        public async Task CancelProcessWorkAsync(string taskId)
        {
            await _http.PostAsync($"ai-extract/process-work/cancel/{taskId}", null);
        }
    }
}
