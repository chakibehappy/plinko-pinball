using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropTargetCollider : MonoBehaviour
{
    [SerializeField] private List<DropTargetItemCollider> dropTargetItems;
    int hitCount = 0;
    [SerializeField] private MainGame game;
    public float multiplier;

    [SerializeField] private AudioSource Sfx;
    [SerializeField] private AudioClip sfxClip;

    void Start()
    {
        ResetDropTarget();
    }

    public void ResetDropTarget()
    {
        hitCount = 0;
        dropTargetItems.ForEach(item => { item.ResetDropTargetItem(); });
    }

    public void TurnOnAllDropTarget()
    {
        dropTargetItems.ForEach(item => { item.TargetOnHit(); });
    }

    public void DropTargetOnHit()
    {
        Sfx.PlayOneShot(sfxClip);
        if (hitCount < dropTargetItems.Count)
        {
            hitCount++;
            //if (hitCount == dropTargetItems.Count)
            if (hitCount == 1)
            {
                //game.AddMultiplier();
                //Debug.Log("Drop Target Active");
                TurnOnAllDropTarget();
                game.UnlockDropTargetAndBashToy();
            }
        }
    }

}
