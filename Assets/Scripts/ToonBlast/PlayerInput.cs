using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private bool canPlay;

    // Start is called before the first frame update
    void Start()
    {
        GameEventManager.current.onGridFillStart += DisablePlay;
        GameEventManager.current.onGridFillFinish += EnablePlay;

        canPlay = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!canPlay)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (hit.collider != null)
            {
                Vector2 blockPosInGrid = new Vector2(hit.collider.transform.position.x, hit.collider.transform.position.y);
                Color blockColor = hit.collider.GetComponent<SpriteRenderer>().color;

                //Debug.Log(blockPosInGrid + " " + blockColor);

                GameEventManager.current.BlockClick(blockPosInGrid, blockColor);
            }
        }
    }

    private void EnablePlay()
    {
        canPlay = true;
    }

    private void DisablePlay()
    {
        canPlay = false;
    }
}
