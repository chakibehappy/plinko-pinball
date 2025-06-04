using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlinkoLampManager : MonoBehaviour
{
    [SerializeField] private Sprite[] plinkoLampSprite;
    [SerializeField] private List<Transform> plinkoRows;
    List<List<SpriteRenderer>> plinkoLamp = new();
    [SerializeField] private float lightOnDelay = 0.15f;

    private void Start()
    {
        foreach (Transform t in plinkoRows)
        {
            List<SpriteRenderer> lamps = new();
            for (int i = 0; i < t.childCount; i++)
            {
                if(t.GetChild(i).childCount > 0)
                {
                    lamps.Add(t.GetChild(i).GetChild(0).gameObject.GetComponent<SpriteRenderer>());
                }
            }
            plinkoLamp.Add(lamps);
        }

        StartCoroutine(LightDownArrowEffect());
    }

    IEnumerator LightDownArrowEffect()
    {
        int activePhase = 0;
        while (true)
        {
            for (int i = 0; i < plinkoLamp.Count; i++) // row
            {
                for (int j = 0; j < plinkoLamp[i].Count; j++)
                {
                    plinkoLamp[i][j].sprite = plinkoLampSprite[0];
                }
            }

            if (activePhase == 0)
            {
                plinkoLamp[1][1].sprite = plinkoLampSprite[1];
                plinkoLamp[1][2].sprite = plinkoLampSprite[1];
                plinkoLamp[2][2].sprite = plinkoLampSprite[1];
            }
            else if (activePhase == 1)
            {
                plinkoLamp[1][0].sprite = plinkoLampSprite[1];
                plinkoLamp[1][3].sprite = plinkoLampSprite[1];
                plinkoLamp[2][1].sprite = plinkoLampSprite[1];
                plinkoLamp[2][3].sprite = plinkoLampSprite[1];
                plinkoLamp[3][2].sprite = plinkoLampSprite[1];
                plinkoLamp[3][3].sprite = plinkoLampSprite[1];
            }
            else if (activePhase == 2)
            {
                plinkoLamp[2][0].sprite = plinkoLampSprite[1];
                plinkoLamp[2][4].sprite = plinkoLampSprite[1];
                plinkoLamp[3][1].sprite = plinkoLampSprite[1];
                plinkoLamp[3][4].sprite = plinkoLampSprite[1];
                plinkoLamp[4][2].sprite = plinkoLampSprite[1];
                plinkoLamp[4][4].sprite = plinkoLampSprite[1];
                plinkoLamp[5][3].sprite = plinkoLampSprite[1];
                plinkoLamp[5][4].sprite = plinkoLampSprite[1];
            }
            else if (activePhase == 3)
            {
                plinkoLamp[3][0].sprite = plinkoLampSprite[1];
                plinkoLamp[3][5].sprite = plinkoLampSprite[1];
                plinkoLamp[4][1].sprite = plinkoLampSprite[1];
                plinkoLamp[4][5].sprite = plinkoLampSprite[1];
                plinkoLamp[5][2].sprite = plinkoLampSprite[1];
                plinkoLamp[5][5].sprite = plinkoLampSprite[1];
                plinkoLamp[6][3].sprite = plinkoLampSprite[1];
                plinkoLamp[6][5].sprite = plinkoLampSprite[1];
                plinkoLamp[7][4].sprite = plinkoLampSprite[1];
                plinkoLamp[7][5].sprite = plinkoLampSprite[1];
            }
            else if (activePhase == 4)
            {
                plinkoLamp[4][0].sprite = plinkoLampSprite[1];
                plinkoLamp[4][6].sprite = plinkoLampSprite[1];
                plinkoLamp[5][1].sprite = plinkoLampSprite[1];
                plinkoLamp[5][6].sprite = plinkoLampSprite[1];
                plinkoLamp[6][2].sprite = plinkoLampSprite[1];
                plinkoLamp[6][6].sprite = plinkoLampSprite[1];
                plinkoLamp[7][3].sprite = plinkoLampSprite[1];
                plinkoLamp[7][6].sprite = plinkoLampSprite[1];
            }
            else if (activePhase == 5)
            {
                plinkoLamp[5][0].sprite = plinkoLampSprite[1];
                plinkoLamp[5][7].sprite = plinkoLampSprite[1];
                plinkoLamp[6][1].sprite = plinkoLampSprite[1];
                plinkoLamp[6][7].sprite = plinkoLampSprite[1];
                plinkoLamp[7][2].sprite = plinkoLampSprite[1];
                plinkoLamp[7][7].sprite = plinkoLampSprite[1];
            }
            else if (activePhase == 6)
            {
                plinkoLamp[6][0].sprite = plinkoLampSprite[1];
                plinkoLamp[6][8].sprite = plinkoLampSprite[1];
                plinkoLamp[7][1].sprite = plinkoLampSprite[1];
                plinkoLamp[7][8].sprite = plinkoLampSprite[1];
            }

            yield return new WaitForSeconds(lightOnDelay);
            activePhase++;
            if (activePhase == 7)
            {
                activePhase = 0;
            }
        }
    }

}
