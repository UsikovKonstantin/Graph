using System;
using System.Collections;
using System.Collections.Generic;

namespace GraphClassLibrary
{
    public class NodeC<T>
    {
        public NodeC(T data)
        {
            Data = data;
        }
        public T Data { get; set; }
        public NodeC<T> Next { get; set; }
    }
    public class CircularLinkedList<T> : IEnumerable<T>  // кольцевой связный список
    {
        NodeC<T> head; // головной/первый элемент
        NodeC<T> tail; // последний/хвостовой элемент
        int count;  // количество элементов в списке

        // добавление элемента
        public void Add(T data)
        {
            NodeC<T> node = new NodeC<T>(data);
            // если список пуст
            if (head == null)
            {
                head = node;
                tail = node;
                tail.Next = head;
            }
            else
            {
                node.Next = head;
                tail.Next = node;
                tail = node;
            }
            count++;
        }
        public bool Remove(T data)
        {
            NodeC<T> current = head;
            NodeC<T> previous = null;

            if (IsEmpty) return false;

            do
            {
                if (current.Data.Equals(data))
                {
                    // Если узел в середине или в конце
                    if (previous != null)
                    {
                        // убираем узел current, теперь previous ссылается не на current, а на current.Next
                        previous.Next = current.Next;

                        // Если узел последний,
                        // изменяем переменную tail
                        if (current == tail)
                            tail = previous;
                    }
                    else // если удаляется первый элемент
                    {

                        // если в списке всего один элемент
                        if (count == 1)
                        {
                            head = tail = null;
                        }
                        else
                        {
                            head = current.Next;
                            tail.Next = current.Next;
                        }
                    }
                    count--;
                    return true;
                }

                previous = current;
                current = current.Next;
            } while (current != head);

            return false;
        }

        public int Count { get { return count; } }
        public bool IsEmpty { get { return count == 0; } }

        public void Clear()
        {
            head = null;
            tail = null;
            count = 0;
        }

        public bool Contains(T data)
        {
            NodeC<T> current = head;
            if (current == null) return false;
            do
            {
                if (current.Data.Equals(data))
                    return true;
                current = current.Next;
            }
            while (current != head);
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this).GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            NodeC<T> current = head;
            do
            {
                if (current != null)
                {
                    yield return current.Data;
                    current = current.Next;
                }
            }
            while (current != head);
        }
    }



    public class DoublyCNode<T>
    {
        public DoublyCNode(T data)
        {
            Data = data;
        }
        public T Data { get; set; }
        public DoublyCNode<T> Previous { get; set; }
        public DoublyCNode<T> Next { get; set; }
    }
    public class CircularDoublyLinkedList<T> : IEnumerable<T>  // кольцевой двусвязный список
    {
        DoublyCNode<T> head; // головной/первый элемент
        int count;  // количество элементов в списке

        // добавление элемента
        public void Add(T data)
        {
            DoublyCNode<T> node = new DoublyCNode<T>(data);

            if (head == null)
            {
                head = node;
                head.Next = node;
                head.Previous = node;
            }
            else
            {
                node.Previous = head.Previous;
                node.Next = head;
                head.Previous.Next = node;
                head.Previous = node;
            }
            count++;
        }
        // удаление элемента
        public bool Remove(T data)
        {
            DoublyCNode<T> current = head;

            DoublyCNode<T> removedItem = null;
            if (count == 0) return false;

            // поиск удаляемого узла
            do
            {
                if (current.Data.Equals(data))
                {
                    removedItem = current;
                    break;
                }
                current = current.Next;
            }
            while (current != head);

            if (removedItem != null)
            {
                // если удаляется единственный элемент списка
                if (count == 1)
                    head = null;
                else
                {
                    // если удаляется первый элемент
                    if (removedItem == head)
                    {
                        head = head.Next;
                    }
                    removedItem.Previous.Next = removedItem.Next;
                    removedItem.Next.Previous = removedItem.Previous;
                }
                count--;
                return true;
            }
            return false;
        }
        public int Count { get { return count; } }
        public bool IsEmpty { get { return count == 0; } }

        public void Clear()
        {
            head = null;
            count = 0;
        }

        public bool Contains(T data)
        {
            DoublyCNode<T> current = head;
            if (current == null) return false;
            do
            {
                if (current.Data.Equals(data))
                    return true;
                current = current.Next;
            }
            while (current != head);
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this).GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            DoublyCNode<T> current = head;
            do
            {
                if (current != null)
                {
                    yield return current.Data;
                    current = current.Next;
                }
            }
            while (current != head);
        }
    }
}