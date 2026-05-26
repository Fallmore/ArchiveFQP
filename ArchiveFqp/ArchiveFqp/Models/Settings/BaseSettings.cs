using DocumentFormat.OpenXml.Office2010.ExcelAc;

namespace ArchiveFqp.Models.Settings
{
    public class BaseSettings
    {
        /// <summary>
        /// Словарь разрешенных файлов со словарем разрешенных расширений
        /// и : файл-расширения
        /// </summary>
        public Dictionary<string, List<string>> AllowedFiles { get; set; } = new()
        {
            { "Пояснительная записка (Word)", [".doc", ".docx"] },
            { "Пояснительная записка (Pdf)", [".pdf"] },
            { "Презентация", [".ppt", ".pptx"] },
            { "Исходный код",[".zip", ".rar", ".7z"] },
            { "База данных", [".sql", ".bak", ".backup", ".dump", ".txt"] },
            { "Пароли",[".txt"] }
        };

        /// <summary>
        /// Словарь необходимых файлов у типов работ: тип работы-файлы
        /// </summary>
        public Dictionary<string, List<string>> RequiredFiles { get; set; } = new()
        {
            {"ВКРБ", ["Пояснительная записка (Word)", "Пояснительная записка (Pdf)", "Презентация"]},
            {"МД", ["Пояснительная записка (Word)", "Пояснительная записка (Pdf)", "Презентация"]},
            {"Отчет по практике", ["Пояснительная записка (Word)", "Пояснительная записка (Pdf)", "Презентация"]}
        };

        //TODO Можете сделать иерархию файлов через БД, у меня нет времени делать и переделывать(

        #region Для реализации иерархии файлов. Но нужны таблицы в БД, потому что json мало будет под это
        //public List<AllowedFile> AllowedFiles { get; set; } =
        //[
        //    new () {Name = "Пояснительная записка (Word)", IsDeletable = false, Extensions= new() { { true, [".doc", ".docx"] } } },
        //    new () {Name = "Пояснительная записка (PDF)", IsDeletable = false, Extensions= new() { { true, [".pdf"] } } },
        //    new () {Name = "Презентация", IsDeletable = false, Extensions= new() { { true, [".ppt", ".pptx"] } } },
        //    new () {Name = "Исходный код", Extensions= new() { { false, [".zip", ".rar", ".7z"] } } },
        //    new () {Name = "База данных", Extensions= new() { { false, [".sql", ".bak", ".backup", ".dump", ".txt"] } } },
        //    new () {Name = "Пароли", Extensions= new() { { false, [".txt"] } } },
        //];

        //public List<RequiredFile> RequiredFiles { get; set; } =
        //    [
        //    new () {WorkType = "ВКРБ", Files= new() { { true, ["Пояснительная записка (Word)", "Пояснительная записка (Pdf)", "Презентация"] } } },
        //    new () {WorkType = "МД", Files= new() { { true, ["Пояснительная записка (Word)", "Пояснительная записка (Pdf)", "Презентация"] } } },
        //    new () {WorkType = "Отчет по практике", Files= new() { { true, ["Пояснительная записка (Word)", "Пояснительная записка (Pdf)", "Презентация"] } } },
        //];

        //public class AllowedFile()
        //{
        //    public string Name { get; set; } = string.Empty;

        //    /// <summary>
        //    /// Словарь расширений с булевым значением важности: важность-расширение
        //    /// </summary>
        //    public Dictionary<bool, List<string>> Extensions { get; set; } = new();

        //    public bool IsDeletable { get; set; } = true;
        //}

        //public class RequiredFile()
        //{
        //    public string WorkType { get; set; } = string.Empty;

        //    /// <summary>
        //    /// Словарь файлов с булевым значением важности: важность-расширение
        //    /// </summary>
        //    public Dictionary<bool, List<string>> Files { get; set; } = new();
        //}

        //// Управление типами файлов
        //public void AddFileType(string name, List<string> extensions, bool isDeletable = true)
        //{
        //    if (AllowedFiles.Any(f => f.Name == name))
        //        return;

        //    var extensionsDict = new Dictionary<bool, List<string>>();
        //    if (extensions.Any())
        //    {
        //        extensionsDict[false] = extensions;
        //    }

        //    AllowedFiles.Add(new AllowedFile
        //    {
        //        Name = name,
        //        IsDeletable = isDeletable,
        //        Extensions = extensionsDict
        //    });

        //}

        //public void RemoveFileType(string name)
        //{
        //    var fileType = AllowedFiles.FirstOrDefault(f => f.Name == name);
        //    if (fileType != null && fileType.IsDeletable)
        //    {
        //        AllowedFiles.Remove(fileType);
        //    }
        //}

        //// Управление расширениями
        //public void AddExtension(string fileName, string extension, bool isImportant = false)
        //{
        //    var fileType = AllowedFiles.FirstOrDefault(f => f.Name == fileName);
        //    if (fileType == null) return;

        //    if (!extension.StartsWith("."))
        //        extension = "." + extension;

