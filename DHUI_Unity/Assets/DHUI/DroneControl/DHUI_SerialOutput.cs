using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System;
using System.Threading;

/// <summary>
/// This Namespace contains the Core features of DHUI – Drone Haptic User Interface.
/// These Scripts are essential for the system to work.
/// </summary>
namespace DHUI.Core
{
    /// <summary>
    /// This class handles the output of the values to serial. 
    /// It establishes a conncection to the serial-port, transforms the integer-values to a specific string-format and sends them to the serial at around 66Hz.
    /// </summary>
    public class DHUI_SerialOutput : MonoBehaviour
    {
        [SerializeField][Tooltip("Name of the Serial-Port.")]
        private string serialPortName = "COM6";

        [SerializeField][Tooltip("Should the Output-Values be logged for debugging?")]
        private bool logOutputValues = false;

        // Serial-Port we open and write to.
        private SerialPort serialPort = null;

        // Thread handling the output to serial. (66Hz)
        private Thread serialOutputThread;

        // The Values we send to the serial.
        private int[] currentOutputValues = { 1000, 1500, 1500, 1500, 1000, 1000, 1000, 1000 };

        #region Public Methods
        /// <summary>
        /// Sets the values that should be written to serial.
        /// </summary>
        /// <param name="values">Output values</param>
        public void SetValues(int[] values)
        {
            currentOutputValues = values;
        }

        #endregion Public Methods

        #region Start/End

        /// <summary>
        /// On Start: Setup the Serial Connection and Start the Output Loop
        /// </summary>
        private void Start()
        {
            OpenSerialConnection();
            StartOutputLoop();
        }
        /// <summary>
        /// On Disable: End the Output-Loop and Close the Serial Connection
        /// </summary>
        private void OnDisable()
        {
            EndOutputLoop();
            CloseSerialConnection();
        }
        /// <summary>
        /// On Destroy: End the Output-Loop and Close the Serial Connection
        /// </summary>
        private void OnDestroy()
        {
            EndOutputLoop();
            CloseSerialConnection();
        }

        #endregion Start/End

        #region Output Loop

        /// <summary>
        /// Starts the thread handling the output loop.
        /// </summary>
        private void StartOutputLoop()
        {
            serialOutputThread = new Thread(OutputLoop);
            serialOutputThread.Start();
        }
        /// <summary>
        /// Ends the thread with the output loop.
        /// </summary>
        private void EndOutputLoop()
        {
            serialOutputThread.Abort();
            serialOutputThread.Join();
        }
        /// <summary>
        /// This loop takes the current values and writes them into the serial port at ~66Hz.
        /// </summary>
        private void OutputLoop()
        {
            while (true)
            {
                int[] copy = currentOutputValues;
                string message = BuildString(copy);
                WriteToSerial(message);
                if (logOutputValues)
                {
                    Debug.Log(message);
                }
                Thread.Sleep(25);
            }
        }

        #endregion Output Loop

        #region Serial Connection
        /// <summary>
        /// Opens the Connection to the serial port. 
        /// </summary>
        private void OpenSerialConnection()
        {
            
            serialPort = new SerialPort(serialPortName, 115200, Parity.Odd, 8, StopBits.One);
            if (serialPort != null)
            {
                if (serialPort.IsOpen)
                {
                    serialPort.Close();
                    Debug.LogWarning("<b>DHUI</b> | SerialOutput | SerialPort was already open. Closing it now and reopening it.");
                }
                else
                {
                    try
                    {
                        serialPort.Open();
                        serialPort.ReadTimeout = 50;
                        Debug.Log("<b>DHUI</b> | SerialOutput | SerialPort opened.");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("<b>DHUI</b> | SerialOutput | Error trying to open Serial-Port: " + ex.Message);
                    }
                }
            }
        }
        /// <summary>
        /// Writes a string to the opened serial port.
        /// </summary>
        /// <param name="msg">Message to write to serial.</param>
        private void WriteToSerial(string msg)
        {
            if (serialPort != null && serialPort.IsOpen)
                serialPort.Write(msg);
        }
        /// <summary>
        /// Closes the connection to the serial port.
        /// </summary>
        private void CloseSerialConnection()
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
                Debug.Log("<b>DHUI</b> | SerialOutput | SerialPort closed.");
            }
        }
        #endregion Serial Connection

        #region Utils

        /// <summary>
        /// Builds a string out of an Int-Array.
        /// </summary>
        /// <param name="values">Int-Array containing the values, which should be build into a string.</param>
        /// <returns>
        /// String in this specific format: 
        /// <1000, 1500, 1500, 1500, 1000, 1000, 1000, 1000> 
        /// </returns>
        private string BuildString(int[] values)
        {
            string message = "<";
            foreach (int i in values)
            {
                message += i + ", ";
            }
            message = message.Remove(message.Length - 2);
            message += ">";
            return message;
        }
        #endregion Utils
    }
}
