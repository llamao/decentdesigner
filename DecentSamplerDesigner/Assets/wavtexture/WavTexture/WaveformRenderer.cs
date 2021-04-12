// WavTexture - Audio waveform to texture converter
// https://github.com/keijiro/WavTexture

using UnityEngine;
using UnityEngine.UI;

namespace WavTexture
{
    /// Basic waveform graph renderer.
    public class WaveformRenderer : MonoBehaviour
    {
        #region Editable properties
        public float startTime = 0;
        public float zoom = 1000;
        public int selectedSample = 100;
        public float selectedNormalized = 0;

        public float xScale = 600;
        public float yScale = 140;

        public InputField startInput;
        public InputField durationInput;

        public float samplePosNormalized;

        public RectTransform thisRect;
        public RectTransform contianerRect;

        /// Source waveform.
        public WavTexture wavTexture {
            get { return _wavTexture; }
            set { _wavTexture = value; }
        }

        [SerializeField, Tooltip("Source waveform.")]
        WavTexture _wavTexture;

        /// Specifies which channel in the wavTexture to be rendered.
        public int channel {
            get { return _channel; }
            set { _channel = value; }
        }

        [SerializeField, Tooltip("Specifies which channel in the wavTexture to be rendered.")]
        int _channel;

        /// Synchronizes the position to this audio source.
        //public AudioSource positionSource {
        //    get { return _positionSource; }
        //    set { _positionSource = value; }
        //}

        //[SerializeField, Tooltip("Synchronized the position to this audio source.")]
        //AudioSource _positionSource;

        /// Color of the graph line.
        public Color color {
            get { return _color; }
            set { _color = value; }
        }

        [SerializeField, Tooltip("Color of the graph line.")]
        Color _color = new Color(1, 1, 1, 0.5f);

        /// Graph resolution (the number of vertices in the graph).
        public int resolution {
            get { return _resolution; }
            set { _resolution = value; OnValidate(); }
        }

        [SerializeField, Tooltip("Graph resolution (the number of vertices in the graph).")]
        int _resolution = 1024;

        #endregion

        #region Internal resources

        public Shader _shader;

        #endregion

        #region Private members

        Mesh _mesh;
        Material _material;
        MaterialPropertyBlock _propertyBlock;

        void Setup()
        {
            if (_mesh == null)
            {
                // Simple straight line mesh.
                var vertices = new Vector3[_resolution];
                for (var i = 0; i < _resolution; i++)
                    vertices[i] = new Vector3((float)i / (_resolution - 1), 0, 0);

                var indices = new int[(_resolution - 1) * 2];
                for (var i = 0; i < _resolution - 1; i++)
                {
                    indices[i * 2] = i;
                    indices[i * 2 + 1] = i + 1;
                }

                _mesh = new Mesh();
                _mesh.hideFlags = HideFlags.DontSave;
                _mesh.vertices = vertices;
                _mesh.SetIndices(indices, MeshTopology.Lines, 0);
                _mesh.bounds = new Bounds(Vector3.zero, Vector3.one);
                _mesh.UploadMeshData(true);
            }

            if (_material == null)
            {
                _material = new Material(_shader);
                _propertyBlock = new MaterialPropertyBlock();
            }
        }

        #endregion

        #region MonoBehaviour functions

        void OnValidate()
        {
            _resolution = Mathf.Clamp(_resolution, 2, 65535);

            if (_mesh != null)
            {
                Destroy(_mesh);
                _mesh = null;
            }
        }

        void OnDestroy()
        {
            if (_mesh != null)
                Destroy(_mesh);

            if (_material != null)
                Destroy(_material);
        }

        void Update()
        {
            Setup();

            //var time = _positionSource != null ? _positionSource.time : Time.time;

            float clipLength = _wavTexture._length * 4;
            startTime = Mathf.Clamp(startTime, 0, clipLength - zoom);
            var time = startTime;



            zoom = Mathf.Clamp(zoom, 10, Mathf.Clamp(clipLength - startTime, 0, clipLength));

            _propertyBlock.SetTexture("_WavTex", _wavTexture.GetTexture(_channel));
            _propertyBlock.SetFloat("_StartTime", time);
            //_propertyBlock.SetFloat("_Duration", (float)_wavTexture.sampleRate / 60);
            _propertyBlock.SetFloat("_Duration", zoom);
            _propertyBlock.SetColor("_Color", _color);
            _propertyBlock.SetFloat("_ScaleX", xScale);
            _propertyBlock.SetFloat("_ScaleY", yScale);

            Graphics.DrawMesh(
                _mesh, transform.localToWorldMatrix, _material,
                gameObject.layer, null, 0, _propertyBlock
            );

            samplePosNormalized = Mathf.InverseLerp(startTime, startTime + zoom, selectedSample);

            thisRect.localScale = contianerRect.rect.size;

            if (!durationInput.isFocused)
            {
                durationInput.SetTextWithoutNotify(zoom.ToString("f0"));
            }

            if (!startInput.isFocused)
            {
                startInput.SetTextWithoutNotify(time.ToString("f0"));
            }

            selectedNormalized = selectedSample / clipLength;

            

        }

        public void SetStartTime()
        {
            startTime = int.Parse(startInput.text);
            zoom = Mathf.Clamp(zoom, 10, (_wavTexture._length * 4) - startTime);
        }

        public void SetDurationTime()
        {
            zoom = int.Parse(durationInput.text);
        }

        public void ResetViewer()
        {
            zoom = Mathf.Infinity;
            startTime = 0;
        }

        #endregion

        private void OnGUI()
        {
            //GUI.Label(new Rect(100, 100, 200, 30), ViewportPosFromSample(selectedSample).ToString());
            //GUI.Box(new Rect(ViewportPosFromSample(selectedSample), 100,10,100),"");


            //GUI.Box(new Rect(ViewportPosFromSample(SampleNumFromScreenPos(Input.mousePosition, leftEdge, rightEdge)), 100,10,100),"");



            //GUI.Box(new Rect(rightEdge, 100,5,100),"");
            //GUI.Box(new Rect(leftEdge, 100,5,100),"");
        }

        int SamplePosNormalised(int sampleNum)
        {

            float n = Mathf.InverseLerp(startTime, startTime + zoom, sampleNum);

            //float r = Mathf.Lerp(leftEdge, rightEdge, n);
            
            return (int)n;
        }

        public int SampleNumFromScreenPos(Vector2 pos, float left, float right)
        {
            if (pos.x < left)
            {
                return 0;
            }
            if (pos.x > right)
            {
                return 1;
            }

            // normalise pos.x
            float n = Mathf.InverseLerp(left, right, pos.x);

            float s = Mathf.Lerp(startTime, startTime + zoom, n);

            return (int)s;
        }

        public float NormalisedPos(Vector2 pos, float left, float right)
        {
            return SamplePosNormalised(SampleNumFromScreenPos(pos, left, right));
            //return Mathf.InverseLerp(startTime, startTime + zoom, SampleNumFromScreenPos(pos, left, right));
        }

        float Remap(float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
    }
}
