using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Nglib.DATA.COLLECTIONS
{
    public class ListResult<T> : ICollection<T>, IEnumerable<T>
    {
        public ListResult() { }
        public ListResult(IEnumerable<T> orgnData) { if(orgnData!=null) this.data.AddRange(orgnData); }

        public IEnumerator<T> GetEnumerator()
        {
            return this.data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.data.GetEnumerator();
        }




        public List<T> data { get; set; } = new List<T>();

        public ResultInfos info { get; set; } = new ResultInfos();

        public int Count => this.data.Count;

        public bool IsReadOnly => false;

        public void Clear(){this.data.Clear();}

        public bool Contains(T item)       {return this.data.Contains(item);}

        public void CopyTo(T[] array, int arrayIndex){ this.data.CopyTo(array, arrayIndex); }

        public bool Remove(T item){ return this.data.Remove(item); }


        public void Add(T item) { this.data.Add(item); }
        public void AddRange(IEnumerable<T> items)
        {
 
            this.data.AddRange(items);
            //if(info.TotalResult==0)
        }

        public static ListResult<T> FromList(IEnumerable<T> orgnData)
        {
            if (orgnData == null) return null;
            return new ListResult<T>(orgnData);
        }

        public List<T> ToList()
        {
            List<T> retour = new List<T>(this.data);
            return retour;
        }


        public class ResultInfos
        {
            public int TotalResult { get; set; }
        }

    }
}
