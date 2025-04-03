using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;

namespace GameOfLife.UI
{
    public class RuleEditorForm : Form
    {
        private readonly List<CheckBox> _birthRuleCheckboxes = new List<CheckBox>();
        private readonly List<CheckBox> _survivalRuleCheckboxes = new List<CheckBox>();
        private Button _applyButton;
        private Button _cancelButton;
        
        // Стандартные шаблоны правил
        private Dictionary<string, string> _rulePresets = new Dictionary<string, string>()
        {
            { "Conway's Life (B3/S23)", "B3/S23" },
            { "HighLife (B36/S23)", "B36/S23" },
            { "Day & Night (B3678/S34678)", "B3678/S34678" },
            { "Assimilation (B345/S4567)", "B345/S4567" },
            { "Mazectric (B3/S1234)", "B3/S1234" },
            { "Maze (B3/S12345)", "B3/S12345" },
            { "Seeds (B2/S)", "B2/S" },
            { "2x2 (B36/S125)", "B36/S125" },
            { "Replicator (B1357/S1357)", "B1357/S1357" }
        };
        
        // Приватное поле вместо свойства
        private string _ruleString;
        
        // Метод для получения текущих правил
        public string GetRuleString()
        {
            return _ruleString;
        }
        
        public RuleEditorForm(string currentRules)
        {
            _ruleString = currentRules;
            InitializeComponent();
            ParseRulesForCheckboxes(currentRules);
        }
        
