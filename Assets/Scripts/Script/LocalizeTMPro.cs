using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class LocalizeTMPro : MonoBehaviour
{
    [SerializeField] TMP_FontAsset _fontMaterial_ENG;
    [SerializeField] TMP_FontAsset _fontMaterial_JPN;
    [SerializeField] Font _font_ENG;
    [SerializeField] Font _font_JPN;
    [SerializeField] int _fontSize_ENG = -1;
    [SerializeField] int _fontSize_JPN = -1;
    [TextArea] public string _text_ENG;
    [TextArea] public string _text_JPN;

    TextMeshProUGUI _textMeshPro = null;
    Text _text = null;

    int _updateFrame = 50;
    int _timerCount = 0;

    void ChangeText()
    {
        if (_textMeshPro == null && _text == null)
        {
            return;
        }

        if (ContinuousController.instance != null)
        {
            switch (ContinuousController.instance.language)
            {
                case Language.ENG:
                    if (_textMeshPro != null)
                    {
                        if (_fontMaterial_ENG != null)
                        {
                            _textMeshPro.font = _fontMaterial_ENG;
                        }

                        if (!string.IsNullOrEmpty(_text_ENG))
                        {
                            _textMeshPro.text = _text_ENG;
                        }

                        if (_fontSize_ENG > 0)
                        {
                            _textMeshPro.fontSize = _fontSize_ENG;
                        }
                    }

                    else if (_text != null)
                    {
                        if (_font_ENG != null)
                        {
                            _text.font = _font_ENG;
                        }

                        if (!string.IsNullOrEmpty(_text_ENG))
                        {
                            _text.text = _text_ENG;
                        }

                        if (_fontSize_ENG > 0)
                        {
                            _text.fontSize = _fontSize_ENG;
                        }
                    }

                    break;

                case Language.JPN:
                    if (_textMeshPro != null)
                    {
                        if (_fontMaterial_ENG != null)
                        {
                            _textMeshPro.font = _fontMaterial_JPN;
                        }

                        if (!string.IsNullOrEmpty(_text_JPN))
                        {
                            _textMeshPro.text = _text_JPN;
                        }

                        if (_fontSize_JPN > 0)
                        {
                            _textMeshPro.fontSize = _fontSize_JPN;
                        }
                    }

                    else if (_text != null)
                    {
                        if (_font_JPN != null)
                        {
                            _text.font = _font_JPN;
                        }

                        if (!string.IsNullOrEmpty(_text_JPN))
                        {
                            _text.text = _text_JPN;
                        }

                        if (_fontSize_JPN > 0)
                        {
                            _text.fontSize = _fontSize_JPN;
                        }
                    }

                    break;
            }
        }
    }

    void Awake()
    {
        _textMeshPro = GetComponent<TextMeshProUGUI>();
        _text = GetComponent<Text>();
    }

    private void OnEnable()
    {
        ChangeText();
    }

    void Update()
    {
        #region update per a few frames
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

        ChangeText();
    }
}
