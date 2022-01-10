using System;
using System.Collections.Generic;
using System.Linq;

using AltNetworkUtility.Services.Windows;

using Xamarin.Forms;

namespace AltNetworkUtility.About
{
    public partial class AboutBoxPage : ContentPage
    {
        public AboutBoxPage()
        {
            InitializeComponent();
        }

        void Switch_Toggled(System.Object sender, Xamarin.Forms.ToggledEventArgs e)
        {
            if (!AcknowledgmentsPane.IsVisible)
            {
                AcknowledgmentsPane.IsVisible = true;
                WidthRequest = Grid.ColumnDefinitions.Sum(cd => cd.Width.Value);
            }
            else
            {
                AcknowledgmentsPane.IsVisible = false;
                WidthRequest = Grid.ColumnDefinitions.First().Width.Value;
            }

            Xamarin.Forms.DependencyService.Get<IAboutBoxWindowService>().ResizeToFit(this);
        }
    }
}
