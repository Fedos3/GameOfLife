using System.Drawing;
using System.Drawing.Drawing2D;

namespace GameOfLife.UI
{
    public static class ThemeManager
    {
        public static class Colors
        {
            // Основные цвета приложения 
            public static Color Alive = Color.FromArgb(76, 175, 80);        // Зеленый
            public static Color Dead = Color.FromArgb(33, 33, 33);          // Черный
            public static Color Grid = Color.FromArgb(60, 60, 60);          // Темно-серый
            public static Color Reverse = Color.FromArgb(244, 67, 54);      // Красный
            public static Color Minimap = Color.FromArgb(45, 45, 45);       // Темно-серый
            public static Color Background = Color.FromArgb(18, 18, 18);    // Очень темный
            public static Color PanelBackground = Color.FromArgb(30, 30, 30); // Темно-серый
            public static Color Text = Color.FromArgb(230, 230, 230);       // Светло-серый
            
            // Дополнительные цвета
            public static Color ButtonHover = Color.FromArgb(100, 200, 100);  // Светло-зеленый
            public static Color ButtonPressed = Color.FromArgb(60, 130, 60);  // Темно-зеленый
            public static Color BorderActive = Color.FromArgb(100, 200, 100); // Зеленая рамка
            public static Color BorderInactive = Color.FromArgb(80, 80, 80);  // Серая рамка
            public static Color Warning = Color.FromArgb(255, 193, 7);      // Желтый
            public static Color Error = Color.FromArgb(244, 67, 54);        // Красный
            public static Color Success = Color.FromArgb(76, 175, 80);      // Зеленый
            public static Color Info = Color.FromArgb(33, 150, 243);        // Синий
            
            // Цвета элементов управления
            public static Color ControlBackground = Color.FromArgb(50, 50, 50); // Темно-серый
            public static Color ControlBorder = Color.FromArgb(100, 100, 100);  // Серый
            public static Color ControlForeground = Color.FromArgb(230, 230, 230); // Светло-серый
        }

        public static class Fonts
        {
            public static Font Title = new Font("Segoe UI", 16, FontStyle.Bold);
            public static Font Subtitle = new Font("Segoe UI", 12, FontStyle.Bold);
            public static Font Normal = new Font("Segoe UI", 10);
            public static Font Small = new Font("Segoe UI", 8);
            public static Font ToolButton = new Font("Segoe UI", 12);
            public static Font Monospace = new Font("Consolas", 10);
        }

        public static class Sizes
        {
            public const int CellSize = 20;
            public const int GridPadding = 20;
            public const int MinimapHeight = 150;
            public const int ToolPanelWidth = 40;
            public const int ControlPanelWidth = 250;
            public const int ButtonHeight = 35;
            public const int ButtonWidth = 180;
            public const int ToolButtonSize = 30;
            public const int Padding = 10;
        }
        
        // Вспомогательные методы для стилизации
        public static void ApplyRoundedCorners(Control control, int radius = 5)
        {
            using (var path = new GraphicsPath())
            {
                path.AddArc(0, 0, radius, radius, 180, 90);
                path.AddArc(control.Width - radius, 0, radius, radius, 270, 90);
                path.AddArc(control.Width - radius, control.Height - radius, radius, radius, 0, 90);
                path.AddArc(0, control.Height - radius, radius, radius, 90, 90);
                path.CloseAllFigures();
                
                control.Region = new Region(path);
            }
        }
        
        // Генерация градиента для элементов UI
        public static LinearGradientBrush CreateGradientBrush(Rectangle rect, Color startColor, Color endColor, LinearGradientMode mode = LinearGradientMode.Vertical)
        {
            return new LinearGradientBrush(rect, startColor, endColor, mode);
        }
    }
} 