        //    // Проверяем, существует ли уже такое расширение
        //    foreach (var extGroup in fileType.Extensions)
        //    {
        //        if (extGroup.Value.Contains(extension))
        //            return;
        //    }

        //    if (!fileType.Extensions.ContainsKey(isImportant))
        //    {
        //        fileType.Extensions[isImportant] = new List<string>();
        //    }

        //    fileType.Extensions[isImportant].Add(extension);
        //}

        //public void RemoveExtension(string fileName, string extension)
        //{
        //    var fileType = AllowedFiles.FirstOrDefault(f => f.Name == fileName);
        //    if (fileType == null) return;

        //    foreach (var extGroup in fileType.Extensions.ToList())
        //    {
        //        if (extGroup.Value.Contains(extension))
        //        {
        //            // Важные расширения нельзя удалять
        //            if (extGroup.Key)
        //                return;

        //            extGroup.Value.Remove(extension);

        //            // Если группа стала пустой, удаляем её
        //            if (!extGroup.Value.Any())
        //            {
        //                fileType.Extensions.Remove(extGroup.Key);
        //            }

        //            break;
        //        }
        //    }
        //}

        //public bool CanDeleteExtension(string fileName, string extension)
        //{
        //    var fileType = AllowedFiles.FirstOrDefault(f => f.Name == fileName);
        //    if (fileType == null) return false;

        //    foreach (var extGroup in fileType.Extensions)
        //    {
        //        if (extGroup.Value.Contains(extension))
        //        {
        //            return !extGroup.Key; // Важные расширения нельзя удалять
        //        }
        //    }

        //    return true;
        //}

        //// Управление необходимыми файлами
        //public void AddRequiredFile(string workType, string fileName, bool isImportant = false)
        //{
        //    var requiredFile = RequiredFiles.FirstOrDefault(r => r.WorkType == workType);
        //    if (requiredFile == null)
        //    {
        //        requiredFile = new RequiredFile
        //        {
        //            WorkType = workType,
        //            Files = new Dictionary<bool, List<string>>()
        //        };
        //        RequiredFiles.Add(requiredFile);
        //    }

        //    // Проверяем, существует ли уже такой файл
        //    foreach (var fileGroup in requiredFile.Files)
        //    {
        //        if (fileGroup.Value.Contains(fileName))
        //            return;
        //    }

        //    if (!requiredFile.Files.ContainsKey(isImportant))
        //    {
        //        requiredFile.Files[isImportant] = new List<string>();
        //    }

        //    requiredFile.Files[isImportant].Add(fileName);
        //}

        //public void RemoveRequiredFile(string workType, string fileName)
        //{
        //    var requiredFile = RequiredFiles.FirstOrDefault(r => r.WorkType == workType);
        //    if (requiredFile == null) return;

        //    foreach (var fileGroup in requiredFile.Files.ToList())
        //    {
        //        if (fileGroup.Value.Contains(fileName))
        //        {
        //            // Важные файлы нельзя удалять
        //            if (fileGroup.Key)
        //                return;

        //            fileGroup.Value.Remove(fileName);

        //            // Если группа стала пустой, удаляем её
        //            if (!fileGroup.Value.Any())
        //            {
        //                requiredFile.Files.Remove(fileGroup.Key);
        //            }

        //            // Если не осталось файлов, удаляем всю запись
        //            if (!requiredFile.Files.Any())
        //            {
        //                RequiredFiles.Remove(requiredFile);
        //            }

        //            break;
        //        }
        //    }
        //}

        //public bool CanDeleteRequiredFile(string workType, string fileName)
        //{
        //    var requiredFile = RequiredFiles.FirstOrDefault(r => r.WorkType == workType);
        //    if (requiredFile == null) return false;

        //    foreach (var fileGroup in requiredFile.Files)
        //    {
        //        if (fileGroup.Value.Contains(fileName))
        //        {
        //            return !fileGroup.Key; // Важные файлы нельзя удалять
        //        }
        //    }

        //    return true;
        //}

        //public bool CanDeleteWorkType(string workType)
        //{
        //    var requiredFile = RequiredFiles.FirstOrDefault(r => r.WorkType == workType);
        //    if (requiredFile == null) return true;

        //    // Проверяем, есть ли важные файлы
        //    return !requiredFile.Files.Any(f => f.Key);
        //}

        //public void RemoveWorkType(string workType)
        //{
        //    var requiredFile = RequiredFiles.FirstOrDefault(r => r.WorkType == workType);
        //    if (requiredFile != null && CanDeleteWorkType(workType))
        //    {
        //        RequiredFiles.Remove(requiredFile);
        //    }
        //}

        //public void OnWorkTypeChanged(string oldWorkType, string newWorkType)
        //{
        //    if (oldWorkType == newWorkType) return;

        //    var requiredFile = RequiredFiles.FirstOrDefault(r => r.WorkType == oldWorkType);
        //    if (requiredFile != null)
        //    {
        //        requiredFile.WorkType = newWorkType;
        //    }
        //}
        #endregion
    }
}
