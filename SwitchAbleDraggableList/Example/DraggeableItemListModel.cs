using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SwitchAbleDraggableList.Views;

namespace SwitchAbleDraggableList.Example
{
    public class DraggeableItemListModel : ICellsSwitchedListner
    {
        public List<DraggeableItemModel> ItemList { get; private set; } = new List<DraggeableItemModel>();

        public Action onDataUpdated { get; set; }

        public DraggeableItemListModel()
        {
            ItemList=new List<DraggeableItemModel>()
            {
                new DraggeableItemModel { Text = "1" },
                new DraggeableItemModel { Text = "2" },
                new DraggeableItemModel { Text = "3" },
                new DraggeableItemModel { Text = "4" },
                new DraggeableItemModel { Text = "5" },
                new DraggeableItemModel { Text = "6" },
                new DraggeableItemModel { Text = "7" },
                new DraggeableItemModel { Text = "8" },
                new DraggeableItemModel { Text = "9" },
            };
        }

        public async Task CellsSwitched(List<int> newOrder)
        {
            await Task.Delay(10);
            var newViewModelList = new List<DraggeableItemModel>(this.ItemList.Count);
            foreach (var pos in newOrder)
            {
                newViewModelList.Add(this.ItemList[pos]);
            }

            this.ItemList=newViewModelList;

            if (onDataUpdated!=null)
            {
                onDataUpdated.Invoke();
            }            
        }
    }
}
