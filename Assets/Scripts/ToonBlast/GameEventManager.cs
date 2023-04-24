using System;
using UnityEngine;

public class GameEventManager : MonoBehaviour
{
    public static GameEventManager current;

    // Start is called before the first frame update
    void Start()
    {
        if (current == null)
            current = this;
    }

    public event Action<Vector2, Color> onBlockClick;
    public void BlockClick(Vector2 blockPos, Color blockColor)
    {
        if (onBlockClick != null)
            onBlockClick(blockPos, blockColor);
    }

    public event Action onGridFillStart;
    public void GridFillStart()
    {
        if (onGridFillStart != null)
            onGridFillStart();
    }

    public event Action onGridFillFinish;
    public void GridFillFinish()
    {
        if (onGridFillFinish != null)
            onGridFillFinish();
    }
}
