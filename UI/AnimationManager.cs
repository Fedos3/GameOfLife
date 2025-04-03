using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GameOfLife.UI
{
    public class AnimationManager
    {
        private readonly Dictionary<Point, float> _cellAnimations = new Dictionary<Point, float>();
        private readonly System.Windows.Forms.Timer _animationTimer;
        private const float ANIMATION_SPEED = 0.1f;
        private const float MAX_ALPHA = 1.0f;

        public event EventHandler AnimationUpdated;

        public AnimationManager()
        {
            _animationTimer = new System.Windows.Forms.Timer
            {
                Interval = 16 // ~60 FPS
            };
            _animationTimer.Tick += AnimationTimer_Tick;
        }

        public void StartAnimation(Point point)
        {
            if (!_cellAnimations.ContainsKey(point))
            {
                _cellAnimations[point] = 0.0f;
                _animationTimer.Start();
            }
        }

        public float GetAnimationAlpha(Point point)
        {
            return _cellAnimations.TryGetValue(point, out float alpha) ? alpha : MAX_ALPHA;
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            bool hasAnimations = false;
            var keys = _cellAnimations.Keys.ToList();

            foreach (var point in keys)
            {
                _cellAnimations[point] += ANIMATION_SPEED;
                if (_cellAnimations[point] >= MAX_ALPHA)
                {
                    _cellAnimations.Remove(point);
                }
                else
                {
                    hasAnimations = true;
                }
            }

            if (!hasAnimations)
            {
                _animationTimer.Stop();
            }

            AnimationUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void Stop()
        {
            _animationTimer.Stop();
            _cellAnimations.Clear();
        }
    }
} 