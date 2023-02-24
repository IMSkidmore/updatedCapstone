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
    public sealed partial class User_info_page : Page
    {
        public User_info_page()
        {
            this.InitializeComponent();
        }

        public void SetC1(string text)
        {
            TextC1.Text = text;
        }

        public void SetC2(string text)
        {
            TextC2.Text = text;
        }

        public void SetC3(string text)
        {
            TextC3.Text = text;
        }

        public void SetC4(string text)
        {
            TextC4.Text = text;
        }

        private void Next_skip_button_Click(object sender, RoutedEventArgs e)
        {
            Display_page disp = new Display_page();
            this.Content = disp;
        }
    }
}
