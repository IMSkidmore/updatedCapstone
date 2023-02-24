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
using System.IO;
using System.IO.Ports;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Remade_pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class Display_page : Page
    {

        public SerialPort myPort = new SerialPort("COM3", 115200);
        ToBoardClass getValue = new ToBoardClass();

        public Display_page()
        {
            this.InitializeComponent();
            
        }

        private void menu_button_Click(object sender, RoutedEventArgs e)
        {
            MainPage beginning = new MainPage();
            this.Content = beginning;
        }

        private void check_runtime_btn_Click(object sender, RoutedEventArgs e)
        {
            int i = 0;
            string num_in_runtime = curr_run_info_box.Text.ToString();
            bool result = int.TryParse(num_in_runtime, out i);
            if (result)
            {
                if (i > 120)
                {
                    Warning_page warning = new Warning_page();
                    this.Content = warning;

                }
                else
                {

                }
            }
        }

        public void start_btn_Click(object sender, RoutedEventArgs e)
        {
            //open serial port
            myPort.Open();
            
            //notify() any threads that need it
        }

        public void stop_btn_Click(object sender, RoutedEventArgs e)
        {
            //close serial port
            myPort.Close();

            //wait() any threads that need it
        }

        private void return_temp_block_SelectionChanged(object sender, RoutedEventArgs e)
        {
            int tempVal = getValue.getTemp();
            return_temp_block.Text = tempVal.ToString();
        }

        private void return_volt_block_SelectionChanged(object sender, RoutedEventArgs e)
        {
            int voltVal = getValue.getVoltage();
            return_temp_block.Text = voltVal.ToString();
        }

        private void return_current_block_SelectionChanged(object sender, RoutedEventArgs e)
        {
            int currentVal = getValue.getVoltage();
            return_temp_block.Text = currentVal.ToString();
        }

        private void return_power_block_SelectionChanged(object sender, RoutedEventArgs e)
        {
            int powerVal = getValue.getVoltage();
            return_temp_block.Text = powerVal.ToString();
        }
    }
}
