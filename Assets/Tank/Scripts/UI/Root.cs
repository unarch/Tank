using UnityEngine;
    
public class Root : MonoBehaviour
{
    private void Start() {
        PanelMgr.instance.OpenPanel<TitlePanel>("");
    }
}
