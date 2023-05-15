using System;
using System.Collections;
using System.Collections.Generic;

namespace GraphClassLibrary
{
    public class NodeST<T>
    {
        public NodeST(T data)
        {
            Data = data;
        }
        public T Data { get; set; }
        public NodeST<T> Next { get; set; }
    }

    public class NodeStack<T> : IEnumerable<T>
    {
        NodeST<T> head;
        int count;

        public bool IsEmpty
        {
            get { return count == 0; }
        }
        public int Count
        {
            get { return count; }
        }

        public void Push(T item)
        {
            // увеличиваем стек
            NodeST<T> node = new NodeST<T>(item);
            node.Next = head; // переустанавливаем верхушку стека на новый элемент
            head = node;
            count++;
        }
        public T Pop()
        {
            // если стек пуст, выбрасываем исключение
            if (IsEmpty)
                throw new InvalidOperationException("Стек пуст");
            NodeST<T> temp = head;
            head = head.Next; // переустанавливаем верхушку стека на следующий элемент
            count--;
            return temp.Data;
        }
        public T Peek()
        {
            if (IsEmpty)
                throw new InvalidOperationException("Стек пуст");
            return head.Data;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this).GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            NodeST<T> current = head;
            while (current != null)
            {
                yield return current.Data;
                current = current.Next;
            }
        }
    }
}