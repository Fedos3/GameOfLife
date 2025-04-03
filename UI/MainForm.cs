using GameOfLife.Models;
using GameOfLife.Services;
using System.Drawing.Drawing2D;
using System.Linq;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace GameOfLife.UI
{
    public partial class MainForm : Form
    {
        private TabControl _tabControl;
        private Button _addTabButton;

        public MainForm()
        {
            InitializeComponent();
            this.BackColor = ThemeManager.Colors.Background;
            this.ForeColor = ThemeManager.Colors.Text;
            this.Text = "Игра 'Жизнь'";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(1280, 720);
            
            InitializeToolStrip();
            InitializeTabs();
            AddNewTab();
        }

        private void InitializeComponent()
        {
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1200, 800);
        }

        private void InitializeToolStrip()
        {
            ToolStrip toolStrip = new ToolStrip();
            toolStrip.BackColor = ThemeManager.Colors.PanelBackground;
            toolStrip.ForeColor = ThemeManager.Colors.Text;
            toolStrip.Renderer = new CustomToolStripRenderer();
            
            ToolStripButton newTabButton = new ToolStripButton();
            newTabButton.Text = "Новая вкладка";
            newTabButton.Click += (sender, e) => AddNewTab();
            
            ToolStripButton closeTabButton = new ToolStripButton();
            closeTabButton.Text = "Закрыть вкладку";
            closeTabButton.Click += (sender, e) => CloseCurrentTab();
            
            toolStrip.Items.Add(newTabButton);
            toolStrip.Items.Add(closeTabButton);
            
            this.Controls.Add(toolStrip);
        }

        private void InitializeTabs()
        {
            _tabControl = new TabControl();
            _tabControl.Dock = DockStyle.Fill;
            _tabControl.BackColor = ThemeManager.Colors.PanelBackground;
            _tabControl.ForeColor = ThemeManager.Colors.Text;
            _tabControl.Padding = new Point(10, 3);
            
            _addTabButton = new Button();
            _addTabButton.Text = "+";
            _addTabButton.Size = new Size(25, 25);
            _addTabButton.Click += (sender, e) => AddNewTab();
            
            this.Controls.Add(_tabControl);
            this.Controls.Add(_addTabButton);
            
            _tabControl.Selected += (sender, e) => UpdateAddButtonPosition();
            _tabControl.ControlAdded += (sender, e) => UpdateAddButtonPosition();
            _tabControl.ControlRemoved += (sender, e) => UpdateAddButtonPosition();
            _tabControl.SizeChanged += (sender, e) => UpdateAddButtonPosition();
            
            UpdateAddButtonPosition();
        }

        private void UpdateAddButtonPosition()
        {
            if (_tabControl.TabCount > 0)
            {
                Rectangle lastTabRect = _tabControl.GetTabRect(_tabControl.TabCount - 1);
                _addTabButton.Location = new Point(
                    lastTabRect.X + lastTabRect.Width + 5,
                    lastTabRect.Y + (lastTabRect.Height - _addTabButton.Height) / 2
                );
            }
            else
            {
                _addTabButton.Location = new Point(10, 10);
            }
        }

        private void AddNewTab()
        {
            TabPage tabPage = new TabPage();
            tabPage.Text = $"Симуляция {_tabControl.TabCount + 1}";
            tabPage.BackColor = ThemeManager.Colors.Background;
            tabPage.ForeColor = ThemeManager.Colors.Text;
            tabPage.Padding = new Padding(0);
            
            GameFieldForm gameForm = new GameFieldForm();
            gameForm.TopLevel = false;
            gameForm.FormBorderStyle = FormBorderStyle.None;
            gameForm.Dock = DockStyle.Fill;
            gameForm.Visible = true;
            
            tabPage.Controls.Add(gameForm);
            _tabControl.TabPages.Add(tabPage);
            _tabControl.SelectedTab = tabPage;
            
            UpdateAddButtonPosition();
            gameForm.Focus();
        }

        private void CloseCurrentTab()
        {
            if (_tabControl.TabCount > 0)
            {
                TabPage currentTab = _tabControl.SelectedTab;
                _tabControl.TabPages.Remove(currentTab);
                currentTab.Dispose();
            }
            
            if (_tabControl.TabCount == 0)
            {
                AddNewTab();
            }
            
            UpdateAddButtonPosition();
        }
    }
    
    class CustomToolStripRenderer : ToolStripProfessionalRenderer
    {
        public CustomToolStripRenderer() : base(new CustomColorTable())
        {
        }
        
        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            // Не рисуем границу
        }
    }
    
    class CustomColorTable : ProfessionalColorTable
    {
        public override Color ToolStripGradientBegin => ThemeManager.Colors.PanelBackground;
        public override Color ToolStripGradientMiddle => ThemeManager.Colors.PanelBackground;
        public override Color ToolStripGradientEnd => ThemeManager.Colors.PanelBackground;
        public override Color MenuStripGradientBegin => ThemeManager.Colors.PanelBackground;
        public override Color MenuStripGradientEnd => ThemeManager.Colors.PanelBackground;
        public override Color MenuItemSelected => ThemeManager.Colors.ButtonHover;
        public override Color MenuItemBorder => ThemeManager.Colors.ControlBorder;
        public override Color ButtonSelectedHighlight => ThemeManager.Colors.ButtonHover;
        public override Color ButtonSelectedGradientBegin => ThemeManager.Colors.ButtonHover;
        public override Color ButtonSelectedGradientEnd => ThemeManager.Colors.ButtonHover;
    }
} 