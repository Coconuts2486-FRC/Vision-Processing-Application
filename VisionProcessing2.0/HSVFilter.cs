using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionProcessing2._0
{
    class HSVFilter
    {
        #region Local and public variables
        /// <summary>
        /// Provides the context for what value to change.
        /// </summary>
        public enum Context
        {
            lowerHue,
            upperHue,
            lowerSaturation,
            upperSaturation,
            lowerValue,
            upperValue
        }
        
        public Hsv lowerFilter;
        public Hsv upperFilter;

        private int lowerHue;
        private int upperHue;
        private int lowerSaturation;
        private int upperSaturation;
        private int lowerValue;
        private int upperValue;
        #endregion
        #region Constructors
        /// <summary>
        /// Uses default values (0-180, 0-255, 0-255).
        /// </summary>
        public HSVFilter()
        {
            lowerHue = 0;
            upperHue = 180;
            lowerSaturation = 0;
            upperSaturation = 255;
            lowerValue = 0;
            upperValue = 255;
            update();
        }
        /// <summary>
        /// Supply custom filters.
        /// </summary>
        /// <param name="lowerFilter"></param>
        /// <param name="upperFilter"></param>
        public HSVFilter(Hsv lowerFilter, Hsv upperFilter)
        {
            this.lowerFilter = lowerFilter;
            this.upperFilter = upperFilter;
        }
        #endregion
        #region Set values
        /// <summary>
        /// Modifies the range of values.
        /// Hue is 180 degrees, while saturation and value are 255.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="context"></param>
        public void setValue(int val, Context context)
        {
            switch (context)
            {
                case Context.lowerHue:
                    lowerHue = val;
                    break;
                case Context.upperHue:
                    upperHue = val;
                    break;
                case Context.lowerSaturation:
                    lowerSaturation = val;
                    break;
                case Context.upperSaturation:
                    upperSaturation = val;
                    break;
                case Context.lowerValue:
                    lowerValue = val;
                    break;
                case Context.upperValue:
                    upperValue = val;
                    break;
                default:
                    break;
            }
            update();
        }
        private void update()
        {
            lowerFilter = new Hsv(lowerHue, lowerSaturation, lowerValue);
            upperFilter = new Hsv(upperHue, upperSaturation, upperValue);
        }
        #endregion
        
        /// <summary>
        /// Formats the range.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Printout of current range. \nHue: " + lowerHue + "-" + upperHue + "\nSaturation: " + lowerSaturation + "-"+ upperSaturation + "\nValue: " + lowerValue + "-" + upperValue;
        }
    }
}
