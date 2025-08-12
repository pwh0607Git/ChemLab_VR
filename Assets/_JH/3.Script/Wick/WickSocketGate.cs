using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class WickSocketGate : MonoBehaviour
{
    public SoilCover soilCover;
    public XRSocketInteractor socket;

    private void Start()
    {
        if (socket) socket.gameObject.SetActive(false); // 처음엔 꺼둠
    }

    public void EnableSocket()
    {
        if (socket) socket.gameObject.SetActive(true);
    }
}
