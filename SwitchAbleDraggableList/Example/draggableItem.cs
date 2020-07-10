using System;
using System.Globalization;
using SwitchAbleDraggableList.Views;
using SwitchAbleDraggableList.Views.Gestures;
using Xamarin.Forms;

namespace SwitchAbleDraggableList.Example {
    public interface DraggeableViewModel {
        string Text { get; set; }
    }
    public class DraggableItem : DraggableView, INeighboringCellDragging

    {
        public DraggeableViewModel ViewModel { get; set; }
        public DraggableItem (DraggeableViewModel ViewModel) {
            this.ViewModel = ViewModel;
            this.DragDirection = DragDirectionType.Vertical;
            this.DragMode = DragMode.LongPress;
            var newConent = new StackLayout {
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                BackgroundColor = Color.Gray,
                Padding = new Thickness (10),
                HeightRequest = 100,
                WidthRequest = Device.Info.PixelScreenSize.Width,
                
                Children = {
                    new Label {
                        
                        VerticalTextAlignment=TextAlignment.Center ,
                        Text = ViewModel.Text,
                        HorizontalTextAlignment = TextAlignment.Center,
                        FontSize = 44,
                    }
                }

            };
            this.Content = newConent;
            var tapGestureRecognizer = new TapGestureRecognizer ();
            tapGestureRecognizer.Tapped += (s, e) => { };
            newConent.GestureRecognizers.Add (tapGestureRecognizer);
        }

        public void NeighbouringCellDragEnded () {
            (this.Content as StackLayout).BackgroundColor = Color.Gray;

        }

        public void NeighbouringCellDragStarted () {
            (this.Content as StackLayout).BackgroundColor = Color.DarkGray;

        }
        public override void DragStarted () {
            base.DragStarted ();
            this.RotateTo (2.5, 50);
        }

        public override void DragEnded () {
            base.DragEnded ();
            this.RotateTo (0, 50);
        }
    }

}