using Config;
using UnityEngine;

namespace Scenes.Vortex
{
    // This class should be a perfect match of your JSON config
    [CreateAssetMenu(fileName = "VortexConfigData", menuName = "Config/VortexConfigData")]
    public class VortexConfigData : ConfigData
    {
        // Gets called when config is loaded
        public override void Load()
        {
            // Your code here

            base.Load();
        }
        
        // Use this function to reset all values to default
        public override void ResetToDefault(bool silent = false)
        {
            // Your code here

            base.ResetToDefault(silent);
        }
    }
}
