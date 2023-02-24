using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remade_pages
{
    class ToBoardClass : Display_page
    {
        public int getTemp()
        {
            int temp = 0;

            try
            {
                //dc/dc temp
                /*myPort.Write("get tempmon2.in\r");
                string tempStr = myPort.ReadExisting();
                temp = Int32.Parse(tempStr);*/
            }

            catch (Exception ex)
            {

            }

            return temp;
        }

        public int getVoltage()
        {
            int volt = 0;

            try
            {
                
                /*myPort.Write("get battvmon.in\r");
                string tempStr = myPort.ReadExisting();
                volt = Int32.Parse(tempStr);*/
            }

            catch (Exception ex)
            {

            }

            return volt;
        }

        public int getPower()
        {
            int pow = 0;

            try
            {

                /*myPort.Write("get battvmon.in\r");
                string tempStr = myPort.ReadExisting();
                pow = Int32.Parse(tempStr);*/
            }

            catch (Exception ex)
            {

            }

            return pow;
        }

        public int getCurrent()
        {
            int current = 0;

            try
            {

                /*myPort.Write("get battimon.in\r");
                string tempStr = myPort.ReadExisting();
                current = Int32.Parse(tempStr);*/
            }

            catch (Exception ex)
            {

            }

            return current;
        }

        public bool checkWarningTemp(int temp)
        {
            bool flag = true;

            //if (temp > 120)
                //flag = false;

            return flag;
        }

        public bool checkWarningVolt(int volt)
        {
            bool flag = true;

            //if (volt > 120)
            //flag = false;

            return flag;
        }

        public bool checkWarningPow(int pow)
        {
            bool flag = true;

            //if (pow > 120)
            //flag = false;

            return flag;
        }

        public bool checkWarningCurrent(int current)
        {
            bool flag = true;

            //if (current > 120)
            //flag = false;

            return flag;
        }
    }
}
