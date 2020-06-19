using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DHUI.Core
{
    /// <summary>
    /// Base (abstract) class for handling the output-values of the drone controller.
    /// The actual output, including the output frequency/loop, should be handled in the derived classes. 
    /// </summary>
    public abstract class DHUI_Output : MonoBehaviour
    {
        // The Values we output.
        protected int[] currentOutputValues = { 1000, 1500, 1500, 1500, 1000, 1000, 1000, 1000 };
        
        #region Public Methods
        /// <summary>
        /// Sets the values that should be outputted.
        /// </summary>
        /// <param name="values">Output values</param>
        public void SetValues(int[] values)
        {
            currentOutputValues = values;
        }

        /// <summary>
        /// Gets the current output values.
        /// </summary>
        /// <returns>The current values to output.</returns>
        public int[] GetCurrentValues()
        {
            return currentOutputValues;
        }
        #endregion Public Methods
    }

}