using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateGrid
{
    //Setzt die maximale Pfadlänge für den Hauptpfad
    private int counter;
    private Cell[,] grid;

    public GenerateGrid(int grid_width, int grid_height, int maxLengthOfMainPath)
    {
        this.counter = maxLengthOfMainPath;
        this.grid = new Cell[grid_width, grid_height];
    }

    public enum CellType { Main, Side, EMPTY, Start, End}
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
        // Initialisiere Grid
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                Cell cell = new Cell(CellType.EMPTY);
                //Add Left free Edge
                if (x > 0)
                {
                    cell.freeEdges.Add(Vector2Int.left);
                }
                //Add Right free Edge
                if (x < grid.GetLength(0) - 1)
                {
                    cell.freeEdges.Add(Vector2Int.right);
                }
                //Add Top free Edge (Im Grid ist Top und Down im Vergelich zu Vector2Int verstauscht, da Origin in linken oberen Ecke)
                if (y > 0)
                {
                    cell.freeEdges.Add(Vector2Int.down);
                }
                //Add Bottom free Edge
                if (y < grid.GetLength(1) - 1)
                {
                    cell.freeEdges.Add(Vector2Int.up);
                }
                grid[x, y] = cell;
            }
        }

        //Inititalisiere Start
        //Wählt zufälligen Start (nicht am Rand), wählt einen zufälligen Weg, entfernt alle anderen Kanten
        Vector2Int randomStart = new Vector2Int(Random.Range(1,grid.GetLength(0)-1), Random.Range(1,grid.GetLength(1)-1));
        setType(randomStart, CellType.Start);
        int randIndex = Random.Range(0, grid[randomStart.x, randomStart.y].freeEdges.Count);
        Vector2Int pickedEdge = grid[randomStart.x, randomStart.y].freeEdges[randIndex];
        markEdge(randomStart, pickedEdge);
        RemoveAllEdges(randomStart);

        //Geht weiter zur nächsten Zelle
        //Von dort aus wird rekursiv der Hauptweg und alle Nebenwege generiert
        NextCell(randomStart + pickedEdge, CellType.Main);

        DebugPrintGrid();
        return grid;
    }
    private void NextCell(Vector2Int pos, CellType type)
    {
        if (grid[pos.x, pos.y].type == CellType.EMPTY)
        {
            setType(pos, type);
        }
        DebugPrintGrid();
        if (type == CellType.Main)
        {
            //Test All neighbours if they are also Main, if so remove the connection to avoid direct shortcuts (shortcuts with sideways still possible)
            grid[pos.x, pos.y].freeEdges.RemoveAll(edge =>
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

            while (counter > 0)
            {
                counter--;
                //Erweitere Hauptweg, (in extra Funktion TODO)
                //Pick random free Edge if available
                if (grid[pos.x, pos.y].freeEdges.Count > 0)
                {
                    int randIndex = Random.Range(0, grid[pos.x, pos.y].freeEdges.Count);
                    Vector2Int pickedEdge = grid[pos.x, pos.y].freeEdges[randIndex];
                    markEdge(pos, pickedEdge);
                    NextCell(pos + pickedEdge, CellType.Main);
                }
            }
            if (counter == 0)
            {
                counter--;
                setType(pos, CellType.End);
                RemoveAllEdges(pos);
            }
        }
        // Wähle eine zufällige Teilmenge der verbliebenden freien Pfade als Nebenpfade
        for (int i = 0; i < grid[pos.x, pos.y].freeEdges.Count; i++)
        {
            int rand = Random.Range(0, 2);
            if (rand == 0)
            {
                Vector2Int pickedEdge = grid[pos.x, pos.y].freeEdges[i];
                markEdge(pos, pickedEdge);
                NextCell(pos + pickedEdge, CellType.Side);
            }
        }
        //Entferne Alle nicht ausgewählten Pfade
        RemoveAllEdges(pos);
    }



    private void setType(Vector2Int idx, CellType type)
    {
        grid[idx.x, idx.y].type = type;
    }
    private void markEdge(Vector2Int cell_idx, Vector2Int edge)
    {
        //Update edges in Cell
        Cell cell = grid[cell_idx.x, cell_idx.y];
        cell.markedEdges.Add(edge);
        cell.freeEdges.Remove(edge);
        grid[cell_idx.x, cell_idx.y] = cell;
        //Update NeighbourCell
        Vector2Int neighbourIndex = cell_idx + edge;
        Cell neighbourCell = grid[neighbourIndex.x, neighbourIndex.y];
        neighbourCell.markedEdges.Add(-edge);
        neighbourCell.freeEdges.Remove(-edge);
        grid[neighbourIndex.x, neighbourIndex.y] = neighbourCell;
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
