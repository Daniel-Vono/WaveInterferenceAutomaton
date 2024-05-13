// File Name: Node.cs
// Creation Date: Dec. 20, 2023
// Description: Represents a node for a linked list that carries data


//Represents a node for a linked list that carries data
class Node<T>
{
    //Stores the data of the node
    public T Value { get; private set; }

    //Stores a reference to the next node
    public Node<T> Next { get; set; }

    /// <summary>
    /// Creates a new node
    /// </summary>
    /// <param name="value">The value of the node</param>
    public Node(T value)
    {
        //Initializes the value of the node and the reference to the next node
        Value = value;
        Next = null;
    }
}