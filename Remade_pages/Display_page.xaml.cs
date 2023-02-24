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

        /*public SerialPort myPort = new SerialPort("COM3", 115200);
        ToBoardClass getValue = new ToBoardClass();*/

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
            /*myPort.Open();*/

            //possible idea: On start - we create threads for each value we want to return, then setup a while
            //loop for each thread. the while loop will check that the stop button hasn't been clicked, once the stop button is clicked we flag
            //it with a bool and then we can wait() all the threads. Within each while loop all we're doing is updating the value, probably making
            //the thread wait for like 2 seconds. The reasoning behind having each method have it's own individual thread is then they can all run at the
            //same time while still being independent. Bad part is threads share runtime space, may lead to worse performance.

            //Other option is to have one method that goes through and changes all the values in one go, we dedicate one thread to that method, then that just goes through
            //in one while loop and constantly updates going one at a time. Pros - one thread = less shared runtime space. bad - probably less frequent updates.

            //every time we update each value in the while loop we also went to call a checkForWarning method for the specific value so we can immediately let the user
            //know action needs to be taken.

            /*int tempVal = getValue.getTemp();
            return_temp_block.Text = tempVal.ToString();

            int voltVal = getValue.getVoltage();
            return_temp_block.Text = voltVal.ToString();

            int currentVal = getValue.getVoltage();
            return_temp_block.Text = currentVal.ToString();

            int powerVal = getValue.getVoltage();
            return_temp_block.Text = powerVal.ToString();*/


        }

        public void stop_btn_Click(object sender, RoutedEventArgs e)
        {
            //close serial port
            /*myPort.Close();*/

            //wait() any threads that need it
        }

        //Basically just storing the code needed to get the value from the method and update the value here, shouldn't stay in here though.
        //This could show go above after the user hits the start button to constantly get the values. pseudocode up above in start button
        //refers to this

        private void return_temp_block_SelectionChanged(object sender, RoutedEventArgs e)
        {
            //making assumptions about threads, may need to declare threads and change as needed for each method
            /*int tempVal = getValue.getTemp();
            return_temp_block.Text = tempVal.ToString();*/
        }

        private void return_volt_block_SelectionChanged(object sender, RoutedEventArgs e)
        {
            /*int voltVal = getValue.getVoltage();
            return_temp_block.Text = voltVal.ToString();*/
        }

        private void return_current_block_SelectionChanged(object sender, RoutedEventArgs e)
        {
            /*int currentVal = getValue.getVoltage();
            return_temp_block.Text = currentVal.ToString();*/
        }

        private void return_power_block_SelectionChanged(object sender, RoutedEventArgs e)
        {
            /*int powerVal = getValue.getVoltage();
            return_temp_block.Text = powerVal.ToString();*/
        }
    }
}
