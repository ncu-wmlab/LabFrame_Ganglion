using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LabFrame.Ganglion
{
    public class LabFrameGanglionDemo : MonoBehaviour
    {
        [SerializeField] Text _text;
        [SerializeField] Text _eegText;
        [SerializeField] Text _impText;


        // Start is called before the first frame update
        void Start()
        {            
            // Init Lab Frame, set userid as "Ganglion_Demo"
            LabDataManager.Instance.LabDataInit("GanglionDemo");
        }

        // Update is called once per frame
        void Update()
        {
            _text.text = $"<b>Connected: </b>{GanglionManager.Instance.IsConnected}\n" + 
                         $"- <b>Using EEG: </b>{GanglionManager.Instance.IsUsingEEG}\n" + 
                         $"- <b>Using Impedance: </b>{GanglionManager.Instance.IsUsingImpedance}\n\n";
            
            var eeg = GanglionManager.Instance.GetEegData();
            var impedance = GanglionManager.Instance.GetImpedanceData();

            _eegText.text = "<b>EEG:</b> \n";
            if(eeg != null)
            {
                // foreach (var value in eeg.EEGValues)
                // {
                //     _eegText.text += value.ToString("0.00") + "\n";
                // }
                _eegText.text = $"ch1_1 {eeg.ch1_1:0.00} ch1_2 {eeg.ch1_2:0.00}\nch2_1 {eeg.ch2_1:0.00} ch2_2 {eeg.ch2_2:0.00}\n"
                              + $"ch3_1 {eeg.ch3_1:0.00} ch3_2 {eeg.ch3_2:0.00}\nch4_1 {eeg.ch4_1:0.00} ch4_2 {eeg.ch4_2:0.00}";
            }
            _impText.text = "<b>Impedance:</b> \n";            
            foreach (var value in impedance.ImpedanceValues)
            {
                _impText.text += value.ToString("00") + "\n";
            }
            
        }

        public void StartWriteLabData()
        {            
            if(GanglionManager.Instance.IsConnected && !GanglionManager.Instance.IsUsingEEG)
            {
                // Start recording data
                GanglionManager.Instance.StreamData();                
            }
        }
        public void StopWriteLabData()
        {
            if(GanglionManager.Instance.IsConnected && GanglionManager.Instance.IsUsingEEG)
            {
                // Stop recording data
                GanglionManager.Instance.StopStreamData();
            }
        }

        public void StartWriteImpedance()
        {
            if(GanglionManager.Instance.IsConnected && !GanglionManager.Instance.IsUsingImpedance)
            {
                // Start recording impedance
                GanglionManager.Instance.StreamImpedance();
            }
        }
        public void StopWriteImpedance()
        {
            if(GanglionManager.Instance.IsConnected && GanglionManager.Instance.IsUsingImpedance)
            {
                // Stop recording impedance
                GanglionManager.Instance.StopStreamImpedance();
            }
        }

        public void Connect()
        {
            GanglionManager.Instance.ManualConnect();
        }

        public void Disconnect()
        {
            GanglionManager.Instance.ManualDisconnect();
        }

        public void ExitApp()
        {
            Application.Quit();
        }
    }
}
