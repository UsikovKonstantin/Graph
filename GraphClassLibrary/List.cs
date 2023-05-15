using System;
using System.Collections;
using System.Collections.Generic;

namespace GraphClassLibrary
{
    public class ListNode<T>
    {
        public ListNode(T data)
        {
            Data = data;
        }
        public T Data { get; set; }
        public ListNode<T> Next { get; set; }
    }

    public class LinkedList<T> : IEnumerable<T>  // односвязный список
    {
        ListNode<T> head; // головной/первый элемент
        ListNode<T> tail; // последний/хвостовой элемент
        int count;  // количество элементов в списке

        // добавление элемента
        public void Add(T data)
        {
            ListNode<T> node = new ListNode<T>(data);

            if (head == null)
                head = node;
            else
                tail.Next = node;
            tail = node;

            count++;
        }

        // удаление элемента
        public bool Remove(T data)
        {
            ListNode<T> current = head;
            ListNode<T> previous = null;

            while (current != null && current.Data != null)
            {
                if (current.Data.Equals(data))
                {
                    // Если узел в середине или в конце
                    if (previous != null)
                    {
                        // убираем узел current, теперь previous ссылается не на current, а на current.Next
                        previous.Next = current.Next;

                        // Если current.Next не установлен, значит узел последний,
                        // изменяем переменную tail
                        if (current.Next == null)
                            tail = previous;
                    }
                    else
                    {
                        // если удаляется первый элемент
                        // переустанавливаем значение head
                        head = head?.Next;

                        // если после удаления список пуст, сбрасываем tail
                        if (head == null)
                            tail = null;
                    }
                    count--;
                    return true;
                }

                previous = current;
                current = current.Next;
            }
            return false;
        }

        public int Count { get { return count; } }
        public bool IsEmpty { get { return count == 0; } }

        // очистка списка
        public void Clear()
        {
            head = null;
            tail = null;
            count = 0;
        }

        // содержит ли список элемент
        public bool Contains(T data)
        {
            ListNode<T> current = head;
            while (current != null && current.Data != null)
            {
                if (current.Data.Equals(data)) return true;
                current = current.Next;
            }
            return false;
        }

        // добвление в начало
        public void AppendFirst(T data)
        {
            ListNode<T> node = new ListNode<T>(data);
            node.Next = head;
            head = node;
            if (count == 0)
                tail = head;
            count++;
        }

        // Добавление по индексу
        public void AddAt(T val, int index)
        {
            ListNode<T> pr = null;
            var nxt = head;
            for (int i = 0; i < index; i++)
            {
                pr = nxt;
                nxt = nxt.Next;
            }

            if (pr == null)
            {
                AppendFirst(val);
                return;
            }

            var saveNxt = nxt;
            pr.Next = new ListNode<T>(val);
            pr.Next.Next = saveNxt;
            count++;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            ListNode<T> current = head;
            while (current != null)
            {
                yield return current.Data;
                current = current.Next;
            }
        }

        // реализация интерфейса IEnumerable
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>)this).GetEnumerator();
        }

        // Удаление первого элемента.
        public T RemoveFirst()
        {
            var item = head;
            head = item.Next;
            if (head == null)
                tail = null;
            count--;
            return item.Data;
        }

        // Удаление элмента по индексу.
        public T RemoveAt(int index)
        {
            if (index == 0)
                return RemoveFirst();
            ListNode<T> pr = null;
            var nxt = head;
            for (int i = 0; i < index; i++)
            {
                pr = nxt;
                nxt = nxt.Next;
            }
            count--;
            var res = nxt.Data;
            pr.Next = nxt.Next;
            return res;
        }

        public T this[int index]
        {
            get
            {
                // если индекс имеется в стеке
                if (index >= 0 && index < Count)
                {
                    ListNode<T> cur = head;
                    for (int i = 0; i < index; i++)
                    {
                        cur = cur.Next;
                    }
                    return cur.Data; // то возвращаем объект по индексу
                }
                else
                    throw new ArgumentOutOfRangeException(); // иначе генерируем исключение
            }
            set
            {
                // если индекс есть в стеке
                if (index >= 0 && index < Count)
                {
                    ListNode<T> cur = head;
                    for (int i = 0; i < Count; i++)
                    {
                        cur = cur.Next;
                    }
                    cur.Data = value;  // переустанавливаем значение по индексу
                    //cur.Next
                    //AddAt(value, index);
                }
            }
        }
    }
}