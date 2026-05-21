/*
 * Script: ExitButtonScript.cs
 * Purpose: UI button handler to exit to main menu and reset score state.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ExitButtonScript : MonoBehaviour
{
        [SerializeField]
        int sceneIndexToLoad = 0;

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
            ActivateExit();
        }
    }

    public void buttonClicked(){
        Debug.Log("Exit Button Clicked");
        ScoreState.Reset();
        ActivateExit();
    }

    void ActivateExit()
    {
        if (lastActivationFrame == Time.frameCount)
        {
            return;
        }

        lastActivationFrame = Time.frameCount;
        ScoreState.Reset();
        SceneManager.LoadScene(sceneIndexToLoad);
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
