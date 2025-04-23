using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

[ExecuteAlways]
public class LevelTapAway : MonoBehaviour
{
    private const string TapAwayDragTutorCompletedKey = "TapAwayDragTutorCompleted";

    private const string TapAwayTapTutorCompletedKey = "TapAwayTapTutorCompleted";
    [SerializeField] private Block _blockPrefab;
    [SerializeField] [Range(1, 50)] private int _sizeX = 5;
    [SerializeField] [Range(1, 50)] private int _sizeY = 5;
    [SerializeField] [Range(1, 50)] private int _sizeZ = 5;
    [SerializeField] [Range(0, 3000)] private int _randomSeed;
    [SerializeField] [Range(1, 100000)] private int _countMoves = 1;
    [SerializeField] [Range(0.5f, 3f)] private float _addScale = 1f;

    private readonly MovesController _movesController = new();

    private bool _autoplayStart;
    private List<Block> _blocks = new();
    private Camera _camera;
    private int _countRightTapsForAds;

    private bool _dragTutorActive;
    private Vector2 _firstPointerPos;
    private ICameraItem _gameCamera;
    private bool _isReverse;
    private float _levelScale;
    private bool _levelStarted;

    private Map _map;
    private float _pointerRotateDragSum;
    private Vector2 _pointerRotatorLastPos;

    private bool _pressed;
    private float _pressedTime;
    private Touch[] _prevTouches;
    private int _prevTouchesCount;
    private int _rightClickRage;
    private bool _rotateState;
    private int[] _tapAwayTapsForAds;
    private int _tapsForAdsInd;
    private bool _tapTutorActive;
    private Quaternion _targetRotation;
    private bool _useMultiTouch;

    public int BlocksCount => _blocks.Count;
    public int BlocksLeft => _blocks.Count(r => !r.Completed);
    public int MovesLeft => _movesController.MovesLeft;

    public bool Completed { get; private set; }
    public bool Failed { get; private set; }

    private bool TapAwayDragTutorCompleted
    {
        get => PlayerPrefs.GetInt(TapAwayDragTutorCompletedKey, 0) > 0;
        set => PlayerPrefs.SetInt(TapAwayDragTutorCompletedKey, value ? 1 : 0);
    }

    private bool TapAwayTapTutorCompleted
    {
        get => PlayerPrefs.GetInt(TapAwayTapTutorCompletedKey, 0) > 0;
        set => PlayerPrefs.SetInt(TapAwayTapTutorCompletedKey, value ? 1 : 0);
    }

