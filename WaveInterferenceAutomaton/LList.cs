// File Name: LList.cs
// Creation Date: Dec. 20, 2023
// Description: Custom implemenation of the linked list data structure

using System;


class LList<T>
{
    //Stores the head of the linked list
    public Node<T> Head { get; private set; }

    //Stores how many elements are in the linked list
    public short Count { get; private set; }

    /// <summary>
    /// Creates a new instance of an empty linked list
    /// </summary>
    public LList()
    {
        //Sets the head to null since there are no nodes
        Head = null;

        //Sets the count to zero
        Count = 0;
    }

    /// <summary>
    /// Adds the value to the end of the linked list as a node
    /// </summary>
    /// <param name="value">The value to add to the end of the linke list</param>
    public void AddToTail(T value)
    {
        //Stores the value as a node
        Node<T> newNode = new Node<T>(value);

        //If there is no head, make this node the new head and exit the subprogram
        if (Head == null)
        {
            Head = newNode;
            Count++;
            return;
        }

        //Stores the current node being itterated on
        Node<T> currNode = Head;

        //Keep looping until the true tail of the linked list has been found
        while (currNode.Next != null)
        {
            currNode = currNode.Next;
        }

        //Set the next node of the tail to the new node to add and increment the count
        currNode.Next = newNode;
        Count++;
    }

    /// <summary>
    /// Removes the node at a specified index
    /// </summary>
    /// <param name="index">The index of the node to remove</param>
    /// <returns>A boolean representing whether or not the item was deleted successfully or not</returns>
    public bool RemoveAt(short index)
    {
        //Exit the subprogram if there are no nodes in the linked list
        if (IsEmpty()) return false;

        //Exit the subprogram
        if (index + 1 > Count) return false;

        //Stores the current node and the parent of that node
        Node<T> currNode = Head;
        Node<T> currParentNode = null;

        //Loop until the node to delete has been found
        for (short i = 0; i < index; i++)
        {
            currParentNode = currNode;
            currNode = currParentNode.Next;
        }

        //Deletes the node based on where it is in the linked list
        if (index == 0)
        {
            //Replace the head with the next node to delete the old head
            Head = Head.Next;
        }
        else if (index == Count - 1)
        {
            //The node to delete is at the end so set the parent's next node to null
            currParentNode.Next = null;
        }
        else
        {
            //Skips the node to delete in the links so the garbage collecter removes it
            currParentNode.Next = currParentNode.Next.Next;
        }

        //Decrement the count of the linked list
        Count--;

        //Return a successful deletion
        return true;
    }

    /// <summary>
    /// Returns a boolean representing if the linked list is empty
    /// </summary>
    /// <returns>A boolean representing if the linked list is empty</returns>
    public bool IsEmpty()
    {
        //If there are no elements the linked list is empty, otherwise it is not empty
        if (Count == 0) return true;
        return false;
    }

    /// <summary>
    /// Itterates through all elements of the linked list and performs some type of action
    /// </summary>
    /// <param name="behaviour">The action to be done on all elements of the linked list</param>
    public void IterateGeneric(Action<LList<T>, Node<T>, short> behaviour)
    {
        //Exit if the linked list is empty
        if (IsEmpty()) return;

        //Stores the current node
        Node<T> currNode = Head;

        //Loops through all elements in the linked list
        for (short i = 0; i < Count; i++)
        {
            //Do the specified action on the element
            behaviour(this, currNode, i);

            //Set the current node to the next node in the linked list
            currNode = currNode.Next;
        }
    }

    /// <summary>
    /// Empties the linked list
    /// </summary>
    public void Clear()
    {
        //Set the head equal to null and remove all references to existing nodess
        Head = null;

        //Sets the count to zero
        Count = 0;
    }
}