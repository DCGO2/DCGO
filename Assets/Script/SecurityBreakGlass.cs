using UnityEngine;
using System.Collections;
using DG.Tweening;
using System;
public class SecurityBreakGlass : BreakGlass
{
    [SerializeField] Material transparentMatarial;
    [SerializeField] Material blueMatarial;

    Vector3[] defaultPos;
    Vector3[] defaultScale;
    Quaternion[] defaultRotation;

    MeshCollider[] meshColliders;
    MeshRenderer[] meshRenderers;
    // public bool IsBlueGlass => gameObject.activeSelf && meshRenderers.Length >= 1 && meshRenderers[0].material == blueMatarial;
    public bool IsBlueGlass
    {
        get
        {
            return gameObject.activeSelf && _isBlueGlass;
        }

        set
        {
            _isBlueGlass = value;
        }
    }
    bool _isBlueGlass = false;
    public override void Init(Material material)
    {
        base.Init(material);

        defaultPos = new Vector3[rigidBodies.Length];
        defaultScale = new Vector3[rigidBodies.Length];
        defaultRotation = new Quaternion[rigidBodies.Length];

        meshColliders = new MeshCollider[rigidBodies.Length];
        meshRenderers = new MeshRenderer[rigidBodies.Length];

        for (int i = 0; i < rigidBodies.Length; i++)
        {
            Rigidbody rb = rigidBodies[i];

            defaultPos[i] = rb.transform.localPosition;
            defaultScale[i] = rb.transform.localScale;
            defaultRotation[i] = rb.transform.localRotation;

            meshColliders[i] = rb.GetComponent<MeshCollider>();
            meshRenderers[i] = rb.GetComponent<MeshRenderer>();
        }

        IsBlueGlass = false;

        gameObject.SetActive(false);
    }

    public override void Break()
    {
        base.Break();
    }

    public void ShowTransparentMatarial()
    {
        IsBlueGlass = false;
        ShowDefaulPosGlass(transparentMatarial);
    }

    public void ShowBlueMatarial()
    {
        IsBlueGlass = true;
        ShowDefaulPosGlass(blueMatarial);
    }

    void ShowDefaulPosGlass(Material material)
    {
        gameObject.SetActive(true);

        for (int i = 0; i < rigidBodies.Length; i++)
        {
            Rigidbody rb = rigidBodies[i];

            rb.isKinematic = true;
        }

        for (int i = 0; i < meshColliders.Length; i++)
        {
            MeshCollider meshCollider = meshColliders[i];

            meshCollider.enabled = false;
        }

        for (int i = 0; i < rigidBodies.Length; i++)
        {
            Rigidbody rb = rigidBodies[i];

            rb.transform.localPosition = defaultPos[i];
            rb.transform.localScale = defaultScale[i];
            rb.transform.localRotation = defaultRotation[i];
        }

        for (int i = 0; i < meshRenderers.Length; i++)
        {
            MeshRenderer meshRenderer = meshRenderers[i];

            meshRenderer.material = material;
        }

        for (int i = 0; i < meshColliders.Length; i++)
        {
            MeshCollider meshCollider = meshColliders[i];

            meshCollider.enabled = true;
        }
    }

    public override IEnumerator BreakIenumerator()
    {
        ShowBlueMatarial();

        explodeVel = new Vector3(UnityEngine.Random.Range(-20, 20), UnityEngine.Random.Range(-20, 20), 0);

        yield return StartCoroutine(base.BreakIenumerator());

        IsBlueGlass = false;
    }
}
