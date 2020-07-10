using AView = Android.Views;
using Android.Runtime;
using Android.Views;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Content;
using System;
using SwitchAbleDraggableList.Views.Gestures;
using SwitchAbleDraggableList.Utils;
using SwitchAbleDraggableList.Droid.Views.Renderers;
using SwitchAbleDraggableList.Views;

[assembly: ExportRenderer(typeof(DraggableView), typeof(DraggableViewRenderer))]
namespace SwitchAbleDraggableList.Droid.Views.Renderers
{
    public class DraggableViewRenderer : VisualElementRenderer<Xamarin.Forms.View>
    {
        #region Constants

        private const int LONG_PRESS_WAITING_TIME = 300;

        #endregion

        public DraggableViewRenderer(Context context) : base(context)
        {
        }

        #region Properties

        private DebounceableAction StartBouncer { get; set; }

        private bool HasBeenDragged { get; set; }

        private float originalX { get; set; }

        private float originalY { get; set; }

        private float dX { get; set; }

        private float dY { get; set; }

        private bool TouchedDown { get; set; }

        #endregion

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.View> e)
        {
            base.OnElementChanged(e);
            if (e.NewElement != null)
            {
                var dragView = Element as DraggableView;
                dragView.RestorePositionCommand = new Command(() =>
                {
                    if (HasBeenDragged)
                    {
                        this.SetX(originalX);
                        this.SetY(originalY);
                        this.HasBeenDragged = false;
                    }
                });
            }
        }

        private void StartDrag(MotionEvent e)
        {
            //Console.WriteLine("StartDrag e" + e.RawX + "  " + e.RawY);
            if ((Element as DraggableView).DragDirection == DragDirectionType.None)
            {
                return;
            }
            if (this.TouchedDown)
            {
                return;
            }

            Device.StartTimer(new TimeSpan(10), () => // to make sure this happens on the UI thread
            {
                this.BringToFront();
                var dragView = Element as DraggableView;
                if (false == HasBeenDragged)
                {
                    this.originalX = this.GetX();
                    this.originalY = this.GetY();
                    this.HasBeenDragged = true;
                }
                dragView.DragStarted();
                this.PerformHapticFeedback(FeedbackConstants.LongPress, HapticFeedbackConstants.FlagIgnoreGlobalSetting);
                float x = e.RawX;
                float y = e.RawY;
                this.dX = x - this.GetX();
                this.dY = y - this.GetY();
                this.TouchedDown = true;
                return false;
            }
            );
        }

        protected override void OnVisibilityChanged(AView.View changedView, [GeneratedEnum] ViewStates visibility)
        {
            base.OnVisibilityChanged(changedView, visibility);
            if (visibility == ViewStates.Visible)
            {
            }
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            Console.WriteLine("OnTouchEvent e" + e.ActionMasked);
            float x = e.RawX;
            float y = e.RawY;
            var dragView = Element as DraggableView;
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    {
                        if (dragView.DragMode == DragMode.Touch)
                        {
                            if (false == this.TouchedDown)
                            {
                                if (false == HasBeenDragged)
                                {
                                    this.originalX = GetX();
                                    this.originalY = GetY();
                                    this.HasBeenDragged = true;
                                }
                                dragView.DragStarted();
                            }

                            this.TouchedDown = true;
                        }
                        this.dX = x - this.GetX();
                        this.dY = y - this.GetY();
                        break;
                    }
                case MotionEventActions.Move:
                    if (this.TouchedDown)
                    {
                        //Console.WriteLine($"OnTouchEvent move x: {x}, y: {y}, dX: {dX}, dY: {dY}, originalX: {originalX}, originalY: {originalY}, height: {Height} ");
                        if (dragView.DragDirection == DragDirectionType.All || dragView.DragDirection == DragDirectionType.Horizontal)
                        {
                            this.SetX(x - dX);
                        }

                        if (dragView.DragDirection == DragDirectionType.All || dragView.DragDirection == DragDirectionType.Vertical)
                        {
                            this.SetY(y - dY);
                        }
                        //Console.WriteLine($"OnTouchEvent ratiox : {(x - dX) / Width},y-dy: {y-dY},  ratioY: {(y - dY) / Height}"); 
                        (Element as DraggableView).OnDrag((x - dX - originalX) * (Element.Width / Width), (y - dY - originalY) * (Element.Height / Height));
                    }
                    break;
                case MotionEventActions.Up:
                    {
                        //Console.WriteLine("OnTouchEvent.MotionEventActions.Up");
                        this.EndDrag();
                        break;
                    }
                case MotionEventActions.Cancel:
                    {
                        //Console.WriteLine("OnTouchEvent.MotionEventActions.Cancel");
                        this.EndDrag();
                        break;
                    }
            }
            return base.OnTouchEvent(e);
        }

        private void EndDrag()
        {
            Console.WriteLine("EndDrag");
            this.StartBouncer?.Cancel();
            this.StartBouncer = null;

            Device.StartTimer(new TimeSpan(10), () => // to make sure this happens on the UI thread
            {
                if (false == this.TouchedDown)
                {
                    return false;
                }
                this.TouchedDown = false;
                this.RequestDisallowInterceptTouchEvent(this.TouchedDown);
                (Element as DraggableView).DragEnded();
                return false;
            });
        }

        public override bool OnInterceptTouchEvent(MotionEvent e)
        {
            base.OnInterceptTouchEvent(e);
            switch (e.ActionMasked)
            {
                case MotionEventActions.Down:
                    {
                        this.StartBouncer = new DebounceableAction(LONG_PRESS_WAITING_TIME, () => this.StartDrag(e));
                        break;
                    }
                case MotionEventActions.Move:
                    {
                        if (TouchedDown)
                        {
                            this.RequestDisallowInterceptTouchEvent(this.TouchedDown);
                        }
                        break;
                    }
                case MotionEventActions.Cancel:
                case MotionEventActions.Up:
                    {
                        //Console.WriteLine("OnInterceptTouchEvent.MotionEventActions.Cancel");
                        this.EndDrag();
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            //Console.WriteLine("OnInterceptTouchEvent e" + e.ActionMasked.ToString() +  " touchedDown: " + TouchedDown);
            return TouchedDown;
        }
    }
}
