using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using static SwitchAbleDraggableList.Views.DraggableView;

namespace SwitchAbleDraggableList.Views {
    public interface INeighboringCellDragging {
        void NeighbouringCellDragStarted ();
        void NeighbouringCellDragEnded ();
    }
    public interface ICellsSwitchedListner {
        Task CellsSwitched (List<int> newOrder);
    }

    public class SwitchableDraggableViewList : StackLayout {
        public ICellsSwitchedListner CellsSwitchedListner { get; set; }

        public List<DraggableView> OriginalViewList { get; set; } = new List<DraggableView> ();

        private List<DraggableView> SwitchedViewList { get; set; } = new List<DraggableView> ();

        #region Add and remove view

        public void AddView (DraggableView view) {
            this.OriginalViewList.Add (view);
            this.SwitchedViewList.Add (view);
            this.Children.Add (view);
            view.DragStartEvent += this.DragStarted;
            view.OnDragEvent += this.OnDrag;
            view.DragEndEvent += this.DragEnded;
        }

        public void RemoveAt (int index) {
            if (index < 0 || index >= this.OriginalViewList.Count) {
                return;
            }
            var view = OriginalViewList[index];
            this.Remove (view);
            this.Children.Remove (view);
        }

        public void Move (DraggableView view, int toIndex) {
            this.OriginalViewList.Remove (view);
            this.SwitchedViewList.Remove (view);
            this.Children.Remove (view);
            this.OriginalViewList.Insert (toIndex, view);
            this.SwitchedViewList.Insert (toIndex, view);
            this.Children.Insert (toIndex, view);
        }

        public void Remove (DraggableView view) {
            view.OnDragEvent -= this.OnDrag;
            view.DragEndEvent -= this.DragEnded;
            this.OriginalViewList.Remove (view);
            this.SwitchedViewList.Remove (view);
            this.Children.Remove (view);
        }

        public void Insert (int index, DraggableView view) {
            this.OriginalViewList.Insert (index, view);
            this.SwitchedViewList.Insert (index, view);
            this.Children.Insert (index, view);
            view.DragStartEvent += this.DragStarted;
            view.OnDragEvent += this.OnDrag;
            view.DragEndEvent += this.DragEnded;
        }

        #endregion

        public void MoveCellsToTheirOriginalCoordinates () {
            foreach (var view in OriginalViewList) {
                view.TranslateTo (0, 0);
                view.RestorePositionCommand.Execute (null);
            }
        }

        public void Clear () {
            foreach (var view in OriginalViewList) {
                view.DragStartEvent -= this.DragStarted;
                view.OnDragEvent -= this.OnDrag;
                view.DragEndEvent -= this.DragEnded;

                view.TranslateTo (0, 0);
                view.RestorePositionCommand.Execute (null);

            }
            this.OriginalViewList.Clear ();
            this.SwitchedViewList.Clear ();
            this.Children.Clear ();
        }

        #region drag events

        private void DragEnded (object sender, EventArgs e) {
            if (CellsSwitchedListner != null) {
                var newListOrder = new List<int> ();
                for (int i = 0; i < SwitchedViewList.Count; i++) {
                    var toindex = OriginalViewList.FindIndex (v => v == SwitchedViewList[i]);
                    newListOrder.Add (toindex);
                }
                CellsSwitchedListner.CellsSwitched (newListOrder);
            }

        }

        private void DragStarted (object sender, EventArgs e) {
            foreach (var neighbouringCell in OriginalViewList) {
                if (neighbouringCell is INeighboringCellDragging neighbour) {
                    neighbour.NeighbouringCellDragStarted ();
                }

            }
        }

        private void OnDrag (object sender, OnDragEventArgs e) {
            DraggableView view = sender as DraggableView;
            double yDelta = e.YDelta;
            var indexOfViewNow = IndexOfViewInSwitchedList (view);
            var indexOfViewbyRatio = IndexOfViewbyYDelta (view, yDelta);
            if (indexOfViewNow == indexOfViewbyRatio) {
                return;
            }
            if (indexOfViewbyRatio < 0 || indexOfViewbyRatio >= SwitchedViewList.Count) {
                return;
            }
            var switchingView = SwitchedViewList[indexOfViewbyRatio];
            var originalIndexOfSwitchinCell = OriginalIndexOfSwitchinCell (switchingView);
            switchingView.TranslateTo (0, (indexOfViewNow - originalIndexOfSwitchinCell) * (switchingView.Height + Spacing));
            SwitchedViewList[indexOfViewbyRatio] = view;
            SwitchedViewList[indexOfViewNow] = switchingView;
        }

        // createView is a fucntion that can create a DraggableView from the object (viewmodel) supplied to it
        // checkMatch is a funciton that checks whether a certain Draggableview is related to an object (viewmdoel)
        public void ReplaceData (List<Object> viewModelList,
            Func<Object, DraggableView> createView,
            Func<Object, DraggableView, bool> checkMatch
        )

        {
            /* Algo:
            find and remove deleted sectins
            iterate over DirectDepositViewModelList by index: 
            if the section in the index  is the right one 
                continue 
            else if the viewmodel has an existing section find it and remove it from it current paklce and insert it here 
            else if the viewmodel does not have an existing section creatd a new one and add it.
            */
            this.MoveCellsToTheirOriginalCoordinates ();
            for (int j = this.OriginalViewList.Count - 1; j >= 0; j--) {
                var match = viewModelList.FirstOrDefault (
                    vm => {
                        return vm == this.OriginalViewList[j].BindingContext;
                    }
                );
                if (match == null) {
                    this.RemoveAt (j);
                }
            }

            var cachedList = new List<DraggableView> (OriginalViewList);
            for (int i = 0; i < viewModelList.Count; i++) {
                var viewModel = viewModelList[i];
                if (false == ((this.OriginalViewList.Count > i) && this.OriginalViewList[i].BindingContext == viewModel)) {
                    var section = cachedList.Find ((obj) => checkMatch (viewModel, obj));
                    if (null != section) {
                        this.Move (section, i);
                    } else {
                        var newSection = createView (viewModel);
                        this.Insert (i, newSection);
                    }
                }
            }
        }

        #endregion

        #region Helper functions

        private int OriginalIndexOfSwitchinCell (DraggableView switchingView) {
            return OriginalViewList.FindIndex (v => v == switchingView);
        }

        private int IndexOfViewbyYDelta (View view, double yDelta) {
            return OriginalViewList.FindIndex (v => v == view) + (int) (Math.Sign (yDelta) * 0.5 + yDelta / (view.Height + Spacing));
        }

        private int IndexOfViewInSwitchedList (View view) {
            return SwitchedViewList.FindIndex (v => v == view);
        }

        #endregion
    }
}