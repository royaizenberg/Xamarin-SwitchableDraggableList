using System;
using System.Windows.Input;
using SwitchAbleDraggableList.Views.Gestures;
using Xamarin.Forms;

namespace SwitchAbleDraggableList.Views
{
    public class DraggableView : ContentView
    {
        public class OnDragEventArgs : EventArgs
        {        
            public double XDelta { get; set; }
            public double YDelta { get; set; }
        }

        public event EventHandler DragStartEvent;
        public event EventHandler<OnDragEventArgs> OnDragEvent;
        public event EventHandler DragEndEvent;

        public static readonly BindableProperty DragDirectionProperty = BindableProperty.Create(
            propertyName: "DragDirection",
            returnType: typeof(DragDirectionType),
            declaringType: typeof(DraggableView),
            defaultValue: DragDirectionType.All,
            defaultBindingMode: BindingMode.TwoWay);

        public DragDirectionType DragDirection
        {
            get { return (DragDirectionType)GetValue(DragDirectionProperty); }
            set { SetValue(DragDirectionProperty, value); }
        }

        public static readonly BindableProperty DragModeProperty = BindableProperty.Create(
           propertyName: "DragMode",
           returnType: typeof(DragMode),
           declaringType: typeof(DraggableView),
           defaultValue: DragMode.LongPress,
           defaultBindingMode: BindingMode.TwoWay);

        public DragMode DragMode
        {
            get { return (DragMode)GetValue(DragModeProperty); }
            set { SetValue(DragModeProperty, value); }
        }

        public static readonly BindableProperty IsDraggingProperty = BindableProperty.Create(
          propertyName: "IsDragging",
          returnType: typeof(bool),
          declaringType: typeof(DraggableView),
          defaultValue: false,
          defaultBindingMode: BindingMode.TwoWay);

        public bool IsDragging
        {
            get { return (bool)GetValue(IsDraggingProperty); }
            set { SetValue(IsDraggingProperty, value); }
        }

        public static readonly BindableProperty RestorePositionCommandProperty = BindableProperty.Create(nameof(RestorePositionCommand), typeof(ICommand), typeof(DraggableView), default(ICommand), BindingMode.TwoWay, null, OnRestorePositionCommandPropertyChanged);

        static void OnRestorePositionCommandPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var source = bindable as DraggableView;
            if (source == null)
            {
                return;
            }
            source.OnRestorePositionCommandChanged();

        }

        private void OnRestorePositionCommandChanged()
        {
            OnPropertyChanged("RestorePositionCommand");
        }

        public ICommand RestorePositionCommand
        {
            get
            {
                return (ICommand)GetValue(RestorePositionCommandProperty);
            }
            set
            {
                SetValue(RestorePositionCommandProperty, value);
            }
        }

        //public IDraggableViewDragListner DragThresHoldListener { get; set; }

        public virtual void OnDrag(double xDelta,double yDelta)
        {
            this.OnDragEvent?.Invoke(this, new OnDragEventArgs { XDelta = xDelta, YDelta = yDelta });

        }

        public virtual void DragStarted()
        {
            this.DragStartEvent?.Invoke(this, default(EventArgs));
            IsDragging = true;   
        }

        public virtual void DragEnded()
        {
            IsDragging = false;
            this.DragEndEvent?.Invoke(this, default(EventArgs));

        }
    }
}