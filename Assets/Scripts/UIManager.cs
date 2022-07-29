using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Button startRenderBtn;
    public Image renderResultImg;
    public Text renderProcessingTex;

    private void Awake()
    {
        startRenderBtn.onClick.RemoveAllListeners();
        startRenderBtn.onClick.AddListener(OnStartRenderBtnClick);
    }

    private void OnStartRenderBtnClick()
    {
        StartCoroutine(MainRayTracing.Instance.StartRender(OnRenderFinish, OnRendering));
    }

    private void OnRenderFinish()
    {
        renderResultImg.sprite = Sprite.Create(MainRayTracing.Instance.rayTracingRenderTarget, new Rect(0, 0, MainRayTracing.Instance.rayTracingRenderTarget.width, MainRayTracing.Instance.rayTracingRenderTarget.height), new Vector2(0.5f, 0.5f));
        renderProcessingTex.text = String.Empty;
    }

    private void OnRendering(float processing)
    {
        renderProcessingTex.text = string.Format("{0:F}%", processing);
    }
}