    private void Update()
    {
        if (!Application.IsPlaying(this))
        {
            ValidateSteps();
            return;
        }

        if (!_levelStarted)
        {
            return;
        }

        var deltaTime = Time.deltaTime;
        if (Failed || Completed)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, _targetRotation, deltaTime * 13f);
            return;
        }

        Vector2 GetPointerViewMousePos()
        {
            var pos = _camera.ScreenToViewportPoint(Input.mousePosition);
            pos.x *= _camera.aspect;
            return pos;
        }

        Vector2 GetPointerViewPos(Vector3 pointerScreenPos)
        {
            var pos = _camera.ScreenToViewportPoint(pointerScreenPos);
            pos.x *= _camera.aspect;
            return pos;
        }

        var touchesCount = Input.touchCount;
        var touches = Input.touches;
        _useMultiTouch = ((touchesCount > 0 && _useMultiTouch) || touchesCount >= 2) && !_dragTutorActive;
        if (touchesCount >= 2 && !_dragTutorActive)
        {
            var move = Vector2.zero;
            var centerPoint = Vector2.zero;
            var centerPointViewport = Vector3.zero;
            var draggedTouchesCount = 0;
            for (var i = 0; i < touchesCount; i++)
            {
                var touch = touches[i];
                for (var j = 0; j < _prevTouchesCount; j++)
                {
                    var prevTouch = _prevTouches[j];
                    if (touch.fingerId == prevTouch.fingerId)
                    {
                        centerPointViewport += _camera.ScreenToViewportPoint(touch.position);
                        var touchPos = GetPointerViewPos(touch.position);
                        var touchPrevPos = GetPointerViewPos(prevTouch.position);
                        var touchDrag = touchPos - touchPrevPos;
                        move += touchDrag;
                        centerPoint += touchPos;
                        draggedTouchesCount++;
                        break;
                    }
                }
            }

            if (draggedTouchesCount > 1)
            {
                centerPoint /= draggedTouchesCount;
                centerPointViewport /= draggedTouchesCount;
                var scale = 0f;
                for (var i = 0; i < touchesCount; i++)
                {
                    var touch = touches[i];
                    for (var j = 0; j < _prevTouchesCount; j++)
                    {
                        var prevTouch = _prevTouches[j];
                        if (touch.fingerId == prevTouch.fingerId)
                        {
                            var touchPos = GetPointerViewPos(touch.position);
                            var touchPrevPos = GetPointerViewPos(prevTouch.position);
                            var distNow = Vector2.Distance(touchPos, centerPoint);
                            var distPrev = Vector2.Distance(touchPrevPos, centerPoint);
                            scale += distPrev / distNow;
                            break;
                        }
                    }
                }

                scale /= draggedTouchesCount;
                move /= draggedTouchesCount;
                _gameCamera.Scale(scale, centerPointViewport);
                _gameCamera.Move(move * -1f);
            }
        }

        _prevTouchesCount = touchesCount;
        _prevTouches = touches;

        if (!_pressed && Input.GetMouseButtonDown(0) && !UIUtil.IsPointerOverUIObject(Input.mousePosition))
        {
            _pressed = true;
            _pressedTime = 0f;
            _rotateState = false;
            _pointerRotateDragSum = 0f;
            _firstPointerPos = GetPointerViewMousePos();
        }

        if (_pressed && !_useMultiTouch)
        {
            _pressedTime += deltaTime;
            if (!_rotateState)
            {
                if (_pressedTime > 0.5f)
                {
                    _rotateState = true;
                    _pointerRotatorLastPos = GetPointerViewMousePos();
                }
                else
                {
                    var pointerPos = GetPointerViewMousePos();
                    var way = pointerPos - _firstPointerPos;
                    if (way.sqrMagnitude > 0.01f * 0.01f)
                    {
                        _rotateState = true;
                        _pointerRotatorLastPos = pointerPos;
                    }
                }
            }
        }

        if (_rotateState && !_useMultiTouch)
        {
            var pointerPos = GetPointerViewMousePos();
            var pointerWay = pointerPos - _pointerRotatorLastPos;
            var angles = new Vector3(pointerWay.y, -pointerWay.x, 0f) * 300f;
            _targetRotation = Quaternion.Euler(angles) * _targetRotation;
            _pointerRotatorLastPos = pointerPos;

            if (_dragTutorActive)
            {
                _pointerRotateDragSum += pointerWay.magnitude;
            }
        }

        transform.rotation = Quaternion.Lerp(transform.rotation, _targetRotation, deltaTime * 15f);

        if (_useMultiTouch)
        {
            _pressed = false;
            _rotateState = false;
            _pointerRotateDragSum = 0f;
        }
        else if (_pressed && Input.GetMouseButtonUp(0))
        {
            _pressed = false;
            if (!_dragTutorActive && !Failed && !Completed && _movesController.HasMoves() && !_rotateState &&
                Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hitInfo))
            {
                var rb = hitInfo.collider.attachedRigidbody;
                if (rb && rb.TryGetComponent<Block>(out var block))
                {
                    var tapResult = block.Tap(ref _rightClickRage, ref _isReverse, null);
                    if (tapResult != TapOnBlockResult.NotTapped)
                    {
                        if (!_tapTutorActive || tapResult == TapOnBlockResult.RightClick)
                        {
                            if (tapResult == TapOnBlockResult.RightClick)
                            {
                                _countRightTapsForAds++;
                                var tapsForAds = _tapAwayTapsForAds == null || _tapAwayTapsForAds.Length == 0
                                    ? int.MaxValue
                                    : _tapAwayTapsForAds[_tapsForAdsInd];
                                if (_countRightTapsForAds >= tapsForAds)
                                {
                                    var tapAwayNotShowAdsIfThereAreBlocksLeft =
                                        GameConfig.RemoteConfig.tapAwayNotShowAdsIfThereAreBlocksLeft;
                                    var blocksLeft = _blocks.Count(b => !b.Completed);
                                    if (blocksLeft > tapAwayNotShowAdsIfThereAreBlocksLeft)
                                    {
                                        HyperKit.Ads.ShowInterstitial("ad_inter_tap_away", shown =>
                                        {
                                            if (shown)
                                            {
                                                _countRightTapsForAds = 0;
                                                _tapsForAdsInd = Mathf.Min(_tapsForAdsInd + 1,
                                                    _tapAwayTapsForAds.Length - 1);
                                            }
                                        });
                                    }
                                }
                            }

                            _movesController.DecreaseMoves();
                            CheckCompleteOrFail();
                            if (_tapTutorActive)
                            {
                                TapAwayTapTutorCompleted = true;
                                _tapTutorActive = false;
                                CheckTutor();
                            }
                        }
                    }
                }
            }

            _rotateState = false;
            if (_dragTutorActive && _pointerRotateDragSum > GameConfig.RemoteConfig.tapAwayDragLimit)
            {
                TapAwayDragTutorCompleted = true;
                _dragTutorActive = false;
                UITapAwayTutorial.Instance.HideTutorial();
                CheckTutor();
            }

            _pointerRotateDragSum = 0f;
        }
    }

    public void AutoPlay()
    {
        if (!Application.IsPlaying(this) || _autoplayStart)
        {
            return;
        }

        _autoplayStart = true;
        StartCoroutine(Co_AutoPlay());

        IEnumerator Co_AutoPlay()
        {
            bool IsAvalible()
            {
                return _blocks.Count(b => !b.Completed) > 0 && _movesController.HasMoves();
            }

            while (IsAvalible())
            {
                foreach (var block in _blocks)
                {
                    if (!IsAvalible())
                    {
                        _autoplayStart = false;
                        yield break;
                    }

                    if (Physics.Raycast(block.transform.position, block.transform.forward))
                    {
                        continue;
                    }

                    var tapResult = block.Tap(ref _rightClickRage, ref _isReverse, null);
                    if (tapResult != TapOnBlockResult.NotTapped)
                    {
                        if (!_tapTutorActive || tapResult == TapOnBlockResult.RightClick)
                        {
                            _movesController.DecreaseMoves();
                            CheckCompleteOrFail();
                            yield return null;
                            yield return null;
                        }
                    }
                }
            }

            _autoplayStart = false;
        }
    }

    private Block[,,] GetBlocksGrid(List<Block> blocks)
    {
        var min = Vector3Int.one * int.MaxValue;
        var max = Vector3Int.one * int.MinValue;
        foreach (var block in blocks)
        {
            var pos = block.transform.localPosition;
            var posInt = new Vector3Int((int)pos.x, (int)pos.y, (int)pos.z);
            min.x = Mathf.Min(min.x, posInt.x);
            min.y = Mathf.Min(min.y, posInt.y);
            min.z = Mathf.Min(min.z, posInt.z);
            max.x = Mathf.Max(max.x, posInt.x);
            max.y = Mathf.Max(max.y, posInt.y);
            max.z = Mathf.Max(max.z, posInt.z);
        }

        var size = max - min + Vector3Int.one;
        var grid = new Block[size.x, size.y, size.z];
        foreach (var block in blocks)
        {
            var pos = block.transform.localPosition;
            var posInt = new Vector3Int((int)pos.x, (int)pos.y, (int)pos.z) - min;
            grid[posInt.x, posInt.y, posInt.z] = block;
        }

        return grid;
    }

    private void GenerateDirections(List<Block> blocks, int seed)
    {
        blocks = blocks.ToList();
        var grid = GetBlocksGrid(blocks);
        var size = new Vector3Int(grid.GetLength(0), grid.GetLength(1), grid.GetLength(2));
        var tempBlocksArraySize =
            size.x < size.y && size.x < size.z ? size.y * size.z :
            size.y < size.x && size.y < size.z ? size.x * size.z : size.x * size.y;
        var tempBlocks = new Vector3Int[tempBlocksArraySize];
        var random = new Random(seed);
        while (blocks.Count > 0)
        {
            for (var i = 0; i < 6; i++)
            {
                if (blocks.Count == 0)
                {
                    break;
                }

                var dir = (BlockDirection)i;
                var countBlocks = 0;
                switch (dir)
                {
                    case BlockDirection.up:
                        for (var x = 0; x < size.x; x++)
                        {
                            for (var z = 0; z < size.z; z++)
                            {
                                for (var y = size.y - 1; y >= 0; y--)
                                {
                                    var block = grid[x, y, z];
                                    if (block == null)
                                    {
                                        continue;
                                    }

                                    tempBlocks[countBlocks] = new Vector3Int(x, y, z);
                                    countBlocks++;
                                    break;
                                }
                            }
                        }

                        break;
                    case BlockDirection.forward:
                        for (var x = 0; x < size.x; x++)
                        {
                            for (var y = 0; y < size.y; y++)
                            {
                                for (var z = size.z - 1; z >= 0; z--)
                                {
                                    var block = grid[x, y, z];
                                    if (block == null)
                                    {
                                        continue;
                                    }

                                    tempBlocks[countBlocks] = new Vector3Int(x, y, z);
                                    countBlocks++;
                                    break;
                                }
                            }
                        }

                        break;
                    case BlockDirection.left:
                        for (var z = 0; z < size.z; z++)
                        {
                            for (var y = 0; y < size.y; y++)
                            {
                                for (var x = 0; x < size.x; x++)
                                {
                                    var block = grid[x, y, z];
                                    if (block == null)
                                    {
                                        continue;
                                    }

                                    tempBlocks[countBlocks] = new Vector3Int(x, y, z);
                                    countBlocks++;
                                    break;
                                }
                            }
                        }

                        break;
                    case BlockDirection.back:
                        for (var x = 0; x < size.x; x++)
                        {
                            for (var y = 0; y < size.y; y++)
                            {
                                for (var z = 0; z < size.z; z++)
                                {
                                    var block = grid[x, y, z];
                                    if (block == null)
                                    {
                                        continue;
                                    }

                                    tempBlocks[countBlocks] = new Vector3Int(x, y, z);
                                    countBlocks++;
                                    break;
                                }
                            }
                        }

                        break;
                    case BlockDirection.right:
                        for (var z = 0; z < size.z; z++)
                        {
                            for (var y = 0; y < size.y; y++)
                            {
                                for (var x = size.x - 1; x >= 0; x--)
                                {
                                    var block = grid[x, y, z];
                                    if (block == null)
                                    {
                                        continue;
                                    }

                                    tempBlocks[countBlocks] = new Vector3Int(x, y, z);
                                    countBlocks++;
                                    break;
                                }
                            }
                        }

                        break;
                    case BlockDirection.down:
                        for (var x = 0; x < size.x; x++)
                        {
                            for (var z = 0; z < size.z; z++)
                            {
                                for (var y = 0; y < size.y; y++)
                                {
                                    var block = grid[x, y, z];
                                    if (block == null)
                                    {
                                        continue;
                                    }

                                    tempBlocks[countBlocks] = new Vector3Int(x, y, z);
                                    countBlocks++;
                                    break;
                                }
                            }
                        }

                        break;
                }

                var randInd = random.Next(countBlocks);
                var blockInd = tempBlocks[randInd];
                var block1 = grid[blockInd.x, blockInd.y, blockInd.z];
                block1.SetDirection(dir);
                blocks.Remove(block1);
                grid[blockInd.x, blockInd.y, blockInd.z] = null;
            }
        }
    }

    private void SetLuckyBox()
    {
        var blocks = _blocks.ToList();
        var grid = GetBlocksGrid(blocks);
        var size = new Vector3Int(grid.GetLength(0), grid.GetLength(1), grid.GetLength(2));
        while (blocks.Count >= 2)
        {
            for (var i = 0; i < 6; i++)
            {
                if (blocks.Count == 1)
                {
                    break;
                }

                RemoveBlocks();

                void RemoveBlocks()
                {
                    var dir = (BlockDirection)i;
                    switch (dir)
                    {
                        case BlockDirection.up:
                            for (var x = 0; x < size.x; x++)
                            {
                                for (var z = 0; z < size.z; z++)
                                {
                                    for (var y = size.y - 1; y >= 0; y--)
                                    {
                                        var block = grid[x, y, z];
                                        if (block == null)
                                        {
                                            continue;
                                        }

                                        if (block.Direction != dir)
                                        {
                                            break;
                                        }

                                        blocks.Remove(block);
                                        grid[x, y, z] = null;
                                        if (blocks.Count == 1)
                                        {
                                            return;
                                        }

                                        break;
                                    }
                                }
                            }

                            break;
                        case BlockDirection.forward:
                            for (var x = 0; x < size.x; x++)
                            {
                                for (var y = 0; y < size.y; y++)
                                {
                                    for (var z = size.z - 1; z >= 0; z--)
                                    {
                                        var block = grid[x, y, z];
                                        if (block == null)
                                        {
                                            continue;
                                        }

                                        if (block.Direction != dir)
                                        {
                                            break;
                                        }

                                        blocks.Remove(block);
                                        grid[x, y, z] = null;
                                        if (blocks.Count == 1)
                                        {
                                            return;
                                        }

                                        break;
                                    }
                                }
                            }

                            break;
                        case BlockDirection.left:
                            for (var z = 0; z < size.z; z++)
                            {
                                for (var y = 0; y < size.y; y++)
                                {
                                    for (var x = 0; x < size.x; x++)
                                    {
                                        var block = grid[x, y, z];
                                        if (block == null)
                                        {
                                            continue;
                                        }

                                        if (block.Direction != dir)
                                        {
                                            break;
                                        }

                                        blocks.Remove(block);
                                        grid[x, y, z] = null;
                                        if (blocks.Count == 1)
                                        {
                                            return;
                                        }

                                        break;
                                    }
                                }
                            }

                            break;
                        case BlockDirection.back:
                            for (var x = 0; x < size.x; x++)
                            {
                                for (var y = 0; y < size.y; y++)
                                {
                                    for (var z = 0; z < size.z; z++)
                                    {
                                        var block = grid[x, y, z];
                                        if (block == null)
                                        {
                                            continue;
                                        }

                                        if (block.Direction != dir)
                                        {
                                            break;
                                        }

                                        blocks.Remove(block);
                                        grid[x, y, z] = null;
                                        if (blocks.Count == 1)
                                        {
                                            return;
                                        }

                                        break;
                                    }
                                }
                            }

                            break;
                        case BlockDirection.right:
                            for (var z = 0; z < size.z; z++)
                            {
                                for (var y = 0; y < size.y; y++)
                                {
                                    for (var x = size.x - 1; x >= 0; x--)
                                    {
                                        var block = grid[x, y, z];
                                        if (block == null)
                                        {
                                            continue;
                                        }

                                        if (block.Direction != dir)
                                        {
                                            break;
                                        }

                                        blocks.Remove(block);
                                        grid[x, y, z] = null;
                                        if (blocks.Count == 1)
                                        {
                                            return;
                                        }

                                        break;
                                    }
                                }
                            }

                            break;
                        case BlockDirection.down:
                            for (var x = 0; x < size.x; x++)
                            {
                                for (var z = 0; z < size.z; z++)
                                {
                                    for (var y = 0; y < size.y; y++)
                                    {
                                        var block = grid[x, y, z];
                                        if (block == null)
                                        {
                                            continue;
                                        }

                                        if (block.Direction != dir)
                                        {
                                            break;
                                        }

                                        blocks.Remove(block);
                                        grid[x, y, z] = null;
                                        if (blocks.Count == 1)
                                        {
                                            return;
                                        }

                                        break;
                                    }
                                }
                            }

                            break;
                    }
                }
            }
        }

        var block1 = blocks[0];
        _blocks.Remove(block1);

        Destroy(block1.gameObject);
    }

    public void Init(Map map, int rotationSeed, int moves)
    {
        _tapAwayTapsForAds = new[] { 5, 10 };
        _map = map;
        _gameCamera = CameraManager.Instance.GetCameraItem(ECameraType.GAME);
        _levelStarted = false;
        _blocks = GetBlocks();
        if (rotationSeed >= 0)
        {
            GenerateDirections(_blocks, rotationSeed);
        }

        var blocksCount = _blocks.Count;

        _targetRotation = transform.rotation = Quaternion.Euler(-30f, 0f, 0f) * Quaternion.Euler(0f, -45f, 0f);
        var distMin = float.MaxValue;
        var distMax = 0f;
        var nearestBlock = (Block)null;
        foreach (var block in _blocks)
        {
            var blockLocalPos = block.transform.localPosition;
            var dist = block.transform.localPosition.sqrMagnitude;
            if (dist < distMin)
            {
                distMin = dist;
                nearestBlock = block;
            }
            else if (dist > distMax)
            {
                distMax = dist;
            }
        }

        distMin = Mathf.Sqrt(distMin);
        distMax = Mathf.Sqrt(distMax);

        _levelScale = Mathf.Min(1f, 3f / distMax * _addScale);
        _gameCamera.SetMaxZoom(1f / _levelScale);

        _blocks.ForEach(b => b.Init(_levelScale, nearestBlock == b,
            Mathf.InverseLerp(distMin, distMax, b.transform.localPosition.magnitude), () =>
            {
                _levelStarted |= --blocksCount == 0;
                if (_levelStarted)
                {
                    CheckTutor();
                }
            }));
        transform.localScale = Vector3.one * _levelScale;
        _camera = _gameCamera.Camera;
        _countMoves = Mathf.Max(moves, _blocks.Count);

        _movesController.OnMovesLeftChanged += UpdateMovesLeft;
        _movesController.SetMoves(_countMoves);
    }

    private void CheckTutor()
    {
        if (!GameConfig.RemoteConfig.tapAwayTutor)
        {
            return;
        }

        if (!TapAwayDragTutorCompleted)
        {
            _dragTutorActive = true;
            UITapAwayTutorial.Instance.ShowDragTutor();
            return;
        }

        if (!TapAwayTapTutorCompleted)
        {
            _tapTutorActive = true;

            IEnumerator Co_FindTapCell()
            {
                var cameraPosition = _camera.transform.position;

                bool IsBlockOk(Block block)
                {
                    var tr = block.transform;
                    var fw = tr.forward;


                    var pos = tr.position;
                    var ray = new Ray(pos, fw);
                    if (Physics.Raycast(ray, out _))
                    {
                        return false;
                    }

                    var ray1 = new Ray(pos, cameraPosition);
                    if (Physics.Raycast(ray1, out _))
                    {
                        return false;
                    }

                    return true;
                }

                Block GetTapBlock()
                {
                    var nearestBlock = (Block)null;
                    var dist = float.PositiveInfinity;
                    foreach (var block in _blocks)
                    {
                        var dist1 = Vector3.SqrMagnitude(block.transform.position - cameraPosition);
                        if (dist1 < dist && IsBlockOk(block))
                        {
                            dist = dist1;
                            nearestBlock = block;
                        }
                    }

                    return nearestBlock;
                }

                var wait = new WaitForSeconds(0.2f);
                var lastBlock = (Block)null;
                while (!TapAwayTapTutorCompleted)
                {
                    if (lastBlock == null || !IsBlockOk(lastBlock))
                    {
                        var block = GetTapBlock();
                        if (block)
                        {
                            lastBlock = block;
                            UITapAwayTutorial.Instance.ShowTapTutor(block);
                        }
                        else
                        {
                            UITapAwayTutorial.Instance.HideTutorial();
                        }
                    }

                    yield return wait;
                }

                UITapAwayTutorial.Instance.HideTutorial();
                CheckTutor();
            }

            StartCoroutine(Co_FindTapCell());
        }
    }

    private void ValidateSteps()
    {
        var count = 0;
        var blocks = new Transform[transform.childCount];
        var errorString = "";
        for (var i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (!child.TryGetComponent<Block>(out _))
            {
                continue;
            }

            blocks[count] = child;
            for (var j = 0; j < count; j++)
            {
                if (blocks[j].position == child.position)
                {
                    errorString += $"{child.name}\n";
                    break;
                }
            }

            count++;
        }

        if (!string.IsNullOrEmpty(errorString))
        {
            Debug.LogError(errorString);
        }

        ValidateSteps(count);
    }

    private void ValidateSteps(int countBlocks)
    {
        _countMoves = Mathf.Max(_countMoves, countBlocks);
    }

    private List<Block> GetBlocks()
    {
        var blocks = new List<Block>();
        for (var i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (!child.TryGetComponent<Block>(out var block))
            {
                continue;
            }

            blocks.Add(block);
        }

        return blocks;
    }

    private void CheckCompleteOrFail()
    {
        Completed = _blocks.All(b => b.Completed);
        if (Completed)
        {
            _map.LevelCompleted();
            return;
        }

        Failed = !_movesController.HasMoves();
        if (Failed)
        {
            _map.LevelFailed();
        }
    }

    public void AddMoves(int moves)
    {
        Failed = false;
        _movesController.AddMoves(moves);
    }

    private void UpdateMovesLeft(int movesLeft, bool tweenValue)
    {
        var gameScreen = FindObjectOfType<UIGameScreen>();
        if (gameScreen)
        {
            gameScreen.SetMovesWidgetEnabled(true, false);
            gameScreen.UpdateMovesLeft(movesLeft, tweenValue);
        }
    }
}
