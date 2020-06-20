using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceHandler : MonoBehaviour
{
    public Vector3 designatedSpot;
    private float speed = 60.0f;
    public bool isMoving = false;
    public int pieceType = -1;

    public bool fakeMove = false;
    public Vector3 fakeMoveSpot;


    public void Initialize(Vector3 designatedSpot, int pieceType)
    {
        this.designatedSpot = designatedSpot;
        this.pieceType = pieceType;
        isMoving = true;
    }

    public void setDesignatedSpot(Vector3 designatedSpot)
    {
        if(this.designatedSpot == designatedSpot)
        {
            return;
        }
        this.designatedSpot = designatedSpot;
        isMoving = true;
    }

    public void setFakeMoveSpot(Vector3 fakeMoveSpot)
    {
        this.fakeMoveSpot = fakeMoveSpot;
        fakeMove = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (fakeMove)
        {
            if (Vector3.Distance(transform.position, fakeMoveSpot) > 0.0001f)
            {
                this.gameObject.transform.position = Vector3.MoveTowards(transform.position, fakeMoveSpot, speed * Time.deltaTime);
            } else
            {
                isMoving = true;
                fakeMove = false;
            }
            return;
        }
        if(!isMoving)
        {
            return;
        }
        if(Vector3.Distance(transform.position, designatedSpot) > 0.0001f)
        {
            this.gameObject.transform.position = Vector3.MoveTowards(transform.position, designatedSpot, speed * Time.deltaTime);
        } else
        {
            isMoving = false;
        }
    }
}
