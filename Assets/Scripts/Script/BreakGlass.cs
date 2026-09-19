using UnityEngine;
using System.Collections;
using DG.Tweening;

public class BreakGlass : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds0_15 = new WaitForSeconds(0.15f);
    private static WaitForSeconds _waitForSeconds0_1 = new WaitForSeconds(0.1f);

    //[SerializeField] private bool useGravity = false;                            // 重力を有効にするかどうか
    [SerializeField] internal Vector3 explodeVel = new Vector3(0, 0, 0f);      // 爆発の中心地
    [SerializeField] private float explodeForce = 75000;                         // 爆発の威力
    [SerializeField] private float explodeRange = 500;                          // 爆発の範囲
    internal Rigidbody[] rigidBodies;
    [SerializeField] AudioClip breakGlassSE;

    public virtual void Init(Material material)
    {
        rigidBodies = GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rb in rigidBodies)
        {
            rb.isKinematic = true;

            if (material != null)
            {
                rb.GetComponent<MeshRenderer>().material = material;
            }
        }
    }

    public virtual void Break()
    {
        this.gameObject.SetActive(true);
        StartCoroutine(BreakIenumerator());
    }

    public virtual IEnumerator BreakIenumerator()
    {
        ContinuousController.instance.PlaySE(breakGlassSE);

        foreach (Rigidbody rb in rigidBodies)
        {
            rb.isKinematic = false;
            rb.AddExplosionForce(explodeForce, transform.position + explodeVel, explodeRange);
        }

        yield return _waitForSeconds0_1;

        if (this is not SecurityBreakGlass)
        {
            foreach (Rigidbody rb in rigidBodies)
            {
                rb.velocity *= 20;
            }
        }

        yield return _waitForSeconds0_15;

        foreach (Rigidbody rb in rigidBodies)
        {
            rb.isKinematic = true;
        }

        gameObject.SetActive(false);
    }
}