        private void InitializeComponent()
        {
            // Настройка формы
            this.Text = "Редактор правил";
            this.Size = new Size(450, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = ThemeManager.Colors.Background;
            this.ForeColor = ThemeManager.Colors.Text;
            this.FormBorderStyle = FormBorderStyle.None;
            
            // Создаем панель содержимого
            Panel contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.BackColor = ThemeManager.Colors.Background;
            contentPanel.Padding = new Padding(20);
            this.Controls.Add(contentPanel);
            
            // Создаем заголовок
            Label titleLabel = new Label();
            titleLabel.Text = "Редактор правил игры 'Жизнь'";
            titleLabel.Font = ThemeManager.Fonts.Title;
            titleLabel.ForeColor = ThemeManager.Colors.Text;
            titleLabel.Size = new Size(400, 30);
            titleLabel.Location = new Point(20, 20);
            contentPanel.Controls.Add(titleLabel);
            
            // Создаем информационную метку
            Label infoLabel = new Label();
            infoLabel.Text = "Текущие правила: " + _ruleString;
            infoLabel.Font = ThemeManager.Fonts.Normal;
            infoLabel.ForeColor = ThemeManager.Colors.Info;
            infoLabel.Size = new Size(400, 20);
            infoLabel.Location = new Point(20, 55);
            contentPanel.Controls.Add(infoLabel);
            
            // Создаем группы для правил рождения
            GroupBox birthRulesGroup = new GroupBox();
            birthRulesGroup.Text = "Рождение клетки (сколько соседей нужно мертвой клетке)";
            birthRulesGroup.Size = new Size(400, 100);
            birthRulesGroup.Location = new Point(20, 80);
            birthRulesGroup.ForeColor = ThemeManager.Colors.Text;
            birthRulesGroup.Font = ThemeManager.Fonts.Normal;
            birthRulesGroup.BackColor = ThemeManager.Colors.PanelBackground;
            contentPanel.Controls.Add(birthRulesGroup);
            
            // Создаем чекбоксы для правил рождения
            for (int i = 0; i <= 8; i++)
            {
                CheckBox checkBox = new CheckBox();
                checkBox.Text = i.ToString();
                checkBox.Font = ThemeManager.Fonts.Normal;
                checkBox.ForeColor = ThemeManager.Colors.Text;
                checkBox.Size = new Size(40, 20);
                checkBox.Location = new Point(20 + i * 40, 30);
                checkBox.Tag = i;
                checkBox.BackColor = ThemeManager.Colors.PanelBackground;
                
                checkBox.CheckedChanged += (s, e) => {
                    UpdateRuleString();
                };
                
                _birthRuleCheckboxes.Add(checkBox);
                birthRulesGroup.Controls.Add(checkBox);
            }
            
            // Создаем группы для правил выживания
            GroupBox survivalRulesGroup = new GroupBox();
            survivalRulesGroup.Text = "Выживание клетки (сколько соседей нужно живой клетке)";
            survivalRulesGroup.Size = new Size(400, 100);
            survivalRulesGroup.Location = new Point(20, 190);
            survivalRulesGroup.ForeColor = ThemeManager.Colors.Text;
            survivalRulesGroup.Font = ThemeManager.Fonts.Normal;
            survivalRulesGroup.BackColor = ThemeManager.Colors.PanelBackground;
            contentPanel.Controls.Add(survivalRulesGroup);
            
            // Создаем чекбоксы для правил выживания
            for (int i = 0; i <= 8; i++)
            {
                CheckBox checkBox = new CheckBox();
                checkBox.Text = i.ToString();
                checkBox.Font = ThemeManager.Fonts.Normal;
                checkBox.ForeColor = ThemeManager.Colors.Text;
                checkBox.Size = new Size(40, 20);
                checkBox.Location = new Point(20 + i * 40, 30);
                checkBox.Tag = i;
                checkBox.BackColor = ThemeManager.Colors.PanelBackground;
                
                checkBox.CheckedChanged += (s, e) => {
                    UpdateRuleString();
                };
                
                _survivalRuleCheckboxes.Add(checkBox);
                survivalRulesGroup.Controls.Add(checkBox);
            }
            
            // Создаем группу для предустановленных правил
            GroupBox presetsGroup = new GroupBox();
            presetsGroup.Text = "Шаблоны правил";
            presetsGroup.Size = new Size(400, 60);
            presetsGroup.Location = new Point(20, 300);
            presetsGroup.ForeColor = ThemeManager.Colors.Text;
            presetsGroup.Font = ThemeManager.Fonts.Normal;
            presetsGroup.BackColor = ThemeManager.Colors.PanelBackground;
            contentPanel.Controls.Add(presetsGroup);
            
            // Создаем выпадающий список шаблонов
            ComboBox presetsComboBox = new ComboBox();
            presetsComboBox.Size = new Size(360, 30);
            presetsComboBox.Location = new Point(20, 25);
            presetsComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            presetsComboBox.Font = ThemeManager.Fonts.Normal;
            presetsComboBox.BackColor = ThemeManager.Colors.ControlBackground;
            presetsComboBox.ForeColor = ThemeManager.Colors.Text;
            
            // Добавляем элементы в выпадающий список
            presetsComboBox.Items.Add("Выберите шаблон");
            foreach (var preset in _rulePresets)
            {
                presetsComboBox.Items.Add(preset.Key);
            }
            presetsComboBox.SelectedIndex = 0;
            
            // Обработчик события выбора шаблона
            presetsComboBox.SelectedIndexChanged += (s, e) => {
                if (presetsComboBox.SelectedIndex > 0)
                {
                    string selectedPreset = presetsComboBox.SelectedItem.ToString();
                    if (_rulePresets.TryGetValue(selectedPreset, out string ruleString))
                    {
                        ParseRulesForCheckboxes(ruleString);
                        UpdateRuleString();
                    }
                }
            };
            
            presetsGroup.Controls.Add(presetsComboBox);
            
            // Создаем кнопки применить и отменить
            _applyButton = new Button();
            _applyButton.Text = "Применить";
            _applyButton.Size = new Size(180, 40);
            _applyButton.Location = new Point(20, 370);
            _applyButton.BackColor = ThemeManager.Colors.Success;
            _applyButton.ForeColor = ThemeManager.Colors.Text;
            _applyButton.FlatStyle = FlatStyle.Flat;
            _applyButton.FlatAppearance.BorderSize = 0;
            _applyButton.Font = ThemeManager.Fonts.Subtitle;
            _applyButton.Click += (s, e) => {
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            contentPanel.Controls.Add(_applyButton);
            
            _cancelButton = new Button();
            _cancelButton.Text = "Отменить";
            _cancelButton.Size = new Size(180, 40);
            _cancelButton.Location = new Point(210, 370);
            _cancelButton.BackColor = ThemeManager.Colors.Error;
            _cancelButton.ForeColor = ThemeManager.Colors.Text;
            _cancelButton.FlatStyle = FlatStyle.Flat;
            _cancelButton.FlatAppearance.BorderSize = 0;
            _cancelButton.Font = ThemeManager.Fonts.Subtitle;
            _cancelButton.Click += (s, e) => {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };
            contentPanel.Controls.Add(_cancelButton);
            
            // Добавляем обработчик для перемещения формы
            Label dragLabel = new Label();
            dragLabel.BackColor = Color.Transparent;
            dragLabel.Size = new Size(this.Width, 20);
            dragLabel.Location = new Point(0, 0);
            dragLabel.MouseDown += (s, e) => {
                if (e.Button == MouseButtons.Left)
                {
                    const int WM_NCLBUTTONDOWN = 0xA1;
                    const int HT_CAPTION = 0x2;
                    
                    var handle = this.Handle;
                    var message = WM_NCLBUTTONDOWN;
                    var wParam = (IntPtr)HT_CAPTION;
                    var lParam = IntPtr.Zero;
                    
                    // Отправляем сообщение для перемещения окна
                    Native.SendMessage(handle, message, wParam, lParam);
                }
            };
            this.Controls.Add(dragLabel);
            
            // Добавляем кнопку закрытия
            Button closeButton = new Button();
            closeButton.Text = "×";
            closeButton.Size = new Size(30, 30);
            closeButton.Location = new Point(this.Width - 30, 0);
            closeButton.FlatStyle = FlatStyle.Flat;
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.BackColor = ThemeManager.Colors.Error;
            closeButton.ForeColor = ThemeManager.Colors.Text;
            closeButton.Font = new Font("Arial", 14, FontStyle.Bold);
            closeButton.Click += (s, e) => {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };
            this.Controls.Add(closeButton);
        }
        
        // Метод для разбора правил и установки чекбоксов
        private void ParseRulesForCheckboxes(string ruleString)
        {
            // Очищаем все чекбоксы
            foreach (var checkbox in _birthRuleCheckboxes)
            {
                checkbox.Checked = false;
            }
            
            foreach (var checkbox in _survivalRuleCheckboxes)
            {
                checkbox.Checked = false;
            }
            
            // Разбираем строку правил
            try
            {
                string[] parts = ruleString.Split('/');
                
                // Разбираем правила рождения (B)
                if (parts[0].StartsWith("B", StringComparison.OrdinalIgnoreCase))
                {
                    string birthPart = parts[0].Substring(1);
                    foreach (char c in birthPart)
                    {
                        if (char.IsDigit(c) && int.TryParse(c.ToString(), out int digit) && digit >= 0 && digit <= 8)
                        {
                            _birthRuleCheckboxes[digit].Checked = true;
                        }
                    }
                }
                
                // Разбираем правила выживания (S)
                if (parts[1].StartsWith("S", StringComparison.OrdinalIgnoreCase))
                {
                    string survivalPart = parts[1].Substring(1);
                    foreach (char c in survivalPart)
                    {
                        if (char.IsDigit(c) && int.TryParse(c.ToString(), out int digit) && digit >= 0 && digit <= 8)
                        {
                            _survivalRuleCheckboxes[digit].Checked = true;
                        }
                    }
                }
            }
            catch
            {
                // В случае ошибки устанавливаем стандартные правила Conway's Life (B3/S23)
                _birthRuleCheckboxes[3].Checked = true;
                _survivalRuleCheckboxes[2].Checked = true;
                _survivalRuleCheckboxes[3].Checked = true;
                
                _ruleString = "B3/S23";
            }
        }
        
        // Метод для обновления строки правил на основе чекбоксов
        private void UpdateRuleString()
        {
            string birthDigits = string.Join("", _birthRuleCheckboxes.Where(cb => cb.Checked).Select(cb => cb.Tag.ToString()));
            string survivalDigits = string.Join("", _survivalRuleCheckboxes.Where(cb => cb.Checked).Select(cb => cb.Tag.ToString()));
            _ruleString = $"B{birthDigits}/S{survivalDigits}";

            // Обновляем информационную метку
            var infoLabel = this.Controls.OfType<Panel>().FirstOrDefault()?.Controls.OfType<Label>().FirstOrDefault(lbl => lbl.Location == new Point(20, 55));
            if (infoLabel != null)
            {
                infoLabel.Text = "Текущие правила: " + _ruleString;
            }
        }
        
        // Переопределяем OnPaint для рисования рамки формы
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            // Рисуем рамку вокруг формы
            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (Pen pen = new Pen(ThemeManager.Colors.BorderActive, 1))
            {
                e.Graphics.DrawRectangle(pen, rect);
            }
        }
    }
    
    // Вспомогательный класс для отправки Windows-сообщений
    internal static class Native
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
    }
} 