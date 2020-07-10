using System;
using System.Timers;

namespace SwitchAbleDraggableList.Utils
{
    public class DebounceableAction
    {
        private Timer timer;
        private Action BounceAction { get; set; }
        private int IntervalMiliSeconds { get; set; }

        public DebounceableAction(int miliSeconds, Action action)
        {
            this.BounceAction = action;
            this.IntervalMiliSeconds = miliSeconds;
            this.Bounce();
        }

        public void Bounce()
        {
            // kill pending timer and pending ticks
            this.timer?.Stop();
            this.timer = null;

            // timer is recreated for each event and effectively
            // resets the timeout. Action only fires after timeout has fully
            // elapsed without other events firing in between
            timer = new Timer(this.IntervalMiliSeconds);
            timer.AutoReset = false;
            timer.Elapsed += (sender, e) =>
            {
                if (timer == null)
                    return;

                timer?.Stop();
                timer = null;
                this.BounceAction?.Invoke();
                this.BounceAction = null;
            };

            timer.Start();
        }

        public void Cancel()
        {
            this.timer?.Stop();
            this.BounceAction = null;
            this.timer = null;
        }
    }
}