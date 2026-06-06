using DocumentFormat.OpenXml.Bibliography;

namespace ArchiveFqp.Services.FuzzySearch
{
    public static class StringSimilarity
    {
        public static string Normalize(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";

            return input.ToLower()
                .Replace("ё", "е")
                .Replace("  ", " ")
                .Trim();
        }

        /// <summary>
        /// Расстояние Левенштейна между двумя строками.
        /// 0 = строки идентичны, чем больше — тем сильнее различаются.
        /// </summary>
        public static int LevenshteinDistance(string a, string b)
        {
            if (string.IsNullOrEmpty(a)) return b?.Length ?? 0;
            if (string.IsNullOrEmpty(b)) return a.Length;

            var d = new int[a.Length + 1, b.Length + 1];

            for (int i = 0; i <= a.Length; i++) d[i, 0] = i;
            for (int j = 0; j <= b.Length; j++) d[0, j] = j;

            for (int i = 1; i <= a.Length; i++)
            {
                for (int j = 1; j <= b.Length; j++)
                {
                    int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost
                    );
                }
            }

            return d[a.Length, b.Length];
        }

        /// <summary>
        /// Нормализованная схожесть от 0.0 до 1.0.
        /// 1.0 = идентичны, 0.0 = полностью разные.
        /// </summary>
        public static double Similarity(string a, string b)
        {
            int maxLen = Math.Max(a.Length, b.Length);
            if (maxLen == 0) return 1.0;
            return 1.0 - (double)LevenshteinDistance(a, b) / maxLen;
        }

        /// <summary>
        /// Модификация Левенштейна, которая считает перестановку соседних 
        /// символов (частые опечатки) за 1 операцию, а не за 2.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int DamerauLevenshtein(string a, string b)
        {
            int lenA = a.Length, lenB = b.Length;
            var d = new int[lenA + 1, lenB + 1];

            for (int i = 0; i <= lenA; i++) d[i, 0] = i;
            for (int j = 0; j <= lenB; j++) d[0, j] = j;

            for (int i = 1; i <= lenA; i++)
            {
                for (int j = 1; j <= lenB; j++)
                {
                    int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost
                    );

                    // Перестановка соседних символов
                    if (i > 1 && j > 1 && a[i - 1] == b[j - 2] && a[i - 2] == b[j - 1])
                    {
                        d[i, j] = Math.Min(d[i, j], d[i - 2, j - 2] + cost);
                    }
                }
            }

            return d[lenA, lenB];
        }

        public static double JaccardSimilarity(string a, string b)
        {
            var setA = a.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
            var setB = b.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();

            int intersection = setA.Intersect(setB).Count();
            int union = setA.Union(setB).Count();

            return union == 0 ? 0 : (double)intersection / union;
        }

        /// <summary>
        /// Находит ближайшее совпадение из списка объектов, имеющих поле "Название"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="list"></param>
        /// <param name="threshold">Порог схожести. 1.0 - идентично, 0.0 - разные</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static (T? Best, double Score) FindBest<T>(string source, List<T> list, double threshold) where T : class
        {
            var fieldInfo = typeof(T).GetProperty("Название") 
                ?? throw new Exception("Поле \"Название\" не найдено");

            string normalized = Normalize(source);
            T? bestItem = null;
            double bestScore = 0;

            foreach (var item in list)
            {
                string normalizedName = Normalize((string)fieldInfo.GetValue(item)!);

                // Комбинированная метрика: Левенштейн + Jaccard
                double levenshteinScore = StringSimilarity.Similarity(normalized, normalizedName);
                double jaccardScore = JaccardSimilarity(normalized, normalizedName);
                double combined = (levenshteinScore + jaccardScore) / 2.0;

                if (combined > bestScore)
                {
                    bestScore = combined;
                    bestItem = item;
                }
            }

            if (bestScore < threshold) return (null, bestScore);

            return (bestItem, bestScore);
        }

        public static (string? Best, double Score) FindBest(string source, List<string> list, double threshold)
        {
            string normalized = Normalize(source);
            string? bestItem = null;
            double bestScore = 0;

            foreach (var item in list)
            {
                string normalizedName = Normalize(item);

                // Комбинированная метрика: Левенштейн + Jaccard
                double levenshteinScore = StringSimilarity.Similarity(normalized, normalizedName);
                double jaccardScore = JaccardSimilarity(normalized, normalizedName);
                double combined = (levenshteinScore + jaccardScore) / 2.0;

                if (combined > bestScore)
                {
                    bestScore = combined;
                    bestItem = item;
                }
            }

            if (bestScore < threshold) return (null, bestScore);

            return (bestItem, bestScore);
        }
    }
}
