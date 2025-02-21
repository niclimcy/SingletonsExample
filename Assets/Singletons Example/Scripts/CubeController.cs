using UnityEngine;

public class CubeController : MonoBehaviour
{
    // Possible actions for music control
    public enum Control
    {
        Play,
        Pause,
        Skip
    }

    public Control ControlSelection;

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("Grabber"))
        {
            switch (ControlSelection)
            {
                case Control.Play:
                    MusicPlayer.Instance.Play();
                    break;
                case Control.Pause:
                    MusicPlayer.Instance.Pause();
                    break;
                case Control.Skip:
                    MusicPlayer.Instance.Skip();
                    break;
            }
        }
    }
}
