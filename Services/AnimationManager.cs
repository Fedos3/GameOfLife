using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GameOfLife.Services
{
    public class AnimationManager : IDisposable
    {
        private readonly Dictionary<Point, float> _animations;
        private readonly System.Windows.Forms.Timer _timer;
        private const float ANIMATION_SPEED = 0.1f;
        private const float MAX_ALPHA = 1.0f;
        private bool _disposed = false;

        public event EventHandler AnimationUpdated;

        public AnimationManager()
        {
            _animations = new Dictionary<Point, float>();
            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 16; // ~60 FPS
            _timer.Tick += Timer_Tick;
        }

        public void StartAnimation(Point point)
        {
            if (!_animations.ContainsKey(point))
            {
                _animations[point] = 0.0f;
                if (!_timer.Enabled)
                {
                    _timer.Start();
                }
            }
        }

        public float GetAnimationAlpha(Point point)
        {
            return _animations.TryGetValue(point, out float alpha) ? alpha : MAX_ALPHA;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var pointsToRemove = new List<Point>();

            foreach (var kvp in _animations)
            {
                var point = kvp.Key;
                var alpha = kvp.Value;

                alpha = Math.Min(alpha + ANIMATION_SPEED, MAX_ALPHA);
                _animations[point] = alpha;

                if (alpha >= MAX_ALPHA)
                {
                    pointsToRemove.Add(point);
                }
            }

            foreach (var point in pointsToRemove)
            {
                _animations.Remove(point);
            }

            if (_animations.Count == 0)
            {
                _timer.Stop();
            }

            AnimationUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _timer.Stop();
                    _timer.Tick -= Timer_Tick;
                    _timer.Dispose();
                    _animations.Clear();
                }
                _disposed = true;
            }
        }

        ~AnimationManager()
        {
            Dispose(false);
        }
    }
} 