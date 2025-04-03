using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace GameOfLife.UI
{
    public partial class RuleEditorForm : Form
    {
        // Стандартные шаблоны правил
        private readonly Dictionary<string, string> _ruleTemplates = new Dictionary<string, string>
        {
            { "Conway's Life", "B3/S23" },
            { "Day & Night", "B3678/S34678" },
            { "HighLife", "B36/S23" },
            { "Seeds", "B2/S" },
            { "Stains", "B3678/S235678" },
            { "Replicator", "B1357/S1357" },
            { "Maze", "B3/S12345" },
            { "2x2", "B36/S125" }
        };
        
        private string _currentRules;
        
        public string GetCurrentRules()
        {
            return _currentRules;
        }
        
        public event EventHandler<string> RulesChanged;
        
        private readonly CheckBox[] _birthCheckboxes = new CheckBox[9];
        private readonly CheckBox[] _survivalCheckboxes = new CheckBox[9];
        private Label _infoLabel;
        
        public RuleEditorForm(string initialRules = "B3/S23")
        {
            _currentRules = initialRules;
            
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(450, 400);
            this.BackColor = ThemeManager.Colors.PanelBackground;
            this.ForeColor = ThemeManager.Colors.Text;
            
            Panel contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.Padding = new Padding(20);
            this.Controls.Add(contentPanel);
            
            Label titleLabel = new Label();
            titleLabel.Text = "Редактор правил";
            titleLabel.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            titleLabel.ForeColor = ThemeManager.Colors.Text;
            titleLabel.AutoSize = true;
            titleLabel.Location = new Point(20, 20);
            contentPanel.Controls.Add(titleLabel);
            
            _infoLabel = new Label();
            _infoLabel.Text = "Текущие правила: " + _currentRules;
            _infoLabel.Font = new Font("Segoe UI", 10);
            _infoLabel.ForeColor = ThemeManager.Colors.Info;
            _infoLabel.AutoSize = true;
            _infoLabel.Location = new Point(20, 60);
            contentPanel.Controls.Add(_infoLabel);
            
            GroupBox birthGroup = new GroupBox();
            birthGroup.Text = "Рождение (B)";
            birthGroup.Font = new Font("Segoe UI", 10);
            birthGroup.ForeColor = ThemeManager.Colors.Text;
            birthGroup.Size = new Size(190, 80);
            birthGroup.Location = new Point(20, 100);
            contentPanel.Controls.Add(birthGroup);
            
            for (int i = 0; i < 9; i++)
            {
                _birthCheckboxes[i] = new CheckBox();
                _birthCheckboxes[i].Text = i.ToString();
                _birthCheckboxes[i].AutoSize = true;
                _birthCheckboxes[i].Location = new Point(15 + i * 20, 30);
                _birthCheckboxes[i].ForeColor = ThemeManager.Colors.Text;
                _birthCheckboxes[i].CheckedChanged += (s, e) => UpdateRulesFromCheckboxes();
                birthGroup.Controls.Add(_birthCheckboxes[i]);
            }
            
            GroupBox survivalGroup = new GroupBox();
            survivalGroup.Text = "Выживание (S)";
            survivalGroup.Font = new Font("Segoe UI", 10);
            survivalGroup.ForeColor = ThemeManager.Colors.Text;
            survivalGroup.Size = new Size(190, 80);
            survivalGroup.Location = new Point(230, 100);
            contentPanel.Controls.Add(survivalGroup);
            
            for (int i = 0; i < 9; i++)
            {
                _survivalCheckboxes[i] = new CheckBox();
                _survivalCheckboxes[i].Text = i.ToString();
                _survivalCheckboxes[i].AutoSize = true;
                _survivalCheckboxes[i].Location = new Point(15 + i * 20, 30);
                _survivalCheckboxes[i].ForeColor = ThemeManager.Colors.Text;
                _survivalCheckboxes[i].CheckedChanged += (s, e) => UpdateRulesFromCheckboxes();
                survivalGroup.Controls.Add(_survivalCheckboxes[i]);
            }
            
            GroupBox templatesGroup = new GroupBox();
            templatesGroup.Text = "Шаблоны правил";
            templatesGroup.Font = new Font("Segoe UI", 10);
            templatesGroup.ForeColor = ThemeManager.Colors.Text;
            templatesGroup.Size = new Size(400, 80);
            templatesGroup.Location = new Point(20, 200);
            contentPanel.Controls.Add(templatesGroup);
            
            ComboBox templatesComboBox = new ComboBox();
            templatesComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            templatesComboBox.Location = new Point(20, 30);
            templatesComboBox.Size = new Size(360, 25);
            templatesComboBox.BackColor = ThemeManager.Colors.ControlBackground;
            templatesComboBox.ForeColor = ThemeManager.Colors.Text;
            templatesGroup.Controls.Add(templatesComboBox);
            
            foreach (var template in _ruleTemplates)
            {
                templatesComboBox.Items.Add($"{template.Key} ({template.Value})");
            }
            
            templatesComboBox.SelectedIndexChanged += (s, e) =>
            {
                if (templatesComboBox.SelectedIndex >= 0)
                {
                    string key = _ruleTemplates.Keys.ElementAt(templatesComboBox.SelectedIndex);
                    ParseRules(_ruleTemplates[key]);
                    UpdateRulesFromCheckboxes();
                }
            };
            
            Button applyButton = new Button();
            applyButton.Text = "Применить";
            applyButton.Size = new Size(120, 35);
            applyButton.Location = new Point(190, 300);
            applyButton.BackColor = ThemeManager.Colors.Success;
            applyButton.ForeColor = Color.White;
            applyButton.FlatStyle = FlatStyle.Flat;
            applyButton.FlatAppearance.BorderSize = 0;
            applyButton.Click += (s, e) =>
            {
                RulesChanged?.Invoke(this, _currentRules);
                this.Close();
            };
            contentPanel.Controls.Add(applyButton);
            
            Button cancelButton = new Button();
            cancelButton.Text = "Отмена";
            cancelButton.Size = new Size(120, 35);
            cancelButton.Location = new Point(320, 300);
            cancelButton.BackColor = ThemeManager.Colors.ControlBackground;
            cancelButton.ForeColor = ThemeManager.Colors.Text;
            cancelButton.FlatStyle = FlatStyle.Flat;
            cancelButton.FlatAppearance.BorderSize = 1;
            cancelButton.Click += (s, e) => this.Close();
            contentPanel.Controls.Add(cancelButton);
            
            contentPanel.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    NativeMethods.ReleaseCapture();
                    NativeMethods.SendMessage(this.Handle, NativeMethods.WM_NCLBUTTONDOWN, NativeMethods.HT_CAPTION, 0);
                }
            };
            
            Button closeButton = new Button();
            closeButton.Text = "×";
            closeButton.Size = new Size(30, 30);
            closeButton.Location = new Point(this.Width - 40, 10);
            closeButton.FlatStyle = FlatStyle.Flat;
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.BackColor = Color.Transparent;
            closeButton.ForeColor = ThemeManager.Colors.Text;
            closeButton.Font = new Font("Arial", 12, FontStyle.Bold);
            closeButton.Click += (s, e) => this.Close();
            this.Controls.Add(closeButton);
            
            ParseRules(_currentRules);
        }
        
        private void ParseRules(string rules)
        {
            for (int i = 0; i < 9; i++)
            {
                _birthCheckboxes[i].Checked = false;
                _survivalCheckboxes[i].Checked = false;
            }
            
            try
            {
                Regex regex = new Regex(@"B([0-8]*)\/S([0-8]*)");
                Match match = regex.Match(rules);
                
                if (match.Success)
                {
                    string birthPart = match.Groups[1].Value;
                    foreach (char c in birthPart)
                    {
                        int num = int.Parse(c.ToString());
                        if (num >= 0 && num <= 8)
                        {
                            _birthCheckboxes[num].Checked = true;
                        }
                    }
                    
                    string survivalPart = match.Groups[2].Value;
                    foreach (char c in survivalPart)
                    {
                        int num = int.Parse(c.ToString());
                        if (num >= 0 && num <= 8)
                        {
                            _survivalCheckboxes[num].Checked = true;
                        }
                    }
                }
                else
                {
                    ParseRules("B3/S23");
                }
            }
            catch
            {
                ParseRules("B3/S23");
            }
        }
        
        private void UpdateRulesFromCheckboxes()
        {
            string birthPart = "B";
            string survivalPart = "S";
            
            for (int i = 0; i < 9; i++)
            {
                if (_birthCheckboxes[i].Checked) birthPart += i;
                if (_survivalCheckboxes[i].Checked) survivalPart += i;
            }
            
            _currentRules = birthPart + "/" + survivalPart;
            _infoLabel.Text = "Текущие правила: " + _currentRules;
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            using (Pen p = new Pen(ThemeManager.Colors.BorderActive))
            {
                e.Graphics.DrawRectangle(p, 0, 0, this.Width - 1, this.Height - 1);
            }
        }
    }
    
    internal static class NativeMethods
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
    }
} 