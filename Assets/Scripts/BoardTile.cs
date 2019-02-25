using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BoardTile : MonoBehaviour
{
    SpriteRenderer _spriteRenderer;

    public TileType Type { get; private set; }

    public static List<Sprite> TypeToSpriteLookup;

    public TileState State { get; private set; }

    Coroutine _stateCoroutine;

    Vector3 defaultLocalScale;

    [SerializeField]
    float selectedAnimationSpeed = 10F;

    [SerializeField]
    float selectedAnimationSizeChangeFactor;

    [SerializeField]
    float matchAnimationSpeed = 5F;

    public float lastTimeOfMovement;

    Coroutine _movementCoroutine;

    /// <summary>coordinates of this tile on the board. Managed by Board intstance</summary>
    public Vector2Int Coordinates{get;set;}

    public enum TileState
    {
        standard,
        selected,
        match,
    }

    public struct TileType
    {
        int _key;

        public TileType(int key)
        {
            _key = key;
        }

        public static implicit operator int(TileType tileType)
        {
            return tileType._key;
        }

        // User-defined conversion from Digit to double
        public static implicit operator TileType(int value)
        {
            return new TileType(value);
        }

        public static bool operator ==(TileType t1, TileType t2)
        {
            return t1._key == t2._key;
        }

        public static bool operator !=(TileType t1, TileType t2)
        {
            return t1._key != t2._key;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TileType))
            {
                return false;
            }

            var type = (TileType)obj;
            return _key == type._key;
        }

        public override int GetHashCode()
        {
            return _key;
        }

        public override string ToString()
        {
            return "Tile Type " + _key;
        }
    }

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        defaultLocalScale = transform.localScale;
    }

    public void StartMovingTo(Vector2 targetPosition, float duration)
    {
        if(_movementCoroutine != null)
        {
            StopCoroutine(_movementCoroutine);
        }

        _movementCoroutine = StartCoroutine(MovementCoroutine(targetPosition, duration));
    }

    private IEnumerator MovementCoroutine(Vector3 targetPosition, float duration)
    {
        if (duration > 0F)
        {
            Vector3 startPositon = transform.position;

            Vector3 startToTargetVector = targetPosition - startPositon;

            float distanceLeft = startToTargetVector.magnitude;

            Vector3 movementDirection = startToTargetVector / distanceLeft;

            float speed = distanceLeft / duration;

            while (distanceLeft > 0F)
            {
                transform.position += movementDirection * Time.deltaTime * speed;

                distanceLeft -= Time.deltaTime * speed;

                yield return new WaitForEndOfFrame();
            }
        }
        transform.position = targetPosition;
    }

    public void Initialize(Sprite sprite, Vector2Int coordinates)
    {
        Coordinates = coordinates;

        // set sprite and if necessary add it to the static lookup table
        _spriteRenderer.sprite = sprite;

        if (TypeToSpriteLookup == null)
        {
            TypeToSpriteLookup = new List<Sprite>();
        }

        // determine type
        int index = TypeToSpriteLookup.IndexOf(sprite);

        if (index < 0)
        {
            Type = new TileType(TypeToSpriteLookup.Count);

            TypeToSpriteLookup.Add(_spriteRenderer.sprite);
        }
        else
        {
            Type = new TileType(index);
        }
    }

    public void ChangeState(TileState state)
    {
        if (_stateCoroutine != null)
        {
            StopCoroutine(_stateCoroutine);
        }

        transform.localScale = defaultLocalScale;

        _spriteRenderer.color = Color.white;

        State = state;

        switch (state)
        {
            case TileState.standard:
                break;

            case TileState.selected:
                _stateCoroutine = StartCoroutine(PulsingAnimationCoroutine(Color.gray, selectedAnimationSpeed));
                break;

            case TileState.match:
                _stateCoroutine = StartCoroutine(PulsingAnimationCoroutine(Color.red, selectedAnimationSpeed));
                break;

            default:
                throw new System.Exception("can not handle state: " + state + " (" + this.name + ")");
        }
    }

    IEnumerator PulsingAnimationCoroutine(Color color, float animationSpeed)
    {
        float startTime = Time.time;

        _spriteRenderer.color = color;
        
        while (State == TileState.selected)
        {
            transform.localScale = defaultLocalScale + Vector3.one * Mathf.Sin((Time.time - startTime) * animationSpeed) * selectedAnimationSizeChangeFactor;

            yield return new WaitForEndOfFrame();
        }
    }
}
