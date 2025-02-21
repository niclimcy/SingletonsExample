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

    // Public reference to the music player
    public MusicPlayer musicPlayer;

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("Grabber"))
        {
            switch (ControlSelection)
            {
                case Control.Play:
                    musicPlayer.Play();
                    break;
                case Control.Pause:
                    musicPlayer.Pause();
                    break;
                case Control.Skip:
                    musicPlayer.Skip();
                    break;
            }
        }
    }
}
