using ArchiveFqp.Interfaces.ReferenceData;
using ArchiveFqp.Interfaces.User;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Attribute;
using ArchiveFqp.Models.DTO.Student;
using ArchiveFqp.Models.DTO.Work;
using ArchiveFqp.Models.ReferenceData;
using ArchiveFqp.Models.Search;
using ArchiveFqp.Services.FileUpload;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;

namespace ArchiveFqp.Services.Report
{
    public class ReportService
    {
        private readonly IReferenceDataService _refDataService;
        private readonly WorkFileUploadService _workFileUploadService;
        private readonly IUserService _userService;

        public ReportService(IReferenceDataService refDataService,
            WorkFileUploadService workFileUploadService, IUserService userService)
        {
            _refDataService = refDataService;
            _workFileUploadService = workFileUploadService;
            _userService = userService;
        }

        private struct Statistic
        {
            public string Name { get; set; }
            public int Count { get; set; }
        }

        public enum SectionPageOrientationValues
        {
            Portrait,
            Landscape
        }

        public async Task<byte[]> GenerateReportAsync(ReportConfig config, WorkSearchModel searchModel,
            List<WorkDisplayDto>? works, ReferenceDataSnapshot? snapshot)
        {
            works ??= [];
            snapshot ??= await _refDataService.GetAllAsync();

            using var memoryStream = new MemoryStream();
            using (var wordDocument = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document))
            {
                var mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());

                var portraitSection = CreateSection(SectionPageOrientationValues.Landscape);
                body.AppendChild(portraitSection);

                // Заголовок отчета
                AddHeading(body, config.ReportTitle, 1);
                await AddReportHeader(body, searchModel, works, snapshot);

                if (works.Count != 0)
                {
                    // Статистика
                    if (config.IncludeStatistics)
                    {
                        await AddStatisticsSection(body, works, config);
                    }

                    // Распределение по атрибутам
                    if (config.IncludeDistribution && config.SelectedAttributes.Count != 0)
                    {
                        await AddAttributeDistributionSection(body, works, config);
                    }

                    // Список работ
                    if (config.IncludeWorksList)
                    {
                        // Разрыв страницы
                        body.AppendChild(new Paragraph(new Run(new Break() { Type = BreakValues.Page })));

                        await AddWorksListSection(body, works, config);
                    }
                }

                body.AppendChild(new Paragraph(new Run(new Break() { Type = BreakValues.Page })));
            }

