
using GrimbartTales.Platformer2D.CharacterController;
using UnityEngine;

namespace ItorahDebug {
    public class NoClipController : MonoBehaviour
    {
        private bool noClipEnabled = false;

        public Plugin mainPlugin;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.N))
            {
                ToggleNoClip();
            }

            if (noClipEnabled)
            {
                // Allow the player to move freely without collision
                Vector2 move = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
                mainPlugin.itorah.transform.Translate(move * Time.deltaTime * 30f); // Adjust speed as necessary
            }
        }

        private void ToggleNoClip()
        {
            noClipEnabled = !noClipEnabled;
            if (noClipEnabled)
            {
                Rigidbody2D rb2d = mainPlugin.itorah.GetComponent<Rigidbody2D>();
                if (rb2d != null)
                {
                    rb2d.isKinematic = true; // Disable physics interactions
                    rb2d.velocity = Vector2.zero; // Reset velocity to prevent movement glitches
                }
                ScriptableInputActions inputActions = mainPlugin.itorah.GetComponent<ScriptableInputActions>();
                if (inputActions != null)
                {
                    inputActions.enabled = false; // Disable input actions to prevent normal movement
                }
                Plugin.Logger.LogDebug("No-clip mode enabled");
            }
            else
            {
                Rigidbody2D rb2d = mainPlugin.itorah.GetComponent<Rigidbody2D>();
                if (rb2d != null)
                {
                    rb2d.isKinematic = false; // Re-enable physics interactions
                }
                ScriptableInputActions inputActions = mainPlugin.itorah.GetComponent<ScriptableInputActions>();
                if (inputActions != null)
                {
                    inputActions.enabled = true; // Re-enable input actions for normal movement
                }
                Plugin.Logger.LogDebug("No-clip mode disabled");
            }
        }
    }
}