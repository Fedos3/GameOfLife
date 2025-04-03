using GameOfLife.Models;
using GameOfLife.Services;
using System.Drawing.Drawing2D;
using System.Linq;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace GameOfLife.UI
{
    public class MainForm : Form
    {
        private TabControl _tabControl;
        private ToolStripButton _addTabButton;
        private ToolStripButton _closeTabButton;

        public MainForm()
        {
            SetupComponents();
            InitializeToolStrip();
            AddNewTab();
        }

        private void SetupComponents()
        {
            Text = "Game of Life";
            Size = new Size(1200, 1000);
            StartPosition = FormStartPosition.CenterScreen;

            _tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            Controls.Add(_tabControl);
        }

        private void InitializeToolStrip()
        {
            var toolStrip = new ToolStrip();

            _addTabButton = new ToolStripButton
            {
                Text = "Новая вкладка",
                Image = null // Здесь можно добавить иконку
            };
            _addTabButton.Click += AddTabButton_Click;

            _closeTabButton = new ToolStripButton
            {
                Text = "Закрыть вкладку",
                Image = null // Здесь можно добавить иконку
            };
            _closeTabButton.Click += CloseTabButton_Click;

            toolStrip.Items.Add(_addTabButton);
            toolStrip.Items.Add(_closeTabButton);

            Controls.Add(toolStrip);
        }

        private void AddTabButton_Click(object sender, EventArgs e)
        {
            AddNewTab();
        }

        private void CloseTabButton_Click(object sender, EventArgs e)
        {
            if (_tabControl.SelectedTab != null)
            {
                var gameForm = (GameFieldForm)_tabControl.SelectedTab.Controls[0];
                gameForm.Dispose();
                _tabControl.TabPages.Remove(_tabControl.SelectedTab);
            }
        }

        private void AddNewTab()
        {
            var gameForm = new GameFieldForm();
            var tabPage = new TabPage($"Игра {_tabControl.TabCount + 1}")
            {
                Padding = new Padding(0)
            };

            gameForm.TopLevel = false;
            gameForm.FormBorderStyle = FormBorderStyle.None;
            gameForm.Dock = DockStyle.Fill;
            gameForm.Visible = true;

            tabPage.Controls.Add(gameForm);
            _tabControl.TabPages.Add(tabPage);
            _tabControl.SelectedTab = tabPage;
            
            // Даём форме фокус, чтобы убедиться, что все элементы отрисованы
            gameForm.Focus();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            foreach (TabPage tabPage in _tabControl.TabPages)
            {
                if (tabPage.Controls.Count > 0)
                {
                    var gameForm = (GameFieldForm)tabPage.Controls[0];
                    gameForm.Dispose();
                }
            }
        }
    }
} 