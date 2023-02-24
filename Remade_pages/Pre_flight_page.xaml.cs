using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Remade_pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Pre_flight_page : Page
    {
        public Pre_flight_page()
        {
            this.InitializeComponent();
        }

        private void next_btn_Click(object sender, RoutedEventArgs e)
        {
            User_info_page next = new User_info_page();

            if (Check1.IsChecked == true)
            {
                next.SetC1((string)Check1.Content);
            }
            if (Check2.IsChecked == true)
            {
                next.SetC2((string)Check2.Content);
            }
            if (Check3.IsChecked == true)
            {
                next.SetC3((string)Check3.Content);
            }
            if (Check4.IsChecked == true)
            {
                next.SetC4((string)Check4.Content);
            }


            this.Content = next;
            //this.Visibility = Visibility.Collapsed; //hide the current window when opening the second window
            //objNextPage.Visibility = Visibility.Visible;
        }

        private void beginning_btn_Click(object sender, RoutedEventArgs e)
        {
            MainPage beginning = new MainPage();
            this.Content = beginning;
        }

        private void skip_btn_Click(object sender, RoutedEventArgs e)
        {
            Display_page disp = new Display_page();
            this.Content = disp;
        }
    }
}
