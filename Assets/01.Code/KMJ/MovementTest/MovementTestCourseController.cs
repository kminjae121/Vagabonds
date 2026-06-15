using _Code.EntityCompo.Move;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Code.MovementTest
{
    public class MovementTestCourseController : MonoBehaviour
    {
        private const string GeneratedRootName = "Generated Movement Test Course";

        [SerializeField] private PlayerMoveCompo _movement;
        [SerializeField] private Transform _playerStart;
        [SerializeField] private bool _buildCourseOnAwake = true;
        [SerializeField] private bool _disableLegacyRunway = true;
        [SerializeField] private Texture2D _prototypeTexture;
        [SerializeField, Min(0.01f)] private float _textureTilesPerMeter = 0.25f;
        [SerializeField] private Vector3 _startPosition = new(0f, 1.05f, 0f);
        [SerializeField] private Vector3 _finishPosition = new(0f, 2f, 260f);
        [SerializeField] private float _startSpeedThreshold = 1f;
        [SerializeField] private float _startDistanceThreshold = 0.75f;

        private Transform _generatedRoot;
        private Rigidbody _playerRigidbody;
        private Vector3 _lastPlayerPosition;
        private Vector3 _initialPlayerPosition;
        private float _startedAt;
        private float _finishedAt;
        private float _maxSpeed;
        private float _speedSampleTotal;
        private float _distanceTravelled;
        private int _speedSampleCount;
        private bool _hasStarted;
        private bool _isFinished;

        public bool HasStarted => _hasStarted;
        public bool IsFinished => _isFinished;
        public float ElapsedTime => GetElapsedTime();
        public float FinishTime => _isFinished ? _finishedAt - _startedAt : 0f;
        public float MaxSpeed => _maxSpeed;
        public float AverageSpeed => _speedSampleCount > 0 ? _speedSampleTotal / _speedSampleCount : 0f;
        public float DistanceTravelled => _distanceTravelled;
        public PlayerMoveCompo Movement => _movement;

        private void Awake()
        {
            if (_movement == null)
                _movement = FindFirstObjectByType<PlayerMoveCompo>();

            if (_movement != null)
            {
                _playerRigidbody = _movement.GetComponentInChildren<Rigidbody>();
                _initialPlayerPosition = _movement.transform.position;
            }
            else
            {
                _initialPlayerPosition = _startPosition;
            }

            if (_buildCourseOnAwake)
                BuildCourse();

            ResetRunState();
        }

        private void Update()
        {
            if (_movement == null)
                return;

            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                ResetCourse();

            UpdateRunState();
        }

        public void FinishRun()
        {
            if (!_hasStarted || _isFinished)
                return;

            _isFinished = true;
            _finishedAt = Time.time;
        }

        public void ResetCourse()
        {
            if (_movement == null)
                return;

            Transform playerTransform = _movement.transform;
            playerTransform.SetPositionAndRotation(GetStartPosition(), Quaternion.identity);

            if (_playerRigidbody == null)
                _playerRigidbody = _movement.GetComponentInChildren<Rigidbody>();

            if (_playerRigidbody != null)
            {
                _playerRigidbody.linearVelocity = Vector3.zero;
                _playerRigidbody.angularVelocity = Vector3.zero;
            }

            _movement.ClearBloodStacks();
            ResetRunState();
        }

        private void UpdateRunState()
        {
            Vector3 currentPosition = _movement.transform.position;
            float speed = _movement.CurrentHorizontalSpeed;

            if (!_hasStarted)
            {
                float startDistance = Vector3.Distance(Flatten(currentPosition), Flatten(GetStartPosition()));
                if (speed >= _startSpeedThreshold || startDistance >= _startDistanceThreshold)
                    StartRun(currentPosition);
                else
                    _lastPlayerPosition = currentPosition;
            }

            if (!_hasStarted || _isFinished)
                return;

            Vector3 frameDistance = Flatten(currentPosition) - Flatten(_lastPlayerPosition);
            _distanceTravelled += frameDistance.magnitude;
            _maxSpeed = Mathf.Max(_maxSpeed, speed);
            _speedSampleTotal += speed;
            _speedSampleCount++;
            _lastPlayerPosition = currentPosition;
        }

        private void StartRun(Vector3 currentPosition)
        {
            _hasStarted = true;
            _isFinished = false;
            _startedAt = Time.time;
            _finishedAt = 0f;
            _lastPlayerPosition = currentPosition;
            _maxSpeed = _movement.CurrentHorizontalSpeed;
            _speedSampleTotal = 0f;
            _speedSampleCount = 0;
            _distanceTravelled = 0f;
        }

        private void ResetRunState()
        {
            _hasStarted = false;
            _isFinished = false;
            _startedAt = 0f;
            _finishedAt = 0f;
            _maxSpeed = 0f;
            _speedSampleTotal = 0f;
            _speedSampleCount = 0;
            _distanceTravelled = 0f;
            _lastPlayerPosition = _movement != null ? _movement.transform.position : GetStartPosition();
        }

        private float GetElapsedTime()
        {
            if (!_hasStarted)
                return 0f;

            return (_isFinished ? _finishedAt : Time.time) - _startedAt;
        }

        private Vector3 GetStartPosition()
        {
            if (_playerStart != null)
                return _playerStart.position;

            return _initialPlayerPosition == Vector3.zero ? _startPosition : _initialPlayerPosition;
        }

        private void BuildCourse()
        {
            if (_disableLegacyRunway)
                DisableLegacyRunway();

            Transform existingRoot = transform.Find(GeneratedRootName);
            if (existingRoot != null)
                Destroy(existingRoot.gameObject);

            _generatedRoot = new GameObject(GeneratedRootName).transform;
            _generatedRoot.SetParent(transform, false);

            CreateBlock("Main Practice Floor", new Vector3(0f, -0.16f, 130f), new Vector3(70f, 0.2f, 290f));
            CreateBlock("Start Pad", new Vector3(0f, -0.08f, 0f), new Vector3(18f, 0.24f, 16f));
            CreateBlock("Wide Acceleration Field", new Vector3(0f, -0.06f, 28f), new Vector3(34f, 0.24f, 40f));

            CreateBlock("Bunnyhop Practice Lane", new Vector3(0f, -0.04f, 76f), new Vector3(26f, 0.24f, 42f));
            for (int i = 0; i < 5; i++)
            {
                float x = i % 2 == 0 ? -8f : 8f;
                float z = 58f + i * 9f;
                CreateBlock($"Optional Bunnyhop Marker {i + 1}", new Vector3(x, 0.02f, z), new Vector3(8f, 0.12f, 3f));
            }

            CreateBlock("Open Steering Field", new Vector3(4f, -0.04f, 132f), new Vector3(52f, 0.24f, 58f));
            CreateBlock("Soft Curve Guide A", new Vector3(-16f, 0.04f, 112f), new Vector3(8f, 0.18f, 8f));
            CreateBlock("Soft Curve Guide B", new Vector3(18f, 0.04f, 132f), new Vector3(8f, 0.18f, 8f));
            CreateBlock("Soft Curve Guide C", new Vector3(-12f, 0.04f, 152f), new Vector3(8f, 0.18f, 8f));

            CreateBlock("Left Wall Kick Practice Floor", new Vector3(-14f, -0.03f, 190f), new Vector3(28f, 0.24f, 42f));
            CreateBlock("Left Wall Kick Surface", new Vector3(-28f, 2f, 190f), new Vector3(0.35f, 4.2f, 42f));
            CreateBlock("Left Wall Kick Landing Field", new Vector3(4f, -0.02f, 198f), new Vector3(34f, 0.24f, 36f));

            CreateBlock("Right Wall Kick Practice Floor", new Vector3(14f, -0.03f, 226f), new Vector3(28f, 0.24f, 42f));
            CreateBlock("Right Wall Kick Surface", new Vector3(28f, 2f, 226f), new Vector3(0.35f, 4.2f, 42f));
            CreateBlock("Right Wall Kick Landing Field", new Vector3(-4f, -0.02f, 234f), new Vector3(34f, 0.24f, 36f));

            CreateBlock("Free Combo Yard", new Vector3(0f, -0.01f, 252f), new Vector3(60f, 0.24f, 30f));
            CreateBlock("Finish Landing", new Vector3(0f, 0f, 260f), new Vector3(26f, 0.28f, 18f));
            CreateFinishTrigger();
        }

        private void DisableLegacyRunway()
        {
            GameObject legacyRunway = GameObject.Find("Movement Test Runway");
            if (legacyRunway != null)
                legacyRunway.SetActive(false);
        }

        private void CreateBlock(string blockName, Vector3 position, Vector3 scale)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = blockName;
            block.transform.SetParent(_generatedRoot, false);
            block.transform.position = position;
            block.transform.localScale = scale;

            if (block.TryGetComponent(out MeshRenderer renderer))
                renderer.sharedMaterial = CreatePrototypeMaterial(blockName, scale);
        }

        private void CreateFinishTrigger()
        {
            GameObject trigger = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trigger.name = "Finish Trigger";
            trigger.transform.SetParent(_generatedRoot, false);
            trigger.transform.position = _finishPosition;
            trigger.transform.localScale = new Vector3(14f, 4f, 3f);

            if (trigger.TryGetComponent(out MeshRenderer renderer))
                renderer.sharedMaterial = CreatePrototypeMaterial("Finish Trigger", trigger.transform.localScale);

            if (trigger.TryGetComponent(out Collider triggerCollider))
                triggerCollider.isTrigger = true;

            MovementTestFinishTrigger finishTrigger = trigger.AddComponent<MovementTestFinishTrigger>();
            finishTrigger.Initialize(this);
        }

        private Material CreatePrototypeMaterial(string materialName, Vector3 scale)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");

            Material material = new(shader)
            {
                name = materialName,
                color = Color.white,
                hideFlags = HideFlags.DontSave
            };

            ApplyPrototypeTexture(material, GetTextureTiling(scale));
            return material;
        }

        private void ApplyPrototypeTexture(Material material, Vector2 tiling)
        {
            if (_prototypeTexture == null)
                return;

            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", _prototypeTexture);
                material.SetTextureScale("_BaseMap", tiling);
            }

            if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex", _prototypeTexture);
                material.SetTextureScale("_MainTex", tiling);
            }
        }

        private Vector2 GetTextureTiling(Vector3 scale)
        {
            return new Vector2(
                Mathf.Max(1f, Mathf.Abs(scale.x) * _textureTilesPerMeter),
                Mathf.Max(1f, Mathf.Abs(scale.z) * _textureTilesPerMeter));
        }

        private static Vector3 Flatten(Vector3 value)
        {
            value.y = 0f;
            return value;
        }
    }

    public class MovementTestFinishTrigger : MonoBehaviour
    {
        private MovementTestCourseController _course;

        public void Initialize(MovementTestCourseController course)
        {
            _course = course;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_course == null)
                return;

            PlayerMoveCompo movement = other.GetComponentInParent<PlayerMoveCompo>();
            if (movement == null || movement != _course.Movement)
                return;

            _course.FinishRun();
        }
    }
}
