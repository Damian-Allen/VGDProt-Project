using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Maze : MonoBehaviour {

    public IntVector2 size;
    public MazeCell cellPrefab;
    private MazeCell [,] cells;
    public float generationStepDelay;
    public MazePassage passagePrefab;
    public MazeWall wallPrefab;

    public MazeCell GetCell (IntVector2 coordinates) {
        return cells[coordinates.x, coordinates.z];
    }

    public IEnumerator Generate() {
        WaitForSeconds delay = new WaitForSeconds(generationStepDelay);
        cells = new MazeCell[size.x , size.z];
        List<MazeCell> activeCells = new List<MazeCell>();
        DoFirstGeneartionStep(activeCells);
        // while (activeCells.Count > 1) {
        //     yield return delay;
        //     StartUpMiddle(activeCells);
        // }
        while (activeCells.Count > 0) {
            yield return delay;
            DoNextGeneartionStep(activeCells);
        }
    }



    // public void Generate() {
    //     //WaitForSeconds delay = new WaitForSeconds(generationStepDelay);
    //     cells = new MazeCell[size.x , size.z];
    //     List<MazeCell> activeCells = new List<MazeCell>();
    //     DoFirstGeneartionStep(activeCells);
    //     while (activeCells.Count > 0) {
    //         //yield return delay;
    //         DoNextGeneartionStep(activeCells);
    //     }
    // }



    public IntVector2 StartingCoordinates {
        get {
            return new IntVector2(size.x / 2, 0);
        }
    }
    public IntVector2 TopMiddleCorner {
        get {
            return new IntVector2(size.x / 2, size.z / 2 + 1);
        }
    }
    public IntVector2 TopRightCorner {
        get {
            return new IntVector2(size.x / 2 + 1, size.z / 2 + 1);
        }
    }


    public bool ContainsCoordinates (IntVector2 coordinates) {
        return coordinates.x >= 0 && coordinates.x < size.x && coordinates.z >= 0 && coordinates.z < size.z;
    }

    public bool InMiddle (IntVector2 coordinates) {
        return coordinates.x <= size.x / 2 + 1 && coordinates.x >= size.x / 2 - 1 && 
        coordinates.z <= size.z / 2 + 1 && coordinates.z >= size.z /2 - 1;
    }

    public bool JustOutMiddle (IntVector2 coordinates) {
        return coordinates.x <= size.x / 2 + 2 && coordinates.x >= size.x / 2 - 2 && 
        coordinates.z <= size.z / 2 + 2 && coordinates.z >= size.z /2 - 2 &&
        !(coordinates.x <= size.x / 2 + 1 && coordinates.x >= size.x / 2 - 1 && 
        coordinates.z <= size.z / 2 + 1 && coordinates.z >= size.z /2 - 1) && 
        !(coordinates.x == TopMiddleCorner.x && coordinates.z == TopMiddleCorner.z + 1);
    }

    private void DoFirstGeneartionStep(List<MazeCell> activeCells) {
        //activeCells.Add(CreateCell(StartingCoordinates));
        activeCells.Add(CreateCell(TopRightCorner));
        
    }
    private void DoNextGeneartionStep(List<MazeCell> activeCells) {
        int currentIndex = activeCells.Count - 1;
        MazeCell currentCell = activeCells[currentIndex];
        if (currentCell.IsFullyInitialized) {
            activeCells.RemoveAt(currentIndex);
            return;
        }
        MazeDirection direction = currentCell.RandomUninitializedDirection;
        IntVector2 coordinates = currentCell.coordinates + direction.ToIntVector2();
        if ( ContainsCoordinates(coordinates)) {
            if ( InMiddle(coordinates)) {
                MazeCell neighbor = GetCell(coordinates);
                if (neighbor == null){
                    neighbor = CreateCell(coordinates);
                    CreatePassage(currentCell, neighbor, direction);
                    activeCells.Add(neighbor);    
                }
                else {
                    CreatePassage(currentCell, neighbor, direction);
                    activeCells.Add(neighbor);
                }
            
            }
            else if (JustOutMiddle(coordinates)) {
                    CreateWall(currentCell, null, direction);
            }
            else {
                MazeCell neighbor = GetCell(coordinates);
                
                if (neighbor == null) {
                    neighbor = CreateCell(coordinates);
                    CreatePassage(currentCell, neighbor, direction);
                    activeCells.Add(neighbor);
                }
                else if (neighbor.coordinates.x == TopMiddleCorner.x && neighbor.coordinates.z == TopMiddleCorner.z) {
                        neighbor = CreateCell(coordinates);
                        CreatePassage(currentCell, neighbor, direction);
                }
                else {
                    CreateWall(currentCell, neighbor, direction);
                }
            }   
        }
        else {
            CreateWall(currentCell, null, direction);
        }
    }

    private MazeCell CreateCell (IntVector2 coordinates) {
        MazeCell newCell = Instantiate(cellPrefab) as MazeCell;
        cells[coordinates.x, coordinates.z] = newCell;
        newCell.coordinates = coordinates;
        newCell.name = "Maze Cell " + coordinates.x + "," + coordinates.z;
        newCell.transform.parent = transform;
        newCell.transform.localPosition = new Vector3(coordinates.x - size.x * 0.5f + 0.5f, 0f, coordinates.z - size.z * 0.5f + 0.5f);
        return newCell;
    }

    private void CreatePassage (MazeCell cell, MazeCell otherCell, MazeDirection direction) {
        MazePassage passage = Instantiate(passagePrefab) as MazePassage;
        passage.Initialize(cell, otherCell, direction);
        passage = Instantiate(passagePrefab) as MazePassage;
        passage.Initialize(otherCell, cell, direction.GetOpposite());
    }

    private void CreateWall (MazeCell cell, MazeCell otherCell, MazeDirection direction) {
        MazeWall wall = Instantiate(wallPrefab) as MazeWall;
        wall.Initialize(cell, otherCell, direction);
        if (otherCell != null) {
            wall = Instantiate(wallPrefab) as MazeWall;
            wall.Initialize(otherCell, cell, direction.GetOpposite());
        }
    }



}
