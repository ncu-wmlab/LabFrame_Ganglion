using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LabFrame.Ganglion
{
    public class LabFrameGanglionDemo : MonoBehaviour
    {
        [SerializeField] Text _text;

        // Start is called before the first frame update
        void Start()
        {            
            // GanglionManager.Instance
        }

        // Update is called once per frame
        void Update()
        {
            _text.text = $"<b>Connected: </b>{GanglionManager.Instance.IsConnected}\n";
            
            var eeg = GanglionManager.Instance.GetEegData();
            var impedance = GanglionManager.Instance.GetImpedanceData();
            if(eeg != null)
            {
                _text.text += "<b>EEG:</b> \n";
                foreach (var value in eeg.EEGValues)
                {
                    _text.text += value.ToString("0.00") + "\n";
                }
            }
            _text.text += "\n";
            if(impedance != null)
            {
                _text.text += "<b>Impedance:</b> \n";
                foreach (var value in impedance.ImpedanceValues)
                {
                    _text.text += value.ToString("00") + "\n";
                }
            }
        }

        public void StartWriteLabData()
        {
            GanglionManager.Instance.StartWriteLabData();
        }
        public void StopWriteLabData()
        {
            GanglionManager.Instance.StopWriteLabData();
        }
    }
}
