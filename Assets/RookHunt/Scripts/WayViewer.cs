using System.Collections.Generic;
using UnityEngine;
using System;

public class WayViewer : MonoBehaviour
{
    [SerializeField] private bool unLocker;
    [SerializeField] private RookHuntGameController RHCСs;
    [SerializeField] private GameObject Square;
    [NonSerialized] private List<GameObject> AllSquares = new List<GameObject>();
    [NonSerialized] private int WayID;

    void Update()
    {
        if(unLocker)
        {
            if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.Minus) || AllSquares.Count == 0)
            {
                foreach (var GO in AllSquares)
                    Destroy(GO.gameObject);
                AllSquares.Clear();

                WayID = Math.Clamp(WayID += Input.GetKeyDown(KeyCode.Equals) ? 1 : Input.GetKeyDown(KeyCode.Minus) ? -1 : 0, 0, RHCСs.Ways.Count - 1);
                print(WayID);
                Vector2 prevV2 = RHCСs.Ways[WayID].gameObject.transform.position;
                foreach (Vector2 v2 in RHCСs.Ways[WayID].PathPoints)
                {
                    // line 
                    AllSquares.Add(Instantiate(Square, Vector3.Lerp(v2, prevV2, 0.5f), Quaternion.Euler(0, 0, Mathf.Atan2(prevV2.y - v2.y, prevV2.x - v2.x) * Mathf.Rad2Deg)));
                    AllSquares[AllSquares.Count - 1].transform.localScale = new Vector2(Vector2.Distance(prevV2, v2), 0.05f);

                    // dot
                    AllSquares.Add(Instantiate(Square, v2, Quaternion.identity));
                    prevV2 = AllSquares[AllSquares.Count - 1].transform.position;
                }
            }
            else
            {
                int id = 0;
                Vector2 prevV2 = RHCСs.Ways[WayID].gameObject.transform.position;
                foreach (Vector2 v2 in RHCСs.Ways[WayID].PathPoints)
                {
                    AllSquares[id].transform.position = Vector3.Lerp(v2, prevV2, 0.5f);
                    AllSquares[id].transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(prevV2.y - v2.y, prevV2.x - v2.x) * Mathf.Rad2Deg);
                    AllSquares[id].transform.localScale = new Vector2(Vector2.Distance(prevV2, v2), 0.05f);
                    id++;
                    AllSquares[id].transform.position = v2;
                    prevV2 = AllSquares[id].transform.position;
                    id++;
                }
            }
        }
    }
}
