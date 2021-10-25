using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour
{
    [Header("Board Variables")]
    public int column;
    public int row;
    public int previousColumn;
    public int previousRow;
    public int targetX;
    public int targetY;
    public bool isMatched = false; // Совпадение

    private FindMatches findMatches;
    private Board board;
    private GameObject otherDot;
    private Vector2 firstTouchPosition; // Координаты первой позиции
    private Vector2 finalTouchPosition; //Координаты второй позиции
    private Vector2 tempPosition; //Целевая позиция
    public float swipeAngle = 0; // Угол взаимодействия
    public float swipeResist = 1f;

    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<Board>();
        findMatches = FindObjectOfType<FindMatches>();
        //targetX = (int)transform.position.x;
        //targetY = (int)transform.position.y;
        //row = targetY;
        //column = targetX;
        //previousColumn = column;
        //previousRow = row;
    }

    // Update is called once per frame
    void Update()
    {
        // FindMatches();
        if (isMatched)
        {
            SpriteRenderer mySprite = GetComponent<SpriteRenderer>();
            mySprite.color = new Color(1f, 1f, 1f, .2f); // При совпадении меняем компонент
        }
        targetX = column;
        targetY = row;
        if (Mathf.Abs(targetX - transform.position.x) > .1) // Move towards the target
        {
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .4f);
            if (board.allDots[column, row] != this.gameObject)
            {
                board.allDots[column, row] = this.gameObject;
            }
            findMatches.FindAllMatches();
        }
        else // Directly set the position
        {   
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = tempPosition;
        }
        if (Mathf.Abs(targetY - transform.position.y) > .1) // Move towards the target
        {
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .4f);
            if (board.allDots[column, row] != this.gameObject)
            {
                board.allDots[column, row] = this.gameObject;
            }
            findMatches.FindAllMatches();
        }
        else // Directly set the position
        {
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = tempPosition;
        }
    }

    public IEnumerator CheckMoveCo()
    {
        yield return new WaitForSeconds(.5f);
        if (otherDot != null)
        {
            if (!isMatched && !otherDot.GetComponent<Dot>().isMatched)
            {
                otherDot.GetComponent<Dot>().row = row;
                otherDot.GetComponent<Dot>().column = column;
                row = previousRow;
                column = previousColumn;
                yield return new WaitForSeconds(.5f);
                board.currentState = GameState.move;
            }
            else
            {
                board.DestroyMatches();
            }
            otherDot = null;
        }
        else board.currentState = GameState.move;
    }

    private void OnMouseDown()
    {
        if (board.currentState == GameState.move)
        {
            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); // Конвертирует в общую систему координат из кординаты объекта
            // Debug.Log(firstTouchPosition);
        }

    }

    private void OnMouseUp()
    {
        if (board.currentState == GameState.move)
        { 
            finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateAngle();
        }

    }

    void CalculateAngle()
    {
        if (Mathf.Abs(finalTouchPosition.y - firstTouchPosition.y) > swipeResist || Mathf.Abs(finalTouchPosition.x - firstTouchPosition.x) > swipeResist) // Проверка на то что две точки не совпадают
        {
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI; // Угол между касанием и отпусканием в радианах с переводом в градусы
            //Debug.Log(swipeAngle);
            MovePieces();
            board.currentState = GameState.wait; //После действия со смещением режим ожидания 
        }
        else 
        {
            board.currentState = GameState.move;
        }
    }

    void MovePieces()
    {
        if (swipeAngle > -45 && swipeAngle <= 45 && column < board.width - 1) // Правый свайп
        {
            otherDot = board.allDots[column + 1, row];
            previousColumn = column;
            previousRow = row;
            otherDot.GetComponent<Dot>().column -= 1;
            column += 1;
            Debug.Log("Right");
        }
        else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.heigth - 1) // Верхний свайп
        { 
            otherDot = board.allDots[column, row + 1];
            previousColumn = column;
            previousRow = row;
            otherDot.GetComponent<Dot>().row -= 1;
            row += 1;
            Debug.Log("Up");
        }
        else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0 ) // Левый свайп
        {
            otherDot = board.allDots[column - 1, row];
            previousColumn = column;
            previousRow = row;
            otherDot.GetComponent<Dot>().column += 1;
            column -= 1;
            Debug.Log("Left");
        }
        else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0) // Нижний свайп
        {
            otherDot = board.allDots[column, row - 1];
            previousColumn = column;
            previousRow = row;
            otherDot.GetComponent<Dot>().row += 1;
            row -= 1;
            Debug.Log("Down");
        }
        StartCoroutine(CheckMoveCo());
    }

    void FindMatches() // Поиск совпадений
    {
        if (column > 0 && column < board.width - 1) // По горизонтали
        {
            GameObject leftDot1 = board.allDots[column - 1, row]; // Левый объект
            GameObject rightDot1 = board.allDots[column + 1, row]; // Правый объект
            if (leftDot1 != null && rightDot1 != null)
            {
                if (leftDot1.tag == this.gameObject.tag && rightDot1.tag == this.gameObject.tag)
                {
                    leftDot1.GetComponent<Dot>().isMatched = true;
                    rightDot1.GetComponent<Dot>().isMatched = true;
                    isMatched = true; // Совпадение
                }
            }
        }
        if (row > 0 && row < board.heigth - 1) // По вертикали
        {
            GameObject upDot1 = board.allDots[column, row + 1]; // Верхний объект
            GameObject downDot1 = board.allDots[column, row - 1]; // Нижний объект
            if (upDot1 != null && downDot1 != null)
            {
                if (upDot1.tag == this.gameObject.tag && downDot1.tag == this.gameObject.tag)
                {
                    upDot1.GetComponent<Dot>().isMatched = true;
                    downDot1.GetComponent<Dot>().isMatched = true;
                    isMatched = true; // Совпадение
                }
            }
        }
    }
}