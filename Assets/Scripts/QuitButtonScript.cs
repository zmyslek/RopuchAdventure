/*
 * Script: QuitButtonScript.cs
 * Purpose: UI button handler to quit the application.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitButtonScript : MonoBehaviour
{
    RectTransform buttonRect;
    Canvas parentCanvas;
    int lastActivationFrame = -1;

    // Start is called before the first frame update
    void Start()
    {
        buttonRect = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && IsPointerOverButton())
        {
            QuitGame();
        }
    }

    public void buttonClicked(){
        Debug.Log("Quit Button Clicked");
        QuitGame();
    }

    void QuitGame()
    {
        if (lastActivationFrame == Time.frameCount)
        {
            return;
        }

        lastActivationFrame = Time.frameCount;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    bool IsPointerOverButton()
    {
        if (buttonRect == null)
        {
            return false;
        }

        Camera eventCamera = null;
        if (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            eventCamera = parentCanvas.worldCamera;
        }

        return RectTransformUtility.RectangleContainsScreenPoint(buttonRect, Input.mousePosition, eventCamera);
    }
}
