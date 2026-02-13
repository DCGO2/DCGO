using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HideCannotSelectObject : MonoBehaviour
{
    [SerializeField] Transform _cardUnmasksParent;
    bool _first = false;

    List<PermanentUnMask> _permanentUnMasks = new List<PermanentUnMask>();
    public void Init()
    {
        Close();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void SetUpHideCannotSelectObject(List<FieldPermanentCard> fieldPermanentCards, bool isHatch)
    {
        gameObject.SetActive(true);

        _permanentUnMasks = new List<PermanentUnMask>();

        for (int i = 0; i < _cardUnmasksParent.childCount; i++)
        {
            _cardUnmasksParent.GetChild(i).gameObject.SetActive(false);

            if (i < fieldPermanentCards.Count)
            {
                Image image = _cardUnmasksParent.GetChild(i).GetComponent<Image>();

                if (image != null)
                {
                    _permanentUnMasks.Add(new PermanentUnMask(fieldPermanentCards[i]?.ThisPermanent ?? null, image));
                }
            }
        }

        foreach (PermanentUnMask permanentUnMask in _permanentUnMasks)
        {
            permanentUnMask.Image.gameObject.SetActive(false);
        }

        if (isHatch)
        {
            // _permanentUnMasks = new List<PermanentUnMask>(){};

            if (_permanentUnMasks.Count >= 1)
            {
                _permanentUnMasks[0].Image.gameObject.SetActive(true);

                _permanentUnMasks[0].Image.transform.position = GManager.instance.You.HatchObject.transform.position;

                _permanentUnMasks[0].Image.transform.localPosition += new Vector3(-5.7f, -7.82f);

                _permanentUnMasks[0].Image.transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
        }

        else
        {
            for (int i = 0; i < fieldPermanentCards.Count; i++)
            {
                if (i < _permanentUnMasks.Count)
                {
                    _permanentUnMasks[i].Image.gameObject.SetActive(true);

                    _permanentUnMasks[i].Image.transform.position = fieldPermanentCards[i].transform.position;

                    if (fieldPermanentCards[i].ThisPermanent.IsSuspended && ContinuousController.instance.turnSuspendedCards)
                    {
                        _permanentUnMasks[i].Image.transform.localRotation = Quaternion.Euler(0, 0, 90);
                    }

                    else
                    {
                        _permanentUnMasks[i].Image.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    }
                }
            }
        }
    }

    int _timerCount = 0;
    int _updateFrame = 20;

    void Update()
    {
        #region ���t���[���Ɉ�x�������f
        _timerCount++;

        if (_timerCount < _updateFrame)
        {
            return;
        }

        else
        {
            _timerCount = 0;
        }
        #endregion

        if (ContinuousController.instance != null)
        {
            Quaternion localRotation = Quaternion.Euler(0, 0, 0);

            if (ContinuousController.instance.turnSuspendedCards)
            {
                localRotation = Quaternion.Euler(0, 0, 90);
            }

            foreach (PermanentUnMask permanentUnMask in _permanentUnMasks)
            {
                if (permanentUnMask.Permanent != null)
                {
                    if (permanentUnMask.Permanent.IsSuspended)
                    {
                        permanentUnMask.Image.transform.localRotation = localRotation;
                    }
                }
            }
        }
    }
}

class PermanentUnMask
{
    public PermanentUnMask(Permanent permanent, Image image)
    {
        Permanent = permanent;
        Image = image;
    }
    public Permanent Permanent { get; set; }
    public Image Image { get; private set; }
}
