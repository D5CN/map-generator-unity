using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaterWaveGenerator // : EffectTool
{
    /*
    public RenderTexture _rt;

    public int RT_SIZE_SCALE = 2;
    private Material _mat;

    public float frequency = 2.5f;
    public float scale = 0.1f;
    public float power = 2;
    public float centralized = 0.25f;
    public float falloff = 3;

    public Renderer waterRenderer;

    private List<Vector4> _list = new List<Vector4>();


    public bool demo;
    public override void restart()
    {
        float aspectRatio = 1.0f;

        if (_rt == null)
        {
            if (waterRenderer == null)
                waterRenderer = GetComponent<MeshRenderer>();

            if (waterRenderer != null)
            {
                if (GetComponent<MeshCollider>() == null)
                    gameObject.AddComponent<MeshCollider>();

                int w = (int)waterRenderer.bounds.size.x * RT_SIZE_SCALE;
                int h = (int)waterRenderer.bounds.size.z * RT_SIZE_SCALE;
                _rt = new RenderTexture(w, h, 0);
                waterRenderer.sharedMaterial.SetTexture("_RippleTex", _rt);

                aspectRatio = (float)w / (float)h;
            }
        }
        if (_mat == null)
        {
            _mat = new Material(Shader.Find("Luoyinan/Scene/Water/WaterRipple"));
            _mat.SetVector("_RippleData", new Vector4(frequency, scale, centralized, falloff));
            _mat.SetFloat("_AspectRatio", aspectRatio);
        }
    }

    public void setDrop(Vector3 pos)
    {
        Vector3 rel = pos - transform.position;
        float width = waterRenderer.bounds.size.x;
        float height = waterRenderer.bounds.size.z;
        Vector4 dd = new Vector4(rel.x / width + 0.5f, rel.z / height + 0.5f, 0, power); // MVP空间位置[0, 1]
        _list.Add(dd);
    }

    void Update()
    {
        int count = _list.Count;
        float deltaTime = Time.deltaTime;
        RenderTexture oldRT = RenderTexture.active;
        Graphics.SetRenderTarget(_rt);
        GL.Clear(false, true, Color.black);

        if (count > 0)
        {
            _mat.SetVector("_RippleData", new Vector4(frequency, scale, centralized, falloff));
        }

        for (int i = count - 1; i >= 0; i--)
        {
            Vector4 drop = _list[i];
            drop.z = drop.z + deltaTime;

            if (drop.z > 3)
            {
                _list.RemoveAt(i);
                continue;
            }
            else
            {
                _list[i] = drop;
            }

            GL.PushMatrix();
            _mat.SetPass(0);
            _mat.SetVector("_Drop1", drop);
            GL.LoadOrtho();
            GL.Begin(GL.QUADS);
            GL.Vertex3(0, 1, 0);
            GL.Vertex3(1, 1, 0);
            GL.Vertex3(1, 0, 0);
            GL.Vertex3(0, 0, 0);
            GL.End();
            GL.PopMatrix();
        }
        RenderTexture.active = oldRT;

        if (demo)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                setDrop(transform.position);
            }
        }
    }

    public override void end()
    {
        if (_rt != null)
        {
            _rt.Release();
        }
        _rt = null;
    }
    */
}