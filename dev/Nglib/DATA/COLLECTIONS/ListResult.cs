using System.Collections;
using System.Collections.Generic;

namespace Nglib.DATA.COLLECTIONS
{
    public class ListResult<T> : ICollection<T>, IEnumerable<T>
    {
        public ListResult()
        {
        }

        public ListResult(IEnumerable<T> orgnData)
        {
            if (orgnData != null) data.AddRange(orgnData);
        }


        public List<T> data { get; set; } = new();

        public ResultInfos info { get; set; } = new();

        public IEnumerator<T> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return data.GetEnumerator();
        }

        public int Count => data.Count;

        public bool IsReadOnly => false;

        public void Clear()
        {
            data.Clear();
        }

        public bool Contains(T item)
        {
            return data.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            data.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return data.Remove(item);
        }


        public void Add(T item)
        {
            data.Add(item);
        }

        public void AddRange(IEnumerable<T> items)
        {
            data.AddRange(items);
            //if(info.TotalResult==0)
        }

        public static ListResult<T> FromList(IEnumerable<T> orgnData)
        {
            if (orgnData == null) return null;
            return new ListResult<T>(orgnData);
        }

        public List<T> ToList()
        {
            var retour = new List<T>(data);
            return retour;
        }

        public override string ToString()
        {
            return data?.ToString();
        }


        public static ListResult<T> PrepareForError(string errorMsg)
        {
            var retour = new ListResult<T>();
            retour.info.ErrorMessage = errorMsg;
            return retour;
        }

        public class ResultInfos
        {
            public string ErrorMessage { get; set; }
            public int TotalResult { get; set; }
        }
        //public static ListResult<object> PrepareForError(string errorMsg)
        //{
        //    var retour = new ListResult<object>();
        //    retour.info.ErrorMessage = errorMsg;
        //    return retour;
        //}
    }
}