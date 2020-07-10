using System;
using System.Collections.Generic;
using SwitchAbleDraggableList.Views;
using Xamarin.Forms;

namespace SwitchAbleDraggableList.Example
{
    public class ExamplePage : ContentPage
    {
        private SwitchableDraggableViewList SwitchableDraggableViewList { get; set; } = new SwitchableDraggableViewList();

        private DraggeableItemListModel DraggeableItemListModel { get; set; } = new DraggeableItemListModel();

        public ExamplePage()
        {

            this.BackgroundColor = Color.White;


            var ScrollContentLayout = new StackLayout();
            var ScrollView = new ScrollView
            {
                Content = ScrollContentLayout
            };

            var MainLayout = new RelativeLayout
            {
                Margin = new Thickness(0, 100, 0, 0),
            };
            MainLayout.Children.Add( ((View)ScrollView),
                                         Constraint.Constant(0),
                                         Constraint.Constant(0),
                                         Constraint.RelativeToParent((parent) => { return parent.Width; }),
                                         Constraint.RelativeToParent((parent) => { return parent.Height; }));
            this.Content = MainLayout;

            ScrollContentLayout.Children.Add(SwitchableDraggableViewList);
            this.SwitchableDraggableViewList.CellsSwitchedListner = this.DraggeableItemListModel;
            this.DraggeableItemListModel.onDataUpdated = this.UpdateData;
            this.UpdateData();
        }


        private void UpdateData()
        {
            this.SwitchableDraggableViewList.ReplaceData(
               new List<Object>(DraggeableItemListModel.ItemList),
               (Object viewModel) => new DraggableItem(viewModel as DraggeableViewModel),
               (Object viewModel, DraggableView view) => ((view as DraggableItem).ViewModel == viewModel)
            );
        }
    }
 }