using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GameOfLife.Services
{
    /// <summary>
    /// Класс для разбора и применения правил игры "Жизнь"
    /// </summary>
    public class RuleParser
    {
        private readonly HashSet<int> _birthRules = new HashSet<int>();
        private readonly HashSet<int> _survivalRules = new HashSet<int>();

        /// <summary>
        /// Инициализирует новый экземпляр парсера правил
        /// </summary>
        /// <param name="rules">Строка правил в формате "B3/S23"</param>
        public RuleParser(string rules = "B3/S23")
        {
            ParseRules(rules);
        }

        /// <summary>
        /// Разбирает строку правил на условия рождения и выживания
        /// </summary>
        private void ParseRules(string rules)
        {
            // Очищаем предыдущие правила
            _birthRules.Clear();
            _survivalRules.Clear();

            // Валидация формата
            if (string.IsNullOrWhiteSpace(rules))
            {
                // Устанавливаем стандартные правила по умолчанию
                _birthRules.Add(3);
                _survivalRules.Add(2);
                _survivalRules.Add(3);
                return;
            }

            // Используем регулярное выражение для проверки формата и извлечения чисел
            var rulePattern = new Regex(@"^B([0-8]*)\/S([0-8]*)$", RegexOptions.IgnoreCase);
            var match = rulePattern.Match(rules);

            if (!match.Success)
            {
                // Если формат некорректный, используем правила по умолчанию
                _birthRules.Add(3);
                _survivalRules.Add(2);
                _survivalRules.Add(3);
                return;
            }

            // Извлекаем правила рождения
            var birthPart = match.Groups[1].Value;
            foreach (char c in birthPart)
            {
                if (int.TryParse(c.ToString(), out int number) && number >= 0 && number <= 8)
                {
                    _birthRules.Add(number);
                }
            }

            // Извлекаем правила выживания
            var survivalPart = match.Groups[2].Value;
            foreach (char c in survivalPart)
            {
                if (int.TryParse(c.ToString(), out int number) && number >= 0 && number <= 8)
                {
                    _survivalRules.Add(number);
                }
            }

            // Если правила пустые, используем стандартные
            if (_birthRules.Count == 0 && _survivalRules.Count == 0)
            {
                _birthRules.Add(3);
                _survivalRules.Add(2);
                _survivalRules.Add(3);
            }
        }

        /// <summary>
        /// Проверяет, должна ли мертвая клетка ожить по правилам рождения
        /// </summary>
        /// <param name="neighbors">Количество живых соседей</param>
        /// <returns>true, если клетка должна ожить; иначе false</returns>
        public bool BirthRule(int neighbors)
        {
            return _birthRules.Contains(neighbors);
        }

        /// <summary>
        /// Проверяет, должна ли живая клетка выжить по правилам выживания
        /// </summary>
        /// <param name="neighbors">Количество живых соседей</param>
        /// <returns>true, если клетка должна выжить; иначе false</returns>
        public bool SurvivalRule(int neighbors)
        {
            return _survivalRules.Contains(neighbors);
        }

        /// <summary>
        /// Возвращает строковое представление правил
        /// </summary>
        public override string ToString()
        {
            var birth = string.Join("", _birthRules.OrderBy(n => n));
            var survival = string.Join("", _survivalRules.OrderBy(n => n));
            return $"B{birth}/S{survival}";
        }
    }
} 