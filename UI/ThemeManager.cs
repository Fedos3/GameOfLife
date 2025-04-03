using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GameOfLife.UI
{
    public static class ThemeManager
    {
        public static class Colors
        {
            // Основные цвета приложения
            public static Color Alive = Color.FromArgb(76, 175, 80);
            public static Color Dead = Color.FromArgb(33, 33, 33);
            public static Color Grid = Color.FromArgb(60, 60, 60);
            public static Color Reverse = Color.FromArgb(244, 67, 54);
            public static Color Minimap = Color.FromArgb(45, 45, 45);
            public static Color Background = Color.FromArgb(18, 18, 18);
            public static Color PanelBackground = Color.FromArgb(30, 30, 30);
            public static Color Text = Color.FromArgb(230, 230, 230);
            
            // Дополнительные цвета
            public static Color ButtonHover = Color.FromArgb(100, 200, 100);
            public static Color ButtonPressed = Color.FromArgb(60, 130, 60);
            public static Color BorderActive = Color.FromArgb(100, 200, 100);
            public static Color BorderInactive = Color.FromArgb(80, 80, 80);
            public static Color Warning = Color.FromArgb(255, 193, 7);
            public static Color Error = Color.FromArgb(244, 67, 54);
            public static Color Success = Color.FromArgb(76, 175, 80);
            public static Color Info = Color.FromArgb(33, 150, 243);
            
            // Цвета элементов управления
            public static Color ControlBackground = Color.FromArgb(50, 50, 50);
            public static Color ControlBorder = Color.FromArgb(100, 100, 100);
            public static Color ControlForeground = Color.FromArgb(230, 230, 230);
        }
        
        public static class Sizes
        {
            public static int CellSize = 20;
            public static int MinimapCellSize = 2;
            public static int ButtonHeight = 30;
            public static int ButtonWidth = 120;
            public static int LabelHeight = 20;
            public static int PanelPadding = 10;
            public static int GroupPadding = 5;
            public static int BorderSize = 1;
        }
        
        public static class Fonts
        {
            public static Font Default = new Font("Segoe UI", 9F);
            public static Font Title = new Font("Segoe UI", 12F, FontStyle.Bold);
            public static Font Button = new Font("Segoe UI", 9F);
            public static Font Label = new Font("Segoe UI", 9F);
            public static Font Small = new Font("Segoe UI", 8F);
        }
        
        // Вспомогательные методы для стилизации
        public static void ApplyButtonStyle(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = Colors.ControlBorder;
            button.FlatAppearance.MouseOverBackColor = Colors.ButtonHover;
            button.FlatAppearance.MouseDownBackColor = Colors.ButtonPressed;
            button.BackColor = Colors.ControlBackground;
            button.ForeColor = Colors.Text;
            button.Font = Fonts.Button;
            button.Height = Sizes.ButtonHeight;
            button.UseVisualStyleBackColor = false;
        }
        
        // Генерация градиента для элементов UI
        public static LinearGradientBrush CreateGradientBrush(Rectangle rect, Color color1, Color color2)
        {
            return new LinearGradientBrush(
                rect,
                color1,
                color2,
                LinearGradientMode.Vertical);
        }
    }
} 