            return memoryStream.ToArray();
        }

        private async Task AddReportHeader(Body body, WorkSearchModel searchModel,
            List<WorkDisplayDto> works, ReferenceDataSnapshot snapshot)
        {
            AddParagraph(body, $"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");

            // Информация о периоде
            {
                var periodText = "Период:";

                if (searchModel.MinYearDefense.HasValue && searchModel.MaxYearDefense.HasValue)
                {
                    if (searchModel.MinYearDefense == searchModel.MaxYearDefense)
                        periodText += $" за {searchModel.MinYearDefense} г.";
                    else
                        periodText += $" за {searchModel.MinYearDefense}-{searchModel.MaxYearDefense} гг.";

                }
                else if (searchModel.MinYearDefense.HasValue)
                    periodText += $" с {searchModel.MinYearDefense} г.";
                else if (searchModel.MaxYearDefense.HasValue)
                    periodText += $" по {searchModel.MaxYearDefense} г.";
                else
                    periodText += $" за все время";

                if (searchModel.MinDateAdded.HasValue && searchModel.MaxDateAdded.HasValue)
                {
                    periodText += "\nПериод добавления: ";
                    if (searchModel.MinDateAdded == searchModel.MaxDateAdded)
                        periodText += $"с {searchModel.MinDateAdded.Value:dd.MM.yyyy}";
                    else
                        periodText += $"с {searchModel.MinDateAdded.Value:dd.MM.yyyy} "
                            + $"по {searchModel.MaxDateAdded.Value:dd.MM.yyyy} ";

                }
                else if (searchModel.MinDateAdded.HasValue)
                    periodText += $"\nПериод изменения: с {searchModel.MinDateAdded.Value:dd.MM.yyyy}";
                else if (searchModel.MaxDateAdded.HasValue)
                    periodText += $"\nПериод изменения: по {searchModel.MaxDateAdded.Value:dd.MM.yyyy}";

                if (searchModel.MinDateChanged.HasValue && searchModel.MaxDateChanged.HasValue)
                {
                    periodText += "\nПериод изменения: ";
                    if (searchModel.MinDateChanged == searchModel.MaxDateChanged)
                        periodText += $"с {searchModel.MaxDateChanged.Value:dd.MM.yyyy}";
                    else
                        periodText += $"с {searchModel.MinDateChanged.Value:dd.MM.yyyy} "
                            + $"по {searchModel.MaxDateChanged.Value:dd.MM.yyyy} ";

                }
                else if (searchModel.MinDateChanged.HasValue)
                    periodText += $"\nПериод изменения: с {searchModel.MinDateChanged.Value:dd.MM.yyyy}";
                else if (searchModel.MaxDateChanged.HasValue)
                    periodText += $"\nПериод изменения: по {searchModel.MaxDateChanged.Value:dd.MM.yyyy}";

                AddParagraph(body, periodText);
            }

            // Параметры поиска
            AddHeading(body, "Параметры поиска", 2);
            var searchParams = await GetSearchParametersDescription(searchModel, snapshot);
            if (searchParams.Count == 0) AddParagraph(body, "отсутствуют");
            foreach (var param in searchParams)
            {
                AddParagraph(body, $"• {param}");
            }

            AddParagraph(body, $"Всего найдено работ: {works.Count}");
            AddEmptyParagraph(body);
        }

        private async Task<List<string>> GetSearchParametersDescription(WorkSearchModel searchModel,
            ReferenceDataSnapshot snapshot)
        {
            var params_ = new List<string>();

            if (!string.IsNullOrEmpty(searchModel.SearchText))
                params_.Add($"Тема содержит: \"{searchModel.SearchText}\"");

            if (searchModel.IdStudent.HasValue && searchModel.IdStudent.Value > 0)
            {
                var student = await _userService.GetStudentDisplayAsync(searchModel.IdStudent.Value);
                params_.Add($"Студент: {student?.Пользователь.ФИО ?? "Не найден"}");
            }

            if (searchModel.IdTeacher.HasValue && searchModel.IdTeacher.Value > 0)
            {
                var teacher = await _userService.GetTeacherDisplayAsync(searchModel.IdTeacher.Value);
                params_.Add($"Руководитель: {teacher?.Пользователь.ФИО ?? "Не найден"}");
            }

            if (searchModel.IdPost.HasValue && searchModel.IdPost.Value > 0)
                params_.Add($"Должность: {snapshot.Posts.FirstOrDefault(x => x.IdДолжности == searchModel.IdPost)?.Название ?? ""}");

            if (searchModel.IdInstitute.HasValue && searchModel.IdInstitute.Value > 0)
                params_.Add($"Институт: {snapshot.Institutes.FirstOrDefault(x => x.IdИнститута == searchModel.IdInstitute)?.Название ?? ""}");

            if (searchModel.IdDepartment.HasValue && searchModel.IdDepartment.Value > 0)
                params_.Add($"Кафедра: {snapshot.Departments.FirstOrDefault(x => x.IdКафедры == searchModel.IdDepartment)?.Название ?? ""}");

            if (searchModel.IdDirection.HasValue && searchModel.IdDirection.Value > 0)
                params_.Add($"Направление: {snapshot.Directions.FirstOrDefault(x => x.IdНаправления == searchModel.IdDirection)?.Название ?? ""}");

            if (searchModel.IdProfile.HasValue && searchModel.IdProfile.Value > 0)
                params_.Add($"Профиль: {snapshot.Profiles.FirstOrDefault(x => x.IdПрофиля == searchModel.IdProfile)?.Название ?? ""}");

            if (searchModel.IdWorkType > 0)
                params_.Add($"Тип работы: {snapshot.WorkTypes.FirstOrDefault(x => x.IdТипаРаботы == searchModel.IdWorkType)?.Название ?? ""}");

            if (searchModel.IdWorkStatus > 0)
                params_.Add($"Статус: {snapshot.WorkStatuses.FirstOrDefault(x => x.IdСтатусаРаботы == searchModel.IdWorkStatus)?.Название ?? ""}");

            if (searchModel.IdWorkAccess > 0)
                params_.Add($"Доступ: {snapshot.WorkAccess.FirstOrDefault(x => x.IdДоступаРаботы == searchModel.IdWorkAccess)?.Название ?? ""}");

            if (searchModel.IdConsultants.Count != 0 && searchModel.IdConsultants.First() != -1)
            {
                List<int> consultants = snapshot.Consultants
                    .Where(x => searchModel.IdConsultants.Contains(x.Id))
                    .Select(x => x.IdПреподавателя)
                    .ToList();
                List<int> teachers = snapshot.Teachers
                    .Where(x => consultants.Contains(x.IdПреподавателя))
                    .Select(x => x.IdПользователя)
                    .ToList();
                var consultantsDto = await _userService.GetTeacherDisplayAsync(teachers);
                params_.Add($"Консультанты: {string.Join(", ",
                    consultantsDto!
                        .Where(x => searchModel.IdConsultants
                            .Contains(x.Пользователь.Пользователь.IdПользователя))
                            .Select(x => x.Пользователь.ФИО))}"
                );
            }

            if (searchModel.IdReviewers.Count != 0 && searchModel.IdReviewers.First() != -1)
            {
                List<int> reviewers = snapshot.Reviewers
                    .Where(x => searchModel.IdReviewers.Contains(x.Id))
                    .Select(x => x.IdПреподавателя)
                    .ToList();
                List<int> teachers = snapshot.Teachers
                    .Where(x => reviewers.Contains(x.IdПреподавателя))
                    .Select(x => x.IdПользователя)
                    .ToList();
                var reviewersDto = await _userService.GetTeacherDisplayAsync(teachers);
                params_.Add($"Консультанты: {string.Join(", ",
                    reviewersDto!
                        .Where(x => searchModel.IdConsultants
                            .Contains(x.Пользователь.Пользователь.IdПользователя))
                            .Select(x => x.Пользователь.ФИО))}"
                );
            }

            if (searchModel.MinPages.HasValue)
                params_.Add($"Минимум страниц: {searchModel.MinPages.Value}");

            if (searchModel.MaxPages.HasValue)
                params_.Add($"Максимум страниц: {searchModel.MaxPages.Value}");

            if (searchModel.SelectedAttributes.Any())
                params_.Add($"Атрибутов для анализа: {searchModel.SelectedAttributes.Count}");

            return params_;
        }

        private async Task AddStatisticsSection(Body body, List<WorkDisplayDto> works, ReportConfig config)
        {
            AddHeading(body, "Статистический анализ", 2);

            var table = new Table();
            AddTableBorders(table);

            if (config.GroupByWorkType)
            {
                AddStatisticsSubsection(body, "Распределение по типам работ", works.Count,
                    works.GroupBy(w => w.ТипРаботы)
                        .Select(g => new Statistic { Name = g.Key, Count = g.Count() }));
            }

            if (config.GroupByStatus)
            {
                AddStatisticsSubsection(body, "Распределение по статусам", works.Count,
                    works.GroupBy(w => w.СтатусРаботы)
                        .Select(g => new Statistic { Name = g.Key, Count = g.Count() }));
            }

            if (config.GroupByYear)
            {
                AddStatisticsSubsection(body, "Распределение по годам добавления", works.Count,
                    works.GroupBy(w => w.ДатаДобавления.Year)
                        .OrderBy(g => g.Key)
                        .Select(g => new Statistic { Name = g.Key.ToString(), Count = g.Count() }));
            }

            if (config.GroupByInstitute)
            {
                AddStatisticsSubsection(body, "Распределение по институтам", works.Count,
                    works.GroupBy(w => w.Студент.Структура.Институт.Название)
                        .OrderBy(g => g.Key)
                        .Select(g => new Statistic { Name = g.Key.ToString(), Count = g.Count() }));
            }

            // Общая статистика
            AddHeading(body, "Общая статистика", 3);
            var statsTable = new Table();
            AddTableBorders(statsTable);

            var totalPages = works.Sum(w => w.КоличСтраниц);
            AddTableRow(statsTable, ["Общее количество работ", works.Count.ToString()]);
            AddTableRow(statsTable, ["Общее количество страниц", totalPages.ToString()]);
            AddTableRow(statsTable, ["Среднее количество страниц", (totalPages / works.Count).ToString()]);

            var tasks = works
                .Select(x =>
                    _workFileUploadService.VerifyUploadedFilesAsync(x.Эцп ?? "", x.Местоположение ?? ""));
            var results = await Task.WhenAll(tasks);
            int valid = results.Count(x => x.IsValid);
            AddTableRow(statsTable, ["Верные ЭЦП", valid.ToString()]);
            AddTableRow(statsTable, ["Неверные ЭЦП", (results.Length - valid).ToString()]);

            body.AppendChild(statsTable);
            AddEmptyParagraph(body);
        }

        private static void AddStatisticsSubsection(Body body, string title,
            int countWorks, IEnumerable<Statistic> distribution)
        {
            AddHeading(body, title, 3);
            var table = new Table();
            AddTableBorders(table);
            AddTableRow(table, ["Параметр", "Количество", "Процент"], true);

            foreach (var item in distribution)
            {
                AddTableRow(table, [item.Name, item.Count.ToString(), (((double)item.Count / countWorks) * 100).ToString("F2")]);
            }

            body.AppendChild(table);
            AddEmptyParagraph(body);
        }

        private async Task AddAttributeDistributionSection(Body body, List<WorkDisplayDto> works, ReportConfig config)
        {
            List<AttributeValuesDto> attributeValuesDto = await _refDataService.GetAsync<AttributeValuesDto>();

            AddHeading(body, "Анализ по атрибутам", 2);
            var table = new Table();
            AddTableBorders(table);
            AddTableRow(table, ["Название", "Количество работ", "Процент"], true);

            var idWorks = works.Select(w => w.IdРаботы).ToList();
            var workAttributes = works.Select(x => x.Атрибуты).ToList();
            foreach (var idAttr in config.SelectedAttributes)
            {
                AttributeValuesDto values = attributeValuesDto.First(x => x.IdАтрибута == idAttr);
                List<List<AttributeDto>?> allAttributeValues = workAttributes
                    .Where(x => x?.Exists(y => y.IdАтрибута == idAttr) ?? false)
                    .ToList();
                AttributeDto? attribute = allAttributeValues.FirstOrDefault()
                    ?.FirstOrDefault(x => x.IdАтрибута == idAttr);

                if (attribute != null)
                {
                    AddTableRow(table, [attribute.Название, allAttributeValues!.Count.ToString(), (((double)allAttributeValues.Count / workAttributes.Count) * 100).ToString("F2")], true);
                    foreach (string data in values.Данные)
                    {
                        int attributes = allAttributeValues.Count(x => x?.Exists(y => y.Данные == data) ?? false);
                        if (attributes != 0)
                            AddTableRow(table, [data, attributes.ToString(), (((double)attributes / allAttributeValues.Count) * 100).ToString("F2")]);
                    }
                }
                else
                {
                    string attrName = (await _refDataService.GetAsync<Атрибут>()).First(x => x.IdАтрибута == idAttr).Название;
                    AddTableRow(table, [attrName, "0", "0"]);
                }
            }
            body.AppendChild(table);
            AddEmptyParagraph(body);
        }

        private static async Task AddWorksListSection(Body body, List<WorkDisplayDto> works, ReportConfig config)
        {
            AddHeading(body, config.ListTitle, 2);

            var table = new Table();
            AddTableBorders(table);

            // Формируем заголовки на основе настроек
            List<string> headers = [];
            headers.Add("№");
            if (config.ShowTitle) headers.Add("Тема");
            if (config.ShowStudent) headers.Add("Студент");
            if (config.ShowSupervisor) headers.Add("Руководитель");
            if (config.ShowType) headers.Add("Тип");
            if (config.ShowStatus) headers.Add("Статус");
            if (config.ShowAccess) headers.Add("Доступ");
            if (config.ShowPages) headers.Add("Страницы");
            if (config.ShowDateAdd) headers.Add("Дата добавления");
            if (config.ShowDateChange) headers.Add("Дата изменения");
            if (config.ShowYearDefense) headers.Add("Год окончания");
            if (config.ShowConsultants) headers.Add("Консультанты");
            if (config.ShowReviewers) headers.Add("Рецензенты");

            List<List<AttributeDto>?> workAttributes = [];
            if (config.ShowAttributes && config.SelectedAttributes.Count != 0)
            {
                workAttributes = works.Select(x => x.Атрибуты).ToList();
                foreach (var idAttr in config.SelectedAttributes)
                {
                    AttributeDto? attribute = workAttributes
                        .FirstOrDefault(x => x?.Exists(y => y.IdАтрибута == idAttr) ?? false)
                        ?.FirstOrDefault(x => x.IdАтрибута == idAttr);
                    if (attribute != null) headers.Add(attribute.Название);
                }
            }

            AddTableRow(table, headers.ToArray(), true);

            // Заполняем данными
            int i = 0;
            foreach (var work in works)
            {
                i++;
                List<string>? rowData = [];

                rowData.Add(i.ToString());
                if (config.ShowTitle) rowData.Add(work.Тема);
                if (config.ShowStudent) rowData.Add(work.Студент.Пользователь.ФИО);
                if (config.ShowSupervisor) rowData.Add(work.Руководитель.Пользователь.ФИО);
                if (config.ShowType) rowData.Add(work.ТипРаботы);
                if (config.ShowStatus) rowData.Add(work.СтатусРаботы);
                if (config.ShowAccess) rowData.Add(work.ДоступРаботы);
                if (config.ShowPages) rowData.Add(work.КоличСтраниц.ToString());
                if (config.ShowDateAdd) rowData.Add(work.ДатаДобавления.ToString("dd.MM.yyyy"));
                if (config.ShowDateChange) rowData.Add(work.ДатаИзменения?.ToString("dd.MM.yyyy") ?? "-");
                if (config.ShowYearDefense) rowData.Add(work.Студент.ГодОкончания.ToString());

                if (config.ShowConsultants)
                {
                    rowData.Add(string.Join(", ", work.Консультанты?.Select(x => x.Пользователь.ФИО) ?? ["-"]));
                }

                if (config.ShowReviewers)
                {
                    rowData.Add(string.Join(", ", work.Рецензенты?.Select(x => x.Пользователь.ФИО) ?? ["-"]));
                }

                if (config.ShowAttributes && config.SelectedAttributes.Count != 0)
                {
                    foreach (var idAttr in config.SelectedAttributes)
                    {

                        AttributeDto? attribute = work.Атрибуты?.FirstOrDefault(x => x.IdАтрибута == idAttr);
                        rowData.Add(attribute?.Данные ?? "—");
                    }
                }

                AddTableRow(table, rowData.ToArray());
            }

            body.AppendChild(table);
            AddEmptyParagraph(body);
        }

        // Вспомогательные методы для форматирования Word
        private static void AddHeading(Body body, string text, int level)
        {
            var paragraph = new Paragraph();
            var run = new Run(new Text(text));
            var runProps = new RunProperties();
            runProps.AppendChild(new Bold());
            runProps.AppendChild(new RunFonts() { Ascii= "Times New Roman", HighAnsi = "Times New Roman", EastAsia = "Times New Roman" });
            runProps.AppendChild(new FontSize() { Val = (28 - (level - 1) * 4).ToString() });
            run.RunProperties = runProps;
            paragraph.AppendChild(run);
            paragraph.ParagraphProperties = new ParagraphProperties(
                new SpacingBetweenLines() { After = "200", Before = "100" });
            body.AppendChild(paragraph);
        }

        private static void AddParagraph(Body body, string text)
        {
            var paragraph = new Paragraph();
            var run = new Run(new Text(text));
            var runProps = new RunProperties();
            runProps.AppendChild(new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman", EastAsia = "Times New Roman" });
            runProps.AppendChild(new FontSize() { Val = "24" });
            run.RunProperties = runProps;
            paragraph.AppendChild(run);
            body.AppendChild(paragraph);
        }

        private static void AddEmptyParagraph(Body body)
        {
            body.AppendChild(new Paragraph());
        }

        private static void AddTableBorders(Table table)
        {
            var borders = new TableBorders(
                new TopBorder() { Val = BorderValues.Single, Size = 1 },
                new BottomBorder() { Val = BorderValues.Single, Size = 1 },
                new LeftBorder() { Val = BorderValues.Single, Size = 1 },
                new RightBorder() { Val = BorderValues.Single, Size = 1 },
                new InsideHorizontalBorder() { Val = BorderValues.Single, Size = 1 },
                new InsideVerticalBorder() { Val = BorderValues.Single, Size = 1 }
            );
            table.AppendChild(new TableProperties(borders, new TableWidth() { Width = "5000", Type = TableWidthUnitValues.Pct }));
        }

        private static void AddTableRow(Table table, string[] values, bool isHeader = false)
        {
            var tableRow = new TableRow();

            foreach (var value in values)
            {
                var tableCell = new TableCell();
                var paragraph = new Paragraph();
                var run = new Run(new Text(value));
                var runProps = new RunProperties();
                runProps.AppendChild(new FontSize() { Val = "20" });

                paragraph.AppendChild(run);

                if (isHeader)
                {
                    runProps.AppendChild(new Bold());
                    tableCell.AppendChild(new TableCellProperties(
                        new TableCellWidth() { Type = TableWidthUnitValues.Auto },
                        new Shading() { Val = ShadingPatternValues.Clear, Color = "auto", Fill = "D3D3D3" }));
                }
                else
                {
                    runProps.AppendChild(new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman", EastAsia = "Times New Roman" });
                }

                run.RunProperties = runProps;

                tableCell.AppendChild(paragraph);
                tableRow.AppendChild(tableCell);
            }

            table.AppendChild(tableRow);
        }

        /// <summary>
        /// Создает новую секцию с указанной ориентацией
        /// </summary>
        private static SectionProperties CreateSection(SectionPageOrientationValues orientation)
        {
            var sectionProperties = new SectionProperties();

            // Настройка размера страницы
            var pageSize = new PageSize();

            if (orientation == SectionPageOrientationValues.Landscape)
            {
                // Альбомная ориентация: ширина больше высоты
                pageSize.Width = 16838U;  // A4 landscape width (11.69 inches)
                pageSize.Height = 11906U; // A4 landscape height (8.27 inches)
                pageSize.Orient = PageOrientationValues.Landscape;
            }
            else
            {
                // Книжная ориентация
                pageSize.Width = 11906U;  // A4 portrait width (8.27 inches)
                pageSize.Height = 16838U; // A4 portrait height (11.69 inches)
                pageSize.Orient = PageOrientationValues.Portrait;
            }

            sectionProperties.AppendChild(pageSize);

            // Настройка полей
            var pageMargin = new PageMargin()
            {
                Top = 1417,   // 1 inch (in twentieths of a point)
                Bottom = 1417,
                Left = 1417,
                Right = 1417,
                Header = 708,
                Footer = 708,
                Gutter = 0
            };
            sectionProperties.AppendChild(pageMargin);

            // Настройка колонтитулов
            var titlePage = new TitlePage() { Val = true };
            sectionProperties.AppendChild(titlePage);

            return sectionProperties;
        }
    }

    //public class ReportService
    //{
    //    private readonly IDbContextFactory<ArchiveFqpContext> _dbFactory;

    //    public ReportService(IDbContextFactory<ArchiveFqpContext> dbFactory)
    //    {
    //        _dbFactory = dbFactory;
    //    }

    //    public async Task<byte[]> GenerateWorkReportAsync(ReportConfig config, List<Работа> works)
    //    {
    //        using var memoryStream = new MemoryStream();
    //        using (var wordDocument = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document))
    //        {
    //            var mainPart = wordDocument.AddMainDocumentPart();
    //            mainPart.Document = new Document();
    //            var body = mainPart.Document.AppendChild(new Body());

    //            // Добавляем заголовок
    //            AddHeading(body, config.ReportTitle, 1);
    //            AddParagraph(body, $"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
    //            AddParagraph(body, $"Период: {config.DateFrom:dd.MM.yyyy} - {config.DateTo:dd.MM.yyyy}");
    //            AddEmptyParagraph(body);

    //            // Основная статистика
    //            if (config.IncludeStatistics)
    //            {
    //                AddHeading(body, "Статистический отчет", 2);
    //                AddStatisticsTable(body, works);
    //            }

    //            // Распределение по параметрам
    //            if (config.IncludeDistribution)
    //            {
    //                await AddDistributionsAsync(body, works, config);
    //            }

    //            // Список работ
    //            if (config.IncludeWorksList)
    //            {
    //                AddHeading(body, config.ListTitle, 2);
    //                await AddWorksTableAsync(body, works, config);
    //            }

    //            body.AppendChild(new Paragraph(new Run(new Break() { Type = BreakValues.Page })));
    //        }

    //        return memoryStream.ToArray();
    //    }

    //    private void AddHeading(Body body, string text, int level)
    //    {
    //        Paragraph? paragraph = new Paragraph();
    //        var run = new Run();
    //        run.AppendChild(new Text(text));
    //        paragraph.AppendChild(run);

    //        var runProperties = new RunProperties();
    //        runProperties.AppendChild(new Bold());
    //        runProperties.AppendChild(new FontSize() { Val = (24 - (level - 1) * 2).ToString() });
    //        run.RunProperties = runProperties;

    //        paragraph.ParagraphProperties = new ParagraphProperties(
    //            new SpacingBetweenLines() { After = "200" });

    //        body.AppendChild(paragraph);
    //    }

    //    private void AddParagraph(Body body, string text)
    //    {
    //        var paragraph = new Paragraph();
    //        var run = new Run();
    //        run.AppendChild(new Text(text));
    //        paragraph.AppendChild(run);
    //        body.AppendChild(paragraph);
    //    }

    //    private void AddEmptyParagraph(Body body)
    //    {
    //        body.AppendChild(new Paragraph());
    //    }

    //    private void AddStatisticsTable(Body body, List<Работа> works)
    //    {
    //        var table = new Table();
    //        var borders = new TableBorders(
    //            new TopBorder() { Val = BorderValues.Single, Size = 1 },
    //            new BottomBorder() { Val = BorderValues.Single, Size = 1 },
    //            new LeftBorder() { Val = BorderValues.Single, Size = 1 },
    //            new RightBorder() { Val = BorderValues.Single, Size = 1 },
    //            new InsideHorizontalBorder() { Val = BorderValues.Single, Size = 1 },
    //            new InsideVerticalBorder() { Val = BorderValues.Single, Size = 1 }
    //        );
    //        table.AppendChild(new TableProperties(borders));

    //        // Статистика по типам работ
    //        var typeStats = works.GroupBy(w => w.IdТипаРаботыNavigation.Название)
    //            .Select(g => new { Type = g.Key, Count = g.Count() });

    //        AddTableRow(table, new[] { "Тип работы", "Количество" }, true);
    //        foreach (var stat in typeStats)
    //        {
    //            AddTableRow(table, new[] { stat.Type, stat.Count.ToString() });
    //        }

    //        // Статистика по статусам
    //        var statusStats = works.GroupBy(w => w.IdСтатусаРаботыNavigation.Название)
    //            .Select(g => new { Status = g.Key, Count = g.Count() });

    //        AddTableRow(table, new[] { "Статус", "Количество" }, true);
    //        foreach (var stat in statusStats)
    //        {
    //            AddTableRow(table, new[] { stat.Status, stat.Count.ToString() });
    //        }

    //        // Статистика по годам
    //        var yearStats = works.GroupBy(w => w.ДатаДобавления.Year)
    //            .OrderBy(g => g.Key)
    //            .Select(g => new { Year = g.Key, Count = g.Count() });

    //        AddTableRow(table, new[] { "Год добавления", "Количество" }, true);
    //        foreach (var stat in yearStats)
    //        {
    //            AddTableRow(table, new[] { stat.Year.ToString(), stat.Count.ToString() });
    //        }

    //        body.AppendChild(table);
    //        AddEmptyParagraph(body);
    //    }

    //    private async Task AddDistributionsAsync(Body body, List<Работа> works, ReportConfig config)
    //    {
    //        if (config.SelectedAttributes.Any())
    //        {
    //            using var context = await _dbFactory.CreateDbContextAsync();

    //            foreach (var attrId in config.SelectedAttributes)
    //            {
    //                var attribute = await context.Атрибутs.FindAsync(attrId);
    //                if (attribute != null)
    //                {
    //                    AddHeading(body, $"Распределение по атрибуту: {attribute.Название}", 3);

    //                    // Получаем данные по атрибутам для работ
    //                    var workIds = works.Select(w => w.IdРаботы).ToList();
    //                    var attrData = await context.ДанныеПоАтрибs
    //                        .Where(d => workIds.Contains(d.IdРаботы) && d.IdСтруктуры == attrId)
    //                        .GroupBy(d => d.Данные)
    //                        .Select(g => new { Value = g.Key, Count = g.Count() })
    //                        .ToListAsync();

    //                    var table = new Table();
    //                    table.AppendChild(new TableProperties(new TableBorders(
    //                        new TopBorder() { Val = BorderValues.Single, Size = 1 },
    //                        new BottomBorder() { Val = BorderValues.Single, Size = 1 },
    //                        new LeftBorder() { Val = BorderValues.Single, Size = 1 },
    //                        new RightBorder() { Val = BorderValues.Single, Size = 1 },
    //                        new InsideHorizontalBorder() { Val = BorderValues.Single, Size = 1 },
    //                        new InsideVerticalBorder() { Val = BorderValues.Single, Size = 1 }
    //                    )));

    //                    AddTableRow(table, new[] { attribute.Название, "Количество работ" }, true);
    //                    foreach (var data in attrData)
    //                    {
    //                        AddTableRow(table, new[] { data.Value, data.Count.ToString() });
    //                    }

    //                    body.AppendChild(table);
    //                    AddEmptyParagraph(body);
    //                }
    //            }
    //        }
    //    }

    //    private async Task AddWorksTableAsync(Body body, List<Работа> works, ReportConfig config)
    //    {
    //        var table = new Table();
    //        table.AppendChild(new TableProperties(new TableBorders(
    //            new TopBorder() { Val = BorderValues.Single, Size = 1 },
    //            new BottomBorder() { Val = BorderValues.Single, Size = 1 },
    //            new LeftBorder() { Val = BorderValues.Single, Size = 1 },
    //            new RightBorder() { Val = BorderValues.Single, Size = 1 },
    //            new InsideHorizontalBorder() { Val = BorderValues.Single, Size = 1 },
    //            new InsideVerticalBorder() { Val = BorderValues.Single, Size = 1 }
    //        )));

    //        // Заголовки таблицы
    //        var headers = new List<string>();
    //        if (config.ShowWorkId) headers.Add("ID");
    //        if (config.ShowTitle) headers.Add("Тема работы");
    //        if (config.ShowStudent) headers.Add("Студент");
    //        if (config.ShowSupervisor) headers.Add("Руководитель");
    //        if (config.ShowType) headers.Add("Тип работы");
    //        if (config.ShowStatus) headers.Add("Статус");
    //        if (config.ShowPages) headers.Add("Страниц");
    //        if (config.ShowDate) headers.Add("Дата добавления");
    //        if (config.ShowAttributes && config.SelectedAttributes.Any())
    //        {
    //            using var context = await _dbFactory.CreateDbContextAsync();
    //            foreach (var attrId in config.SelectedAttributes)
    //            {
    //                var attr = await context.Атрибутs.FindAsync(attrId);
    //                if (attr != null) headers.Add(attr.Название);
    //            }
    //        }

    //        AddTableRow(table, headers.ToArray(), true);

    //        // Данные
    //        using var contextData = await _dbFactory.CreateDbContextAsync();

    //        foreach (var work in works)
    //        {
    //            var rowData = new List<string>();
    //            if (config.ShowWorkId) rowData.Add(work.IdРаботы.ToString());
    //            if (config.ShowTitle) rowData.Add(work.Тема);
    //            if (config.ShowStudent)
    //            {
    //                var student = work.IdСтудентаNavigation.IdПользователяNavigation;
    //                rowData.Add($"{student.Фамилия} {student.Имя[0]}.{student.Отчество?[0]}.");
    //            }
    //            if (config.ShowSupervisor)
    //            {
    //                var supervisor = work.IdПреподавателяNavigation.IdПользователяNavigation;
    //                rowData.Add($"{supervisor.Фамилия} {supervisor.Имя[0]}.{supervisor.Отчество?[0]}.");
    //            }
    //            if (config.ShowType) rowData.Add(work.IdТипаРаботыNavigation.Название);
    //            if (config.ShowStatus) rowData.Add(work.IdСтатусаРаботыNavigation.Название);
    //            if (config.ShowPages) rowData.Add(work.КоличСтраниц?.ToString() ?? "Н/Д");
    //            if (config.ShowDate) rowData.Add(work.ДатаДобавления.ToString("dd.MM.yyyy"));

    //            if (config.ShowAttributes && config.SelectedAttributes.Any())
    //            {
    //                foreach (var attrId in config.SelectedAttributes)
    //                {
    //                    var attrValue = await contextData.ДанныеПоАтрибs
    //                        .FirstOrDefaultAsync(d => d.IdРаботы == work.IdРаботы && d.IdСтруктуры == attrId);
    //                    rowData.Add(attrValue?.Данные ?? "—");
    //                }
    //            }

    //            AddTableRow(table, rowData.ToArray());
    //        }

    //        body.AppendChild(table);
    //    }

    //    private void AddTableRow(Table table, string[] values, bool isHeader = false)
    //    {
    //        var tableRow = new TableRow();

    //        foreach (var value in values)
    //        {
    //            var tableCell = new TableCell();
    //            var paragraph = new Paragraph();
    //            var run = new Run();
    //            run.AppendChild(new Text(value));
    //            paragraph.AppendChild(run);

    //            if (isHeader)
    //            {
    //                var runProps = new RunProperties();
    //                runProps.AppendChild(new Bold());
    //                run.RunProperties = runProps;
    //                tableCell.AppendChild(new TableCellProperties(new TableCellWidth() { Type = TableWidthUnitValues.Auto }));
    //            }

    //            tableCell.AppendChild(paragraph);
    //            tableRow.AppendChild(tableCell);
    //        }

    //        table.AppendChild(tableRow);
    //    }
    //}
}
