//挂在摄像机上
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class InkPostEffect : PostEffectBase
{
    /// <summary>
    /// 降分辨率未操作
    /// </summary>
    [Range(0, 5)]
    public int downSample = 1;
    /// <summary>
    /// 高斯模糊采样缩放系数
    /// </summary>
    [Range(0, 5)]
    public int samplerScale = 1;
    /// <summary>
    /// 高斯模糊迭代次数
    /// </summary>
    [Range(0, 10)]
    public int count = 1;
    /// <summary>
    /// 边缘宽度
    /// </summary>
    [Range(0.0f, 10.0f)]
    public float edgeWidth = 3.0f;
    /// <summary>
    /// 边缘最小宽度
    /// </summary>
    [Range(0.0f, 1.0f)]
    public float sensitive = 0.35f;
    /// <summary>
    /// 画笔滤波系数
    /// </summary>
    [Range(0, 10)]
    public int paintFactor = 4;
    /// <summary>
    /// 噪声图
    /// </summary>
    public Texture noiseTexture;
    private Camera cam;
    private void Start()
    {
        cam = GetComponent<Camera>();
        //开启深度法线图
        cam.depthTextureMode = DepthTextureMode.DepthNormals;
    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (_Material)
        {
            RenderTexture temp1 = RenderTexture.GetTemporary(source.width >> downSample, source.height >> downSample, 0, source.format);
            RenderTexture temp2 = RenderTexture.GetTemporary(source.width >> downSample, source.height >> downSample, 0, source.format);

            Graphics.Blit(source, temp1);
            for (int i = 0; i < count; i++)
            {
                //高斯模糊横向纵向两次(pass0)
                _Material.SetVector("_offsets", new Vector4(0, samplerScale, 0, 0));
                Graphics.Blit(temp1, temp2, _Material, 0);
                _Material.SetVector("_offsets", new Vector4(samplerScale, 0, 0, 0));
                Graphics.Blit(temp2, temp1, _Material, 0);
            }

            //描边(pass1)
            _Material.SetTexture("_BlurTex", temp1);
            _Material.SetTexture("_NoiseTex", noiseTexture);
            _Material.SetFloat("_EdgeWidth", edgeWidth);
            _Material.SetFloat("_Sensitive", sensitive);
            Graphics.Blit(temp1, temp2, _Material, 1);

            //画笔滤波(pass2)
            _Material.SetTexture("_PaintTex", temp2);
            _Material.SetInt("_PaintFactor", paintFactor);
            Graphics.Blit(temp2, destination, _Material, 2);

            RenderTexture.ReleaseTemporary(temp1);
            RenderTexture.ReleaseTemporary(temp2);
        }
    }
}