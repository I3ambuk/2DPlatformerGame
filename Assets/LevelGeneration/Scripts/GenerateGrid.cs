using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateGrid
{
    public int lengthOfMain = 10;
    private int counter;
    private Cell[,] grid = new Cell[4, 4];

    public enum CellType { Main, Side, EMPTY }
    public struct Cell
    {
        public CellType type;
        public List<Vector2Int> freeEdges;
        public List<Vector2Int> markedEdges;
        public Cell(CellType type = CellType.EMPTY)
        {
            this.type = type;
            this.freeEdges = new List<Vector2Int>();
            this.markedEdges = new List<Vector2Int>();
        }
    }

    public Cell[,] Generate()
    {
        counter = lengthOfMain;
        // Initialisiere Grid
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                Cell cell = new Cell(CellType.EMPTY);
                //Add Left free Edge
                if (x > 0)
                {
                    cell.freeEdges.Add(new Vector2Int(-1, 0));
                }
                //Add Right free Edge
                if (x < grid.GetLength(0) - 1)
                {
                    cell.freeEdges.Add(new Vector2Int(1, 0));
                }
                //Add Top free Edge
                if (y > 0)
                {
                    cell.freeEdges.Add(new Vector2Int(0, -1));
                }
                //Add Bottom free Edge
                if (y < grid.GetLength(1) - 1)
                {
                    cell.freeEdges.Add(new Vector2Int(0, 1));
                }
                grid[x, y] = cell;
            }
        }

        //Inititalisiere Start
        //Random Startpunkt TODO Extra Funktion
        Vector2Int randomStart = new Vector2Int(Random.Range(1,grid.GetLength(0)-1), Random.Range(1,grid.GetLength(1)-1));
        Debug.Log(randomStart.ToString());
        Cell startCell = grid[randomStart.x, randomStart.y];
        startCell.type = CellType.Main;
        //Pick random free Edge
        int randIndex = Random.Range(0, startCell.freeEdges.Count);
        Vector2Int pickedEdge = startCell.freeEdges[randIndex];
        //Update edges in StartCell (set marked and remove all other)
        startCell.markedEdges.Add(pickedEdge);
        startCell.freeEdges.Remove(pickedEdge); //ACHTUNG BEI NEUER FUNKTION!!
        grid[randomStart.x, randomStart.y] = startCell;
        //Update edges in NeighbourCell
        Vector2Int neighbourIndex = randomStart + pickedEdge;
        Cell neighbourCell = grid[neighbourIndex.x, neighbourIndex.y];
        neighbourCell.markedEdges.Add(-pickedEdge);
        neighbourCell.freeEdges.Remove(-pickedEdge);
        grid[neighbourIndex.x, neighbourIndex.y] = neighbourCell;
        RemoveAllEdges(randomStart);
        //Rekursive Call with Neighbour
        NextCell(neighbourIndex, CellType.Main);

        DebugPrintGrid();
        return grid;
    }
    private void RemoveAllEdges(Vector2Int cellIndex)
    {
        Cell cell = grid[cellIndex.x, cellIndex.y];
        cell.freeEdges.ForEach(edge =>
        {
            Cell neighbour = grid[edge.x + cellIndex.x, edge.y + cellIndex.y];
            neighbour.freeEdges.Remove(-edge);
            grid[edge.x + cellIndex.x, edge.y + cellIndex.y] = neighbour;
        });
        cell.freeEdges.Clear();
        grid[cellIndex.x, cellIndex.y] = cell;
    }

    private void NextCell(Vector2Int pos, CellType type)
    {
        Cell cell = grid[pos.x, pos.y];
        cell.type = type;
        grid[pos.x, pos.y] = cell;
        DebugPrintGrid();
        if (type == CellType.Main)
        {
            //Test All neighbours if they are also Main, if so remove the connection to avoid shortcuts
            cell.freeEdges.RemoveAll(edge =>
            {
                Cell neighbour = grid[pos.x + edge.x, pos.y + edge.y];
                if (neighbour.type == CellType.Main)
                {
                    neighbour.freeEdges.Remove(-edge);
                    grid[pos.x + edge.x, pos.y + edge.y] = neighbour;
                    return true;
                }
                return false;
            });
            grid[pos.x, pos.y] = cell;

            if (counter > 0)
            {
                counter--;
                //Erweitere Hauptweg, (in extra Funktion TODO)
                //Pick random free Edge if available
                if (cell.freeEdges.Count > 0)
                {
                    int randIndex = Random.Range(0, cell.freeEdges.Count);
                    Vector2Int pickedEdge = cell.freeEdges[randIndex];
                    //Update edges in StartCell (set marked and remove all other)
                    cell.markedEdges.Add(pickedEdge);
                    cell.freeEdges.Remove(pickedEdge);
                    grid[pos.x, pos.y] = cell;
                    //Update edges in NeighbourCell
                    Vector2Int neighbourIndex = pos + pickedEdge;
                    Cell neighbourCell = grid[neighbourIndex.x, neighbourIndex.y];
                    neighbourCell.markedEdges.Add(-pickedEdge);
                    neighbourCell.freeEdges.Remove(-pickedEdge);
                    grid[neighbourIndex.x, neighbourIndex.y] = neighbourCell;
                    NextCell(neighbourIndex, CellType.Main);
                }
            }
            else
            {
                RemoveAllEdges(pos);
            }
        }
        // Wähle eine zufällige Teilmenge der verbliebenden freien Pfade als Nebenpfade
        for (int i = 0; i < cell.freeEdges.Count; i++)
        {
            int rand = Random.Range(0, 2);
            if (rand == 0)
            {
                Vector2Int pickedEdge = cell.freeEdges[i];
                //Update edges in StartCell (set marked and remove all other)
                cell.markedEdges.Add(pickedEdge);
                cell.freeEdges.Remove(pickedEdge);
                grid[pos.x, pos.y] = cell;
                //Update edges in NeighbourCell
                Vector2Int neighbourIndex = pos + pickedEdge;
                Cell neighbourCell = grid[neighbourIndex.x, neighbourIndex.y];
                neighbourCell.markedEdges.Add(-pickedEdge);
                neighbourCell.freeEdges.Remove(-pickedEdge);
                grid[neighbourIndex.x, neighbourIndex.y] = neighbourCell;
                CellType newType = neighbourCell.type == CellType.Main ? CellType.Main : CellType.Side;
                NextCell(neighbourIndex, newType);
            }
        }
        RemoveAllEdges(pos);
    }

    private void DebugPrintGrid()
    {
        string p = "";
        for (int y = 0; y < grid.GetLength(1); y++)
        {
            string u = "";
            string middle = "";
            string b = "";
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                string left = "   ";
                string right = "   ";
                string bottom = "   ";
                string top = "   ";

                Cell c = grid[x, y];
                string t = c.type == CellType.EMPTY ? " E" : c.type == CellType.Main ? " M" : " S";
                c.markedEdges.ForEach(edge =>
                {
                    if (edge == new Vector2Int(1, 0))
                    {
                        right = "=";
                    }
                    if (edge == new Vector2Int(-1, 0))
                    {
                        left = "=";
                    }
                    if (edge == new Vector2Int(0, 1))
                    {
                        bottom = "||";
                    }
                    if (edge == new Vector2Int(0, -1))
                    {
                        top = "||";
                    }
                });
                u += $"    {top}    ";
                middle += $"{left}{t}{right}";
                b += $"    {bottom}    ";
            }
            p += u + "\n" + middle + "\n" + b + "\n";
        }
        Debug.Log(p);
    }
}
