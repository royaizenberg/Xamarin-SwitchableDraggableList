using System;
using SwitchAbleDraggableList.Example;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SwitchAbleDraggableList
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new ExamplePage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
