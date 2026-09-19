using System;
using System.Linq;
using TMPro;
using UnityEngine;

namespace TweenableObject
{
    [DefaultExecutionOrder(100)]
    [ExecuteInEditMode]
    [RequireComponent(typeof(TextMeshPro))]
    public class BendText : MonoBehaviour
    {
        [SerializeField] private bool isActive;
        [SerializeField] private bool flip;
        [SerializeField] private float radius;
        [SerializeField, Range(0, 360)] private float spread;

        private RectTransform _rectTransform;
        private TextMeshPro _textMeshPro;
        private bool _isActive;
        private bool _flip;
        private float _lastRadius;
        private float _lastSpread;
        private Rect _lastRect;

        private float _maxY;
        private float _minY;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _textMeshPro = GetComponent<TextMeshPro>();
        }

        private void OnEnable()
        {
            ReCalculate();
        }

        private void OnDisable()
        {
            _textMeshPro.ForceMeshUpdate();
        }

        private void Update()
        {
            // EditModeでも動作するようUpdateで更新を検出
            ReCalculate();
        }

        // パラメータやTextMeshProの変更を監視し，変更があればメッシュの再変形を行う
        public void ReCalculate()
        {
            if (!_textMeshPro.havePropertiesChanged && _isActive == isActive && _flip == flip &&
                _lastRadius == radius && _lastSpread == spread && _rectTransform.rect == _lastRect) return;

            _isActive = isActive;
            _flip = flip;
            _lastRadius = radius;
            _lastSpread = spread;
            _lastRect = _rectTransform.rect;

            if (isActive) SetCurve();
            else _textMeshPro.ForceMeshUpdate();

            var enu = _textMeshPro.textInfo.meshInfo.SelectMany(m => m.vertices).Select(v => v.y).ToArray();
            _minY = enu.Min();
            _maxY = enu.Max();
        }

        // Textに対しメッシュの変形を実行
        private void SetCurve()
        {
            _textMeshPro.ForceMeshUpdate();

            var textInfo = _textMeshPro.textInfo;
            var characterCount = textInfo.characterCount;

            var rect = _rectTransform.rect;
            var minX = rect.xMin + _textMeshPro.margin.x;
            var maxX = rect.xMax - _textMeshPro.margin.y;

            for (var i = 0; i < characterCount; i++)
            {
                if (!textInfo.characterInfo[i].isVisible) continue;

                var vertexIndex = textInfo.characterInfo[i].vertexIndex;
                var materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
                var vertices = textInfo.meshInfo[materialIndex].vertices;

                // 文字の中点座標を計算
                Vector3 offsetToMidBaseline =
                    new Vector2((vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2,
                        textInfo.characterInfo[i].baseLine);

                // 変形後の文字の中心位置を計算
                var val = (offsetToMidBaseline.x - minX) / (maxX - minX);
                var pos0 = CalcPositionFromCircle(val);
                var pos1 = CalcPositionFromCircle(val + 0.0001f);

                var rotation = Quaternion.FromToRotation(Vector3.right, (pos1 - pos0).normalized);

                // 各文字の4頂点への処理
                for (var j = 0; j < 4; ++j)
                {
                    var point = vertices[vertexIndex + j];

                    // 中点からの相対ベクトルを算出
                    point -= offsetToMidBaseline;

                    // 移動と回転を適用
                    point = rotation * point + new Vector3(pos0.x, offsetToMidBaseline.y, pos0.z);

                    vertices[vertexIndex + j] = point;
                }
            }

            _textMeshPro.UpdateVertexData();
        }

        // 0~1のvalueに対し，円周上の位置を返す
        private Vector3 CalcPositionFromCircle(float val)
        {
            var angle = (float)(1.5 * Math.PI + (val - 0.5f) * spread * Mathf.Deg2Rad);
            var x = radius * Mathf.Cos(angle);
            var z = (flip ? -1 : 1) * radius * (1 + Mathf.Sin(angle));
            return new Vector3(x, 0, z);
        }

        private readonly Color _rectColor = new Color(0.98f, 0.498f, 0.196f);
        private readonly Color _circleColor = new Color(0.176f, 0.784f, 1);

        private void OnDrawGizmosSelected()
        {
            var old = Gizmos.matrix;
            var transform1 = transform;

            var lossyScale = transform1.lossyScale;
            Gizmos.matrix = Matrix4x4.TRS(transform1.position + (flip ? -1 : 1) * transform1.forward * radius * lossyScale.z, transform1.rotation, lossyScale);

            const int step = 5;
            var rangeMin = (flip ? 0 : 180) - spread / 2;
            var rangeMax = (flip ? 0 : 180) + spread / 2;

            var from = new Vector3(radius * Mathf.Sin(rangeMin * Mathf.Deg2Rad), 0, radius * Mathf.Cos(rangeMin * Mathf.Deg2Rad));

            for (var i = rangeMin; i <= 360 + rangeMin; i += step)
            {
                // 境界部での丸め込み
                var deg = i;
                if (deg - rangeMin > 0 && deg - rangeMin <= step) deg = rangeMin;
                else if (deg - rangeMax > 0 && deg - rangeMax <= step) deg = rangeMax;

                var to = new Vector3(radius * Mathf.Sin(deg * Mathf.Deg2Rad), 0, radius * Mathf.Cos(deg * Mathf.Deg2Rad));

                if (deg == rangeMin || deg == rangeMax)
                {
                    Gizmos.color = _rectColor;
                    Gizmos.DrawLine(new Vector3(to.x, _minY, to.z), new Vector3(to.x, _maxY, to.z));
                    Gizmos.DrawLine(new Vector3(to.x, _minY, to.z), new Vector3(to.x, _maxY, to.z));
                }

                if (from != to && rangeMin <= deg && deg <= rangeMax)
                {
                    Gizmos.color = _rectColor;
                    Gizmos.DrawLine(new Vector3(from.x, _minY, from.z), new Vector3(to.x, _minY, to.z));
                    Gizmos.DrawLine(new Vector3(from.x, _maxY, from.z), new Vector3(to.x, _maxY, to.z));
                }
                else
                {
                    Gizmos.color = _circleColor;
                    Gizmos.DrawLine(from, to);
                }
                from = to;
            }

            Gizmos.matrix = old;
        }
    }
}