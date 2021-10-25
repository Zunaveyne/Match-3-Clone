using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState //Limit player movement using a state machine
{
    wait,
    move
}

public class Board : MonoBehaviour
{
    public GameState currentState = GameState.move;
    public int width;
    public int heigth;
    public int offSet; // Высота падения
    public GameObject tilePrefab; // Префаб ячейки
    private BackgroundTile[,] allTiles; // Все ячейки
    public GameObject destroyEffect;
    public GameObject[] dots; // Префаб из всех точек
    public GameObject[,] allDots; // Все точки
    private FindMatches findMatches;

    // Start is called before the first frame update
    void Start()
    {
        findMatches = FindObjectOfType<FindMatches>();
        allTiles = new BackgroundTile[width, heigth];
        allDots = new GameObject[width, heigth];
        SetUp();
    }

    // Update is called once per frame
    private void SetUp()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < heigth; j++)
            {
                Vector2 tempPosition = new Vector2(i, j + offSet);
                GameObject backgroundTile = Instantiate(tilePrefab, tempPosition, Quaternion.identity) as GameObject;
                backgroundTile.transform.parent = this.transform; // Привязываем генерируемые ячейки к доске
                backgroundTile.name = "(" + i + "," + j + ")"; // Название ячейки адрес
                int dotToUse = Random.Range(0, dots.Length); // Выбираем рандомную из массива

                int maxIterations = 0; // Защита от бесконечности цикла
                while(MatchesAt(i, j, dots[dotToUse]) && maxIterations < 100) // Проверяем есть ли совпадения и меняем объект пока их не будет  
                {
                    dotToUse = Random.Range(0, dots.Length);
                    maxIterations++;
                    //Debug.Log(maxIterations);
                }
                maxIterations = 0;

                GameObject dot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity); // Создаем ее
                dot.GetComponent<Dot>().row = j;
                dot.GetComponent<Dot>().column = i;
                dot.transform.parent = this.transform; // Прицепляем к ячейке
                dot.name = dots[dotToUse].name + "(" + i + "," + j + ")"; // Имя как у ячейки
                allDots[i, j] = dot;
            }
        }
    }

    private bool MatchesAt( int column, int row, GameObject piece)
    {
        if (column > 1)          // Проверяем совпадение по строке 
        {
            // Проверяем до 2 объектов слева
            if (allDots[column - 1, row].tag == piece.tag && allDots[column - 2, row].tag == piece.tag)
            {
                return true;
            }
        }

        if (row > 1)        // Проверяем совпадение по ряду
        {
            // Проверяем до 2 объектов внизу
            if (allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2].tag == piece.tag)
            {
                return true;
            }
        }
        /* Изначальный код
        if (column > 1 && row > 1)
        {
            // Проверяем до 2 объектов слева
            if (allDots[column - 1, row].tag == piece.tag && allDots[column - 2, row].tag == piece.tag)
            {
                return true;
            }
            // Проверяем до 2 объектов внизу
            if (allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2].tag == piece.tag)
            {
                return true;
            }
        }
        else if (column <= 1 || row <= 1)
        {
            if (row > 1)
            {
                if (allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2].tag == piece.tag)
                {
                    return true;
                }
            }
            if (column > 1)
            {
                if (allDots[column - 1, row].tag == piece.tag && allDots[column - 2, row].tag == piece.tag)
                {
                    return true;
                }
            }
        }
        */
        return false;
    }

    private void DestroyMatchesAt(int column, int row) // Разрушаем объект если его переменная isMatched истина
    {
        if (allDots[column, row].GetComponent<Dot>().isMatched)
        {
            findMatches.currentMatches.Remove(allDots[column, row]);
            Instantiate(destroyEffect, allDots[column, row].transform.position, Quaternion.identity);
            //GameObject particle = Instantiate(destroyEffect, allDots[column, row].transform.position, Quaternion.identity);
            //Destroy(particle, .5f);
            Destroy(allDots[column, row]);
            allDots[column, row] = null;
        }
    }

    public void DestroyMatches() // Проверяем все поле на совпадения
    {
        for(int i = 0; i < width; i++)
        {
            for (int j = 0; j < heigth; j++)
            {
                if (allDots[i, j] != null) DestroyMatchesAt(i, j);
            }
        }
        StartCoroutine(DecreaseRowCo());
    }

    private IEnumerator DecreaseRowCo() // Удаление рядов
    {
        int nullCount = 0; // Кол-во пустых ячеек
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < heigth; j++)
            {
                if (allDots[i, j] == null)
                {
                    nullCount++; //
                }
                else if (nullCount > 0)
                {
                    allDots[i, j].GetComponent<Dot>().row -= nullCount;
                    allDots[i, j] = null;
                }
            }
            nullCount = 0;
        }
        yield return new WaitForSeconds(.4f);
        StartCoroutine(FillBoardCo());
    }

    private void RefilBoard() //Перезаполнение доски
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < heigth; j++)
            {
                if (allDots[i, j] == null)
                {
                    Vector2 tempPosition = new Vector2(i, j + offSet); //Координаты нулевой позиции
                    int dotToUse = Random.Range(0, dots.Length);
                    GameObject piece = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                    allDots[i, j] = piece; //Создаем объект в нулевой позиции
                    piece.GetComponent<Dot>().row = j;
                    piece.GetComponent<Dot>().column = i;
                }
            }
        }
    }
    
    private bool MatchesOnBoard() //Проверка на совпадения в реальном врени
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < heigth; j++)
            {
                if (allDots[i , j] != null)
                {
                    if (allDots[i, j].GetComponent<Dot>().isMatched)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private IEnumerator FillBoardCo() //Перезаполнение доски и проверка на совпадения в реальном времени
    {
        RefilBoard();
        yield return new WaitForSeconds(.5f);

        while(MatchesOnBoard())
        {
            yield return new WaitForSeconds(.5f);
            DestroyMatches();
        }
        yield return new WaitForSeconds(.5f);
        currentState = GameState.move;
    }
}
