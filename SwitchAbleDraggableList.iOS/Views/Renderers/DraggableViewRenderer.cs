using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using System.ComponentModel;
using CoreGraphics;
using SwitchAbleDraggableList.Views.Gestures;
using Xamarin.Essentials;
using System;
using SwitchAbleDraggableList.Utils;

using SwitchAbleDraggableList.Views.Renderers;
using SwitchAbleDraggableList.Views;

[assembly: ExportRenderer(typeof(DraggableView), typeof(DraggableViewRenderer))]
namespace SwitchAbleDraggableList.Views.Renderers
{

    public class DraggableViewRenderer : VisualElementRenderer<View>, UIKit.IUIGestureRecognizerDelegate
    {

        #region Constants

        private const int LONG_PRESS_MOVE_PIXELS_LIMIT = 3;
        private const int LONG_PRESS_WAITING_TIME = 300;

        #endregion        

        private CGPoint TouchBeganPoint { get; set; }

        private nfloat OriginalZPosition { get; set; }

        private UIImpactFeedbackGenerator ImpactFeedbackGenerator { get; set; }

        private DebounceableAction StartBouncer { get; set; }

        private bool LongPress { get; set; }

        private bool HasBeenDragged { get; set; }

        private UIPanGestureRecognizer PanGesture { get; set; }

        private CGPoint LastLocation { get; set; }

        private CGPoint OriginalPosition { get; set; }

        private UIGestureRecognizer.Token PanGestureToken { get; set; }

        private bool PanGestureRecognizerStartedRecognizing { get; set; }
               

        void DetectPan()
        {
            var dragView = Element as DraggableView;
            if (this.LongPress || dragView.DragMode == DragMode.Touch)
            {
                this.PanGestureRecognizerStartedRecognizing = true;
                if (this.PanGesture.State == UIGestureRecognizerState.Began)
                {

                    if (false == this.HasBeenDragged)
                    {

                        this.HasBeenDragged = true;
                    }
                }

                CGPoint translation = this.PanGesture.TranslationInView(Superview);
                var currentCenterX = Center.X;
                var currentCenterY = Center.Y;
                if (dragView.DragDirection == DragDirectionType.All || dragView.DragDirection == DragDirectionType.Horizontal)
                {
                    currentCenterX = this.LastLocation.X + translation.X;
                }

                if (dragView.DragDirection == DragDirectionType.All || dragView.DragDirection == DragDirectionType.Vertical)
                {
                    currentCenterY = this.LastLocation.Y + translation.Y;
                }

                Center = new CGPoint(currentCenterX, currentCenterY);
                (Element as DraggableView).OnDrag(translation.X * (Element.Width / dragView.Width), translation.Y * (Element.Height / dragView.Height));
                if ((this.PanGesture.State == UIGestureRecognizerState.Cancelled) || (this.PanGesture.State == UIGestureRecognizerState.Ended))
                {
                    this.EndDrag();
                }
            }
        }

        [Export("gestureRecognizer:shouldRecognizeSimultaneouslyWithGestureRecognizer:")]
        public bool ShouldRecognizeSimultaneously(UIGestureRecognizer gestureRecognizer, UIGestureRecognizer otherGestureRecognizer)
        {
            return (false == this.LongPress);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                RemoveGestureRecognizer(this.PanGesture);
                this.PanGesture.RemoveTarget(PanGestureToken);
            }
            if (e.NewElement != null)
            {
                var dragView = Element as DraggableView;
                this.PanGesture = new UIPanGestureRecognizer();
                this.PanGestureToken = this.PanGesture.AddTarget(DetectPan);
                this.PanGesture.Delegate = (UIKit.IUIGestureRecognizerDelegate)this;
                AddGestureRecognizer(this.PanGesture);

                dragView.RestorePositionCommand = new Command(() =>
                {
                    if (this.HasBeenDragged)
                    {
                        this.Center = this.OriginalPosition;
                        this.HasBeenDragged = false;
                    }
                });
            }
        }

        private void StartDrag()
        {
            if ((Element as DraggableView).DragDirection == DragDirectionType.None)
            {
                return;
            }

            Xamarin.Forms.Device.BeginInvokeOnMainThread(() => // to make sure this happens on the UI thread
            {
                try
                {
                    this.NativeView.Layer.ZPosition = float.MaxValue;
                }
                catch (Exception) { }

                (Element as DraggableView)?.DragStarted();
                if (this.ImpactFeedbackGenerator != null)
                {
                    this.ImpactFeedbackGenerator.ImpactOccurred();
                }
            });
            Device.StartTimer(new TimeSpan(10), () => // to make sure this happens on the UI thread
            {
                this.LastLocation = this.Center;
                this.OriginalPosition = this.Center;
                this.LongPress = true;
                return false;
            });
        }

        private void EndDrag()
        {            
            this.StartBouncer?.Cancel();
            this.StartBouncer = null;

            if (false == this.LongPress)
            {
                return;
            }

            Xamarin.Forms.Device.BeginInvokeOnMainThread(() => // to make sure this happens on the UI thread
            {
                (Element as DraggableView)?.DragEnded();
                try
                {
                    this.NativeView.Layer.ZPosition = this.OriginalZPosition;
                }
                catch (Exception) { }
                this.LongPress = false;
            });
        }

        #region native Touch Managment

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);            
            this.OriginalZPosition = this.NativeView.Layer.ZPosition;
            this.PanGestureRecognizerStartedRecognizing = false;
            if (DeviceInfo.Version.Major >= 10)
            {
                this.ImpactFeedbackGenerator = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Heavy);
                this.ImpactFeedbackGenerator.Prepare();
            }
            this.StartBouncer = new DebounceableAction(LONG_PRESS_WAITING_TIME, () =>
            {                
                this.StartDrag();
            });

            this.LastLocation = this.Center;
            this.OriginalPosition = this.Center;
            this.TouchBeganPoint = new CGPoint((touches.AnyObject as UITouch).LocationInView(this));
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {            
            var locationInView = (touches.AnyObject as UITouch).LocationInView(this);
            if ((Math.Abs(this.TouchBeganPoint.X - locationInView.X) > LONG_PRESS_MOVE_PIXELS_LIMIT) ||
                (Math.Abs(this.TouchBeganPoint.Y - locationInView.Y) > LONG_PRESS_MOVE_PIXELS_LIMIT))
            {
                this.StartBouncer?.Cancel();
                this.StartBouncer = null;
            }
            base.TouchesMoved(touches, evt);
        }

        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {            
            base.TouchesCancelled(touches, evt);
            this.StartBouncer?.Cancel();
            this.StartBouncer = null;
            if (false == this.PanGestureRecognizerStartedRecognizing)
            {
                this.EndDrag();

            }
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);
            this.EndDrag();
        }
        #endregion
    }
}