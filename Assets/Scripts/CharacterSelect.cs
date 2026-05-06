using UnityEngine;
using UnityEngine.UI;

public class CharacterSelect : MonoBehaviour
{
    public Image[] borders;

    private int currentIndex = 0;

    public Color colorA = new Color(0.4f, 1f, 0.4f);
    public Color colorB = new Color(0f, 0.6f, 0f);

    private float timer;
    public float speed = 4f;

    void Start()
    {
        UpdateSelection();
    }

    void Update()
    {
        HandleInput();
        AnimateBorder();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentIndex++;
            if (currentIndex >= borders.Length)
                currentIndex = 0;

            UpdateSelection();
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentIndex--;
            if (currentIndex < 0)
                currentIndex = borders.Length - 1;

            UpdateSelection();
        }
    }

    void UpdateSelection()
    {
        for (int i = 0; i < borders.Length; i++)
        {
            borders[i].gameObject.SetActive(i == currentIndex);
        }
    }

    void AnimateBorder()
    {
        timer += Time.deltaTime * speed;

        if (Mathf.FloorToInt(timer) % 2 == 0)
        {
            borders[currentIndex].color = colorA;
        }
        else
        {
            borders[currentIndex].color = colorB;
        }
    }
}
